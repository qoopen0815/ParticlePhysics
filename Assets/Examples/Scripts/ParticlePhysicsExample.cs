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
    [SerializeField] private ParticlePhysics.Enum.ParticleNum _maxParticle = ParticlePhysics.Enum.ParticleNum.NUM_8K;
    //[SerializeField] private ParticlePhysics.Enum.ParticleType _particleType = ParticlePhysics.Enum.ParticleType.Tetrahedron;
    [SerializeField] private RenderType _renderType = RenderType.SandLike;
    [SerializeField, Range(0.04f, 0.2f)] private float _particleRadius = 0.1f;
    [SerializeField] private Vector3 _spornPos = Vector3.one;     // When a debugging component is available, this variable will be moved there.
    [SerializeField] private VisualEffect _effect;

    [Header("Collision Objects")]
    [SerializeField] private Terrain _terrain;
    [SerializeField] private GameObject[] _objects;

    // ComputeShader
    private ParticleBuffer _particleBuffer;
    private TerrainBuffer _terrainBuffer;
    private SandPhysicsSolver _solver;

    private void Start()
    {
        // Init Buffer
        _particleBuffer = ParticleBuffer.SetAsTetrahedronParticle(
            particles: ParticleState.GenerateSphere((int)_maxParticle, _spornPos, 5),
            radius: _particleRadius);
        _terrainBuffer = new TerrainBuffer(_terrain);

        // Setup Solver
        _solver = new SandPhysicsSolver(Physics.gravity);
        _solver.SetMainParticle(_particleBuffer);
        _solver.SetCollisionObjects(_objects);
        _solver.SetFieldTerrain(_terrain, _spornPos);

        // Setup VFX Graph
        _effect.SetGraphicsBuffer("debugBuffer", _solver._debugger);
        _effect.SetGraphicsBuffer("ParticleBuffer", _particleBuffer.status);
        _effect.SetUInt("ParticleNum", (uint)_particleBuffer.status.count);
        _effect.SetFloat("ParticleSize", _particleRadius);
        _effect.SetInt("RenderType", (int)_renderType);
    }

    private void Update()
    {
        _solver.UpdateParticle(ref _particleBuffer, _terrainBuffer.buffer);
    }

    private void OnDestroy()
    {
        _particleBuffer.Release();
        _terrainBuffer.Release();
        _solver.Release();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(_spornPos, 5);
        Gizmos.DrawWireCube(_spornPos, SandPhysicsSolver.gridSize);
    }
}
