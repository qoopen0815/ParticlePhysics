using System.Runtime.InteropServices;
using UnityEngine;

using ParticlePhysics.Type;
using ParticlePhysics.ParticleSubstance;

namespace ParticlePhysics
{
    public class GranularParticle
    {
        public int num;
        public GraphicsBuffer state;
        public ParticleSubstanceBase substance;

        public void Release()
        {
            state.Release();
            substance.Release();
        }

        public static GranularParticle SetAsSimpleParticle(ParticleState[] particles, float radius = 0.04f, float density = 2000.0f, float mu = 0.05f)
        {
            GranularParticle p =new GranularParticle();
            p.num = particles.Length;
            p.state = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                particles.Length,
                Marshal.SizeOf(typeof(ParticleState)));
            p.state.SetData(particles);
            p.substance = new SimpleSubstance(radius, density, mu);
            PrintInfo(p);
            return p;
        }

        public static GranularParticle SetAsTetrahedronParticle(ParticleState[] particles, float radius = 0.04f, float density = 2000.0f, float mu = 0.05f)
        {
            GranularParticle p = new GranularParticle();
            p.num = particles.Length;
            p.state = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                particles.Length,
                Marshal.SizeOf(typeof(ParticleState)));
            p.state.SetData(particles);
            p.substance = new TetrahedronSubstance(radius, density, mu);
            PrintInfo(p);
            return p;
        }

        public static GranularParticle SetAsCubeParticle(ParticleState[] particles, float radius = 0.04f, float density = 2000.0f, float mu = 0.05f)
        {
            GranularParticle p = new GranularParticle();
            p.num = particles.Length;
            p.state = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                particles.Length,
                Marshal.SizeOf(typeof(ParticleState)));
            p.state.SetData(particles);
            p.substance = new CubeSubstance(radius, density, mu);
            PrintInfo(p);
            return p;
        }

        private static void PrintInfo(GranularParticle p)
        {
            Debug.Log("=== Initialized Particle Data === \n" +
                      "_ElementNum : \t" + p.substance.Elements.count + "\n" +
                      "_ParticleNum : \t" + p.state.count + "\n" +
                      "_ParticleMu : \t" + p.substance.Mu + "\n" +
                      "_ParticleTotalMass : \t" + p.substance.TotalMass + "\n" +
                      "_ParticleInertialMoment : \n" + p.substance.InertialMoment + "\n" +
                      "_ParticleInertialMoment(4x4) : \n" + p.substance.InertialMoment4x4);
        }
    }
}
