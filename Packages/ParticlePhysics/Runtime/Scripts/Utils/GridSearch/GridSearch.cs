using System.Runtime.InteropServices;
using UnityEngine;

namespace ParticlePhysics.Utils
{
    /// <summary>
    /// Represents a 2D unsigned integer vector.
    /// </summary>
    public struct Uint2
    {
        /// <summary>
        /// The X component of the vector.
        /// </summary>
        public uint x;

        /// <summary>
        /// The Y component of the vector.
        /// </summary>
        public uint y;
    }

    /// <summary>
    /// Provides a grid-based search for nearest neighbors.
    /// </summary>
    /// <typeparam name="T">The type of data being searched.</typeparam>
    public class GridSearch<T> : GridSearchBase where T : struct
    {
        /// <summary>
        /// Initializes a new instance of the GridSearch class with the specified parameters.
        /// </summary>
        /// <param name="objNum">The number of objects to search.</param>
        /// <param name="gridSize">The size of the grid search area as a Vector3.</param>
        /// <param name="gridCellSize">The size of each grid cell.</param>
        public GridSearch(int objNum, Vector3 gridSize, float gridCellSize) : base(objNum)
        {
            this.gridResolution = gridSize / gridCellSize;
            this.cellSize = gridCellSize;
            this.totalCellNum = (int)(gridResolution.x * gridResolution.y * gridResolution.z);

            this.GridSearchCS = (ComputeShader)Resources.Load("GridSearch", typeof(ComputeShader));

            InitializeBuffer(objNum);

            Debug.Log("=== Instantiated Grid Sort === \n" +
                      "Size of Grid Search Area : \t" + gridSize + "\n" +
                      "Total number of cells in the grid : \t" + this.totalCellNum + "\n" +
                      "Number of grid cells for each axis : \t" + this.gridResolution + "\n" +
                      "Size of each grid cells : \t" + this.cellSize);
        }

        #region Accessor
        /// <summary>
        /// Gets the cell indices for the specified cell number.
        /// </summary>
        /// <param name="cellNum">The cell number.</param>
        /// <returns>The Uint2 structure representing the cell indices.</returns>
        public Uint2 GetCellIndices(uint cellNum) => BufferUtils.GetData<Uint2>(gridIndicesBuffer)[cellNum];
        #endregion

        /// <summary>
        /// Initializes the graphics buffers used for grid search.
        /// </summary>
        /// <param name="objectNum">The number of objects to search.</param>
        protected override void InitializeBuffer(int objectNum)
        {
            var index = Mathf.CeilToInt(Mathf.Log(objectNum, 2));
            var ceiledObjectNum = Mathf.Pow(2, index);


            // It would be great to dynamically change the size of the buffers declared here.
            // Is there a way to copy the input buffers of GridSort() to handle variable sizes?
            gridBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, objectNum, Marshal.SizeOf(typeof(Uint2)));
            gridPingPongBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, objectNum, Marshal.SizeOf(typeof(Uint2)));
            gridIndicesBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, totalCellNum, Marshal.SizeOf(typeof(Uint2)));
            sortedObjectsBufferOutput = new GraphicsBuffer(GraphicsBuffer.Target.Structured, objectNum, Marshal.SizeOf(typeof(T)));
        }

        /// <summary>
        /// Sets the variables required for the compute shader used in grid search.
        /// </summary>
        protected override void SetCSVariables()
        {
            GridSearchCS.SetFloat("_GridCellSize", cellSize);
            GridSearchCS.SetVector("_GridResolution", gridResolution);
        }

        /// <summary>
        /// Sets the variables required for the given compute shader used in grid search.
        /// </summary>
        /// <param name="shader">The compute shader to set the variables for.</param>
        internal override void SetCSVariables(ComputeShader shader)
        {
            shader.SetFloat("_GridCellSize", cellSize);
            shader.SetVector("_GridResolution", gridResolution);

            Debug.Log("=== Initialized GridSearch Buffer === \n" +
                      "Target Shader : \t" + shader.name + "\n" +
                      "GridCellSize : \t" + this.cellSize + "\n" +
                      "GridResolution : \t" + this.gridResolution);
        }

        /// <summary>
        /// Displays the grid on Gizmos for visual debugging purposes.
        /// </summary>
        /// <param name="gridCenter">The center of the grid.</param>
        /// <param name="cellSize">The size of each grid cell.</param>
        /// <param name="gridResolution">The resolution of the grid as a Vector3Int.</param>
        /// <param name="gridColor">The color to use for drawing the grid.</param>
        internal void ShowGridOnGizmo(Vector3 gridCenter, float cellSize, Vector3Int gridResolution, Color gridColor)
        {
            Gizmos.color = gridColor;
            Gizmos.DrawWireCube(gridCenter, gridResolution);
        }

        /// <summary>
        /// Updates the grid variables based on the given size and resolution.
        /// </summary>
        /// <param name="size">The size of the grid search area as a Vector3.</param>
        /// <param name="resolution">The resolution of the grid as a Vector3.</param>
        internal void UpdateGridVariables(Vector3 size, Vector3 resolution)
        {
            this.cellSize = size.x / resolution.x;
            this.gridResolution = resolution;
            SetCSVariables();
        }

    }
}
