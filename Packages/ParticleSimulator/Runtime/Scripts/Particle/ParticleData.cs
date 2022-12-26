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
    
        public static ParticleData[] GenerateFromMesh(int particleNum)
        {
            var particles = new ParticleData[particleNum];
            return particles;
        }

        public static ParticleData[] GenerateFromTerrain(int particleNum, Terrain terrain)
        {
            var particles = new ParticleData[particleNum];
            return particles;
        }
    };
}
