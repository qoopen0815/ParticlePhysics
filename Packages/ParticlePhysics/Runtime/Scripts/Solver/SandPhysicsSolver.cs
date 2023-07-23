using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

using ParticlePhysics.Utils;

namespace ParticlePhysics.Solver
{
    /// <summary>
    /// Struct to store collision object data used by the SandPhysicsSolver.
    /// </summary>
    internal struct CollisionObjectBuffer
    {
        /// <summary>
        /// The GameObject associated with the collision object.
        /// </summary>
        public GameObject gameObject;

        /// <summary>
        /// Particle buffer for the collision object.
        /// </summary>
        public ParticleBuffer objParticleBuffer;

        /// <summary>
        /// Grid search object for the collision object.
        /// </summary>
        public GridSearch<ParticleState> objectGS;

        private GraphicsBuffer _objGridIndicesBuffer;

        /// <summary>
        /// Graphics buffer for the collision object's grid indices.
        /// </summary>
        public GraphicsBuffer ObjectGridIndicesBuffer => _objGridIndicesBuffer;

        /// <summary>
        /// Graphics buffer for the particle grid indices.
        /// </summary>
        public GraphicsBuffer ParticleGridIndicesBuffer => objectGS.TargetGridIndicesBuffer;

        /// <summary>
        /// Constructor to initialize the CollisionObjectBuffer.
        /// </summary>
        /// <param name="gameObject">The GameObject representing the collision object.</param>
        /// <param name="gridSize">The size of the grid for collision detection.</param>
        /// <param name="gridCellSize">The cell size of the grid for collision detection.</param>
        public CollisionObjectBuffer(GameObject gameObject, Vector3 gridSize, float gridCellSize)
        {
            //gridSizeはgameObjectのサイズで自動で決められるべき
            //gridCellSizeは砂のサイズで自動で決められるべき
            this.gameObject = gameObject;
            this.objParticleBuffer = ParticleBuffer.SetAsSimpleParticle(
                particles: ParticleState.GenerateFromMesh(gameObject.GetComponent<MeshFilter>().mesh),
                radius: 0.1f);
            this.objectGS = new(this.objParticleBuffer.num, gridSize, gridCellSize);
            this.objectGS.GridSort(ref this.objParticleBuffer.status);
            this._objGridIndicesBuffer = this.objectGS.TargetGridIndicesBuffer;
        }

        /// <summary>
        /// Method to release the resources held by the CollisionObjectBuffer.
        /// </summary>
        public void Release()
        {
            objParticleBuffer.Release();
            _objGridIndicesBuffer.Release();
            objectGS.Release();
        }
    }

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
        private List<CollisionObjectBuffer> _objectBuffers = null;

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

        Matrix4x4 _objectTF = Matrix4x4.identity;


        #region Accessor
        public ParticleBuffer RegisteredParticles => _particle;
        public List<GameObject> RegisteredCollisionObjectList => _objectBuffers.Select(x => x.gameObject).ToList();
        public GameObject[] RegisteredCollisionObjectArray => _objectBuffers.Select(x => x.gameObject).ToArray();
        public ParticleCollisionForce[] ParticleCollisionForce => BufferUtils.GetData<ParticleCollisionForce>(_particleCollisionForce);
        public ObjectCollisionForce[] ObjectCollisionForce => BufferUtils.GetData<ObjectCollisionForce>(_particleCollisionForce);
        public TerrainCollisionForce[] TerrainCollisionForce => BufferUtils.GetData<TerrainCollisionForce>(_particleCollisionForce);
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
        public void SetMainParticle(ParticleBuffer particle=null)
        {
            if(particle==null)
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
        /// <param name="objects">An array of GameObjects representing the collision objects.</param>
        public void SetCollisionObjects(GameObject[] objects)
        {
            _objectBuffers = new List<CollisionObjectBuffer>();

            CollisionObjectBuffer data;
            foreach (GameObject obj in objects)
            {
                data = new CollisionObjectBuffer(
                    gameObject: obj,
                    gridSize: new(10, 10, 10),
                    gridCellSize: gridCellSize);
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
            _objectCollisionForce = new GraphicsBuffer(GraphicsBuffer.Target.Structured, objNum, Marshal.SizeOf(typeof(ObjectCollisionForce)));
            _terrainCollisionForce = new GraphicsBuffer(GraphicsBuffer.Target.Structured, objNum, Marshal.SizeOf(typeof(TerrainCollisionForce)));

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
            _fieldGS.GridSort(ref particle.status);
            int kernelID = _shader.FindKernel("ParticleCollisionCS");
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
            foreach(var data in _objectBuffers)
            {
                _objectTF.SetTRS(
                    data.gameObject.transform.position,
                    data.gameObject.transform.rotation,
                    data.gameObject.transform.localScale);

                data.objectGS.GridSort(ref particle.status, data.gameObject.transform);

                int kernelID = _shader.FindKernel("ObjectCollisionCS");

                _shader.SetMatrix("_ObjectTF", _objectTF);
                _shader.SetBuffer(kernelID, "_ObjectCollisionForce", _objectCollisionForce);

                _shader.SetBuffer(kernelID, "_ParticleElementBuffer", particle.substance.Elements);
                _shader.SetBuffer(kernelID, "_ParticleBufferRead", particle.status);
                _shader.SetBuffer(kernelID, "_ParticleGridIndicesBufferRead", data.ParticleGridIndicesBuffer);

                _shader.SetBuffer(kernelID, "_ObjectElementBuffer", data.objParticleBuffer.substance.Elements);
                _shader.SetBuffer(kernelID, "_ObjectParticleBufferRead", data.objParticleBuffer.status);
                _shader.SetBuffer(kernelID, "_ObjGridIndicesBufferRead", data.ObjectGridIndicesBuffer);

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
            var f = new TerrainCollisionForce[particleBuffer.count];
            System.Array.Fill(f, new TerrainCollisionForce(Vector3.zero, Vector3.zero));
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
