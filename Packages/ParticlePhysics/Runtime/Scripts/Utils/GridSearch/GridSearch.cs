using System.Runtime.InteropServices;
using UnityEngine;

namespace ParticlePhysics.Utils
{
    public struct Uint2
    {
        public uint x;
        public uint y;
    }

    public class GridSearch<T> : GridSearchBase where T : struct
    {
        /// <summary>
        /// GridSearch manage nearest neighbour search task.
        /// </summary>
        /// <param name="objNum"></param>
        /// <param name="gridSize"></param>
        /// <param name="gridCellSize"></param>
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
        public Uint2 GetCellIndices(uint cellNum) => BufferUtils.GetData<Uint2>(gridIndicesBuffer)[cellNum];
        #endregion

        protected override void InitializeBuffer(int objectNum)
        {
            // ここで宣言しているBufferの要素数を動的に変更できるといい。
            // GridSort()の入力Bufferをコピーする形で対応できないか？
            gridBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, objectNum, Marshal.SizeOf(typeof(Uint2)));
            gridPingPongBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, objectNum, Marshal.SizeOf(typeof(Uint2)));
            gridIndicesBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, totalCellNum, Marshal.SizeOf(typeof(Uint2)));
            sortedObjectsBufferOutput = new GraphicsBuffer(GraphicsBuffer.Target.Structured, objectNum, Marshal.SizeOf(typeof(T)));
        }

        protected override void SetCSVariables()
        {
            GridSearchCS.SetFloat("_GridCellSize", cellSize);
            GridSearchCS.SetVector("_GridResolution", gridResolution);
        }

        internal override void SetCSVariables(ComputeShader shader)
        {
            shader.SetFloat("_GridCellSize", cellSize);
            shader.SetVector("_GridResolution", gridResolution);

            Debug.Log("=== Initialized GridSearch Buffer === \n" +
                      "Target Shader : \t" + shader.name + "\n" +
                      "GridCellSize : \t" + this.cellSize + "\n" +
                      "GridResolution : \t" + this.gridResolution);
        }

        internal void ShowGridOnGizmo(Vector3 gridCenter, float cellSize, Vector3Int gridResolution, Color gridColor)
        {
            Gizmos.color = gridColor;
            Gizmos.DrawWireCube(gridCenter, gridResolution);
        }

        internal void UpdateGridVariables(Vector3 size, Vector3 resolution)
        {
            this.cellSize = size.x / resolution.x;
            this.gridResolution = resolution;
            SetCSVariables();
        }

    }
}
