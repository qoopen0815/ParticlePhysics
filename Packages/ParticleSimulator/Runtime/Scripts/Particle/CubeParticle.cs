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

        protected override Element[] SetElements(float particleRadius, float particleDensity)
        {
            float CORNER = 1.0f / math.sqrt(3.0f);
            Element[] e = new Element[_elementNum];
            e[0] = new Element(particleRadius, CalculateElementMass(particleRadius, particleDensity), 0.05f, new float3(0.0f, 0.0f, 0.0f) * particleRadius);
            e[1] = new Element(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), 0.05f, new float3(CORNER, CORNER, CORNER) * particleRadius * (1.0f + _elementRatio));
            e[2] = new Element(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), 0.05f, new float3(CORNER, CORNER, -CORNER) * particleRadius * (1.0f + _elementRatio));
            e[3] = new Element(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), 0.05f, new float3(-CORNER, CORNER, -CORNER) * particleRadius * (1.0f + _elementRatio));
            e[4] = new Element(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), 0.05f, new float3(-CORNER, CORNER, CORNER) * particleRadius * (1.0f + _elementRatio));
            e[5] = new Element(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), 0.05f, new float3(CORNER, -CORNER, CORNER) * particleRadius * (1.0f + _elementRatio));
            e[6] = new Element(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), 0.05f, new float3(CORNER, -CORNER, -CORNER) * particleRadius * (1.0f + _elementRatio));
            e[7] = new Element(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), 0.05f, new float3(-CORNER, -CORNER, -CORNER) * particleRadius * (1.0f + _elementRatio));
            e[8] = new Element(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), 0.05f, new float3(-CORNER, -CORNER, CORNER) * particleRadius * (1.0f + _elementRatio));
            return e;
        }
    }
}
