using UnityEngine;
using Unity.Mathematics;
using System.Runtime.InteropServices;

using ParticlePhysics.Type;

namespace ParticlePhysics.ParticleSubstance
{
    public class SimpleSubstance : ParticleSubstanceBase
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
                Marshal.SizeOf(typeof(ParticleElement)));
            elements.SetData(e);
        }

        protected override ParticleElement[] SetElements(float particleRadius, float particleDensity)
        {
            ParticleElement[] e = new ParticleElement[] {
                new ParticleElement(particleRadius, CalculateElementMass(particleRadius, particleDensity), new float3(0.0f, 0.0f, 0.0f) * particleRadius),
            };
            return e;
        }
    }
}
