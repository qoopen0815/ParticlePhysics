using Unity.Mathematics;
using UnityEngine;


namespace GpuDeformableTerrain
{
    public class CubeParticle : GranularSubstance
    {
        private static uint _granularParticleNum = 9;
        public static float particleRatio = 0.3f;

        #region Accessor
        public uint GranularParticleNum => _granularParticleNum;
        public float TotalMass => totalMass;
        public Vector3 CenterOfMass => centerOfMass;
        public float3x3 InertialMoment => inertialMoment;
        public ParticleSubstance[] Particles => particles;
        #endregion

        public CubeParticle(float radius = 0.04f, float density = 2000.0f) : base(_granularParticleNum)
        {
            particles = InitializeParticle(radius, density);
            totalMass = CalculateTotalParticleMass(particles, density);
            centerOfMass = CalculateCenterOfMass(particles);
            inertialMoment = CalculateInverseInertialMoment(particles);
        }

        protected override ParticleSubstance[] InitializeParticle(float particleRadius, float particleDensity)
        {
            float CORNER = 1.0f / math.sqrt(3.0f);
            ParticleSubstance[] p = new ParticleSubstance[_granularParticleNum];
            p[0] = new ParticleSubstance(particleRadius, CalculateParticleMass(particleRadius, particleDensity), new float3(0.0f, 0.0f, 0.0f) * particleRadius);
            p[1] = new ParticleSubstance(particleRadius * particleRatio, CalculateParticleMass(particleRadius * particleRatio, particleDensity), new float3(CORNER, CORNER, CORNER) * particleRadius * (1.0f + particleRatio));
            p[2] = new ParticleSubstance(particleRadius * particleRatio, CalculateParticleMass(particleRadius * particleRatio, particleDensity), new float3(CORNER, CORNER, -CORNER) * particleRadius * (1.0f + particleRatio));
            p[3] = new ParticleSubstance(particleRadius * particleRatio, CalculateParticleMass(particleRadius * particleRatio, particleDensity), new float3(-CORNER, CORNER, -CORNER) * particleRadius * (1.0f + particleRatio));
            p[4] = new ParticleSubstance(particleRadius * particleRatio, CalculateParticleMass(particleRadius * particleRatio, particleDensity), new float3(-CORNER, CORNER, CORNER) * particleRadius * (1.0f + particleRatio));
            p[5] = new ParticleSubstance(particleRadius * particleRatio, CalculateParticleMass(particleRadius * particleRatio, particleDensity), new float3(CORNER, -CORNER, CORNER) * particleRadius * (1.0f + particleRatio));
            p[6] = new ParticleSubstance(particleRadius * particleRatio, CalculateParticleMass(particleRadius * particleRatio, particleDensity), new float3(CORNER, -CORNER, -CORNER) * particleRadius * (1.0f + particleRatio));
            p[7] = new ParticleSubstance(particleRadius * particleRatio, CalculateParticleMass(particleRadius * particleRatio, particleDensity), new float3(-CORNER, -CORNER, -CORNER) * particleRadius * (1.0f + particleRatio));
            p[8] = new ParticleSubstance(particleRadius * particleRatio, CalculateParticleMass(particleRadius * particleRatio, particleDensity), new float3(-CORNER, -CORNER, CORNER) * particleRadius * (1.0f + particleRatio));
            return p;
        }
    }
}
