using ParticlePhysics.Utils;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ParticlePhysics
{
    public class ObjectBuffer
    {
        private GameObject _gameObject;
        private Mesh _mesh;
        private ParticleBuffer _particle;
        private GraphicsBuffer _objectGridIndicesBuffer;
        private GridSearch<ParticleState> _gridSearch;

        #region Accessors
        public GameObject RegisteredObject => _gameObject;
        public ParticleBuffer ObjectParticle => _particle;
        public int ObjectParticleNum => _particle.num;
        public GraphicsBuffer GridIndicesBuffer => _objectGridIndicesBuffer;
        #endregion

        public ObjectBuffer(GameObject gameObject, int particleNum)
        {
            this._gameObject = gameObject;
            this._mesh = gameObject.GetComponent<MeshFilter>().mesh;
            Matrix4x4 objTRS = Matrix4x4.TRS(_mesh.bounds.min, Quaternion.identity, Vector3.one);

            this._particle = ParticleBuffer.SetAsSimpleParticle(
                particles: ParticleState.GenerateFromGameObject(_gameObject, 128),
                radius: 0.1f);

            this._gridSearch = new GridSearch<ParticleState>(
                objNum: _particle.num,
                gridSize: gameObject.GetComponent<MeshFilter>().mesh.bounds.extents * 2.2f,
                gridCellSize: 0.5f);

            this._gridSearch.GridSort(ref this._particle.status, objTRS);

            var buffer = BufferUtils.GetData<Uint2>(this._gridSearch.TargetGridIndicesBuffer);
            this._objectGridIndicesBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, buffer.Length, Marshal.SizeOf(typeof(Uint2)));
            this._objectGridIndicesBuffer.SetData(buffer);

            _gridSearch.Release();

            this._gridSearch = new GridSearch<ParticleState>(
                objNum: particleNum,
                gridSize: gameObject.GetComponent<MeshFilter>().mesh.bounds.extents * 2.2f,
                gridCellSize: 0.5f);
        }

        public void Release()
        {
            _particle.Release();
            _gridSearch.Release();
            _objectGridIndicesBuffer.Release();
        }
    }
}
