using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.VFX;

public enum RenderType
{
    SandLike = 0,
    Velocity = 1,
    Debug = 2
};

public class ParticlePhysicsExample : MonoBehaviour
{
    [Header("Particle Setting")]
    [SerializeField] private ParticlePhysics.Particle.Enum.ParticleNum _maxParticle = ParticlePhysics.Particle.Enum.ParticleNum.NUM_8K;
    [SerializeField] private ParticlePhysics.Particle.Enum.ParticleType _particleType = ParticlePhysics.Particle.Enum.ParticleType.Tetrahedron;
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
    private ParticlePhysics.Solver.SandPhysicsSolver _solver;
    private Mesh _mesh;

    // ComputeShader
    private ParticlePhysics.Particle.Data _particle;
    private ParticlePhysics.Particle.Data _objectParticle;
    private GraphicsBuffer _terrainBuffer;

    private void Start()
    {
        // Init Particle Buffer
        _particle = ParticlePhysics.Particle.Data.SetAsTetrahedronParticle(
            ParticlePhysics.Particle.State.GenerateSphere((int)_maxParticle, _spornPos, 5),
            radius: _particleRadius);

        // Init Object Particle Buffer
        _objectParticle = ParticlePhysics.Particle.Data.SetAsSimpleParticle(
            ParticlePhysics.Particle.State.GenerateFromMesh((int)_maxParticle, _mesh));

        // Init Terrain Bufer
        var t = ParticlePhysics.Utils.TerrainType.GenerateFromTerrain(_terrain);
        _terrainBuffer = new GraphicsBuffer(
            GraphicsBuffer.Target.Structured,
            t.Length,
            Marshal.SizeOf(typeof(ParticlePhysics.Utils.TerrainType)));
        _terrainBuffer.SetData(t);


        _solver = new ParticlePhysics.Solver.SandPhysicsSolver(
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
