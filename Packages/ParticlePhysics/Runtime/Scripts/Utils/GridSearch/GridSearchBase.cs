using UnityEngine;

namespace ParticlePhysics.Utils
{
    /// <summary>
    /// Base class for grid-based search operations.
    /// </summary>
    public abstract class GridSearchBase
    {
        /// <summary>
        /// The Compute Shader used for grid-based search operations.
        /// </summary>
        protected ComputeShader GridSearchCS;

        /// <summary>
        /// Graphics buffer for storing grid data.
        /// </summary>
        protected GraphicsBuffer gridBuffer;

        /// <summary>
        /// Graphics buffer used for ping-pong operation during grid sorting.
        /// </summary>
        protected GraphicsBuffer gridPingPongBuffer;

        /// <summary>
        /// Graphics buffer for storing grid indices.
        /// </summary>
        protected GraphicsBuffer gridIndicesBuffer;

        /// <summary>
        /// Graphics buffer for output of sorted objects.
        /// </summary>
        protected GraphicsBuffer sortedObjectsBufferOutput;

        /// <summary>
        /// Number of objects to be sorted.
        /// </summary>
        /// <param name="objNum">The number of objects to be sorted.</param>
        protected int numObjects;

        /// <summary>
        /// The thread group size used for grid-based search.
        /// </summary>
        protected int threadGroupSize;

        /// <summary>
        /// Constant block size for grid simulation.
        /// </summary>
        protected static readonly int SIMULATION_BLOCK_SIZE_FOR_GRID = 32;

        /// <summary>
        /// Total number of cells in the grid.
        /// </summary>
        protected int totalCellNum;

        /// <summary>
        /// Size of each cell in the grid.
        /// </summary>
        protected float cellSize;

        /// <summary>
        /// Resolution of the grid in 3D space.
        /// </summary>
        protected Vector3 gridResolution;

        /// <summary>
        /// BitonicSort instance for sorting grid data.
        /// </summary>
        private SortTool.BitonicSort _bitonicSort = new SortTool.BitonicSort();

        /// <summary>
        /// Constructor for GridSearchBase.
        /// </summary>
        /// <param name="objNum">The number of objects to be sorted.</param>
        public GridSearchBase(int objNum)
        {
            this.numObjects = objNum;
            this.threadGroupSize = objNum / SIMULATION_BLOCK_SIZE_FOR_GRID;
        }

        #region Accessor
        /// <summary>
        /// Gets the GraphicsBuffer for storing grid indices.
        /// </summary>
        public GraphicsBuffer TargetGridIndicesBuffer => gridIndicesBuffer;

        /// <summary>
        /// Gets the size of each grid cell.
        /// </summary>
        public float CellSize => cellSize;

        /// <summary>
        /// Gets the resolution of the grid in 3D space.
        /// </summary>
        public Vector3 GridResolution => gridResolution;
        #endregion

        /// <summary>
        /// Release all allocated resources.
        /// </summary>
        public void Release()
        {
            gridBuffer.Release();
            gridIndicesBuffer.Release();
            gridPingPongBuffer.Release();
            sortedObjectsBufferOutput.Release();
        }

        /// <summary>
        /// Sorts objects based on their positions using the grid-based search approach.
        /// </summary>
        /// <param name="objectsBufferInput">The input buffer containing objects to be sorted.</param>
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

        /// <summary>
        /// Sorts objects based on their positions using the grid-based search approach with a transformation.
        /// </summary>
        /// <param name="objectsBufferInput">The input buffer containing objects to be sorted.</param>
        /// <param name="gridTF">The transform of the grid.</param>
        public void GridSort(ref GraphicsBuffer objectsBufferInput, Transform gridTF)
        {
            GridSearchCS.SetInt("_ParticleNum", objectsBufferInput.count);
            SetCSVariables();

            Matrix4x4 gridtf = Matrix4x4.TRS(
                pos: gridTF.position,
                q: gridTF.rotation,
                s: gridTF.localScale);

            int kernelID = -1;

            #region GridOptimization
            // Build Grid
            kernelID = GridSearchCS.FindKernel("BuildGridCS");
            GridSearchCS.SetBuffer(kernelID, "_ParticleBufferRead", objectsBufferInput);
            GridSearchCS.SetBuffer(kernelID, "_GridBufferWrite", gridBuffer);
            GridSearchCS.SetMatrix("_GridTF", gridtf.inverse);
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

        public void GridSort(ref GraphicsBuffer objectsBufferInput, Matrix4x4 gridTRS)
        {
            GridSearchCS.SetInt("_ParticleNum", objectsBufferInput.count);
            SetCSVariables();

            int kernelID = -1;

            #region GridOptimization
            // Build Grid
            kernelID = GridSearchCS.FindKernel("BuildGridCS");
            GridSearchCS.SetBuffer(kernelID, "_ParticleBufferRead", objectsBufferInput);
            GridSearchCS.SetBuffer(kernelID, "_GridBufferWrite", gridBuffer);
            GridSearchCS.SetMatrix("_GridTF", gridTRS.inverse);
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

        /// <summary>
        /// Initialize buffer based on the number of objects.
        /// </summary>
        /// <param name="objectNum">The number of objects to be initialized.</param>
        protected abstract void InitializeBuffer(int objectNum);

        /// <summary>
        /// Set the necessary compute shader variables.
        /// </summary>
        protected abstract void SetCSVariables();

        /// <summary>
        /// Set the necessary compute shader variables from another shader.
        /// </summary>
        /// <param name="otherShader">The other compute shader to get variables from.</param>
        internal abstract void SetCSVariables(ComputeShader otherShader);
    }
}
