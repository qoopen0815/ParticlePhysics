using UnityEngine;

namespace ParticleSimulator
{
    public struct ParticleCollisionForce
    {
        public Vector3 acceleration;
        public Vector3 angularAcceleration;

        public ParticleCollisionForce(Vector3 acceleration, Vector3 angularAcceleration)
        {
            this.acceleration = acceleration;
            this.angularAcceleration = angularAcceleration;
        }

        public static ParticleCollisionForce zero() => new ParticleCollisionForce(Vector3.zero, Vector3.zero);

    }

    public struct TerrainCollisionForce
    {
        public Vector3 acceleration;
        public Vector3 angularAcceleration;

        public TerrainCollisionForce(Vector3 acceleration, Vector3 angularAcceleration)
        {
            this.acceleration = acceleration;
            this.angularAcceleration = angularAcceleration;
        }

        public static TerrainCollisionForce zero() => new TerrainCollisionForce(Vector3.zero, Vector3.zero);
    }

    public struct ObjectCollisionForce
    {
        public Vector3 acceleration;
        public Vector3 angularAcceleration;

        public ObjectCollisionForce(Vector3 acceleration, Vector3 angularAcceleration)
        {
            this.acceleration = acceleration;
            this.angularAcceleration = angularAcceleration;
        }

        public static ObjectCollisionForce zero() => new ObjectCollisionForce(Vector3.zero, Vector3.zero);
    }
}
