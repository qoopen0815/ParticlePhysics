using Unity.Mathematics;

namespace ParticleSimulator.Substance
{
    public class SimpleSubstance : ParticleSubstance
    {
        private static uint _elementNum = 1;

        public SimpleSubstance(float radius = 0.04f, float density = 2000.0f, float mu = 0.05f) : base(_elementNum)
        {
            this.mu = mu;
            this.elements = SetElements(radius, density);
            this.totalMass = CalculateTotalMass(elements, density);
            this.centerOfMass = CalculateCenterOfMass(elements);
            this.inertialMoment = CalculateInverseInertialMoment(elements);
        }

        protected override ElementType[] SetElements(float particleRadius, float particleDensity)
        {
            ElementType[] e = new ElementType[] {
                new ElementType(particleRadius, CalculateElementMass(particleRadius, particleDensity), new float3(0.0f, 0.0f, 0.0f) * particleRadius),
            };
            return e;
        }
    }
}
