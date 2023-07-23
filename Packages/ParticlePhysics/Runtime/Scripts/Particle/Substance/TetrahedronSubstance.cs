using UnityEngine;
using Unity.Mathematics;
using System.Runtime.InteropServices;

namespace ParticlePhysics.Substance
{
    /// <summary>
    /// Represents a tetrahedron substance used in particle physics simulations.
    /// </summary>
    public class TetrahedronSubstance : ParticleSubstance
    {
        /// <summary>
        /// The ratio used to calculate the size of the tetrahedron elements relative to the particle radius.
        /// </summary>
        private static float _elementRatio = 0.5f;

        /// <summary>
        /// Initializes a new instance of the TetrahedronSubstance class with the specified parameters.
        /// </summary>
        /// <param name="radius">The radius of the tetrahedron's particle elements.</param>
        /// <param name="density">The density of the tetrahedron substance.</param>
        /// <param name="mu">The friction coefficient of the tetrahedron substance.</param>
        public TetrahedronSubstance(float radius = 0.04f, float density = 2000.0f, float mu = 0.05f)
        {
            this.mu = mu;

            // Set the particle elements using the provided radius and density.
            var e = SetElements(radius, density);
            this.totalMass = CalculateTotalMass(e, density);
            this.centerOfMass = CalculateCenterOfMass(e);
            this.inertialMoment = CalculateInverseInertialMoment(e);

            // Create a graphics buffer and store the particle elements in it.
            elements = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                e.Length,
                Marshal.SizeOf(typeof(ParticleElement)));
            elements.SetData(e);
        }

        /// <summary>
        /// Sets the particle elements of the tetrahedron substance based on the given particle radius and density.
        /// </summary>
        /// <param name="particleRadius">The radius of the tetrahedron's particle elements.</param>
        /// <param name="particleDensity">The density of the tetrahedron substance.</param>
        /// <returns>An array of ParticleElement representing the tetrahedron's particle elements.</returns>
        protected override ParticleElement[] SetElements(float particleRadius, float particleDensity)
        {
            // Define the four particle elements of the tetrahedron using the given radius and density.
            ParticleElement[] e = new ParticleElement[] {
                new ParticleElement(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), new float3(1.0f, 0.0f, -1.0f / math.sqrt(2.0f)) * particleRadius * _elementRatio),
                new ParticleElement(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), new float3(-1.0f, 0.0f, -1.0f / math.sqrt(2.0f)) * particleRadius * _elementRatio),
                new ParticleElement(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), new float3(0.0f, 1.0f, 1.0f / math.sqrt(2.0f)) * particleRadius * _elementRatio),
                new ParticleElement(particleRadius * _elementRatio, CalculateElementMass(particleRadius * _elementRatio, particleDensity), new float3(0.0f, -1.0f, 1.0f / math.sqrt(2.0f)) * particleRadius * _elementRatio)
            };
            return e;
        }
    }
}
