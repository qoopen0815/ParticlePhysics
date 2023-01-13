using UnityEngine;

namespace ParticlePhysics.Particle
{
    public struct Element
    {
        public float radius;
        public float mass;
        public Vector3 offsetFromParticleCenter;

        public Element(float radius, float mass, Vector3 offsetFromParticleCenter)
        {
            this.radius = radius;
            this.mass = mass;
            this.offsetFromParticleCenter = offsetFromParticleCenter;
        }
    }
}
