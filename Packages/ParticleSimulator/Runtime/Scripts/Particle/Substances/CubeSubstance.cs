using Unity.Mathematics;

namespace ParticleSimulator.Substance
{
    public class CubeSubstance : ParticleSubstance
    {
        private static uint _elementNum = 9;
        private static float _elementRatio = 0.3f;

        public CubeSubstance(float radius = 0.04f, float density = 2000.0f, float mu = 0.05f) : base(_elementNum)
        {
            this.mu = mu;
            this.elements = SetElements(radius, density);
            this.totalMass = CalculateTotalMass(elements, density);
            this.centerOfMass = CalculateCenterOfMass(elements);
            this.inertialMoment = CalculateInverseInertialMoment(elements);
        }

        protected override ElementType[] SetElements(float particleRadius, float particleDensity)
        {
            float CORNER = 1.0f / math.sqrt(3.0f);
            ElementType[] e = new ElementType[] {
                new ElementType(particleRadius, CalculateElementMass(particleRadius, particleDensity), new float3(0.0f, 0.0f, 0.0f) * particleRadius),
                new ElementType(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), new float3(CORNER, CORNER, CORNER) * particleRadius * (1.0f + _elementRatio)),
                new ElementType(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), new float3(CORNER, CORNER, -CORNER) * particleRadius * (1.0f + _elementRatio)),
                new ElementType(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), new float3(-CORNER, CORNER, -CORNER) * particleRadius * (1.0f + _elementRatio)),
                new ElementType(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), new float3(-CORNER, CORNER, CORNER) * particleRadius * (1.0f + _elementRatio)),
                new ElementType(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), new float3(CORNER, -CORNER, CORNER) * particleRadius * (1.0f + _elementRatio)),
                new ElementType(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), new float3(CORNER, -CORNER, -CORNER) * particleRadius * (1.0f + _elementRatio)),
                new ElementType(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), new float3(-CORNER, -CORNER, -CORNER) * particleRadius * (1.0f + _elementRatio)),
                new ElementType(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), new float3(-CORNER, -CORNER, CORNER) * particleRadius * (1.0f + _elementRatio))
            };
            return e;
        }
    }
}
