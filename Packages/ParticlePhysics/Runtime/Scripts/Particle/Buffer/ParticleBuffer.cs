using System.Runtime.InteropServices;
using UnityEngine;

namespace ParticlePhysics
{
    public class ParticleBuffer
    {
        public readonly int num;
        public GraphicsBuffer status;
        public Substance.ParticleSubstance substance;

        public ParticleBuffer(int particleNum)
        {
            this.num = particleNum;
        }

        public void Release()
        {
            status.Release();
            substance.Release();
        }

        public static ParticleBuffer SetAsSimpleParticle(ParticleState[] particles, float radius = 0.04f, float density = 2000.0f, float mu = 0.05f)
        {
            ParticleBuffer p = new ParticleBuffer(particles.Length);
            p.status = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                particles.Length,
                Marshal.SizeOf(typeof(ParticleState)));
            p.status.SetData(particles);
            p.substance = new Substance.SimpleSubstance(radius, density, mu);
            return p;
        }

        public static ParticleBuffer SetAsTetrahedronParticle(ParticleState[] particles, float radius = 0.04f, float density = 2000.0f, float mu = 0.05f)
        {
            ParticleBuffer p = new ParticleBuffer(particles.Length);
            p.status = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                particles.Length,
                Marshal.SizeOf(typeof(ParticleState)));
            p.status.SetData(particles);
            p.substance = new Substance.TetrahedronSubstance(radius, density, mu);
            return p;
        }

        public static ParticleBuffer SetAsCubeParticle(ParticleState[] particles, float radius = 0.04f, float density = 2000.0f, float mu = 0.05f)
        {
            ParticleBuffer p = new ParticleBuffer(particles.Length);
            p.status = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                particles.Length,
                Marshal.SizeOf(typeof(ParticleState)));
            p.status.SetData(particles);
            p.substance = new Substance.CubeSubstance(radius, density, mu);
            return p;
        }
    }
}
