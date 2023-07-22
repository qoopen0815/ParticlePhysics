﻿using UnityEngine;

namespace ParticlePhysics.Solver
{
    public struct ParticleCollisionForce
    {
        public Vector3 force;
        public Vector3 torque;

        public ParticleCollisionForce(Vector3 force, Vector3 torque)
        {
            this.force = force;
            this.torque = torque;
        }

        public static ParticleCollisionForce Zero() => new ParticleCollisionForce(Vector3.zero, Vector3.zero);

    }

    public struct TerrainCollisionForce
    {
        public Vector3 force;
        public Vector3 torque;

        public TerrainCollisionForce(Vector3 force, Vector3 torque)
        {
            this.force = force;
            this.torque = torque;
        }

        public static TerrainCollisionForce Zero() => new TerrainCollisionForce(Vector3.zero, Vector3.zero);
    }

    public struct ObjectCollisionForce
    {
        public Vector3 force;
        public Vector3 torque;

        public ObjectCollisionForce(Vector3 force, Vector3 torque)
        {
            this.force = force;
            this.torque = torque;
        }

        public static ObjectCollisionForce Zero() => new ObjectCollisionForce(Vector3.zero, Vector3.zero);
    }
}
