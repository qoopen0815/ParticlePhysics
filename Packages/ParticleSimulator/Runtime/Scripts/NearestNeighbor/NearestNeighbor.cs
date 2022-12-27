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
        public NearestNeighbor(int particleNum, Vector3 gridSize, Vector3 gridResolution) : base(particleNum)
        {
            this.gridResolution = gridResolution;
            this.gridCellSize = gridSize.x / this.gridResolution.x;
            this.gridCenter = gridCellSize * gridResolution / 2;
            this.totalCellNum = (int)(gridResolution.x * gridResolution.y * gridResolution.z);

            this.NearestNeighborCS = (ComputeShader)Resources.Load("ComputeShader/GridSearch", typeof(ComputeShader));

            InitializeBuffer();

            //Debug.Log("=== Instantiated Grid Sort === \n" +
            //          "Size of Search Area : \t" + searchArea + "\n" +
            //          "Total number of cells in the grid : \t" + this.totalCellNum + "\n" +
            //          "Number of grid cells for each axis : \t" + this.gridCellNum + "\n" +
            //          "Size of each grid cells : \t" + this.gridCellSize);
        }

        protected override void InitializeBuffer()
        {
            gridBuffer = new ComputeBuffer(particleNum, Marshal.SizeOf(typeof(Uint2)));
            gridPingPongBuffer = new ComputeBuffer(particleNum, Marshal.SizeOf(typeof(Uint2)));
            gridIndicesBuffer = new ComputeBuffer(totalCellNum, Marshal.SizeOf(typeof(Uint2)));
            sortedObjectsBufferOutput = new ComputeBuffer(particleNum, Marshal.SizeOf(typeof(T)));
        }

        protected override void SetCSVariables()
        {
            NearestNeighborCS.SetVector("_GridCenter", gridCenter);
            NearestNeighborCS.SetFloat("_GridCellSize", gridCellSize);
            NearestNeighborCS.SetVector("_GridResolution", gridResolution);
        }

        public void ShowSearchAreaOnGizmo()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(gridCenter, gridCellSize * gridResolution);
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
