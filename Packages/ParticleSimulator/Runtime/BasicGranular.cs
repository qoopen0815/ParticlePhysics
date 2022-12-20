using Unity.Mathematics;
using UnityEngine;

namespace ParticleSimulator.Substance
{
    public class BasicGranular
    {
        protected Particle[] particles;
        protected uint _granularParticleNum = 1;
        protected float _particleRatio = 1.0f;

        protected float totalMass;
        protected float3 centerOfMass;
        protected float3x3 inertialMoment;

        #region Accessor
        public uint GranularParticleNum => _granularParticleNum;
        public float TotalMass => totalMass;
        public Vector3 CenterOfMass => centerOfMass;
        public float3x3 InertialMoment => inertialMoment;
        public Particle[] Particles => particles;
        #endregion

        protected BasicGranular(uint particleNum)
        {
            this.particles = new Particle[particleNum];
        }

        protected float CalculateParticleMass(float particleRadius, float particleDensity)
        {
            float particle_volume = (4.0f / 3.0f) * math.PI * math.pow(particleRadius, 3);
            float particle_mass = particle_volume * particleDensity;
            return particle_mass;
        }

        protected float3x3 CalculateInverseInertialMoment(Particle[] componentParticles)
        {
            float3x3 inertialMoment = float3x3.zero;

            foreach (Particle particle in componentParticles)
            {
                float3x3 particleInertialMoment = float3x3.identity * (2.0f / 5.0f) * particle.mass * math.pow(particle.radius, 2);

                float3 offset = particle.offsetFromGranularCenter;
                float offsetSquared = math.dot(offset, offset);
                float3x3 offsetParticleInertialMoment = particleInertialMoment + particle.mass * (offsetSquared * float3x3.identity - math.mul(offset, offset));
                inertialMoment += offsetParticleInertialMoment;
            }
            inertialMoment = math.transpose(inertialMoment);

            return inertialMoment;
        }

        protected float CalculateTotalParticleMass(Particle[] componentParticles, float particleDensity)
        {
            float totalParticleMass = 0;
            foreach (Particle particle in componentParticles)
            {
                totalParticleMass += CalculateParticleMass(particle.radius, particleDensity);
            }
            return totalParticleMass;
        }

        protected float3 CalculateCenterOfMass(Particle[] componentParticles)
        {
            float3 centerOfMass = float3.zero;
            foreach (Particle particle in componentParticles)
            {
                centerOfMass += new float3(particle.offsetFromGranularCenter);
            }
            return centerOfMass / (float)this.particles.Length;
        }

        protected virtual Particle[] InitializeParticle(float particleRadius, float particleDensity)
        {
            Particle[] p = new Particle[_granularParticleNum];
            p[0] = new Particle(particleRadius * _particleRatio, CalculateParticleMass(particleRadius * _particleRatio, particleDensity), new float3(1.0f, 0.0f, -1.0f / math.sqrt(2.0f)) * particleRadius * _particleRatio);
            return p;
        }
    }
}
