using UnityEngine;

namespace ParticleSimulator.NearestNeighbor
{
    public abstract class NearestNeighborBase
    {
        protected GraphicsBuffer gridBuffer;
        protected GraphicsBuffer gridPingPongBuffer;
        protected GraphicsBuffer gridIndicesBuffer;
        protected GraphicsBuffer sortedObjectsBufferOutput;

        protected SortTool.BitonicSort bitonicSort;

        protected ComputeShader NearestNeighborCS;

        protected int totalCellNum;
        protected float gridCellSize;
        protected Vector3 gridResolution;
        protected Vector3 gridCenter;

        public NearestNeighborBase()
        {
            bitonicSort = new SortTool.BitonicSort();
        }

        #region Accessor
        public GraphicsBuffer GridIndicesBuffer => gridIndicesBuffer;
        public Vector3 GridCenter { get => gridCenter; set => gridCenter = value; }
        public float GridCellSize => gridCellSize;
        public Vector3 GridResolution => gridResolution;
        #endregion

        public void Release()
        {
            gridBuffer.Release();
            gridIndicesBuffer.Release();
            gridPingPongBuffer.Release();
            sortedObjectsBufferOutput.Release();
        }

        public void GridSort(ref GraphicsBuffer objectsBufferInput)
        {
            NearestNeighborCS.SetInt("_ParticleNum", objectsBufferInput.count);
            SetCSVariables();

            #region GridOptimization
            // Build Grid
            int kernelID = NearestNeighborCS.FindKernel("BuildGridCS");
            NearestNeighborCS.SetBuffer(kernelID, "_ParticleBufferRead", objectsBufferInput);
            NearestNeighborCS.SetBuffer(kernelID, "_GridBufferWrite", gridBuffer);
            NearestNeighborCS.GetKernelThreadGroupSizes(kernelID, out var x, out var y, out var z);
            NearestNeighborCS.Dispatch(kernelID, (int)(objectsBufferInput.count / x), 1, 1);

            // Sort Grid
            bitonicSort.Sort(ref gridBuffer, ref gridPingPongBuffer);

            // Build Grid Indices
            kernelID = NearestNeighborCS.FindKernel("ClearGridIndicesCS");
            NearestNeighborCS.SetBuffer(kernelID, "_GridIndicesBufferWrite", gridIndicesBuffer);
            NearestNeighborCS.GetKernelThreadGroupSizes(kernelID, out x, out y, out z);
            NearestNeighborCS.Dispatch(kernelID, (int)(totalCellNum / x), 1, 1);

            kernelID = NearestNeighborCS.FindKernel("BuildGridIndicesCS");
            NearestNeighborCS.SetBuffer(kernelID, "_GridBufferRead", gridBuffer);
            NearestNeighborCS.SetBuffer(kernelID, "_GridIndicesBufferWrite", gridIndicesBuffer);
            NearestNeighborCS.GetKernelThreadGroupSizes(kernelID, out x, out y, out z);
            NearestNeighborCS.Dispatch(kernelID, (int)(objectsBufferInput.count / x), 1, 1);

            // Rearrange
            kernelID = NearestNeighborCS.FindKernel("RearrangeParticlesCS");
            NearestNeighborCS.SetBuffer(kernelID, "_GridBufferRead", gridBuffer);
            NearestNeighborCS.SetBuffer(kernelID, "_ParticleBufferRead", objectsBufferInput);
            NearestNeighborCS.SetBuffer(kernelID, "_ParticleBufferWrite", sortedObjectsBufferOutput);
            NearestNeighborCS.GetKernelThreadGroupSizes(kernelID, out x, out y, out z);
            NearestNeighborCS.Dispatch(kernelID, (int)(objectsBufferInput.count / x), 1, 1);
            #endregion GridOptimization

            (sortedObjectsBufferOutput, objectsBufferInput) = (objectsBufferInput, sortedObjectsBufferOutput);
        }

        #region GPUSort

        #endregion GPUSort 

        protected abstract void InitializeBuffer(int objectNum);

        protected abstract void SetCSVariables();
    }
}
