using UnityEngine;
using Unity.Mathematics;
using System.Runtime.InteropServices;

using ParticlePhysics.Type;

namespace ParticlePhysics.ParticleSubstance
{
    public class CubeSubstance : ParticleSubstanceBase
    {
        private static float _elementRatio = 0.3f;

        public CubeSubstance(float radius = 0.04f, float density = 2000.0f, float mu = 0.05f)
        {
            this.mu = mu;

            var e = SetElements(radius, density);
            this.totalMass = CalculateTotalMass(e, density);
            this.centerOfMass = CalculateCenterOfMass(e);
            this.inertialMoment = CalculateInverseInertialMoment(e);

            elements = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                e.Length,
                Marshal.SizeOf(typeof(ParticleElement)));
            elements.SetData(e);
        }

        protected override ParticleElement[] SetElements(float particleRadius, float particleDensity)
        {
            float CORNER = 1.0f / math.sqrt(3.0f);
            ParticleElement[] e = new ParticleElement[] {
                new ParticleElement(particleRadius, CalculateElementMass(particleRadius, particleDensity), new float3(0.0f, 0.0f, 0.0f) * particleRadius),
                new ParticleElement(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), new float3(CORNER, CORNER, CORNER) * particleRadius * (1.0f + _elementRatio)),
                new ParticleElement(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), new float3(CORNER, CORNER, -CORNER) * particleRadius * (1.0f + _elementRatio)),
                new ParticleElement(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), new float3(-CORNER, CORNER, -CORNER) * particleRadius * (1.0f + _elementRatio)),
                new ParticleElement(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), new float3(-CORNER, CORNER, CORNER) * particleRadius * (1.0f + _elementRatio)),
                new ParticleElement(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), new float3(CORNER, -CORNER, CORNER) * particleRadius * (1.0f + _elementRatio)),
                new ParticleElement(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), new float3(CORNER, -CORNER, -CORNER) * particleRadius * (1.0f + _elementRatio)),
                new ParticleElement(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), new float3(-CORNER, -CORNER, -CORNER) * particleRadius * (1.0f + _elementRatio)),
                new ParticleElement(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), new float3(-CORNER, -CORNER, CORNER) * particleRadius * (1.0f + _elementRatio))
            };
            return e;
        }
    }
}
