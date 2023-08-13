using ParticlePhysics.Utils;
using UnityEngine;

namespace ParticlePhysics
{
    public class ObjectBuffer
    {
        private GameObject _object;
        private Mesh _mesh;

        private ParticleBuffer _particle;
        private GraphicsBuffer _objectGridIndicesBuffer;
        
        private Vector3 _gridCenter;
        private Vector3 _gridSize;
        private float _gridCellSize;
        private GridSearch<ParticleState> _gridSearch;

        #region Accessors
        public GameObject Object => _object;
        public ParticleBuffer ObjectParticle => _particle;
        public GraphicsBuffer ObjectGridIndicesBuffer => _objectGridIndicesBuffer;
        public Vector3 GridCenter => _gridCenter;
        public Vector3 GridSize => _gridSize;
        public float GridCellSize => _gridCellSize;
        #endregion

        public ObjectBuffer(GameObject gameObject, int particleNum)
        {
            // memo: このgridCellSizeは砂のサイズで自動で決められるべき
            this._object = gameObject;
            this._mesh = gameObject.GetComponent<MeshFilter>().mesh;
            Matrix4x4 objTRS = Matrix4x4.TRS(_mesh.bounds.min, Quaternion.identity, Vector3.one);

            this._particle = ParticleBuffer.SetAsSimpleParticle(
                particles: ParticleState.GenerateFromGameObject(_object, 128),
                radius: 0.1f);

            this._gridSize = gameObject.GetComponent<MeshFilter>().mesh.bounds.extents * 2.2f;
            this._gridCenter = _gridSize * 0.5f;
            //this._gridCellSize = 0.5f;
            this._gridCellSize = 1.0f;

            this._gridSearch = new GridSearch<ParticleState>(_particle.num, _gridSize, _gridCellSize);
            this._gridSearch.GridSort(ref this._particle.status, objTRS);
            this._objectGridIndicesBuffer = this._gridSearch.TargetGridIndicesBuffer;

            this._gridSearch = new GridSearch<ParticleState>(particleNum, _gridSize, _gridCellSize);
        }

        public void Release()
        {
            _particle.Release();
            _gridSearch.Release();
            _objectGridIndicesBuffer.Release();
        }
    }
}
