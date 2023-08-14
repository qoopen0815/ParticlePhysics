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

    private float _particleRadius = 0.1f;

    private ObjectBuffer _objectBuffer;

    // Start is called before the first frame update
    void Start()
    {
        _objectBuffer = new ObjectBuffer(targetObject, (int)particleNum);

        // Setup VFX Graph
        effect.SetUInt("ParticleNum", (uint)_objectBuffer.ObjectParticle.num);
        effect.SetFloat("ParticleSize", _particleRadius);
        effect.SetGraphicsBuffer("ParticleBuffer", _objectBuffer.ObjectParticle.status);
    }

    // Update is called once per frame
    void Update()
    {
        var index = BufferUtils.GetData<Uint2>(_objectBuffer.ObjectParticleGridIndicesBuffer)[highlightCellId];
        effect.SetUInt("FirstIndex", index.x);
        effect.SetUInt("LastIndex", index.y);
    }

    private void OnDestroy()
    {
        _objectBuffer.Release();
    }
}
