﻿using System.Runtime.InteropServices;
using UnityEngine;

namespace ParticleSimulator.NearestNeighbour
{
    internal struct Uint2
    {
        public uint x;
        public uint y;
    }

    internal class GridSearch<T> : GridSearchBase where T : struct
    {
        /// <summary>
        /// GridSearch manage nearest neighbour search task.
        /// </summary>
        /// <param name="objNum"></param>
        /// <param name="gridSize"></param>
        /// <param name="gridCellSize"></param>
        public GridSearch(int objNum, Vector3 gridSize, float gridCellSize) : base(objNum)
        {
            this.gridCenter = gridSize / 2;
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

        protected override void InitializeBuffer(int objectNum)
        {
            gridBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, objectNum, Marshal.SizeOf(typeof(Uint2)));
            gridPingPongBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, objectNum, Marshal.SizeOf(typeof(Uint2)));
            gridIndicesBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, totalCellNum, Marshal.SizeOf(typeof(Uint2)));
            sortedObjectsBufferOutput = new GraphicsBuffer(GraphicsBuffer.Target.Structured, objectNum, Marshal.SizeOf(typeof(T)));
        }

        protected override void SetCSVariables()
        {
            GridSearchCS.SetVector("_GridCenter", gridCenter);
            GridSearchCS.SetFloat("_GridCellSize", cellSize);
            GridSearchCS.SetVector("_GridResolution", gridResolution);
        }

        internal override void SetCSVariables(ComputeShader shader)
        {
            shader.SetVector("_GridCenter", gridCenter);
            shader.SetFloat("_GridCellSize", cellSize);
            shader.SetVector("_GridResolution", gridResolution);

            Debug.Log("=== Initialized GridSearch Buffer === \n" +
                      "_GridCenter : \t" + this.gridCenter + "\n" +
                      "_GridCellSize : \t" + this.cellSize + "\n" +
                      "_GridResolution : \t" + this.gridResolution);
        }

        internal void ShowGridOnGizmo(Vector3 gridCenter, float cellSize, Vector3Int gridResolution, Color gridColor)
        {
            Gizmos.color = gridColor;
            Gizmos.DrawWireCube(gridCenter, gridResolution);
        }

        internal void UpdateGridVariables(Vector3 center, Vector3 size, Vector3 resolution)
        {
            this.gridCenter = center;
            this.cellSize = size.x / resolution.x;
            this.gridResolution = resolution;
            SetCSVariables();
        }

    }
}
