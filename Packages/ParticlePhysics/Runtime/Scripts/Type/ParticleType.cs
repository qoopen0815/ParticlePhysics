using UnityEngine;
using UnityEngine.VFX;

namespace ParticlePhysics.Type
{
    public struct ParticleElement
    {
        public float radius;
        public float mass;
        public Vector3 offsetFromParticleCenter;

        public ParticleElement(float radius, float mass, Vector3 offsetFromParticleCenter)
        {
            this.radius = radius;
            this.mass = mass;
            this.offsetFromParticleCenter = offsetFromParticleCenter;
        }
    }

    [VFXType(VFXTypeAttribute.Usage.GraphicsBuffer)]
    public struct ParticleState
    {
        public Vector3 position;
        public Vector3 velocity;
        public Vector4 orientation;
        public Vector3 angularVelocity;

        public static ParticleState[] GeneratePoint(int particleNum, Vector3 centerPos)
        {
            var particles = new ParticleState[particleNum];
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

        public static ParticleState[] GenerateSphere(int particleNum, Vector3 centerPos, float radius)
        {
            var particles = new ParticleState[particleNum];
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
    
        public static ParticleState[] GenerateFromMesh(Mesh mesh)
        {
            var verts = ParticleCollider.GetVertsOnMeshSurface(mesh, 128);
            var particles = new ParticleState[verts.Count];
            var identityOrientation = Quaternion.identity;
            for (int i = 0; i < verts.Count; i++)
            {
                particles[i].position = verts[i];
                particles[i].velocity = Vector3.zero;
                particles[i].orientation = new Vector4(identityOrientation.x, identityOrientation.y, identityOrientation.z, identityOrientation.w);
                particles[i].angularVelocity = Vector3.zero;
            }
            return particles;
        }

        public static ParticleState[] GenerateFromGameObject(GameObject obj)
        {
            var verts = ParticleCollider.GetVertsOnMeshSurface(obj.GetComponent<MeshFilter>().mesh, 128);
            var particles = new ParticleState[verts.Count];
            var identityOrientation = Quaternion.identity;
            var m = Matrix4x4.identity;
            m.SetTRS(obj.transform.position, obj.transform.rotation, obj.transform.localScale);
            for (int i = 0; i < verts.Count; i++)
            {
                particles[i].position = m.MultiplyPoint3x4(verts[i]);
                particles[i].velocity = Vector3.zero;
                particles[i].orientation = new Vector4(identityOrientation.x, identityOrientation.y, identityOrientation.z, identityOrientation.w);
                particles[i].angularVelocity = Vector3.zero;
            }
            return particles;
        }
    };
}
