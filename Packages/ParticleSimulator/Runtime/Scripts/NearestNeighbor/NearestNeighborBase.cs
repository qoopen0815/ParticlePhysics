using UnityEngine;

namespace ParticleSimulator.NearestNeighbor
{
    public abstract class NearestNeighborBase
    {
        protected ComputeBuffer gridBuffer;
        protected ComputeBuffer gridPingPongBuffer;
        protected ComputeBuffer gridIndicesBuffer;
        protected ComputeBuffer sortedObjectsBufferOutput;

        protected int particleNum;

        protected SortTool.BitonicSort bitonicSort;

        protected ComputeShader NearestNeighborCS;
        protected static readonly int THREAD_SIZE_X = 32;

        protected int threadGroupSize;
        protected int totalCellNum;
        protected float gridCellSize;
        protected Vector3 gridResolution;
        protected Vector3 gridCenter;

        public NearestNeighborBase(int particleNum)
        {
            this.particleNum = particleNum;
            this.threadGroupSize = particleNum / THREAD_SIZE_X;
            bitonicSort = new SortTool.BitonicSort(particleNum);
        }

        #region Accessor
        public ComputeBuffer GridIndicesBuffer => gridIndicesBuffer;
        public Vector3 GridCenter { get => gridCenter; set => gridCenter = value; }
        #endregion

        public void Release()
        {
            BufferUtils.ReleaseBuffer(gridBuffer);
            BufferUtils.ReleaseBuffer(gridIndicesBuffer);
            BufferUtils.ReleaseBuffer(gridPingPongBuffer);
            BufferUtils.ReleaseBuffer(sortedObjectsBufferOutput);
        }

        public void GridSort(ref ComputeBuffer objectsBufferInput)
        {
            NearestNeighborCS.SetInt("_ParticleNum", this.particleNum);
            SetCSVariables();

            int kernel = 0;

            #region GridOptimization
            // Build Grid
            kernel = NearestNeighborCS.FindKernel("BuildGridCS");
            NearestNeighborCS.SetBuffer(kernel, "_GranularsBufferRead", objectsBufferInput);
            NearestNeighborCS.SetBuffer(kernel, "_GridBufferWrite", gridBuffer);
            NearestNeighborCS.Dispatch(kernel, threadGroupSize, 1, 1);

            // Sort Grid
            bitonicSort.Sort(ref gridBuffer, ref gridPingPongBuffer);

            // Build Grid Indices
            kernel = NearestNeighborCS.FindKernel("ClearGridIndicesCS");
            NearestNeighborCS.SetBuffer(kernel, "_GridIndicesBufferWrite", gridIndicesBuffer);
            NearestNeighborCS.Dispatch(kernel, (int)(totalCellNum / THREAD_SIZE_X), 1, 1);

            kernel = NearestNeighborCS.FindKernel("BuildGridIndicesCS");
            NearestNeighborCS.SetBuffer(kernel, "_GridBufferRead", gridBuffer);
            NearestNeighborCS.SetBuffer(kernel, "_GridIndicesBufferWrite", gridIndicesBuffer);
            NearestNeighborCS.Dispatch(kernel, threadGroupSize, 1, 1);

            // Rearrange
            kernel = NearestNeighborCS.FindKernel("RearrangeParticlesCS");
            NearestNeighborCS.SetBuffer(kernel, "_GridBufferRead", gridBuffer);
            NearestNeighborCS.SetBuffer(kernel, "_GranularsBufferRead", objectsBufferInput);
            NearestNeighborCS.SetBuffer(kernel, "_GranularsBufferWrite", sortedObjectsBufferOutput);
            NearestNeighborCS.Dispatch(kernel, threadGroupSize, 1, 1);
            #endregion GridOptimization

            BufferUtils.SwapComputeBuffer(ref sortedObjectsBufferOutput, ref objectsBufferInput);
        }

        #region GPUSort

        #endregion GPUSort 

        protected abstract void InitializeBuffer();

        protected abstract void SetCSVariables();
    }
}
