using Unity.Mathematics;

namespace ParticleSimulator.Substance
{
    public class CubeGranular : BasicGranular
    {
        private static new uint _granularParticleNum = 9;
        private static new float _particleRatio = 0.3f;

        public CubeGranular(float radius = 0.04f, float density = 2000.0f) : base(_granularParticleNum)
        {
            particles = InitializeParticle(radius, density);
            totalMass = CalculateTotalParticleMass(particles, density);
            centerOfMass = CalculateCenterOfMass(particles);
            inertialMoment = CalculateInverseInertialMoment(particles);
        }

        protected override Particle[] InitializeParticle(float particleRadius, float particleDensity)
        {
            float CORNER = 1.0f / math.sqrt(3.0f);
            Particle[] p = new Particle[_granularParticleNum];
            p[0] = new Particle(particleRadius, CalculateParticleMass(particleRadius, particleDensity), new float3(0.0f, 0.0f, 0.0f) * particleRadius);
            p[1] = new Particle(particleRadius * _particleRatio, CalculateParticleMass(particleRadius * _particleRatio, particleDensity), new float3(CORNER, CORNER, CORNER) * particleRadius * (1.0f + _particleRatio));
            p[2] = new Particle(particleRadius * _particleRatio, CalculateParticleMass(particleRadius * _particleRatio, particleDensity), new float3(CORNER, CORNER, -CORNER) * particleRadius * (1.0f + _particleRatio));
            p[3] = new Particle(particleRadius * _particleRatio, CalculateParticleMass(particleRadius * _particleRatio, particleDensity), new float3(-CORNER, CORNER, -CORNER) * particleRadius * (1.0f + _particleRatio));
            p[4] = new Particle(particleRadius * _particleRatio, CalculateParticleMass(particleRadius * _particleRatio, particleDensity), new float3(-CORNER, CORNER, CORNER) * particleRadius * (1.0f + _particleRatio));
            p[5] = new Particle(particleRadius * _particleRatio, CalculateParticleMass(particleRadius * _particleRatio, particleDensity), new float3(CORNER, -CORNER, CORNER) * particleRadius * (1.0f + _particleRatio));
            p[6] = new Particle(particleRadius * _particleRatio, CalculateParticleMass(particleRadius * _particleRatio, particleDensity), new float3(CORNER, -CORNER, -CORNER) * particleRadius * (1.0f + _particleRatio));
            p[7] = new Particle(particleRadius * _particleRatio, CalculateParticleMass(particleRadius * _particleRatio, particleDensity), new float3(-CORNER, -CORNER, -CORNER) * particleRadius * (1.0f + _particleRatio));
            p[8] = new Particle(particleRadius * _particleRatio, CalculateParticleMass(particleRadius * _particleRatio, particleDensity), new float3(-CORNER, -CORNER, CORNER) * particleRadius * (1.0f + _particleRatio));
            return p;
        }
    }
}
