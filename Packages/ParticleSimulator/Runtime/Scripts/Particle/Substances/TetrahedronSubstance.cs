using UnityEngine;
using Unity.Mathematics;
using System.Runtime.InteropServices;

namespace ParticleSimulator.Substance
{
    public class TetrahedronSubstance : ParticleSubstance
    {
        private static float _elementRatio = 0.5f;

        public TetrahedronSubstance(float radius = 0.04f, float density = 2000.0f, float mu = 0.05f)
        {
            this.mu = mu;

            var e = SetElements(radius, density);
            this.totalMass = CalculateTotalMass(e, density);
            this.centerOfMass = CalculateCenterOfMass(e);
            this.inertialMoment = CalculateInverseInertialMoment(e);

            elements = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                e.Length,
                Marshal.SizeOf(typeof(ElementType)));
            elements.SetData(e);
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
