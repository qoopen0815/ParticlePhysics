using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.VFX;

namespace ParticleSimulator
{
    public class ParticlePhysics : MonoBehaviour
    {
        #region Inspector
        [Header("Particle Setting")]
        [SerializeField] private ParticleNumEnum _particleNum;
        [SerializeField] private ParticleTypeEnum _particleType;
        // Particle Type -> 粒子形状を選択できるようにする
        [SerializeField, Range(0.02f, 0.1f)] private float _particleRadius;
        [SerializeField] private Vector3 _spornPos; // いずれは消す。またはデバッグ用コンポーネントを別途用意する。
        [SerializeField] private VisualEffect _effect;

        [Header("Physics Setting")]
        [SerializeField] private Vector3 _gravity = Physics.gravity;        // 重力
        [SerializeField] private float _maxTimestep = 0.0005f;     // 最大時間刻み幅

        [Header("Collision Objects")]
        [SerializeField] private Terrain _terrain;
        [SerializeField] private List<GameObject> _objects;
        
        [Header("Option Setting")] // ここもいずれは消したい。
        [SerializeField] private Vector3 _gridSize = new Vector3(64, 64, 64);
        [SerializeField] private Vector3 _gridResolution = new Vector3(100, 100, 100);
        #endregion

        #region Variables
        // Objects
        private Mesh _mesh;
        private NearestNeighbor.NearestNeighbor<ParticleType> _nearestNeighbor;

        // ComputeShader
        private ComputeShader _solver;
        private Particle _particleBuffer;
        private Particle _objectParticleBuffer;
        private GraphicsBuffer _terrainBuffer;
        private GraphicsBuffer _particleCollisionForce;
        private GraphicsBuffer _objectCollisionForce;
        private GraphicsBuffer _terrainCollisionForce;
        private GraphicsBuffer _tempBufferWrite;
        #endregion

        #region Mono
        private void Awake()
        {
            _solver = (ComputeShader)Resources.Load("MolecularDynamics");
        }

        private void Start()
        {
            InitBuffer();
            SetupShader();

            _nearestNeighbor = new NearestNeighbor.NearestNeighbor<ParticleType>((int)_particleNum, _gridSize, _gridResolution);
            _nearestNeighbor.UpdateGridVariables(_spornPos, _gridSize, _gridResolution);
            _effect.SetGraphicsBuffer("ParticleBuffer", _particleBuffer.status);
            _effect.SetUInt("ParticleNum", (uint)_particleBuffer.status.count);
        }

        private void Update()
        {
            UpdateParticle(ref _particleBuffer.status);
        }

        //private void OnRenderObject()
        //{
        //    _particleRenderMat.SetPass(0);
        //    _particleRenderMat.SetBuffer("_ParticleBuffer", _particleBuffer.status);
        //    Graphics.DrawProceduralNow(MeshTopology.Points, (int)_particleNum);
        //}

        private void OnDestroy()
        {
            _particleBuffer.Release();
            _objectParticleBuffer.Release();
            _terrainBuffer.Release();
            _particleCollisionForce.Release();
            _terrainCollisionForce.Release();
            _objectCollisionForce.Release();
            _tempBufferWrite.Release();
            _nearestNeighbor.Release();
        }

        private void OnDrawGizmos()
        {
            //_nearestNeighbor.ShowGridOnGizmo();
        }

        #endregion

        public void UpdateParticle(ref GraphicsBuffer particles)
        {
            //_nearestNeighbor.GridSort(ref _particleBuffer.status);
            //CalculateParticleCollisionForce(ref _particleCollisionForce, _particleBuffer);
            ////CalculateTerrainCollision(ref _terrainCollisionForce, _particleBuffer, _terrainBuffer);
            //CalculateObjectCollision(ref _objectCollisionForce, _particleBuffer, _objectParticleBuffer);
            Integrate(ref _particleBuffer, _particleCollisionForce, _objectCollisionForce, _terrainCollisionForce);
        }

        private void InitBuffer()
        {
            // Init Particle Buffer
            _particleBuffer = Particle.SetAsTetrahedronParticle(
                ParticleType.GenerateSphere((int)_particleNum, _spornPos, 1));

            // Init Object Particle Buffer
            _objectParticleBuffer = Particle.SetAsSimpleParticle(
                ParticleType.GenerateFromMesh((int)_particleNum, _mesh));

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
            _solver.SetFloat("_Friction", 0.995f);
            _solver.SetVector("_GridResolution", _gridResolution);
            _solver.SetFloat("_GridCellSize", _gridSize.x / _gridResolution.x);
            _solver.SetInt("_ElementNum", _particleBuffer.substance.Elements.Length);
            _solver.SetInt("_ParticleNum", (int)_particleNum);
            _solver.SetFloat("_ParticleMu", _particleBuffer.substance.Mu);
            _solver.SetFloat("_ParticleTotalMass", _particleBuffer.substance.TotalMass);
            _solver.SetInt("_Resolution", _terrain.terrainData.heightmapResolution);
            _solver.SetVector("_Ratio", new Vector3(_terrain.terrainData.heightmapResolution / _terrain.terrainData.size.x,
                                    1 / _terrain.terrainData.size.y,
                                    _terrain.terrainData.heightmapResolution / _terrain.terrainData.size.z));
            _solver.SetMatrix(Shader.PropertyToID("_GranularInertialMoment"), Matrix4x4.identity);
        }

        private void CalculateParticleCollisionForce(ref GraphicsBuffer forceBuffer, Particle particleBuffer)
        {
            int kernelID = _solver.FindKernel("ParticleCollisionCS");
            _solver.SetVector("_GridCenter", _nearestNeighbor.GridCenter);
            _solver.SetBuffer(kernelID, "_ElementBuffer", particleBuffer.elementSubstance);
            _solver.SetBuffer(kernelID, "_ParticleBufferRead", particleBuffer.status);
            _solver.SetBuffer(kernelID, "_GridIndicesBufferRead", _nearestNeighbor.GridIndicesBuffer);
            _solver.SetBuffer(kernelID, "_ParticleCollisionForce", forceBuffer);
            _solver.GetKernelThreadGroupSizes(kernelID, out var x, out var y, out var z);
            _solver.Dispatch(kernelID, (int)(particleBuffer.status.count / x), 1, 1);
        }

        private void CalculateObjectCollision(ref GraphicsBuffer forceBuffer, Particle particleBuffer, Particle objectBuffer)
        {
            var f = new ObjectCollisionForce[particleBuffer.status.count];
            System.Array.Fill(f, new ObjectCollisionForce(Vector3.zero, Vector3.zero));
            forceBuffer.SetData(f);
        }

        private void CalculateTerrainCollision(ref GraphicsBuffer forceBuffer, Particle particleBuffer, GraphicsBuffer terrainBuffer)
        {
            int kernelID = _solver.FindKernel("TerrainCollisionCS");
            _solver.SetBuffer(kernelID, "_TerrainBuffer", terrainBuffer);
            _solver.SetBuffer(kernelID, "_TerrainCollisionForce", forceBuffer);
            _solver.SetBuffer(kernelID, "_ParticleBufferRead", particleBuffer.status);
            _solver.GetKernelThreadGroupSizes(kernelID, out var x, out var y, out var z);
            _solver.Dispatch(kernelID, (int)(particleBuffer.status.count / x), 1, 1);
            (particleBuffer.status, _tempBufferWrite) = (_tempBufferWrite, particleBuffer.status);
            var f = new TerrainCollisionForce[particleBuffer.status.count];
            System.Array.Fill(f, new TerrainCollisionForce(Vector3.zero, Vector3.zero));
            forceBuffer.SetData(f);
        }

        private void Integrate(ref Particle particleBuffer, GraphicsBuffer particleCollisionForce, GraphicsBuffer objectCollisionForce, GraphicsBuffer terrainCollisionForce)
        {
            int kernelID = _solver.FindKernel("IntegrateCS");
            _solver.SetFloat("_TimeStep", Mathf.Min(_maxTimestep, Time.deltaTime));
            _solver.SetBuffer(kernelID, "_ParticleCollisionForce", particleCollisionForce);
            _solver.SetBuffer(kernelID, "_ObjectCollisionForce", objectCollisionForce);
            _solver.SetBuffer(kernelID, "_TerrainCollisionForce", terrainCollisionForce);
            _solver.SetBuffer(kernelID, "_TerrainBuffer", _terrainBuffer);
            _solver.SetBuffer(kernelID, "_ParticleBufferRead", particleBuffer.status);
            _solver.SetBuffer(kernelID, "_ParticleBufferWrite", _tempBufferWrite);
            _solver.GetKernelThreadGroupSizes(kernelID, out var x, out var y, out var z);
            _solver.Dispatch(kernelID, (int)(particleBuffer.status.count / x), 1, 1);
            (particleBuffer.status, _tempBufferWrite) = (_tempBufferWrite, particleBuffer.status);
        }
    }
}
