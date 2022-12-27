using UnityEngine;
using UnityEngine.VFX;

namespace ParticleSimulator
{
    [VFXType(VFXTypeAttribute.Usage.GraphicsBuffer)]
    public struct ParticleType
    {
        public Vector3 position;
        public Vector3 velocity;
        public Vector4 orientation;
        public Vector3 angularVelocity;

        public static ParticleType[] GeneratePoint(int particleNum, Vector3 centerPos)
        {
            var particles = new ParticleType[particleNum];
            var identityOrientation = Quaternion.identity;
            for (int i = 0; i < particleNum; i++)
            {
                particles[i].position = centerPos;
                particles[i].velocity = Vector3.zero;
                particles[i].orientation = new Vector4(identityOrientation.x, identityOrientation.y, identityOrientation.z, identityOrientation.w);
                particles[i].angularVelocity = Vector3.zero;
            }
            return particles;
        }

        public static ParticleType[] GenerateSphere(int particleNum, Vector3 centerPos, float radius)
        {
            var particles = new ParticleType[particleNum];
            var identityOrientation = Quaternion.identity;
            for (int i = 0; i < particleNum; i++)
            {
                particles[i].position = centerPos + Random.insideUnitSphere * radius;   // 球形に粒子を初期化する
                particles[i].velocity = Vector3.zero;
                particles[i].orientation = new Vector4(identityOrientation.x, identityOrientation.y, identityOrientation.z, identityOrientation.w);
                particles[i].angularVelocity = Vector3.zero;
            }
            return particles;
        }
    
        public static ParticleType[] GenerateFromMesh(int particleNum, Mesh mesh)
        {
            var particles = new ParticleType[particleNum];
            return particles;
        }
    };
}
