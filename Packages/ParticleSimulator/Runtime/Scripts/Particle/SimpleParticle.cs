using Unity.Mathematics;

namespace ParticleSimulator.Substance
{
    public class SimpleParticle : ParticleSubstance
    {
        private static uint _elementNum = 1;
        private static float _elementRatio = 0.5f;

        public SimpleParticle(float radius = 0.04f, float density = 2000.0f) : base(_elementNum)
        {
            elements = SetElements(radius, density);
            totalMass = CalculateTotalMass(elements, density);
            centerOfMass = CalculateCenterOfMass(elements);
            inertialMoment = CalculateInverseInertialMoment(elements);
        }

        protected override ElementType[] SetElements(float particleRadius, float particleDensity)
        {
            ElementType[] e = new ElementType[] {
                new ElementType(particleRadius, CalculateElementMass(particleRadius, particleDensity), 0.05f, new float3(0.0f, 0.0f, 0.0f) * particleRadius),
            };
            return e;
        }
    }
}
