using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.VFX;

namespace ParticleSimulator
{
    public enum RenderType
    {
        SandLike = 0,
        Velocity = 1,
        Debug = 2
    };

    public class ParticlePhysics : MonoBehaviour
    {
        [Header("Particle Setting")]
        [SerializeField] private ParticleNumEnum _maxParticle = ParticleNumEnum.NUM_8K;
        [SerializeField] private ParticleTypeEnum _particleType = ParticleTypeEnum.Tetrahedron;
        [SerializeField] private RenderType _renderType = RenderType.SandLike;
        [SerializeField, Range(0.04f, 0.2f)] private float _particleRadius = 0.1f;
        [SerializeField] private Vector3 _spornPos = Vector3.one;     // When a debugging component is available, this variable will be moved there.
        [SerializeField] private VisualEffect _effect;

        [Header("Collision Objects")]
        [SerializeField] private Terrain _terrain;
        [SerializeField] private List<GameObject> _objects;
        
        [Header("Option Setting")]  // Will be erased in the future.
        [SerializeField] private Vector3 _gridSize = new(64, 64, 64);
        [SerializeField] private float _gridCellSize = 0.5f;


        // Objects
        private SandPhysicsSolver _solver;
        private Mesh _mesh;

        // ComputeShader
        private Particle _particle;
        private Particle _objectParticle;
        private GraphicsBuffer _terrainBuffer;

        private void Start()
        {
            // Init Particle Buffer
            _particle = Particle.SetAsTetrahedronParticle(
                ParticleStatus.GenerateSphere((int)_maxParticle, _spornPos, 5),
                radius: _particleRadius);

            // Init Object Particle Buffer
            _objectParticle = Particle.SetAsSimpleParticle(
                ParticleStatus.GenerateFromMesh((int)_maxParticle, _mesh));

            // Init Terrain Bufer
            var t = TerrainType.GenerateFromTerrain(_terrain);
            _terrainBuffer = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                t.Length,
                Marshal.SizeOf(typeof(TerrainType)));
            _terrainBuffer.SetData(t);


            _solver = new SandPhysicsSolver(
                particle: _particle,
                gridSize: _gridSize,
                gridCellSize: _gridCellSize,
                gridCenter: _spornPos,
                terrainResolution: _terrain.terrainData.heightmapResolution,
                terrainRatio: new Vector3(_terrain.terrainData.heightmapResolution / _terrain.terrainData.size.x,
                                    1 / _terrain.terrainData.size.y,
                                    _terrain.terrainData.heightmapResolution / _terrain.terrainData.size.z),
                terrainFriction: 0.955f);

            _effect.SetGraphicsBuffer("ParticleBuffer", _particle.status);
            _effect.SetGraphicsBuffer("debugBuffer", _solver._debugger);
            _effect.SetUInt("ParticleNum", (uint)_particle.status.count);
            _effect.SetFloat("ParticleSize", _particleRadius);
            _effect.SetInt("RenderType", (int)_renderType);
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

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_spornPos, 5);
            Gizmos.DrawWireCube(_spornPos, _gridSize);
        }
    }
}
