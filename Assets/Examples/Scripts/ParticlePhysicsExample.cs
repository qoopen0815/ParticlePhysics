using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.VFX;

using ParticlePhysics;
using ParticlePhysics.Solver;

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
    [SerializeField] private GameObject[] _objects;

    [Header("Option Setting")]  // Will be erased in the future.
    [SerializeField] private Vector3 _gridSize = new(64, 64, 64);
    [SerializeField] private float _gridCellSize = 0.5f;


    // ComputeShader
    private ParticleBuffer _particle;
    private GraphicsBuffer _terrainBuffer;
    private SandPhysicsSolver _solver;

    private void Start()
    {
        // Init Particle Buffer
        _particle = ParticleBuffer.SetAsTetrahedronParticle(
            ParticleState.GenerateSphere((int)_maxParticle, _spornPos, 5),
            radius: _particleRadius);

        // Init Terrain Bufer
        var t = ParticlePhysics.Utils.TerrainType.GenerateFromTerrain(_terrain);
        _terrainBuffer = new GraphicsBuffer(
            GraphicsBuffer.Target.Structured,
            t.Length,
            Marshal.SizeOf(typeof(ParticlePhysics.Utils.TerrainType)));
        _terrainBuffer.SetData(t);

        _solver = new SandPhysicsSolver(Physics.gravity);
        _solver.SetMainParticle(_particle);
        _solver.SetCollisionObjects(_objects, _gridSize, _gridCellSize, _spornPos);
        _solver.SetFieldTerrain(_terrain, _gridSize, _gridCellSize, _spornPos);

        _effect.SetGraphicsBuffer("debugBuffer", _solver._debugger);
        _effect.SetGraphicsBuffer("ParticleBuffer", _particle.status);
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
