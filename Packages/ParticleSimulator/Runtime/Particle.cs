using UnityEngine;

namespace ParticleSimulator.Substance
{
    public struct Particle
    {
        public float radius;
        public float mass;
        public Vector3 offsetFromGranularCenter;

        public Particle(float radius, float mass, Vector3 offsetFromGranularCenter)
        {
            this.radius = radius;
            this.mass = mass;
            this.offsetFromGranularCenter = offsetFromGranularCenter;
        }
    }
}
