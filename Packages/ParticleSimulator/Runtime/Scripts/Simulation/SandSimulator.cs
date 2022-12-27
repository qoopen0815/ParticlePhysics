using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.VFX;

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

    public class SandSimulator : MonoBehaviour
    {
        // Particle Data
        [SerializeField] private ParticleNumEnum _particleNum = ParticleNumEnum.NUM_8K;      // 粒子数
        [SerializeField] private float _particleRadius = 0.1f;                               // 粒子半径
        [SerializeField] private float _particleDensity = 2000.0f;                           // 粒子密度
        [SerializeField] private Material _particleRenderMat;
        private Substance.ParticleSubstance _particleSubstance = new Substance.TetrahedronParticle();

        // Terrain Data
        [SerializeField] private Terrain _terrain;

        // Object Data
        [SerializeField] private Mesh _mesh;

        // Simulation
        [SerializeField] private float _maxAllowableTimestep = 0.0005f;     // 最大時間刻み幅
        [SerializeField] private Vector3 _gravity = Physics.gravity;        // 重力

        // ComputeShader
        private ComputeShader _solver;
        private ParticleBuffer _particleBuffer;
        private ParticleBuffer _objectParticleBuffer;
        private GraphicsBuffer _terrainBuffer;
        private GraphicsBuffer _particleCollisionForce;
        private GraphicsBuffer _objectCollisionForce;
        private GraphicsBuffer _terrainCollisionForce;
        private GraphicsBuffer _tempBufferWrite;

        // GPU
        private int THREAD_SIZE_X = 32;     // コンピュートシェーダ側のスレッド数

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
        void OnEnable()
        {
            InitBuffer();
            GetComponent<VisualEffect>().SetGraphicsBuffer("ParticlesBuffer", _particleBuffer.datas);
            GetComponent<VisualEffect>().enabled = true;
        }

        private void OnDestroy()
        {
            _particleBuffer.Release();
            _objectParticleBuffer.Release();
            _terrainBuffer.Release();
            _particleCollisionForce.Release();
            _terrainCollisionForce.Release();
            _objectCollisionForce.Release();
            _tempBufferWrite.Release();
        }
        #endregion

        public void UpdateParticle(ref GraphicsBuffer particles)
        {
            CalculateParticleCollisionForce(ref _particleCollisionForce, _particleBuffer);
            CalculateTerrainCollision(ref _terrainCollisionForce, _particleBuffer, _terrainBuffer);
            CalculateObjectCollision(ref _objectCollisionForce, _particleBuffer, _objectParticleBuffer);
            Integrate(ref _particleBuffer, _particleCollisionForce, _objectCollisionForce, _terrainCollisionForce);
        }

        private void InitBuffer()
        {
            // Init Particle Buffer
            var p = ParticleType.GenerateSphere((int)_particleNum, new Vector3(50, 100, 50), 30);
            var ps = new Substance.TetrahedronParticle();
            _particleBuffer = new ParticleBuffer(p, ps.Elements);

            // Init Object Particle Buffer
            var o = ParticleType.GenerateFromMesh((int)_particleNum, _mesh);
            var os = new Substance.SimpleParticle();
            _objectParticleBuffer = new ParticleBuffer(o, os.Elements);

            // Init Terrain Bufer
            var t = TerrainType.GenerateFromTerrain(_terrain);
            _terrainBuffer = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                t.Length,
                Marshal.SizeOf(typeof(TerrainType)));
            _terrainBuffer.SetData(t);

            // Init Force Bufer
            _particleCollisionForce = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                (int)_particleNum,
                Marshal.SizeOf(typeof(ParticleCollisionForce)));
            _objectCollisionForce = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                (int)_particleNum,
                Marshal.SizeOf(typeof(ObjectCollisionForce)));
            _terrainCollisionForce = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                (int)_particleNum,
                Marshal.SizeOf(typeof(TerrainCollisionForce)));

            // Init temp buffer
            _tempBufferWrite = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                (int)_particleNum,
                Marshal.SizeOf(typeof(ParticleType)));
        }

        private void SetupShader()
        {
            _solver.SetVector("_Gravity", _gravity);
            //_solver.SetVector("_GridResolution", gridResolution);
            //_solver.SetFloat("_GridCellSize", gridSize.x / gridResolution.x);
            _solver.SetInt("_ParticleNum", (int)_particleNum);
            _solver.SetFloat("_ParticleTotalMass", _particleSubstance.TotalMass);
            //_solver.SetInt("_GranularParticleNum", (int)_particleSubstance.GranularParticleNum);
            _solver.SetMatrix(Shader.PropertyToID("_GranularInertialMoment"), Matrix4x4.identity);
        }

        private void CalculateParticleCollisionForce(ref GraphicsBuffer forceBuffer, ParticleBuffer particleBuffer)
        {
            //int kernelID = -1;
            //int threadGroupsX = particleBuffer.datas.count / THREAD_SIZE_X;

            //_solver.SetVector("_GridCenter", nearestNeighbor.GridCenter);
            //kernelID = _solver.FindKernel("ContactForceCS");
            //_solver.SetBuffer(kernelID, "_ParticleSubstancesBuffer", particleBuffer.substances);
            //_solver.SetBuffer(kernelID, "_GranularsBufferRead", particleBuffer.datas);
            //_solver.SetBuffer(kernelID, "_GranularsBufferWrite", _tempBufferWrite);
            //_solver.SetBuffer(kernelID, "_GridIndicesBufferRead", nearestNeighbor.GridIndicesBuffer);
            //_solver.Dispatch(kernelID, threadGroupsX, 1, 1);

            var f = new ParticleCollisionForce[particleBuffer.datas.count];
            System.Array.Fill(f, new ParticleCollisionForce(Vector3.zero, Vector3.zero));
            forceBuffer.SetData(f);
        }

        private void CalculateObjectCollision(ref GraphicsBuffer forceBuffer, ParticleBuffer particleBuffer, ParticleBuffer objectBuffer)
        {
            var f = new ObjectCollisionForce[particleBuffer.datas.count];
            System.Array.Fill(f, new ObjectCollisionForce(Vector3.zero, Vector3.zero));
            forceBuffer.SetData(f);
        }

        private void CalculateTerrainCollision(ref GraphicsBuffer forceBuffer, ParticleBuffer particleBuffer, GraphicsBuffer terrainBuffer)
        {
            var f = new TerrainCollisionForce[particleBuffer.datas.count];
            System.Array.Fill(f, new TerrainCollisionForce(Vector3.zero, Vector3.zero));
            forceBuffer.SetData(f);
        }

        private void Integrate(ref ParticleBuffer particleBuffer, GraphicsBuffer particleCollisionForce, GraphicsBuffer objectCollisionForce, GraphicsBuffer terrainCollisionForce)
        {
            int kernelID = _solver.FindKernel("IntegrateCS");
            _solver.SetFloat("_TimeStep", Mathf.Min(_maxAllowableTimestep, Time.deltaTime));
            _solver.SetBuffer(kernelID, "_ParticleCollisionForce", particleCollisionForce);
            _solver.SetBuffer(kernelID, "_ObjectCollisionForce", objectCollisionForce);
            _solver.SetBuffer(kernelID, "_TerrainCollisionForce", terrainCollisionForce);
            _solver.SetBuffer(kernelID, "_ParticleBufferRead", particleBuffer.datas);
            _solver.SetBuffer(kernelID, "_ParticleBufferWrite", _tempBufferWrite);
            //_solver.SetBuffer(kernelID, "_TerrainBuffer", terrainBuffer);
            _solver.GetKernelThreadGroupSizes(kernelID, out var x, out var y, out var z);
            _solver.Dispatch(kernelID, (int)(particleBuffer.datas.count / x), 1, 1);
            (particleBuffer.datas, _tempBufferWrite) = (_tempBufferWrite, particleBuffer.datas);
            //var f = new TerrainCollisionForce[particleBuffer.datas.count];
            //System.Array.Fill(f, new TerrainCollisionForce(Vector3.zero, Vector3.zero));
            //forceBuffer.SetData(f);
        }
    }
}
