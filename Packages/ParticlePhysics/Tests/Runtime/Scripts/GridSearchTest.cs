using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.VFX;

using ParticlePhysics;
using ParticlePhysics.Enum;
using ParticlePhysics.Utils;

namespace PackageTest
{
    /// <summary>
    /// Particle system with Grid Optimization
    /// </summary>
    internal class GridSearchTest : MonoBehaviour
    {
        public ParticleNum particleNum = ParticleNum.NUM_8K;
        public Vector3 particleSpornPos;
        public int dispIdx;

        private ComputeShader _shader;
        private ParticleBuffer _particleBuffer;
        private GraphicsBuffer _tmpBufferWrite;
        private GridSearch<ParticleState> _gridSearch;

        public GameObject gridObj;
        public Vector3 range = new Vector3(128, 128, 128); // grid size
        public float gridDim = 16;  // cell size

        public VisualEffect effect;
        public VisualEffect effect2;
        private GraphicsBuffer _debugBufferWrite;

        void Start()
        {
            _particleBuffer = ParticleBuffer.SetAsTetrahedronParticle(
                particles: ParticleState.GenerateCube((int)particleNum, particleSpornPos, 5),
                radius: 0.1f);
            _gridSearch = new GridSearch<ParticleState>(_particleBuffer.num, range, gridDim);
            _shader = (ComputeShader)Resources.Load("GridSearchTest");

            _tmpBufferWrite = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _particleBuffer.num, Marshal.SizeOf(typeof(ParticleState)));
            _debugBufferWrite = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _particleBuffer.num, Marshal.SizeOf(typeof(ParticleState)));

            // Setup VFX Graph
            effect.SetGraphicsBuffer("ParticleBuffer", _particleBuffer.status);
            effect.SetUInt("ParticleNum", (uint)_particleBuffer.status.count);
            effect.SetFloat("ParticleSize", 0.1f);

            //effect2.SetGraphicsBuffer("ParticleBuffer", _debugBufferWrite);
            //effect2.SetUInt("ParticleNum", (uint)_particleBuffer.status.count);
            //effect2.SetFloat("ParticleSize", 0.1f);
        }

        void Update()
        {
            Matrix4x4 gridtf = Matrix4x4.TRS(
                pos: gridObj.transform.position,
                q: gridObj.transform.rotation,
                s: gridObj.transform.localScale);

            //int _kernelID = _shader.FindKernel("DebugTest");
            //_shader.SetBuffer(_kernelID, "_ParticleBufferRead", _particleBuffer.status);
            //_shader.SetBuffer(_kernelID, "_ParticleBufferWrite", _debugBufferWrite);
            //_shader.SetMatrix("_GridTF", gridtf.inverse);
            //_shader.GetKernelThreadGroupSizes(_kernelID, out var _x, out _, out _);
            //_shader.Dispatch(_kernelID, (int)(_particleBuffer.num / _x), 1, 1);

            _gridSearch.GridSort(ref _particleBuffer.status, gridObj.transform);

            // ---- Your Particle Process -------------------------------------------------------------------
            _shader.SetInt("_NumParticles", _particleBuffer.num);
            _shader.SetInt("_DispIdx", (int)(dispIdx * (int)particleNum * 0.001f));
            _shader.SetVector("_GridResolution", range);
            _shader.SetFloat("_GridCellSize", gridDim);

            int kernelID = _shader.FindKernel("Update");
            _shader.SetMatrix("_GridTF", gridtf.inverse);
            _shader.SetBuffer(kernelID, "_ParticleBufferRead", _particleBuffer.status);
            _shader.SetBuffer(kernelID, "_ParticleBufferWrite", _tmpBufferWrite);
            _shader.SetBuffer(kernelID, "_GridIndicesBufferRead", _gridSearch.TargetGridIndicesBuffer);   // Get and use a GridIndicesBuffer to find neighbor
            _shader.GetKernelThreadGroupSizes(kernelID, out var x, out _, out _);
            _shader.Dispatch(kernelID, (int)(_particleBuffer.num / x), 1, 1);
            // ---- Your Particle Process -------------------------------------------------------------------

            (_particleBuffer.status, _tmpBufferWrite) = (_tmpBufferWrite, _particleBuffer.status);
        }

        void OnDestroy()
        {
            _tmpBufferWrite.Release();
            _debugBufferWrite.Release();
            _particleBuffer.Release();
            _gridSearch.Release();
        }
        private void OnDrawGizmos()
        {
            var cache = Gizmos.matrix;
            Gizmos.color = Color.cyan;
            Gizmos.matrix = Matrix4x4.TRS(gridObj.transform.position, gridObj.transform.rotation, gridObj.transform.lossyScale);
            Gizmos.DrawWireCube(range / 2, range);
            Gizmos.matrix = cache;
        }
    }


}