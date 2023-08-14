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
        private GraphicsBuffer _objectParticleGridIndicesBuffer;
        private GridSearch<ParticleState> _gridSearch;

        #region Accessors
        public GameObject RegisteredObject => _gameObject;
        public ParticleBuffer ObjectParticle => _particle;
        public int ObjectParticleNum => _particle.num;
        public GraphicsBuffer ObjectParticleGridIndicesBuffer => _objectParticleGridIndicesBuffer;
        public GraphicsBuffer CollisionGridIndicesBuffer => _gridSearch.TargetGridIndicesBuffer;
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
                gridSize: _gameObject.GetComponent<MeshFilter>().mesh.bounds.extents * 2.2f,
                gridCellSize: 0.5f);

            this._gridSearch.GridSort(ref this._particle.status, objTRS);

            var buffer = BufferUtils.GetData<Uint2>(this._gridSearch.TargetGridIndicesBuffer);
            this._objectParticleGridIndicesBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, buffer.Length, Marshal.SizeOf(typeof(Uint2)));
            this._objectParticleGridIndicesBuffer.SetData(buffer);

            _gridSearch.Release();

            this._gridSearch = new GridSearch<ParticleState>(
                objNum: particleNum,
                gridSize: _gameObject.GetComponent<MeshFilter>().mesh.bounds.extents * 2.2f,
                gridCellSize: 0.5f);
        }

        // GridSort
        public GraphicsBuffer GridSort(ref GraphicsBuffer particle)
        {
            Matrix4x4 objTRS = Matrix4x4.TRS(
                pos: _gameObject.transform.position,
                q: _gameObject.transform.rotation,
                s: _gameObject.transform.localScale);
            _gridSearch.GridSort(ref _particle.status, objTRS);
            return _gridSearch.TargetGridIndicesBuffer;
        }

        public void Release()
        {
            _particle.Release();
            _gridSearch.Release();
            _objectParticleGridIndicesBuffer.Release();
        }
    }
}
