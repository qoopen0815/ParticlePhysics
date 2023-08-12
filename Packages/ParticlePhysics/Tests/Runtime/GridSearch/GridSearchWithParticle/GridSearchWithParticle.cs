using ParticlePhysics;
using ParticlePhysics.Enum;
using ParticlePhysics.Utils;
using System;
using UnityEngine;
using UnityEngine.VFX;

public class GridSearchWithParticle : MonoBehaviour
{
    [Range(0, 999)]
    public int highlightCellId = 0;
    public ParticleNum _particleNum = ParticleNum.NUM_128K;
    public VisualEffect effect;

    private float _particleRadius = 0.1f;

    private float _particleSphereRadius = 5.0f;
    private float _gridSize = 10.0f;
    private float _gridCellSize = 1.0f;

    private ParticleBuffer _particleBuffer;
    private GridSearch<ParticleState> _gridSearch;

    // Start is called before the first frame update
    void Start()
    {
        _particleBuffer = ParticleBuffer.SetAsTetrahedronParticle(
                            //particles: ParticleState.GenerateCube((int)_particleNum, Vector3.one * _particleSphereRadius, _particleSphereRadius * 2),
                            particles: ParticleState.GenerateCube((int)1024*8, Vector3.one * _particleSphereRadius, _particleSphereRadius * 2),
                            radius: _particleRadius);
        _gridSearch = new GridSearch<ParticleState>(_particleBuffer.num, new Vector3(_gridSize, _gridSize, _gridSize), _gridCellSize);

        // Setup VFX Graph
        effect.SetUInt("ParticleNum", (uint)_particleBuffer.status.count);
        effect.SetFloat("ParticleSize", _particleRadius);
        effect.SetGraphicsBuffer("ParticleBuffer", _particleBuffer.status);
    }

    // Update is called once per frame
    void Update()
    {
        _gridSearch.GridSort(ref _particleBuffer.status, this.transform);
        var index = _gridSearch.GetCellIndices((uint)highlightCellId);
        effect.SetUInt("FirstIndex", index.x);
        effect.SetUInt("LastIndex", index.y);
    }

    private void OnDestroy()
    {
        _particleBuffer.Release();
        _gridSearch.Release();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(Vector3.one * _particleSphereRadius, Vector3.one * _gridSize);
        Gizmos.DrawWireCube(
            new Vector3(
                x: (int)((highlightCellId % (_gridSize))),
                y: (int)((highlightCellId % (_gridSize * _gridSize)) / _gridSize),
                z: (int)((highlightCellId % (_gridSize * _gridSize * _gridSize)) / (_gridSize * _gridSize))
                ) + Vector3.one * 0.5f,
                Vector3.one * _gridCellSize
            );
    }
}
