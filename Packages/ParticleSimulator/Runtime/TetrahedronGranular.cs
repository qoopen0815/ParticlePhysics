using Unity.Mathematics;

namespace ParticleSimulator.Substance
{
    public class TetrahedronGranular : BasicGranular
    {
        private static new uint _granularParticleNum = 4;
        private static new float _particleRatio = 0.5f;

        public TetrahedronGranular(float radius = 0.04f, float density = 2000.0f) : base(_granularParticleNum)
        {
            particles = InitializeParticle(radius, density);
            totalMass = CalculateTotalParticleMass(particles, density);
            centerOfMass = CalculateCenterOfMass(particles);
            inertialMoment = CalculateInverseInertialMoment(particles);
        }

        protected override Particle[] InitializeParticle(float particleRadius, float particleDensity)
        {
            Particle[] p = new Particle[_granularParticleNum];
            p[0] = new Particle(particleRadius * _particleRatio, CalculateParticleMass(particleRadius * _particleRatio, particleDensity), new float3(1.0f, 0.0f, -1.0f / math.sqrt(2.0f)) * particleRadius * _particleRatio);
            p[1] = new Particle(particleRadius * _particleRatio, CalculateParticleMass(particleRadius * _particleRatio, particleDensity), new float3(-1.0f, 0.0f, -1.0f / math.sqrt(2.0f)) * particleRadius * _particleRatio);
            p[2] = new Particle(particleRadius * _particleRatio, CalculateParticleMass(particleRadius * _particleRatio, particleDensity), new float3(0.0f, 1.0f, 1.0f / math.sqrt(2.0f)) * particleRadius * _particleRatio);
            p[3] = new Particle(particleRadius * _particleRatio, CalculateParticleMass(particleRadius * _particleRatio, particleDensity), new float3(0.0f, -1.0f, 1.0f / math.sqrt(2.0f)) * particleRadius * _particleRatio);
            return p;
        }
    }
}
