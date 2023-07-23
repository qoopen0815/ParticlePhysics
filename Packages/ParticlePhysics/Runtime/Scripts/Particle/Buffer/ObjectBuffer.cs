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
        public GraphicsBuffer ObjectGridIndicesBuffer => this._gridSearch.TargetGridIndicesBuffer;
        public Vector3 GridCenter => _gridCenter;
        public Vector3 GridSize => _gridSize;
        public float GridCellSize => _gridCellSize;
        public GridSearch<ParticleState> GridSearch => _gridSearch;
        #endregion

        public ObjectBuffer(GameObject gameObject, int particleNum, float gridCellSize = 1.0f)
        {
            // memo: このgridCellSizeは砂のサイズで自動で決められるべき
            this._object = gameObject;
            this._mesh = gameObject.GetComponent<MeshFilter>().mesh;

            this._gridSize = gameObject.GetComponent<MeshFilter>().mesh.bounds.extents * 2.2f;
            this._gridCenter = _gridSize * 0.5f;
            this._gridCellSize = gridCellSize;

            Debug.Log("Size: " + _mesh.bounds.size);
            Debug.Log("Center: " + _mesh.bounds.center);
            Debug.Log("max: " + _mesh.bounds.max);
            Debug.Log("min: " + _mesh.bounds.min);
            Debug.Log("extents: " + _mesh.bounds.extents);

            this._particle = ParticleBuffer.SetAsSimpleParticle(
                particles: ParticleState.GenerateFromGameObject(_object, 32),
                radius: 0.1f);

            this._gridSearch = new GridSearch<ParticleState>(_particle.num, _gridSize, _gridCellSize);
            Matrix4x4 objTRS = Matrix4x4.TRS(_mesh.bounds.min, Quaternion.identity, Vector3.one);
            this._gridSearch.GridSort(ref this._particle.status, objTRS);
            this._objectGridIndicesBuffer = this._gridSearch.TargetGridIndicesBuffer;
            //Graphics.CopyBuffer(this._gridSearch.TargetGridIndicesBuffer, _objectGridIndicesBuffer);
            //_gridSearch.Release();

            //this._gridSearch = new GridSearch<ParticleState>(particleNum, gridSize, gridCellSize);
        }

        public GraphicsBuffer GetCollisionIndices()
        {
            //this._gridSearch.GridSort(ref particle.status);
            Matrix4x4 objTRS = Matrix4x4.TRS(_mesh.bounds.min, Quaternion.identity, Vector3.one);
            this._gridSearch.GridSort(ref this._particle.status, objTRS);
            return this._gridSearch.TargetGridIndicesBuffer;
        }

        public void Release()
        {
            _particle.Release();
            _gridSearch.Release();
            _objectGridIndicesBuffer.Release();
        }
    }
}
