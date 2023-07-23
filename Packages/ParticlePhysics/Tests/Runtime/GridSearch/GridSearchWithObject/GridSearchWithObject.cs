using ParticlePhysics;
using ParticlePhysics.Enum;
using ParticlePhysics.Utils;
using System;
using UnityEngine;
using UnityEngine.VFX;

public class GridSearchWithObject : MonoBehaviour
{
    [Range(0, 125)]
    public int highlightCellId = 0;
    public ParticleNum particleNum = ParticleNum.NUM_128K;
    public VisualEffect effect;
    public GameObject targetObject;

    private float _particleRadius = 0.01f;

    private Vector3 _gridCenter;
    private Vector3 _gridSize;
    private float _gridCellSize = 0.1f;

    private ObjectBuffer _objectBuffer;

    // Start is called before the first frame update
    void Start()
    {
        _objectBuffer = new ObjectBuffer(targetObject, (int)particleNum, _gridCellSize);
        _gridCenter = _objectBuffer.GridCenter;
        _gridSize = _objectBuffer.GridSize;
        _gridCellSize = _objectBuffer.GridCellSize;

        // Setup VFX Graph
        effect.SetUInt("ParticleNum", (uint)_objectBuffer.ObjectParticle.num);
        effect.SetFloat("ParticleSize", _particleRadius);
        effect.SetGraphicsBuffer("ParticleBuffer", _objectBuffer.ObjectParticle.status);
    }

    // Update is called once per frame
    void Update()
    {
        var index = BufferUtils.GetData<Uint2>(_objectBuffer.GetCollisionIndices())[highlightCellId];
        //Debug.Log("x: " + index.x + ",y: " + index.y);
        effect.SetUInt("FirstIndex", index.x);
        effect.SetUInt("LastIndex", index.y);
    }

    private void OnDestroy()
    {
        _objectBuffer.Release();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(_gridCenter, _gridSize);
        Gizmos.DrawWireCube(
            new Vector3(
                x: (int)((highlightCellId % (_gridSize.x))),
                y: (int)((highlightCellId % (_gridSize.x * _gridSize.y)) / _gridSize.x),
                z: (int)((highlightCellId % (_gridSize.x * _gridSize.y * _gridSize.z)) / (_gridSize.x * _gridSize.y))
                ) + Vector3.one * 0.5f,
                Vector3.one * _gridCellSize
            );
    }
}
