using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

using ParticlePhysics.Type;
using ParticlePhysics.Utils.NearestNeighbour;

namespace ParticlePhysics.Solver
{
    internal struct ObjectData
    {
        public GameObject gameObject;
        public GranularParticle particle;

        public ObjectData(GameObject gameObject, GranularParticle particle)
        {
            this.gameObject = gameObject;
            this.particle = particle;
        }
    }

    /// <summary>
    /// This class simulates particle behaviour based on the MolecularDynamics method.
    /// </summary>
    public class MDSolver
    {
        // Simulation
        public readonly Vector3 gravity = Physics.gravity;
        public readonly float maxAllowableTimestep = 0.005f;

        // Registered Items
        private GranularParticle _particle = null;
        private List<ObjectData> _objectDatas = null;
        private Terrain _terrain = null;

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
        private GraphicsBuffer _objWorld;

        //private GridSearch<ParticleState> _nearestNeighbor;
        private GridSearch<ParticleState> _fieldGS = null;
        private List<GridSearch<ParticleState>> _objectGSList = null;

        public GraphicsBuffer _debugger;

        Matrix4x4 _objectTF = Matrix4x4.identity;


        #region Accessor
        public GranularParticle RegisteredParticles => _particle;
        public List<GameObject> RegisteredObjects => _objectDatas.Select(x => x.gameObject).ToList();
        public ParticleCollisionForce[] ParticleCollisionForce => BufferUtils.GetData<ParticleCollisionForce>(_particleCollisionForce);
        public ObjectCollisionForce[] ObjectCollisionForce => BufferUtils.GetData<ObjectCollisionForce>(_particleCollisionForce);
        public TerrainCollisionForce[] TerrainCollisionForce => BufferUtils.GetData<TerrainCollisionForce>(_particleCollisionForce);
        #endregion

        /// <summary>
        /// Note: Please call SetMainParticle(), SetCollisionObjects() and SetTerrain() to register the required data before execution.
        /// </summary>
        /// <param name="gravity"></param>
        /// <param name="maxAllowableTimestep"></param>
        public MDSolver(Vector3 gravity, float maxAllowableTimestep = 0.005f)
        {
            this.gravity = gravity;
            this.maxAllowableTimestep = maxAllowableTimestep;

            _shader = (ComputeShader)Resources.Load("MolecularDynamics");
            _shader.SetVector("_Gravity", gravity);

            Debug.Log("=== Initialized Solver Data === \n" +
                      "Gravity : \t" + this.gravity + "\n" +
                      "Max Allowable Timestep : \t" + this.maxAllowableTimestep);
        }

        public void SetMainParticle(GranularParticle particle)
        {
            _particle = particle;
            _shader.SetInt("_ElementNum", _particle.substance.Elements.count);
            _shader.SetInt("_ParticleNum", _particle.num);
            _shader.SetFloat("_ParticleMu", _particle.substance.Mu);
            _shader.SetFloat("_ParticleTotalMass", _particle.substance.TotalMass);
            _shader.SetMatrix("_ParticleInertialMoment", _particle.substance.InertialMoment4x4);
            InitializeBuffer(_particle.num);
        }

        public void SetCollisionObjects(GameObject[] objects, Vector3 gridSize, float gridCellSize, Vector3 gridCenter)
        {
            _objectGSList = new List<GridSearch<ParticleState>>();
            _objectDatas = new List<ObjectData>();

            ObjectData data;
            foreach (GameObject obj in objects)
            {
                data = new ObjectData(
                    gameObject: obj,
                    particle: GranularParticle.SetAsSimpleParticle(ParticleState.GenerateFromMesh(obj.GetComponent<MeshFilter>().mesh), radius: 0.1f));
                _objectDatas.Add(data);
                
                var gs = new GridSearch<ParticleState>(data.particle.num, gridSize, gridCellSize);
                gs.GridCenter = gridCenter;
                _objectGSList.Add(gs);
            }
        }

        public void SetFieldTerrain(Terrain terrain, Vector3 gridSize, float gridCellSize, Vector3 gridCenter)
        {
            _terrain = terrain;
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
                _fieldGS.GridCenter = gridCenter;
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
        /// Must be called at the end of execution.
        /// </summary>
        public void Release()
        {
            foreach (var objectGS in _objectGSList) objectGS.Release();
            _particleCollisionForce.Release();
            _terrainCollisionForce.Release();
            _objectCollisionForce.Release();
            _tmpBufferWrite.Release();
            _objWorld.Release();
            _fieldGS.Release();
            _debugger.Release();
        }

        public void UpdateParticle(ref GranularParticle particles, GraphicsBuffer terrain)
        {
            CalculateParticleCollisionForce(ref particles);
            CalculateObjectCollision(ref particles);
            //CalculateTerrainCollision(ref _terrainCollisionForce, _particleBuffer, _terrainBuffer);
            Integrate(ref particles.state, terrain);
        }

        private void CalculateParticleCollisionForce(ref GranularParticle particle)
        {
            _fieldGS.GridSort(ref particle.state);
            int kernelID = _shader.FindKernel("ParticleCollisionCS");
            _shader.SetBuffer(kernelID, "_ElementBuffer", particle.substance.Elements);
            _shader.SetBuffer(kernelID, "_ParticleBufferRead", particle.state);
            _shader.SetBuffer(kernelID, "_GridIndicesBufferRead", _fieldGS.GridIndicesBuffer);
            _shader.SetBuffer(kernelID, "_ParticleCollisionForce", _particleCollisionForce);
            //_shader.SetBuffer(kernelID, "_DebugBuffer", _debugger);
            _shader.GetKernelThreadGroupSizes(kernelID, out uint x, out _, out _);
            _shader.Dispatch(kernelID, (int)(particle.num / x), 1, 1);

            //BufferUtils.DebugBuffer<Vector4>(_debugger, _particleNum, 10);
        }

        private void CalculateObjectCollision(ref GranularParticle particle)
        {
            ObjectData data = _objectDatas[0];
            GridSearchBase objectGS = _objectGSList[0];

            _objectTF.SetTRS(
                data.gameObject.transform.position,
                data.gameObject.transform.rotation,
                data.gameObject.transform.localScale);

            objectGS.GridSort(ref data.particle.state);

            int kernelID = _shader.FindKernel("ObjectCollisionCS");
            _shader.SetMatrix("_ObjectTF", _objectTF);
            _shader.SetBuffer(kernelID, "_ElementBuffer", particle.substance.Elements);
            _shader.SetBuffer(kernelID, "_ParticleBufferRead", particle.state);
            _shader.SetBuffer(kernelID, "_ObjectElementBuffer", data.particle.substance.Elements);
            _shader.SetBuffer(kernelID, "_ObjectParticleBufferRead", data.particle.state);
            _shader.SetBuffer(kernelID, "_GridIndicesBufferRead", objectGS.GridIndicesBuffer);
            _shader.SetBuffer(kernelID, "_ObjectCollisionForce", _objectCollisionForce);
            //_shader.SetBuffer(kernelID, "_DebugBuffer", _debugger);
            _shader.GetKernelThreadGroupSizes(kernelID, out uint x, out _, out _);
            _shader.Dispatch(kernelID, (int)(particle.num / x), 1, 1);

            //BufferUtils.DebugBuffer<Vector4>(_debugger, _particleNum, 10);
        }

        private void CalculateTerrainCollision(GraphicsBuffer particleBuffer, GraphicsBuffer terrainBuffer)
        {
            int kernelID = _shader.FindKernel("TerrainCollisionCS");
            _shader.SetBuffer(kernelID, "_TerrainBuffer", terrainBuffer);
            _shader.SetBuffer(kernelID, "_TerrainCollisionForce", _terrainCollisionForce);
            _shader.SetBuffer(kernelID, "_ParticleBufferRead", particleBuffer);
            _shader.GetKernelThreadGroupSizes(kernelID, out uint x, out _, out _);
            _shader.Dispatch(kernelID, (int)(particleBuffer.count / x), 1, 1);
            (particleBuffer, _tmpBufferWrite) = (_tmpBufferWrite, particleBuffer);
            var f = new TerrainCollisionForce[particleBuffer.count];
            System.Array.Fill(f, new TerrainCollisionForce(Vector3.zero, Vector3.zero));
            _terrainCollisionForce.SetData(f);
        }

        private void Integrate(ref GraphicsBuffer particleBuffer, GraphicsBuffer terrain)
        {
            int kernelID = _shader.FindKernel("IntegrateCS");
            _shader.SetFloat("_TimeStep", Mathf.Min(maxAllowableTimestep, Time.deltaTime));
            _shader.SetBuffer(kernelID, "_ParticleCollisionForce", _particleCollisionForce);
            _shader.SetBuffer(kernelID, "_ObjectCollisionForce", _objectCollisionForce);
            _shader.SetBuffer(kernelID, "_TerrainCollisionForce", _terrainCollisionForce);
            _shader.SetBuffer(kernelID, "_ParticleBufferRead", particleBuffer);
            _shader.SetBuffer(kernelID, "_ParticleBufferWrite", _tmpBufferWrite);
            _shader.SetBuffer(kernelID, "_TerrainBuffer", terrain);
            //_shader.SetBuffer(kernelID, "_DebugBuffer", _debugger);
            _shader.GetKernelThreadGroupSizes(kernelID, out uint x, out _, out _);
            _shader.Dispatch(kernelID, (int)(particleBuffer.count / x), 1, 1);
            (particleBuffer, _tmpBufferWrite) = (_tmpBufferWrite, particleBuffer);

            //BufferUtils.DebugBuffer<Vector4>(_debugger, _particleNum, 10);
        }
    }
}
