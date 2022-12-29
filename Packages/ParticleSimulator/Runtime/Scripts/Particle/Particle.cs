using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.VFX;
using ParticleSimulator.Substance;

namespace ParticleSimulator
{
    public class Particle
    {
        // Buffer for status
        public GraphicsBuffer status;

        // Buffer for substance
        public ParticleSubstance substance;
        public GraphicsBuffer elementSubstance;

        public void Release()
        {
            status.Release();
            elementSubstance.Release();
        }

        public static Particle SetAsSimpleParticle(ParticleType[] particles)
        {
            Particle p = new Particle();
            p.status = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                particles.Length,
                Marshal.SizeOf(typeof(ParticleType)));
            p.status.SetData(particles);

            p.substance = new SimpleSubstance();
            p.elementSubstance = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                p.substance.Elements.Length,
                Marshal.SizeOf(typeof(ElementType)));
            p.elementSubstance.SetData(p.substance.Elements);

            return p;
        }

        public static Particle SetAsTetrahedronParticle(ParticleType[] particles)
        {
            Particle p = new Particle();
            p.status = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                particles.Length,
                Marshal.SizeOf(typeof(ParticleType)));
            p.status.SetData(particles);

            p.substance = new TetrahedronSubstance();
            p.elementSubstance = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                p.substance.Elements.Length,
                Marshal.SizeOf(typeof(ElementType)));
            p.elementSubstance.SetData(p.substance.Elements);

            return p;
        }

        public static Particle SetAsCubeParticle(ParticleType[] particles)
        {
            Particle p = new Particle();
            p.status = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                particles.Length,
                Marshal.SizeOf(typeof(ParticleType)));
            p.status.SetData(particles);

            p.substance = new CubeSubstance();
            p.elementSubstance = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                p.substance.Elements.Length,
                Marshal.SizeOf(typeof(ElementType)));
            p.elementSubstance.SetData(p.substance.Elements);

            return p;
        }
    }
}
