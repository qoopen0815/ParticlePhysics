using System.Runtime.InteropServices;
using UnityEngine;

namespace ParticleSimulator
{
    public class SandPhysicsSolver
    {
        public Vector3 gravity = Physics.gravity;
        public float maxTimestep = 0.0005f;
        
        // ComputeShader
        private ComputeShader _solver;
        private GraphicsBuffer _particleCollisionForce;
        private GraphicsBuffer _objectCollisionForce;
        private GraphicsBuffer _terrainCollisionForce;
        private GraphicsBuffer _tmpBufferWrite;

        private NearestNeighbor.NearestNeighbor<ParticleType> _nearestNeighbor;

        public SandPhysicsSolver(Terrain terrain)
        {
            int objNum = 8 * 1024;

            // Simulation
            _solver = (ComputeShader)Resources.Load("MolecularDynamics");
            _solver.SetVector("_Gravity", gravity);
            _solver.SetFloat("_Friction", 0.995f);
            InitBuffer(objNum);

            // Particle
            var p = Particle.SetAsSimpleParticle(ParticleType.GenerateSphere(objNum, new Vector3(50, 10, 50), 5));
            SetParticleCSParams(p);

            // Grid Search
            _nearestNeighbor = new NearestNeighbor.NearestNeighbor<ParticleType>(objNum);
            SetGridSearchCSParams(new Vector3(50, 10, 50), _nearestNeighbor.GridCellSize * _nearestNeighbor.GridResolution, _nearestNeighbor.GridResolution);

            // Terrain
            _solver.SetInt("_Resolution", terrain.terrainData.heightmapResolution);
            _solver.SetVector("_Ratio", new Vector3(terrain.terrainData.heightmapResolution / terrain.terrainData.size.x,
                                    1 / terrain.terrainData.size.y,
                                    terrain.terrainData.heightmapResolution / terrain.terrainData.size.z));
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

        private void InitBuffer(int objNum)
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
                Marshal.SizeOf(typeof(ParticleType)));
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
            _solver.SetVector("_GridCenter", _nearestNeighbor.GridCenter);
            _solver.SetBuffer(kernelID, "_ElementBuffer", elementBuffer);
            _solver.SetBuffer(kernelID, "_ParticleBufferRead", particleBuffer);
            _solver.SetBuffer(kernelID, "_GridIndicesBufferRead", _nearestNeighbor.GridIndicesBuffer);
            _solver.SetBuffer(kernelID, "_ParticleCollisionForce", _particleCollisionForce);
            _solver.GetKernelThreadGroupSizes(kernelID, out var x, out var y, out var z);
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
            _solver.GetKernelThreadGroupSizes(kernelID, out var x, out var y, out var z);
            _solver.Dispatch(kernelID, (int)(particleBuffer.count / x), 1, 1);
            (particleBuffer, _tmpBufferWrite) = (_tmpBufferWrite, particleBuffer);
            var f = new TerrainCollisionForce[particleBuffer.count];
            System.Array.Fill(f, new TerrainCollisionForce(Vector3.zero, Vector3.zero));
            _terrainCollisionForce.SetData(f);
        }

        private void Integrate(ref GraphicsBuffer particleBuffer, GraphicsBuffer terrain)
        {
            int kernelID = _solver.FindKernel("IntegrateCS");
            _solver.SetFloat("_TimeStep", Mathf.Min(maxTimestep, Time.deltaTime));
            _solver.SetBuffer(kernelID, "_ParticleCollisionForce", _particleCollisionForce);
            _solver.SetBuffer(kernelID, "_ObjectCollisionForce", _objectCollisionForce);
            _solver.SetBuffer(kernelID, "_TerrainCollisionForce", _terrainCollisionForce);
            _solver.SetBuffer(kernelID, "_ParticleBufferRead", particleBuffer);
            _solver.SetBuffer(kernelID, "_ParticleBufferWrite", _tmpBufferWrite);
            _solver.SetBuffer(kernelID, "_TerrainBuffer", terrain);
            _solver.GetKernelThreadGroupSizes(kernelID, out var x, out var y, out var z);
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
