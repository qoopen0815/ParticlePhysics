using System.Runtime.InteropServices;
using UnityEngine;
using ParticlePhysics.Particle.Substance;

namespace ParticlePhysics.Particle
{
    public class Particle
    {
        public readonly int num;
        public GraphicsBuffer status;
        public ParticleSubstance substance;

        public Particle(int particleNum)
        {
            this.num = particleNum;
        }

        public void Release()
        {
            status.Release();
            substance.Release();
        }

        public static Particle SetAsSimpleParticle(ParticleStatus[] particles, float radius = 0.04f, float density = 2000.0f, float mu = 0.05f)
        {
            Particle p = new Particle(particles.Length);
            p.status = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                particles.Length,
                Marshal.SizeOf(typeof(ParticleStatus)));
            p.status.SetData(particles);
            p.substance = new SimpleSubstance(radius, density, mu);
            return p;
        }

        public static Particle SetAsTetrahedronParticle(ParticleStatus[] particles, float radius = 0.04f, float density = 2000.0f, float mu = 0.05f)
        {
            Particle p = new Particle(particles.Length);
            p.status = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                particles.Length,
                Marshal.SizeOf(typeof(ParticleStatus)));
            p.status.SetData(particles);
            p.substance = new TetrahedronSubstance(radius, density, mu);
            return p;
        }

        public static Particle SetAsCubeParticle(ParticleStatus[] particles, float radius = 0.04f, float density = 2000.0f, float mu = 0.05f)
        {
            Particle p = new Particle(particles.Length);
            p.status = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                particles.Length,
                Marshal.SizeOf(typeof(ParticleStatus)));
            p.status.SetData(particles);
            p.substance = new CubeSubstance(radius, density, mu);
            return p;
        }
    }
}
