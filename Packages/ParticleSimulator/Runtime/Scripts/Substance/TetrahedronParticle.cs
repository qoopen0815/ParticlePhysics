using Unity.Mathematics;

namespace ParticleSimulator.Substance
{
    public class TetrahedronParticle : ParticleSubstance
    {
        private static uint _elementNum = 4;
        private static float _elementRatio = 0.5f;

        public TetrahedronParticle(float radius = 0.04f, float density = 2000.0f) : base(_elementNum)
        {
            elements = SetElements(radius, density);
            totalMass = CalculateTotalMass(elements, density);
            centerOfMass = CalculateCenterOfMass(elements);
            inertialMoment = CalculateInverseInertialMoment(elements);
        }

        protected override Element[] SetElements(float particleRadius, float particleDensity)
        {
            Element[] e = new Element[_elementNum];
            e[0] = new Element(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), 0.05f, new float3(1.0f, 0.0f, -1.0f / math.sqrt(2.0f)) * particleRadius * _elementRatio);
            e[1] = new Element(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), 0.05f, new float3(-1.0f, 0.0f, -1.0f / math.sqrt(2.0f)) * particleRadius * _elementRatio);
            e[2] = new Element(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), 0.05f, new float3(0.0f, 1.0f, 1.0f / math.sqrt(2.0f)) * particleRadius * _elementRatio);
            e[3] = new Element(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), 0.05f, new float3(0.0f, -1.0f, 1.0f / math.sqrt(2.0f)) * particleRadius * _elementRatio);
            return e;
        }
    }
}
