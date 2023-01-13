using UnityEngine;

namespace ParticleSimulator.NearestNeighbour
{
    internal abstract class GridSearchBase
    {
        protected ComputeShader GridSearchCS;
        protected GraphicsBuffer gridBuffer;
        protected GraphicsBuffer gridPingPongBuffer;
        protected GraphicsBuffer gridIndicesBuffer;
        protected GraphicsBuffer sortedObjectsBufferOutput;

        protected int numObjects;
        protected int threadGroupSize;
        protected static readonly int SIMULATION_BLOCK_SIZE_FOR_GRID = 32;

        protected int totalCellNum;
        protected float cellSize;
        protected Vector3 gridResolution;
        protected Vector3 gridCenter;

        private SortTool.BitonicSort _bitonicSort = new SortTool.BitonicSort();

        public GridSearchBase(int objNum)
        {
            this.numObjects = objNum;
            this.threadGroupSize = objNum / SIMULATION_BLOCK_SIZE_FOR_GRID;
        }

        #region Accessor
        internal GraphicsBuffer GridIndicesBuffer => gridIndicesBuffer;
        internal Vector3 GridCenter { get => gridCenter; set => gridCenter = value; }
        internal float CellSize => cellSize;
        internal Vector3 GridResolution => gridResolution;
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
            GridSearchCS.SetInt("_ParticleNum", objectsBufferInput.count);
            SetCSVariables();

            int kernelID = -1;

            #region GridOptimization
            // Build Grid
            kernelID = GridSearchCS.FindKernel("BuildGridCS");
            GridSearchCS.SetBuffer(kernelID, "_ParticleBufferRead", objectsBufferInput);
            GridSearchCS.SetBuffer(kernelID, "_GridBufferWrite", gridBuffer);
            GridSearchCS.Dispatch(kernelID, threadGroupSize, 1, 1);

            // Sort Grid
            _bitonicSort.Sort(ref gridBuffer, ref gridPingPongBuffer);

            // Build Grid Indices
            kernelID = GridSearchCS.FindKernel("ClearGridIndicesCS");
            GridSearchCS.SetBuffer(kernelID, "_GridIndicesBufferWrite", gridIndicesBuffer);
            GridSearchCS.Dispatch(kernelID, (int)(totalCellNum / SIMULATION_BLOCK_SIZE_FOR_GRID), 1, 1);

            kernelID = GridSearchCS.FindKernel("BuildGridIndicesCS");
            GridSearchCS.SetBuffer(kernelID, "_GridBufferRead", gridBuffer);
            GridSearchCS.SetBuffer(kernelID, "_GridIndicesBufferWrite", gridIndicesBuffer);
            GridSearchCS.Dispatch(kernelID, threadGroupSize, 1, 1);

            // Rearrange
            kernelID = GridSearchCS.FindKernel("RearrangeParticlesCS");
            GridSearchCS.SetBuffer(kernelID, "_GridBufferRead", gridBuffer);
            GridSearchCS.SetBuffer(kernelID, "_ParticleBufferRead", objectsBufferInput);
            GridSearchCS.SetBuffer(kernelID, "_ParticleBufferWrite", sortedObjectsBufferOutput);
            GridSearchCS.Dispatch(kernelID, threadGroupSize, 1, 1);
            #endregion GridOptimization

            (sortedObjectsBufferOutput, objectsBufferInput) = (objectsBufferInput, sortedObjectsBufferOutput);
        }

        protected abstract void InitializeBuffer(int objectNum);

        protected abstract void SetCSVariables();

        internal abstract void SetCSVariables(ComputeShader otherShader);
    }
}
