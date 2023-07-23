using UnityEngine;

namespace ParticlePhysics
{
    /// <summary>
    /// Represents a particle element with properties such as radius, mass, and offset from the particle center.
    /// </summary>
    public struct ParticleElement
    {
        /// <summary>
        /// The radius of the particle element.
        /// </summary>
        public float radius;

        /// <summary>
        /// The mass of the particle element.
        /// </summary>
        public float mass;

        /// <summary>
        /// The offset of the particle element from the particle center in local coordinates.
        /// </summary>
        public Vector3 offsetFromParticleCenter;

        /// <summary>
        /// Initializes a new instance of the ParticleElement struct with the specified parameters.
        /// </summary>
        /// <param name="radius">The radius of the particle element.</param>
        /// <param name="mass">The mass of the particle element.</param>
        /// <param name="offsetFromParticleCenter">The offset of the particle element from the particle center in local coordinates.</param>
        public ParticleElement(float radius, float mass, Vector3 offsetFromParticleCenter)
        {
            this.radius = radius;
            this.mass = mass;
            this.offsetFromParticleCenter = offsetFromParticleCenter;
        }
    }
}
