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
        [SerializeField] private ParticleNumEnum _particleNum = ParticleNumEnum.NUM_8K;      // 粒子数
        [SerializeField] private float _particleRadius = 0.1f;                               // 粒子半径
        [SerializeField] private float _particleDensity = 2000.0f;                           // 粒子密度
        [SerializeField] private Material _particleRenderMat;

        // Terrain Data
        [SerializeField] private Terrain _terrain;

        // Object Data
        [SerializeField] private Mesh _mesh;

        // Simulation
        [SerializeField] private float _maxAllowableTimestep = 0.0005f;     // 最大時間刻み幅
        [SerializeField] private Vector3 _gravity = Physics.gravity;        // 重力
        private float _timeStep;

        // ComputeShader
        private ComputeShader _solver;
        private ParticleBuffer _particleBuffer;
        private ParticleBuffer _objectParticleBuffer;
        private GraphicsBuffer _terrainBuffer;
        private GraphicsBuffer _particleCollisionForce;
        private GraphicsBuffer _objectCollisionForce;
        private GraphicsBuffer _terrainCollisionForce;

        // GPU
        private int _threadSize = 32;     // コンピュートシェーダ側のスレッド数

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
            _particleRenderMat.SetPass(0);
            _particleRenderMat.SetBuffer("_ParticleBuffer", _particleBuffer.datas);
            Graphics.DrawProceduralNow(MeshTopology.Points, (int)_particleNum);
        }

        private void OnDestroy()
        {
            _particleBuffer.Release();
            _objectParticleBuffer.Release();
            _terrainBuffer.Release();
            _particleCollisionForce.Release();
            _terrainCollisionForce.Release();
            _objectCollisionForce.Release();
        }
        #endregion

        private void InitBuffer()
        {
            // Init Particle Buffer
            var p = ParticleType.GenerateSphere((int)_particleNum, new Vector3(0, 0, 0), 30);
            var ps = new Substance.TetrahedronParticle();
            _particleBuffer = new ParticleBuffer(p, ps.Elements);

            // Init Object Particle Buffer
            var o = ParticleType.GenerateFromMesh((int)_particleNum, _mesh);
            var os = new Substance.SimpleParticle();
            _objectParticleBuffer = new ParticleBuffer(o, os.Elements);

            // Init Terrain Bufer
            _terrainBuffer = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                (int)_particleNum,
                Marshal.SizeOf(typeof(TerrainType)));

            // Init Force Bufer
            _particleCollisionForce = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                (int)_particleNum,
                Marshal.SizeOf(typeof(ParticleCollisionForce)));
            _terrainCollisionForce = new GraphicsBuffer(
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
            CalculateParticleCollisionForce(ref _particleBuffer);
            CalculateTerrainCollision(ref _particleBuffer, _terrainBuffer);
            CalculateObjectCollision(ref _particleBuffer, _objectParticleBuffer);
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

        private void CalculateParticleCollisionForce(ref ParticleBuffer particleBuffer)
        {

        }

        private void CalculateTerrainCollision(ref ParticleBuffer particleBuffer, GraphicsBuffer terrainBuffer)
        {

        }

        private void CalculateObjectCollision(ref ParticleBuffer particleBuffer, ParticleBuffer objectBuffer)
        {

        }
    }
}
