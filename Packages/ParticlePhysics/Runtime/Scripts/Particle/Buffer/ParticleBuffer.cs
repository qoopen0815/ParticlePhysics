using System.Runtime.InteropServices;
using UnityEngine;

namespace ParticlePhysics
{
    /// <summary>
    /// Represents a buffer to hold particle data for particle-based physics simulations.
    /// </summary>
    public class ParticleBuffer
    {
        /// <summary>
        /// The number of particles in the buffer.
        /// </summary>
        public readonly int num;

        /// <summary>
        /// The graphics buffer to store particle state data.
        /// </summary>
        public GraphicsBuffer status;

        /// <summary>
        /// The substance associated with the particles.
        /// </summary>
        public Substance.ParticleSubstance substance;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParticleBuffer"/> class with the specified number of particles.
        /// </summary>
        public ParticleBuffer(int particleNum)
        {
            this.num = particleNum;
        }

        /// <summary>
        /// Releases the resources held by the particle buffer.
        /// </summary>
        public void Release()
        {
            status.Release();
            substance.Release();
        }

        /// <summary>
        /// Creates a new <see cref="ParticleBuffer"/> and sets it up as simple particles with the given parameters.
        /// </summary>
        /// <param name="particles">An array of particle states.</param>
        /// <param name="radius">The radius of the particles. (Optional)</param>
        /// <param name="density">The density of the particles. (Optional)</param>
        /// <param name="mu">The friction coefficient of the particles. (Optional)</param>
        /// <returns>The created particle buffer.</returns>
        public static ParticleBuffer SetAsSimpleParticle(ParticleState[] particles, float radius = 0.04f, float density = 2000.0f, float mu = 0.05f)
        {
            ParticleBuffer p = new ParticleBuffer(particles.Length);
            p.status = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                particles.Length,
                Marshal.SizeOf(typeof(ParticleState)));
            p.status.SetData(particles);
            p.substance = new Substance.SimpleSubstance(radius, density, mu);
            return p;
        }

        /// <summary>
        /// Creates a new <see cref="ParticleBuffer"/> and sets it up as tetrahedron particles with the given parameters.
        /// </summary>
        /// <param name="particles">An array of particle states.</param>
        /// <param name="radius">The radius of the particles. (Optional)</param>
        /// <param name="density">The density of the particles. (Optional)</param>
        /// <param name="mu">The friction coefficient of the particles. (Optional)</param>
        /// <returns>The created particle buffer.</returns>
        public static ParticleBuffer SetAsTetrahedronParticle(ParticleState[] particles, float radius = 0.04f, float density = 2000.0f, float mu = 0.05f)
        {
            ParticleBuffer p = new ParticleBuffer(particles.Length);
            p.status = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                particles.Length,
                Marshal.SizeOf(typeof(ParticleState)));
            p.status.SetData(particles);
            p.substance = new Substance.TetrahedronSubstance(radius, density, mu);
            return p;
        }

        /// <summary>
        /// Creates a new <see cref="ParticleBuffer"/> and sets it up as cube particles with the given parameters.
        /// </summary>
        /// <param name="particles">An array of particle states.</param>
        /// <param name="radius">The radius of the particles. (Optional)</param>
        /// <param name="density">The density of the particles. (Optional)</param>
        /// <param name="mu">The friction coefficient of the particles. (Optional)</param>
        /// <returns>The created particle buffer.</returns>
        public static ParticleBuffer SetAsCubeParticle(ParticleState[] particles, float radius = 0.04f, float density = 2000.0f, float mu = 0.05f)
        {
            ParticleBuffer p = new ParticleBuffer(particles.Length);
            p.status = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                particles.Length,
                Marshal.SizeOf(typeof(ParticleState)));
            p.status.SetData(particles);
            p.substance = new Substance.CubeSubstance(radius, density, mu);
            return p;
        }
    }
}
