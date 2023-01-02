using Unity.Mathematics;

namespace ParticleSimulator.Substance
{
    public class TetrahedronSubstance : ParticleSubstance
    {
        private static uint _elementNum = 4;
        private static float _elementRatio = 0.5f;

        public TetrahedronSubstance(float radius = 0.04f, float density = 2000.0f, float mu = 0.05f) : base(_elementNum)
        {
            this.mu = mu;
            elements = SetElements(radius, density);
            totalMass = CalculateTotalMass(elements, density);
            centerOfMass = CalculateCenterOfMass(elements);
            inertialMoment = CalculateInverseInertialMoment(elements);
        }

        protected override ElementType[] SetElements(float particleRadius, float particleDensity)
        {
            ElementType[] e = new ElementType[] {
                new ElementType(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), new float3(1.0f, 0.0f, -1.0f / math.sqrt(2.0f)) * particleRadius * _elementRatio),
                new ElementType(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), new float3(-1.0f, 0.0f, -1.0f / math.sqrt(2.0f)) * particleRadius * _elementRatio),
                new ElementType(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), new float3(0.0f, 1.0f, 1.0f / math.sqrt(2.0f)) * particleRadius * _elementRatio),
                new ElementType(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), new float3(0.0f, -1.0f, 1.0f / math.sqrt(2.0f)) * particleRadius * _elementRatio)
            };
            return e;
        }
    }
}
