using System.Runtime.InteropServices;
using UnityEngine;

namespace ParticleSimulator.NearestNeighbor
{
    public struct Uint2
    {
        public uint x;
        public uint y;
    }

    public class NearestNeighbor<T> : NearestNeighborBase where T : struct
    {
        public NearestNeighbor(int objectNum) : base()
        {
            Vector3 gridSize = new(64, 64, 64);
            Vector3 gridResolution = new(100, 100, 100);
            this.gridResolution = gridResolution;
            this.gridCellSize = gridSize.x / this.gridResolution.x;
            this.gridCenter = gridCellSize * gridResolution / 2;
            this.totalCellNum = (int)(gridResolution.x * gridResolution.y * gridResolution.z);

            this.NearestNeighborCS = (ComputeShader)Resources.Load("GridSearch", typeof(ComputeShader));

            InitializeBuffer(objectNum);

            Debug.Log("=== Instantiated Grid Sort === \n" +
                      //"Size of Search Area : \t" + searchArea + "\n" +
                      "Total number of cells in the grid : \t" + this.totalCellNum + "\n" +
                      "Number of grid cells for each axis : \t" + this.gridResolution + "\n" +
                      "Size of each grid cells : \t" + this.gridCellSize);
        }

        protected override void InitializeBuffer(int objectNum)
        {
            gridBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, objectNum, Marshal.SizeOf(typeof(Uint2)));
            gridPingPongBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, objectNum, Marshal.SizeOf(typeof(Uint2)));
            gridIndicesBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, totalCellNum, Marshal.SizeOf(typeof(Uint2)));
            sortedObjectsBufferOutput = new GraphicsBuffer(GraphicsBuffer.Target.Structured, objectNum, Marshal.SizeOf(typeof(T)));
        }

        protected override void SetCSVariables()
        {
            NearestNeighborCS.SetVector("_GridCenter", gridCenter);
            NearestNeighborCS.SetFloat("_GridCellSize", gridCellSize);
            NearestNeighborCS.SetVector("_GridResolution", gridResolution);
        }

        public void ShowGridOnGizmo(Vector3 gridCenter, float cellSize, Vector3Int gridResolution, Color gridColor)
        {
            Gizmos.color = gridColor;
            Gizmos.DrawWireCube(gridCenter, gridResolution);
        }

        public void UpdateGridVariables(Vector3 center, Vector3 size, Vector3 resolution)
        {
            this.gridCenter = center;
            this.gridCellSize = size.x / resolution.x;
            this.gridResolution = resolution;
            SetCSVariables();
        }

    }
}
