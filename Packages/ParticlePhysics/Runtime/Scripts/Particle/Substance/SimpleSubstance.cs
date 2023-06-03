using UnityEngine;
using Unity.Mathematics;
using System.Runtime.InteropServices;

namespace ParticlePhysics.Particle.Substance
{
    public class SimpleSubstance : ParticleSubstance
    {
        public SimpleSubstance(float radius = 0.04f, float density = 2000.0f, float mu = 0.05f)
        {
            this.mu = mu;

            var e = SetElements(radius, density);
            this.totalMass = CalculateTotalMass(e, density);
            this.centerOfMass = CalculateCenterOfMass(e);
            this.inertialMoment = CalculateInverseInertialMoment(e);

            elements = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                e.Length,
                Marshal.SizeOf(typeof(Element)));
            elements.SetData(e);
        }

        protected override Element[] SetElements(float particleRadius, float particleDensity)
        {
            Element[] e = new Element[] {
                new Element(particleRadius, CalculateElementMass(particleRadius, particleDensity), new float3(0.0f, 0.0f, 0.0f) * particleRadius),
            };
            return e;
        }
    }
}
