﻿using UnityEngine;

namespace ParticlePhysics.Particle
{
    /// <summary>
    /// Represents the force and torque generated due to particle collision.
    /// </summary>
    public struct ParticleCollisionForce
    {
        /// <summary>
        /// The force generated due to particle collision.
        /// </summary>
        /// <param name="force">The force vector.</param>
        /// <param name="torque">The torque vector.</param>
        public ParticleCollisionForce(Vector3 force, Vector3 torque)
        {
            this.force = force;
            this.torque = torque;
        }

        /// <summary>
        /// The force vector.
        /// </summary>
        public Vector3 force;

        /// <summary>
        /// The torque vector.
        /// </summary>
        public Vector3 torque;

        /// <summary>
        /// Returns a new ParticleCollisionForce instance with zero force and torque.
        /// </summary>
        /// <returns>A new ParticleCollisionForce instance with zero force and torque.</returns>
        public static ParticleCollisionForce Zero() => new ParticleCollisionForce(Vector3.zero, Vector3.zero);
    }
}
