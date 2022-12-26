using System.Runtime.InteropServices;
using UnityEngine;

namespace ParticleSimulator
{
    public enum ParticleNumEnum
    {
        NUM_1K = 1024,
        NUM_2K = 1024 * 2,
        NUM_4K = 1024 * 4,
        NUM_8K = 1024 * 8,
        NUM_16K = 1024 * 16,
        NUM_32K = 1024 * 32,
        NUM_64K = 1024 * 64
    };

    public struct ParticleCollisionForce
    {
        public Vector3 acceleration;
        public Vector3 angularAcceleration;
    }
    public struct GroundCollisionForce
    {
        public Vector3 acceleration;
        public Vector3 angularAcceleration;
    }
    public struct ObjectCollisionForce
    {
        public Vector3 acceleration;
        public Vector3 angularAcceleration;
    }

    public class SandSimulator : MonoBehaviour
    {
        // Particle Data
        [SerializeField] private ParticleNumEnum _particleNum = ParticleNumEnum.NUM_8K;      // ���q��
        [SerializeField] private float _particleRadius = 0.1f;                               // ���q���a
        [SerializeField] private float _particleDensity = 2000.0f;                           // ���q���x
        [SerializeField] private Material _renderParticleMat;

        // Simulation
        [SerializeField] private float _maxAllowableTimestep = 0.0005f;     // �ő厞�ԍ��ݕ�
        [SerializeField] private Vector3 _gravity = Physics.gravity;        // �d��
        private float _timeStep;

        // ComputeShader
        private ComputeShader _solver;
        private ParticleBuffer _particleBuffer;
        private GraphicsBuffer _particleCollisionForce;
        private GraphicsBuffer _groundCollisionForce;
        private GraphicsBuffer _objectCollisionForce;

        // GPU
        private int _threadSize = 32;     // �R���s���[�g�V�F�[�_���̃X���b�h��

        #region Accessor
        #endregion

        #region Mono
        private void Awake()
        {
            _solver = (ComputeShader)Resources.Load("MolecularDynamics");
        }

        private void Start()
        {
            SetupShader();
            InitBuffer();
        }

        private void FixedUpdate()
        {
            UpdateParticle(ref _particleBuffer.datas);
        }

        private void OnRenderObject()
        {
            _renderParticleMat.SetPass(0);
            _renderParticleMat.SetBuffer("_ParticleBuffer", _particleBuffer.datas);
            Graphics.DrawProceduralNow(MeshTopology.Points, (int)_particleNum);
        }

        private void OnDestroy()
        {
            _particleBuffer.Release();
            _particleCollisionForce.Release();
            _groundCollisionForce.Release();
            _objectCollisionForce.Release();
        }
        #endregion

        private void InitBuffer()
        {
            ParticleData[] p = ParticleData.GenerateSphere((int)_particleNum, new Vector3(0, 0, 0), 30);
            Substance.ParticleSubstance ps = new Substance.TetrahedronParticle();
            this._particleBuffer = new ParticleBuffer(p, ps.Elements);

            _particleCollisionForce = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                (int)_particleNum,
                Marshal.SizeOf(typeof(ParticleCollisionForce)));
            _groundCollisionForce = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                (int)_particleNum,
                Marshal.SizeOf(typeof(GroundCollisionForce)));
            _objectCollisionForce = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                (int)_particleNum,
                Marshal.SizeOf(typeof(ObjectCollisionForce)));
    }

        public void UpdateParticle(ref GraphicsBuffer particles)
        {

        }

        private void SetupShader()
        {
            _solver.SetVector("_Gravity", _gravity);
            //_solver.SetVector("_GridResolution", gridResolution);
            //_solver.SetFloat("_GridCellSize", gridSize.x / gridResolution.x);
            _solver.SetInt("_ParticleNum", (int)_particleNum);
            //_solver.SetInt("_GranularParticleNum", (int)_particleSubstance.GranularParticleNum);
            //_solver.SetFloat("_GranularTotalMass", _particleSubstance.TotalMass);
            _solver.SetMatrix(Shader.PropertyToID("_GranularInertialMoment"), Matrix4x4.identity);
        }

        private void CalculateParticleCollisionForce(ref GraphicsBuffer particles, GraphicsBuffer substances)
        {

        }

        private void CalculateGroundCollision(ref GraphicsBuffer particles, GraphicsBuffer groundData)
        {

        }

        private void CalculateObjectCollision(ref GraphicsBuffer particles, GraphicsBuffer objectData)
        {

        }
    }
}
