using System.Runtime.InteropServices;
using UnityEngine;
using ParticleSimulator.Substance;

namespace ParticleSimulator
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
            substance.Elements.Release();
        }

        public static Particle SetAsSimpleParticle(ParticleType[] particles)
        {
            Particle p = new Particle(particles.Length);
            p.status = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                particles.Length,
                Marshal.SizeOf(typeof(ParticleType)));
            p.status.SetData(particles);
            p.substance = new SimpleSubstance();
            return p;
        }

        public static Particle SetAsTetrahedronParticle(ParticleType[] particles)
        {
            Particle p = new Particle(particles.Length);
            p.status = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                particles.Length,
                Marshal.SizeOf(typeof(ParticleType)));
            p.status.SetData(particles);
            p.substance = new TetrahedronSubstance();
            return p;
        }

        public static Particle SetAsCubeParticle(ParticleType[] particles)
        {
            Particle p = new Particle(particles.Length);
            p.status = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                particles.Length,
                Marshal.SizeOf(typeof(ParticleType)));
            p.status.SetData(particles);
            p.substance = new CubeSubstance();
            return p;
        }
    }
}
