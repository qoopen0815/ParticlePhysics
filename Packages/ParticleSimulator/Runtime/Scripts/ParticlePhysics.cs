using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.VFX;

namespace ParticleSimulator
{
    public class ParticlePhysics : MonoBehaviour
    {
        [Header("Particle Setting")]
        [SerializeField] private ParticleNumEnum _particleNum;
        [SerializeField] private ParticleTypeEnum _particleType;
        [SerializeField, Range(0.02f, 0.1f)] private float _particleRadius;
        [SerializeField] private Vector3 _spornPos;     // When a debugging component is available, this variable will be moved there.
        [SerializeField] private VisualEffect _effect;

        [Header("Physics Setting")]
        [SerializeField] private Vector3 _gravity = Physics.gravity;
        [SerializeField] private float _maxTimestep = 0.0005f;

        [Header("Collision Objects")]
        [SerializeField] private Terrain _terrain;
        [SerializeField] private List<GameObject> _objects;
        
        [Header("Option Setting")]  // Will be erased in the future.
        [SerializeField] private Vector3 _gridSize = new Vector3(64, 64, 64);
        [SerializeField] private Vector3 _gridResolution = new Vector3(100, 100, 100);


        // Objects
        private SandPhysicsSolver _solver;
        private Mesh _mesh;

        // ComputeShader
        private Particle _particle;
        private Particle _objectParticle;
        private GraphicsBuffer _terrainBuffer;

        #region Mono
        private void Start()
        {
            // Init Particle Buffer
            _particle = Particle.SetAsTetrahedronParticle(
                ParticleStatus.GenerateSphere((int)_particleNum, _spornPos, 5));

            // Init Object Particle Buffer
            _objectParticle = Particle.SetAsSimpleParticle(
                ParticleStatus.GenerateFromMesh((int)_particleNum, _mesh));

            // Init Terrain Bufer
            var t = TerrainType.GenerateFromTerrain(_terrain);
            _terrainBuffer = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                t.Length,
                Marshal.SizeOf(typeof(TerrainType)));
            _terrainBuffer.SetData(t);


            _solver = new SandPhysicsSolver(_terrain);
            _solver.gravity = _gravity;
            _solver.maxTimestep = _maxTimestep;
            _solver.SetParticleCSParams(_particle);
            _solver.SetGridSearchCSParams(_spornPos, _gridSize, _gridResolution);

            _effect.SetGraphicsBuffer("ParticleBuffer", _particle.status);
            _effect.SetUInt("ParticleNum", (uint)_particle.status.count);
        }

        private void Update()
        {
            _solver.UpdateParticle(ref _particle, _terrainBuffer);
        }

        private void OnDestroy()
        {
            _particle.Release();
            _objectParticle.Release();
            _terrainBuffer.Release();
            _solver.Release();
        }
        #endregion
    }
}
