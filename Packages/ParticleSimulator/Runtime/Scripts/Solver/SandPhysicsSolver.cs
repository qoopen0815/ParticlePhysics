﻿using System.Runtime.InteropServices;
using UnityEngine;

namespace ParticleSimulator
{
    public class SandPhysicsSolver
    {
        // Simulation
        private Vector3 _gravity = Physics.gravity;
        //private float _timestep = 0.0005f;

        // Particle
        private int _elementNum;
        private int _particleNum;
        private float _particleMu;
        private float _particleTotalMass;
        private Matrix4x4 _particleInertialMoment;

        // GridSearch
        private float _gridCellSize; // Size of grid cell.
        private Vector3 _gridResolution; // Number of grid cell.
        private Vector3 _gridCenter;

        // Terrain
        private int _terrainResolution; // 地形の解像度
        private Vector3 _terrainRatio; // 地形の解像度とサイズの比
        private float _terrainFriction; // 壁の摩擦

        // ComputeShader
        private ComputeShader _solver;
        private GraphicsBuffer _particleCollisionForce;
        private GraphicsBuffer _objectCollisionForce;
        private GraphicsBuffer _terrainCollisionForce;
        private GraphicsBuffer _tmpBufferWrite;

        private NearestNeighbor.NearestNeighbor<ParticleStatus> _nearestNeighbor;

        public SandPhysicsSolver(
            Particle particle,
            Vector3 gridSize, Vector3 gridResolution, Vector3 gridCenter,
            int terrainResolution, Vector3 terrainRatio, float terrainFriction)
        {
            _elementNum = particle.substance.Elements.count;
            _particleNum = particle.status.count;
            _particleMu = particle.substance.Mu;
            _particleTotalMass = particle.substance.TotalMass;
            //_particleInertialMoment = particle.substance.InertialMoment;
            _particleInertialMoment = Matrix4x4.identity;
            _gridCellSize = gridSize.x / gridResolution.x;
            _gridResolution = gridResolution;
            _gridCenter = gridCenter;
            _terrainResolution = terrainResolution;
            _terrainRatio = terrainRatio;
            _terrainFriction = terrainFriction;

            _solver = (ComputeShader)Resources.Load("MolecularDynamics");
            _solver.SetVector("_Gravity", this._gravity);
            _solver.SetInt("_ElementNum", _elementNum);
            _solver.SetInt("_ParticleNum", _particleNum);
            _solver.SetFloat("_ParticleMu", _particleMu);
            _solver.SetFloat("_ParticleTotalMass", _particleTotalMass);
            _solver.SetMatrix(Shader.PropertyToID("_ParticleInertialMoment"), _particleInertialMoment);
            _solver.SetFloat("_GridCellSize", _gridCellSize);
            _solver.SetVector("_GridResolution", _gridResolution);
            _solver.SetInt("_Resolution", _terrainResolution);
            _solver.SetVector("_Ratio", _terrainRatio);
            _solver.SetFloat("_Friction", _terrainFriction);

            InitCSBuffer(_particleNum);

            _nearestNeighbor = new NearestNeighbor.NearestNeighbor<ParticleStatus>(_particleNum);

        }

        public void SetParticleCSParams(Particle particle)
        {
            _solver.SetInt("_ElementNum", particle.substance.Elements.count);
            _solver.SetInt("_ParticleNum", particle.num);
            _solver.SetFloat("_ParticleMu", particle.substance.Mu);
            _solver.SetFloat("_ParticleTotalMass", particle.substance.TotalMass);
            _solver.SetMatrix(Shader.PropertyToID("_ParticleInertialMoment"), Matrix4x4.identity);
        }

        public void SetGridSearchCSParams(Vector3 center, Vector3 size, Vector3 resolution)
        {
            _nearestNeighbor.UpdateGridVariables(center, size, resolution);
            _solver.SetVector("_GridResolution", resolution);
            _solver.SetFloat("_GridCellSize", size.x / resolution.x);
        }

        private void InitCSBuffer(int objNum)
        {
            // Init Force Bufer
            _particleCollisionForce = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                (int)objNum,
                Marshal.SizeOf(typeof(ParticleCollisionForce)));
            _objectCollisionForce = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                (int)objNum,
                Marshal.SizeOf(typeof(ObjectCollisionForce)));
            _terrainCollisionForce = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                (int)objNum,
                Marshal.SizeOf(typeof(TerrainCollisionForce)));

            // Init temp buffer
            _tmpBufferWrite = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                (int)objNum,
                Marshal.SizeOf(typeof(ParticleStatus)));
        }

        public void UpdateParticle(ref Particle particles, GraphicsBuffer terrain)
        {
            _nearestNeighbor.GridSort(ref particles.status);
            CalculateParticleCollisionForce(particles.status, particles.substance.Elements);
            ////CalculateTerrainCollision(ref _terrainCollisionForce, _particleBuffer, _terrainBuffer);
            //CalculateObjectCollision(ref _objectCollisionForce, _particleBuffer, _objectParticleBuffer);
            Integrate(ref particles.status, terrain);
        }

        public void UpdateTerrain(ref Terrain newTerrain)
        {

        }

        private void CalculateParticleCollisionForce(GraphicsBuffer particleBuffer, GraphicsBuffer elementBuffer)
        {
            int kernelID = _solver.FindKernel("ParticleCollisionCS");
            _solver.SetFloat("_ParticleMu", _particleMu);
            _solver.SetBuffer(kernelID, "_ElementBuffer", elementBuffer);
            _solver.SetBuffer(kernelID, "_ParticleBufferRead", particleBuffer);
            _solver.SetBuffer(kernelID, "_GridIndicesBufferRead", _nearestNeighbor.GridIndicesBuffer);
            _solver.SetBuffer(kernelID, "_ParticleCollisionForce", _particleCollisionForce);
            _solver.GetKernelThreadGroupSizes(kernelID, out uint x, out _, out _);
            _solver.Dispatch(kernelID, (int)(particleBuffer.count / x), 1, 1);
        }

        private void CalculateObjectCollision(GraphicsBuffer particleBuffer, GraphicsBuffer objectBuffer)
        {
            var f = new ObjectCollisionForce[_objectCollisionForce.count];
            System.Array.Fill(f, new ObjectCollisionForce(Vector3.zero, Vector3.zero));
            _objectCollisionForce.SetData(f);
        }

        private void CalculateTerrainCollision(GraphicsBuffer particleBuffer, GraphicsBuffer terrainBuffer)
        {
            int kernelID = _solver.FindKernel("TerrainCollisionCS");
            _solver.SetBuffer(kernelID, "_TerrainBuffer", terrainBuffer);
            _solver.SetBuffer(kernelID, "_TerrainCollisionForce", _terrainCollisionForce);
            _solver.SetBuffer(kernelID, "_ParticleBufferRead", particleBuffer);
            _solver.GetKernelThreadGroupSizes(kernelID, out uint x, out _, out _);
            _solver.Dispatch(kernelID, (int)(particleBuffer.count / x), 1, 1);
            (particleBuffer, _tmpBufferWrite) = (_tmpBufferWrite, particleBuffer);
            var f = new TerrainCollisionForce[particleBuffer.count];
            System.Array.Fill(f, new TerrainCollisionForce(Vector3.zero, Vector3.zero));
            _terrainCollisionForce.SetData(f);
        }

        private void Integrate(ref GraphicsBuffer particleBuffer, GraphicsBuffer terrain)
        {
            int kernelID = _solver.FindKernel("IntegrateCS");
            //_solver.SetFloat("_TimeStep", Mathf.Min(maxTimestep, Time.deltaTime));
            _solver.SetFloat("_TimeStep", Time.deltaTime);
            _solver.SetBuffer(kernelID, "_ParticleCollisionForce", _particleCollisionForce);
            _solver.SetBuffer(kernelID, "_ObjectCollisionForce", _objectCollisionForce);
            _solver.SetBuffer(kernelID, "_TerrainCollisionForce", _terrainCollisionForce);
            _solver.SetBuffer(kernelID, "_ParticleBufferRead", particleBuffer);
            _solver.SetBuffer(kernelID, "_ParticleBufferWrite", _tmpBufferWrite);
            _solver.SetBuffer(kernelID, "_TerrainBuffer", terrain);
            _solver.GetKernelThreadGroupSizes(kernelID, out uint x, out _, out _);
            _solver.Dispatch(kernelID, (int)(particleBuffer.count / x), 1, 1);
            (particleBuffer, _tmpBufferWrite) = (_tmpBufferWrite, particleBuffer);
        }

        public void Release()
        {
            _particleCollisionForce.Release();
            _terrainCollisionForce.Release();
            _objectCollisionForce.Release();
            _tmpBufferWrite.Release();
            _nearestNeighbor.Release();
        }
    }
}
