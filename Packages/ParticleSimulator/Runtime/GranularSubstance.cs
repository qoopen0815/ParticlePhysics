using Unity.Mathematics;
using UnityEngine;

namespace GpuDeformableTerrain
{
    public struct ParticleSubstance
    {
        public float radius;
        public float mass;
        public Vector3 offsetFromGranularCenter;

        public ParticleSubstance(float radius, float mass, Vector3 offsetFromGranularCenter)
        {
            this.radius = radius;
            this.mass = mass;
            this.offsetFromGranularCenter = offsetFromGranularCenter;
        }
    }

    public abstract class GranularSubstance
    {
        protected ParticleSubstance[] particles;
        protected float totalMass;
        protected float3 centerOfMass;
        protected float3x3 inertialMoment;

        protected GranularSubstance(uint particleNum)
        {
            this.particles = new ParticleSubstance[particleNum];
        }

        protected float CalculateParticleMass(float particleRadius, float particleDensity)
        {
            float particle_volume = (4.0f / 3.0f) * math.PI * math.pow(particleRadius, 3);
            float particle_mass = particle_volume * particleDensity;
            return particle_mass;
        }

        protected float3x3 CalculateInverseInertialMoment(ParticleSubstance[] componentParticles)
        {
            float3x3 inertialMoment = float3x3.zero;

            foreach (ParticleSubstance particle in componentParticles)
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

        protected float CalculateTotalParticleMass(ParticleSubstance[] componentParticles, float particleDensity)
        {
            float totalParticleMass = 0;
            foreach (ParticleSubstance particle in componentParticles)
            {
                totalParticleMass += CalculateParticleMass(particle.radius, particleDensity);
            }
            return totalParticleMass;
        }

        protected float3 CalculateCenterOfMass(ParticleSubstance[] componentParticles)
        {
            float3 centerOfMass = float3.zero;
            foreach (ParticleSubstance particle in componentParticles)
            {
                centerOfMass += new float3(particle.offsetFromGranularCenter);
            }
            return centerOfMass / (float)this.particles.Length;
        }

        protected abstract ParticleSubstance[] InitializeParticle(float particleRadius, float particleDensity);
    }
}
