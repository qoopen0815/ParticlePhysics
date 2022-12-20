using Unity.Mathematics;
using UnityEngine;

namespace GpuDeformableTerrain
{
    public class TetrahedronParticle : GranularSubstance
    {
        private static uint _granularParticleNum = 4;
        private static float _particleRatio = 0.5f;

        #region Accessor
        public uint GranularParticleNum => _granularParticleNum;
        public float TotalMass => totalMass;
        public Vector3 CenterOfMass => centerOfMass;
        public float3x3 InertialMoment => inertialMoment;
        public ParticleSubstance[] Particles => particles;
        #endregion

        public TetrahedronParticle(float radius = 0.04f, float density = 2000.0f) : base(_granularParticleNum)
        {
            particles = InitializeParticle(radius, density);
            totalMass = CalculateTotalParticleMass(particles, density);
            centerOfMass = CalculateCenterOfMass(particles);
            inertialMoment = CalculateInverseInertialMoment(particles);
        }

        protected override ParticleSubstance[] InitializeParticle(float particleRadius, float particleDensity)
        {
            ParticleSubstance[] p = new ParticleSubstance[_granularParticleNum];
            p[0] = new ParticleSubstance(particleRadius * _particleRatio, CalculateParticleMass(particleRadius * _particleRatio, particleDensity), new float3(1.0f, 0.0f, -1.0f / math.sqrt(2.0f)) * particleRadius * _particleRatio);
            p[1] = new ParticleSubstance(particleRadius * _particleRatio, CalculateParticleMass(particleRadius * _particleRatio, particleDensity), new float3(-1.0f, 0.0f, -1.0f / math.sqrt(2.0f)) * particleRadius * _particleRatio);
            p[2] = new ParticleSubstance(particleRadius * _particleRatio, CalculateParticleMass(particleRadius * _particleRatio, particleDensity), new float3(0.0f, 1.0f, 1.0f / math.sqrt(2.0f)) * particleRadius * _particleRatio);
            p[3] = new ParticleSubstance(particleRadius * _particleRatio, CalculateParticleMass(particleRadius * _particleRatio, particleDensity), new float3(0.0f, -1.0f, 1.0f / math.sqrt(2.0f)) * particleRadius * _particleRatio);
            return p;
        }
    }
}
