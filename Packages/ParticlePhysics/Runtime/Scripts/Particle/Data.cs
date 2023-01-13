using System.Runtime.InteropServices;
using UnityEngine;

namespace ParticlePhysics.Particle
{
    public class Data
    {
        public readonly int num;
        public GraphicsBuffer status;
        public Particle.Substance.ParticleSubstance substance;

        public Data(int particleNum)
        {
            this.num = particleNum;
        }

        public void Release()
        {
            status.Release();
            substance.Release();
        }

        public static Data SetAsSimpleParticle(State[] particles, float radius = 0.04f, float density = 2000.0f, float mu = 0.05f)
        {
            Data p = new Data(particles.Length);
            p.status = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                particles.Length,
                Marshal.SizeOf(typeof(State)));
            p.status.SetData(particles);
            p.substance = new Substance.SimpleSubstance(radius, density, mu);
            return p;
        }

        public static Data SetAsTetrahedronParticle(State[] particles, float radius = 0.04f, float density = 2000.0f, float mu = 0.05f)
        {
            Data p = new Data(particles.Length);
            p.status = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                particles.Length,
                Marshal.SizeOf(typeof(State)));
            p.status.SetData(particles);
            p.substance = new Substance.TetrahedronSubstance(radius, density, mu);
            return p;
        }

        public static Data SetAsCubeParticle(State[] particles, float radius = 0.04f, float density = 2000.0f, float mu = 0.05f)
        {
            Data p = new Data(particles.Length);
            p.status = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                particles.Length,
                Marshal.SizeOf(typeof(State)));
            p.status.SetData(particles);
            p.substance = new Substance.CubeSubstance(radius, density, mu);
            return p;
        }
    }
}
