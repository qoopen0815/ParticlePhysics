using System.Runtime.InteropServices;
using UnityEngine;

namespace ParticleSimulator
{
    public struct ParticleData
    {
        public Vector3 position;
        public Vector3 velocity;
        public Quaternion orientation;
        public Vector3 angularVelocity;

        public static ParticleData[] GeneratePoint(int particleNum, Vector3 centerPos)
        {
            var particles = new ParticleData[particleNum];
            for (int i = 0; i < particleNum; i++)
            {
                particles[i].position = centerPos;
                particles[i].velocity = Vector3.zero;
                particles[i].orientation = Quaternion.identity;
                particles[i].angularVelocity = Vector3.zero;
            }
            return particles;
        }

        public static ParticleData[] GenerateSphere(int particleNum, Vector3 centerPos, float radius)
        {
            var particles = new ParticleData[particleNum];
            for (int i = 0; i < particleNum; i++)
            {
                particles[i].position = centerPos + Random.insideUnitSphere * radius;   // 球形に粒子を初期化する
                particles[i].velocity = Vector3.zero;
                particles[i].orientation = Quaternion.identity;
                particles[i].angularVelocity = Vector3.zero;
            }
            return particles;
        }
    };

    public class ParticleBuffer
    {
        public GraphicsBuffer datas;
        public GraphicsBuffer substances;

        public ParticleBuffer(
            ParticleData[] particleDatas, 
            Substance.Element[] elementSubstances)
        {
            datas = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                particleDatas.Length,
                Marshal.SizeOf(typeof(ParticleData)));
            datas.SetData(particleDatas);

            substances = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                elementSubstances.Length,
                Marshal.SizeOf(typeof(Substance.Element)));
            substances.SetData(elementSubstances);
        }

        public void Release()
        {
            datas.Release();
            substances.Release();
        }
    }
}
