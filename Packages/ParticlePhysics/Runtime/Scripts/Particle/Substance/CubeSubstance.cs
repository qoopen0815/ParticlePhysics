using UnityEngine;
using Unity.Mathematics;
using System.Runtime.InteropServices;

namespace ParticlePhysics.Substance
{
    /// <summary>
    /// Represents a cube-shaped particle substance.
    /// </summary>
    public class CubeSubstance : ParticleSubstance
    {
        // Member variable for the ratio of the element's size compared to the particle's radius.
        private static float _elementRatio = 0.3f;

        /// <summary>
        /// Initializes a new instance of the CubeSubstance class with the given parameters.
        /// </summary>
        /// <param name="radius">The radius of the cube substance particles.</param>
        /// <param name="density">The density of the cube substance particles.</param>
        /// <param name="mu">The mu value of the cube substance particles.</param>
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

        /// <summary>
        /// Sets the elements for the cube substance particles based on the given particle radius and density.
        /// </summary>
        /// <param name="particleRadius">The radius of the cube substance particles.</param>
        /// <param name="particleDensity">The density of the cube substance particles.</param>
        /// <returns>An array of ParticleElement representing the elements of the cube substance particles.</returns>
        protected override ParticleElement[] SetElements(float particleRadius, float particleDensity)
        {
            // Constants for corner position calculation.
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
