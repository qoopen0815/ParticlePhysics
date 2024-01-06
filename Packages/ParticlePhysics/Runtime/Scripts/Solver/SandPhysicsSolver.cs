using ParticlePhysics.Utils;
using ParticlePhysics.Particle;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ParticlePhysics.Solver
{
    /// <summary>
    /// This class simulates particle behaviour based on the MolecularDynamics method.
    /// </summary>
    public class SandPhysicsSolver
    {
        // Simulation
        public readonly Vector3 gravity = Physics.gravity;
        public readonly float maxAllowableTimestep = 0.005f;

        // Registered Items
        private ParticleBuffer _particle = null;
        private List<ObjectBuffer> _objectBuffers = null;

        // Terrain
        private int _terrainResolution; // 地形の解像度
        private Vector3 _terrainRatio; // 地形の解像度とサイズの比
        private float _terrainFriction; // 壁の摩擦

        // ComputeShader
        private ComputeShader _shader;
        private GraphicsBuffer _particleCollisionForce;
        private GraphicsBuffer _objectCollisionForce;
        private GraphicsBuffer _terrainCollisionForce;
        private GraphicsBuffer _tmpBufferWrite;

        private GridSearch<ParticleState> _fieldGS = null;
        public static Vector3 gridSize = new(40, 30, 40);
        public static float gridCellSize = 0.4f;

        public GraphicsBuffer _debugger;

        Matrix4x4 _gridTF = Matrix4x4.identity;
        Matrix4x4 _objectTF = Matrix4x4.identity;


        #region Accessor
        public ParticleBuffer RegisteredParticles => _particle;
        public List<GameObject> RegisteredCollisionObjectList => _objectBuffers.Select(x => x.RegisteredObject).ToList();
        public GameObject[] RegisteredCollisionObjectArray => _objectBuffers.Select(x => x.RegisteredObject).ToArray();
        public ParticleCollisionForce[] ParticleCollisionForce => BufferUtils.GetData<ParticleCollisionForce>(_particleCollisionForce);
        public ParticleCollisionForce[] ObjectCollisionForce => BufferUtils.GetData<ParticleCollisionForce>(_objectCollisionForce);
        public ParticleCollisionForce[] TerrainCollisionForce => BufferUtils.GetData<ParticleCollisionForce>(_terrainCollisionForce);
        #endregion

        /// <summary>
        /// Constructor for SandPhysicsSolver.
        /// Note: Please call SetMainParticle(), SetCollisionObjects() and SetTerrain() to register the required data before execution.
        /// </summary>
        /// <param name="gravity">The gravity vector to be applied in the simulation.</param>
        /// <param name="maxAllowableTimestep">The maximum allowable timestep for the simulation.</param>
        public SandPhysicsSolver(Vector3 gravity, float maxAllowableTimestep = 0.005f)
        {
            this.gravity = gravity;
            this.maxAllowableTimestep = maxAllowableTimestep;

            _shader = (ComputeShader)Resources.Load("MolecularDynamics");
            _shader.SetVector("_Gravity", gravity);

            Debug.Log("=== Initialized Solver Data === \n" +
                      "Gravity : \t" + this.gravity + "\n" +
                      "Max Allowable Timestep : \t" + this.maxAllowableTimestep);
        }

        /// <summary>
        /// Set the main particle buffer for the simulation.
        /// Note: If no particle buffer is provided, a default tetrahedron particle buffer will be generated.
        /// </summary>
        /// <param name="particle">The main particle buffer to be used for the simulation.</param>
        public void SetMainParticle(ParticleBuffer particle = null)
        {
            if (particle == null)
            {
                particle = ParticleBuffer.SetAsTetrahedronParticle(ParticleState.GenerateSphere((int)Enum.ParticleNum.NUM_8K, Vector3.zero, 3));
            }

            _particle = particle;
            _shader.SetInt("_ElementNum", _particle.substance.Elements.count);
            _shader.SetInt("_ParticleNum", _particle.num);
            _shader.SetFloat("_ParticleMu", _particle.substance.Mu);
            _shader.SetFloat("_ParticleTotalMass", _particle.substance.TotalMass);
            _shader.SetMatrix("_ParticleInertialMoment", _particle.substance.InertialMoment4x4);
            InitializeBuffer(_particle.num);
        }

        /// <summary>
        /// Set the collision objects to be used in the simulation.
        /// </summary>
        /// <param name="gameObjects">An array of GameObjects representing the collision objects.</param>
        public void SetCollisionObjects(GameObject[] gameObjects)
        {
            _objectBuffers = new List<ObjectBuffer>();

            ObjectBuffer data;
            foreach (GameObject gameObject in gameObjects)
            {
                data = new ObjectBuffer(gameObject, _particle.num);
                _objectBuffers.Add(data);
            }
        }

        /// <summary>
        /// Set the terrain data to be used in the simulation.
        /// </summary>
        /// <param name="terrain">The Terrain object representing the terrain data.</param>
        /// <param name="gridCenter">The center position of the grid for collision detection.</param>
        public void SetFieldTerrain(Terrain terrain, Vector3 gridCenter)
        {
            _terrainFriction = 0.955f;
            _terrainResolution = terrain.terrainData.heightmapResolution;
            _terrainRatio = new Vector3(terrain.terrainData.heightmapResolution / terrain.terrainData.size.x,
                                        1 / terrain.terrainData.size.y,
                                        terrain.terrainData.heightmapResolution / terrain.terrainData.size.z);

            _shader.SetFloat("_Friction", _terrainFriction);
            _shader.SetInt("_Resolution", _terrainResolution);
            _shader.SetVector("_Ratio", _terrainRatio);

            Debug.Log("=== Initialized Terrain Data === \n" +
                      "_Resolution : \t" + this._terrainResolution + "\n" +
                      "_Ratio : \t" + this._terrainRatio + "\n" +
                      "_Friction : \t" + this._terrainFriction);

            if (_particle != null)
            {
                _fieldGS = new GridSearch<ParticleState>(_particle.num, gridSize, gridCellSize);
                _fieldGS.SetCSVariables(_shader);
            }
            else
            {
                Debug.LogWarning(
                    "Warning: no particle data has been registered. \n" +
                    "Register particles with SetMainParticle() before reading SetTerrain().");
            }
        }

        private void InitializeBuffer(int objNum)
        {
            // Init Force Bufer
            _particleCollisionForce = new GraphicsBuffer(GraphicsBuffer.Target.Structured, objNum, Marshal.SizeOf(typeof(ParticleCollisionForce)));
            _objectCollisionForce = new GraphicsBuffer(GraphicsBuffer.Target.Structured, objNum, Marshal.SizeOf(typeof(ParticleCollisionForce)));
            _terrainCollisionForce = new GraphicsBuffer(GraphicsBuffer.Target.Structured, objNum, Marshal.SizeOf(typeof(ParticleCollisionForce)));

            // Init temp buffer
            _tmpBufferWrite = new GraphicsBuffer(GraphicsBuffer.Target.Structured, objNum, Marshal.SizeOf(typeof(ParticleState)));
            _debugger = new GraphicsBuffer(GraphicsBuffer.Target.Structured, objNum, Marshal.SizeOf(typeof(Vector4)));
        }

        /// <summary>
        /// Method to release the resources used by the SandPhysicsSolver.
        /// </summary>
        public void Release()
        {
            foreach (var objectData in _objectBuffers) objectData.Release();
            _particleCollisionForce.Release();
            _terrainCollisionForce.Release();
            _objectCollisionForce.Release();
            _tmpBufferWrite.Release();
            _fieldGS.Release();
            _debugger.Release();
        }

        /// <summary>
        /// Update the particle buffer with collision forces and integrate the particles' positions.
        /// </summary>
        /// <param name="particles">The ParticleBuffer to be updated.</param>
        /// <param name="terrain">The GraphicsBuffer representing the terrain data.</param>
        public void UpdateParticle(ref ParticleBuffer particles, GraphicsBuffer terrain)
        {
            CalculateParticleCollisionForce(ref particles);
            CalculateObjectCollision(ref particles);
            //CalculateTerrainCollision(ref _terrainCollisionForce, _particleBuffer, _terrainBuffer);
            Integrate(ref particles.status, terrain);
        }

        private void CalculateParticleCollisionForce(ref ParticleBuffer particle)
        {
            _gridTF.SetTRS(
                pos: new Vector3(50, 15, 50) - gridSize / 2,
                q: Quaternion.identity,
                s: Vector3.one);

            _fieldGS.GridSort(ref particle.status, _gridTF);
            int kernelID = _shader.FindKernel("ParticleCollisionCS");
            _shader.SetMatrix("_gridTF", _gridTF);
            _shader.SetBuffer(kernelID, "_ParticleElementBuffer", particle.substance.Elements);
            _shader.SetBuffer(kernelID, "_ParticleBufferRead", particle.status);
            _shader.SetBuffer(kernelID, "_GridIndicesBufferRead", _fieldGS.TargetGridIndicesBuffer);
            _shader.SetBuffer(kernelID, "_ParticleCollisionForce", _particleCollisionForce);
            //_shader.SetBuffer(kernelID, "_DebugBuffer", _debugger);
            _shader.GetKernelThreadGroupSizes(kernelID, out var x, out _, out _);
            _shader.Dispatch(kernelID, (int)(particle.num / x), 1, 1);

            //BufferUtils.DebugBuffer<Vector4>(_debugger, _particleNum, 10);
        }

        private void CalculateObjectCollision(ref ParticleBuffer particle)
        {
            foreach (var data in _objectBuffers)
            {
                _objectTF.SetTRS(
                    data.RegisteredObject.transform.position,
                    data.RegisteredObject.transform.rotation,
                    data.RegisteredObject.transform.localScale);

                var hoge = data.GridSort(ref particle.status);

                int kernelID = _shader.FindKernel("ObjectCollisionCS");

                _shader.SetMatrix("_ObjectTF", _objectTF);
                _shader.SetBuffer(kernelID, "_ObjectCollisionForce", _objectCollisionForce);

                _shader.SetBuffer(kernelID, "_ParticleElementBuffer", particle.substance.Elements);
                _shader.SetBuffer(kernelID, "_ParticleBufferRead", particle.status);
                _shader.SetBuffer(kernelID, "_ParticleGridIndicesBufferRead", hoge);

                _shader.SetBuffer(kernelID, "_ObjectElementBuffer", data.ObjectParticle.substance.Elements);
                _shader.SetBuffer(kernelID, "_ObjectParticleBufferRead", data.ObjectParticle.status);
                _shader.SetBuffer(kernelID, "_ObjGridIndicesBufferRead", data.ObjectParticleGridIndicesBuffer);

                //_shader.SetBuffer(kernelID, "_DebugBuffer", _debugger);
                _shader.GetKernelThreadGroupSizes(kernelID, out var x, out _, out _);
                _shader.Dispatch(kernelID, (int)(particle.num / x), 1, 1);

                //BufferUtils.DebugBuffer<Vector4>(_debugger, _particleNum, 10);
            }
        }

        private void CalculateTerrainCollision(GraphicsBuffer particleBuffer, GraphicsBuffer terrainBuffer)
        {
            int kernelID = _shader.FindKernel("TerrainCollisionCS");
            _shader.SetBuffer(kernelID, "_TerrainBuffer", terrainBuffer);
            _shader.SetBuffer(kernelID, "_TerrainCollisionForce", _terrainCollisionForce);
            _shader.SetBuffer(kernelID, "_ParticleBufferRead", particleBuffer);
            _shader.GetKernelThreadGroupSizes(kernelID, out var x, out _, out _);
            _shader.Dispatch(kernelID, (int)(particleBuffer.count / x), 1, 1);
            (particleBuffer, _tmpBufferWrite) = (_tmpBufferWrite, particleBuffer);
            var f = new ParticleCollisionForce[particleBuffer.count];
            System.Array.Fill(f, new ParticleCollisionForce(Vector3.zero, Vector3.zero));
            _terrainCollisionForce.SetData(f);
        }

        private void Integrate(ref GraphicsBuffer particleBuffer, GraphicsBuffer terrainBuffer)
        {
            int kernelID = _shader.FindKernel("IntegrateCS");
            _shader.SetFloat("_TimeStep", Mathf.Min(maxAllowableTimestep, Time.deltaTime));
            _shader.SetBuffer(kernelID, "_ParticleCollisionForce", _particleCollisionForce);
            _shader.SetBuffer(kernelID, "_ObjectCollisionForce", _objectCollisionForce);
            _shader.SetBuffer(kernelID, "_TerrainCollisionForce", _terrainCollisionForce);
            _shader.SetBuffer(kernelID, "_ParticleBufferRead", particleBuffer);
            _shader.SetBuffer(kernelID, "_ParticleBufferWrite", _tmpBufferWrite);
            _shader.SetBuffer(kernelID, "_TerrainBuffer", terrainBuffer);
            //_shader.SetBuffer(kernelID, "_DebugBuffer", _debugger);
            _shader.GetKernelThreadGroupSizes(kernelID, out var x, out _, out _);
            _shader.Dispatch(kernelID, (int)(particleBuffer.count / x), 1, 1);
            (particleBuffer, _tmpBufferWrite) = (_tmpBufferWrite, particleBuffer);

            //BufferUtils.DebugBuffer<Vector4>(_debugger, _particleNum, 10);
        }
    }
}
