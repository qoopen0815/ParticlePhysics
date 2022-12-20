using Unity.Mathematics;

namespace ParticleSimulator.Substance
{
    public class CubeParticle : BasicParticle
    {
        private static new uint _elementNum = 9;
        private static new float _elementRatio = 0.3f;

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
            Element[] p = new Element[_elementNum];
            p[0] = new Element(particleRadius, CalculateElementMass(particleRadius, particleDensity), new float3(0.0f, 0.0f, 0.0f) * particleRadius);
            p[1] = new Element(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), new float3(CORNER, CORNER, CORNER) * particleRadius * (1.0f + _elementRatio));
            p[2] = new Element(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), new float3(CORNER, CORNER, -CORNER) * particleRadius * (1.0f + _elementRatio));
            p[3] = new Element(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), new float3(-CORNER, CORNER, -CORNER) * particleRadius * (1.0f + _elementRatio));
            p[4] = new Element(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), new float3(-CORNER, CORNER, CORNER) * particleRadius * (1.0f + _elementRatio));
            p[5] = new Element(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), new float3(CORNER, -CORNER, CORNER) * particleRadius * (1.0f + _elementRatio));
            p[6] = new Element(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), new float3(CORNER, -CORNER, -CORNER) * particleRadius * (1.0f + _elementRatio));
            p[7] = new Element(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), new float3(-CORNER, -CORNER, -CORNER) * particleRadius * (1.0f + _elementRatio));
            p[8] = new Element(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), new float3(-CORNER, -CORNER, CORNER) * particleRadius * (1.0f + _elementRatio));
            return p;
        }
    }
}
