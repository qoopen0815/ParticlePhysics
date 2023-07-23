using UnityEngine;
using Unity.Mathematics;
using System.Runtime.InteropServices;

namespace ParticlePhysics.Substance
{
    /// <summary>
    /// Represents a simple particle substance.
    /// </summary>
    public class SimpleSubstance : ParticleSubstance
    {
        /// <summary>
        /// Constructor for SimpleSubstance class.
        /// </summary>
        /// <param name="radius">The radius of the particles.</param>
        /// <param name="density">The density of the substance.</param>
        /// <param name="mu">The coefficient of friction.</param>
        public SimpleSubstance(float radius = 0.04f, float density = 2000.0f, float mu = 0.05f)
        {
            this.mu = mu;

            // Set up particle elements
            var e = SetElements(radius, density);
            this.totalMass = CalculateTotalMass(e, density);
            this.centerOfMass = CalculateCenterOfMass(e);
            this.inertialMoment = CalculateInverseInertialMoment(e);

            // Create and initialize the graphics buffer for elements
            elements = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                e.Length,
                Marshal.SizeOf(typeof(ParticleElement)));
            elements.SetData(e);
        }

        /// <summary>
        /// Sets up the particle elements for the substance.
        /// </summary>
        /// <param name="particleRadius">The radius of the particles.</param>
        /// <param name="particleDensity">The density of the substance.</param>
        /// <returns>An array of ParticleElement representing the elements.</returns>
        protected override ParticleElement[] SetElements(float particleRadius, float particleDensity)
        {
            ParticleElement[] e = new ParticleElement[] {
                new ParticleElement(particleRadius, CalculateElementMass(particleRadius, particleDensity), new float3(0.0f, 0.0f, 0.0f) * particleRadius),
            };
            return e;
        }
    }
}
