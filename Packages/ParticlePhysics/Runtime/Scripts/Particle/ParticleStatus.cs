using UnityEngine;
using UnityEngine.VFX;

namespace ParticlePhysics.Particle
{
    [VFXType(VFXTypeAttribute.Usage.GraphicsBuffer)]
    public struct ParticleStatus
    {
        public Vector3 position;
        public Vector3 velocity;
        public Vector4 orientation;
        public Vector3 angularVelocity;

        public static ParticleStatus[] GeneratePoint(int particleNum, Vector3 centerPos)
        {
            var particles = new ParticleStatus[particleNum];
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

        public static ParticleStatus[] GenerateSphere(int particleNum, Vector3 centerPos, float radius)
        {
            var particles = new ParticleStatus[particleNum];
            var identityOrientation = Quaternion.identity;
            for (int i = 0; i < particleNum; i++)
            {
                particles[i].position = centerPos + Random.insideUnitSphere * radius;
                particles[i].velocity = Vector3.zero;
                particles[i].orientation = new Vector4(identityOrientation.x, identityOrientation.y, identityOrientation.z, identityOrientation.w);
                particles[i].angularVelocity = Vector3.zero;
            }
            return particles;
        }
    
        public static ParticleStatus[] GenerateFromMesh(int particleNum, Mesh mesh)
        {
            var particles = new ParticleStatus[particleNum];
            return particles;
        }
    };
}
