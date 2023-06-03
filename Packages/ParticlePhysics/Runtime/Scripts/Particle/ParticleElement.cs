using UnityEngine;

namespace ParticlePhysics
{
    public struct ParticleElement
    {
        public float radius;
        public float mass;
        public Vector3 offsetFromParticleCenter;

        public ParticleElement(float radius, float mass, Vector3 offsetFromParticleCenter)
        {
            this.radius = radius;
            this.mass = mass;
            this.offsetFromParticleCenter = offsetFromParticleCenter;
        }
    }
}
