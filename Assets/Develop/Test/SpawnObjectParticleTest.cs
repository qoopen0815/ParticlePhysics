using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.VFX;

using ParticlePhysics;
using ParticlePhysics.Type;

public class SpawnObjectParticleTest : MonoBehaviour
{
    public VisualEffect effect;
    public GameObject obj;

    public GranularParticle p;

    // Start is called before the first frame update
    void Start()
    {
        p = GranularParticle.SetAsTetrahedronParticle(ParticleState.GenerateFromGameObject(obj));

        Debug.Log(p.state.count);

        effect.SetUInt("ParticleNum", (uint)p.state.count);
        effect.SetFloat("ParticleSize", 0.1f);
        effect.SetGraphicsBuffer("ParticleBuffer", p.state);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        p.Release();
    }
}
