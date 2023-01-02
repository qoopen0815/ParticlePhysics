using UnityEngine;

namespace ParticleSimulator.Substance
{
    public struct ElementType
    {
        public float radius;
        public float mass;
        public Vector3 offsetFromParticleCenter;

        public ElementType(float radius, float mass, Vector3 offsetFromParticleCenter)
        {
            this.radius = radius;
            this.mass = mass;
            this.offsetFromParticleCenter = offsetFromParticleCenter;
        }
    }
}
