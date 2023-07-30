using UnityEngine;
using UnityEngine.VFX;

namespace ParticlePhysics
{
    /// <summary>
    /// Represents the state of a particle.
    /// </summary>
    [VFXType(VFXTypeAttribute.Usage.GraphicsBuffer)]
    public struct ParticleState
    {
        /// <summary>
        /// Indicates if the particle is active. 0: false, 1: true.
        /// </summary>
        public uint isActive;

        /// <summary>
        /// The position of the particle in 3D space.
        /// </summary>
        public Vector3 position;

        /// <summary>
        /// The velocity of the particle in 3D space.
        /// </summary>
        public Vector3 velocity;

        /// <summary>
        /// The orientation of the particle represented by a Quaternion in the form of a Vector4 (x, y, z, w).
        /// </summary>
        public Vector4 orientation;

        /// <summary>
        /// The angular velocity of the particle.
        /// </summary>
        public Vector3 angularVelocity;

        /// <summary>
        /// Generates an array of particles at a specific center position, with default values for velocity and orientation.
        /// </summary>
        /// <param name="particleNum">The number of particles to generate.</param>
        /// <param name="centerPos">The center position for generating particles.</param>
        /// <returns>An array of ParticleState representing the generated particles.</returns>
        public static ParticleState[] GeneratePoint(int particleNum, Vector3 centerPos)
        {
            var particles = new ParticleState[particleNum];
            var identityOrientation = Quaternion.identity;
            for (int i = 0; i < particleNum; i++)
            {
                particles[i].isActive = 1;
                particles[i].position = centerPos;
                particles[i].velocity = Vector3.zero;
                particles[i].orientation = new Vector4(identityOrientation.x, identityOrientation.y, identityOrientation.z, identityOrientation.w);
                particles[i].angularVelocity = Vector3.zero;
            }
            return particles;
        }

        /// <summary>
        /// Generates an array of particles distributed within a sphere with a specific center and radius.
        /// </summary>
        /// <param name="particleNum">The number of particles to generate.</param>
        /// <param name="centerPos">The center position for generating particles.</param>
        /// <param name="radius">The radius of the sphere.</param>
        /// <returns>An array of ParticleState representing the generated particles.</returns>
        public static ParticleState[] GenerateSphere(int particleNum, Vector3 centerPos, float radius)
        {
            var particles = new ParticleState[particleNum];
            var identityOrientation = Quaternion.identity;
            for (int i = 0; i < particleNum; i++)
            {
                particles[i].isActive = 1;
                particles[i].position = centerPos + Random.insideUnitSphere * radius;
                particles[i].velocity = Vector3.zero;
                particles[i].orientation = new Vector4(identityOrientation.x, identityOrientation.y, identityOrientation.z, identityOrientation.w);
                particles[i].angularVelocity = Vector3.zero;
            }
            return particles;
        }

        /// <summary>
        /// Generates an array of particles distributed within a cube with a specific center and size.
        /// </summary>
        /// <param name="particleNum">The number of particles to generate.</param>
        /// <param name="centerPos">The center position for generating particles.</param>
        /// <param name="size">The size of the cube.</param>
        /// <returns>An array of ParticleState representing the generated particles.</returns>
        public static ParticleState[] GenerateCube(int particleNum, Vector3 centerPos, float size)
        {
            var particles = new ParticleState[particleNum];
            var identityOrientation = Quaternion.identity;
            for (int i = 0; i < particleNum; i++)
            {
                particles[i].isActive = 1;
                particles[i].position = new Vector3(Random.Range(-size / 2, size / 2), Random.Range(-size / 2, size / 2), Random.Range(-size / 2, size / 2)) + centerPos;
                particles[i].velocity = Vector3.zero;
                particles[i].orientation = new Vector4(identityOrientation.x, identityOrientation.y, identityOrientation.z, identityOrientation.w);
                particles[i].angularVelocity = Vector3.zero;
            }
            return particles;
        }

        /// <summary>
        /// Generates an array of particles on the surface of a given mesh.
        /// </summary>
        /// <param name="mesh">The mesh to generate particles from.</param>
        /// <param name="resolution">The resolution of the generated particles. Default value is 128.</param>
        /// <returns>An array of ParticleState representing the generated particles.</returns>
        public static ParticleState[] GenerateFromMesh(Mesh mesh, int resolution = 128)
        {
            var verts = ParticleCollider.GetVertsOnMeshSurface(mesh, resolution);
            var particles = new ParticleState[verts.Count];
            var identityOrientation = Quaternion.identity;
            for (int i = 0; i < verts.Count; i++)
            {
                particles[i].isActive = 1;
                particles[i].position = verts[i];
                particles[i].velocity = Vector3.zero;
                particles[i].orientation = new Vector4(identityOrientation.x, identityOrientation.y, identityOrientation.z, identityOrientation.w);
                particles[i].angularVelocity = Vector3.zero;
            }
            return particles;
        }

        /// <summary>
        /// Generates an array of particles on the surface of a given mesh.
        /// </summary>
        /// <param name="mesh">The mesh to generate particles from.</param>
        /// <param name="trs">The transformation matrix (translation, rotation, scale).</param>
        /// <param name="resolution">The resolution of the generated particles. Default value is 128.</param>
        /// <returns>An array of ParticleState representing the generated particles.</returns>
        public static ParticleState[] GenerateFromMesh(Mesh mesh, Matrix4x4 trs, int resolution = 128)
        {
            var verts = ParticleCollider.GetVertsOnMeshSurface(mesh, resolution);
            var particles = new ParticleState[verts.Count];
            var identityOrientation = Quaternion.identity;
            for (int i = 0; i < verts.Count; i++)
            {
                particles[i].isActive = 1;
                particles[i].position = trs.MultiplyPoint3x4(verts[i]);
                particles[i].velocity = Vector3.zero;
                particles[i].orientation = new Vector4(identityOrientation.x, identityOrientation.y, identityOrientation.z, identityOrientation.w);
                particles[i].angularVelocity = Vector3.zero;
            }
            return particles;
        }

        /// <summary>
        /// Generates an array of particles on the surface of a mesh attached to the given GameObject.
        /// </summary>
        /// <param name="obj">The GameObject with a mesh to generate particles from.</param>
        /// <param name="resolution">The resolution of the generated particles. Default value is 128.</param>
        /// <returns>An array of ParticleState representing the generated particles.</returns>
        public static ParticleState[] GenerateFromGameObject(GameObject obj, int resolution = 128)
        {
            var verts = ParticleCollider.GetVertsOnMeshSurface(obj.GetComponent<MeshFilter>().mesh, resolution);
            var particles = new ParticleState[verts.Count];
            var identityOrientation = Quaternion.identity;
            var trs = Matrix4x4.identity;
            trs.SetTRS(obj.transform.position, obj.transform.rotation, obj.transform.localScale);
            for (int i = 0; i < verts.Count; i++)
            {
                particles[i].isActive = 1;
                particles[i].position = trs.MultiplyPoint3x4(verts[i]);
                particles[i].velocity = Vector3.zero;
                particles[i].orientation = new Vector4(identityOrientation.x, identityOrientation.y, identityOrientation.z, identityOrientation.w);
                particles[i].angularVelocity = Vector3.zero;
            }
            return particles;
        }
    };
}
