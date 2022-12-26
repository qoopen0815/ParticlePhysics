using UnityEngine;

namespace ParticleSimulator.Substance
{
    public struct ElementType
    {
        public float radius;
        public float mass;
        public float mu;
        public Vector3 offsetFromParticleCenter;

        public ElementType(float radius, float mass, float mu, Vector3 offsetFromParticleCenter)
        {
            this.radius = radius;
            this.mass = mass;
            this.mu = mu;
            this.offsetFromParticleCenter = offsetFromParticleCenter;
        }
    }
}
