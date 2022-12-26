using Unity.Mathematics;

namespace ParticleSimulator.Substance
{
    public class CubeParticle : ParticleSubstance
    {
        private static uint _elementNum = 9;
        private static float _elementRatio = 0.3f;

        public CubeParticle(float radius = 0.04f, float density = 2000.0f) : base(_elementNum)
        {
            elements = SetElements(radius, density);
            totalMass = CalculateTotalMass(elements, density);
            centerOfMass = CalculateCenterOfMass(elements);
            inertialMoment = CalculateInverseInertialMoment(elements);
        }

        protected override ElementType[] SetElements(float particleRadius, float particleDensity)
        {
            float CORNER = 1.0f / math.sqrt(3.0f);
            ElementType[] e = new ElementType[] {
                new ElementType(particleRadius, CalculateElementMass(particleRadius, particleDensity), 0.05f, new float3(0.0f, 0.0f, 0.0f) * particleRadius),
                new ElementType(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), 0.05f, new float3(CORNER, CORNER, CORNER) * particleRadius * (1.0f + _elementRatio)),
                new ElementType(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), 0.05f, new float3(CORNER, CORNER, -CORNER) * particleRadius * (1.0f + _elementRatio)),
                new ElementType(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), 0.05f, new float3(-CORNER, CORNER, -CORNER) * particleRadius * (1.0f + _elementRatio)),
                new ElementType(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), 0.05f, new float3(-CORNER, CORNER, CORNER) * particleRadius * (1.0f + _elementRatio)),
                new ElementType(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), 0.05f, new float3(CORNER, -CORNER, CORNER) * particleRadius * (1.0f + _elementRatio)),
                new ElementType(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), 0.05f, new float3(CORNER, -CORNER, -CORNER) * particleRadius * (1.0f + _elementRatio)),
                new ElementType(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), 0.05f, new float3(-CORNER, -CORNER, -CORNER) * particleRadius * (1.0f + _elementRatio)),
                new ElementType(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), 0.05f, new float3(-CORNER, -CORNER, CORNER) * particleRadius * (1.0f + _elementRatio))
            };
            return e;
        }
    }
}
