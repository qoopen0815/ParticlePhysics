using UnityEngine;
using UnityEngine.VFX;

using ParticlePhysics;
using ParticlePhysics.Enum;

public class GenerateParticleTest : MonoBehaviour
{
    public ParticleNum particleNum = ParticleNum.NUM_32K;
    public VisualEffect grobalParticleEffect;
    public VisualEffect objectParticleEffect;
    public GameObject obj;

    ParticleBuffer _globalParticle;
    ParticleBuffer _objectParticle;

    // Start is called before the first frame update
    void Start()
    {
        _globalParticle = ParticleBuffer.SetAsTetrahedronParticle(
            particles: ParticleState.GenerateCube((int)particleNum, Vector3.one * 5.0f, 5.0f * 2),
            radius: 0.1f);
        _objectParticle = ParticleBuffer.SetAsTetrahedronParticle(
            particles: ParticleState.GenerateFromMesh(obj.GetComponent<MeshFilter>().mesh),
            radius: 0.1f);

        // Setup VFX Graph(for Grobal Particle)
        grobalParticleEffect.SetUInt("ParticleNum", (uint)_globalParticle.status.count);
        grobalParticleEffect.SetFloat("ParticleSize", 0.1f);
        grobalParticleEffect.SetGraphicsBuffer("ParticleBuffer", _globalParticle.status);
        // Setup VFX Graph(for Object Particle)
        objectParticleEffect.SetUInt("ParticleNum", (uint)_objectParticle.status.count);
        objectParticleEffect.SetFloat("ParticleSize", 0.1f);
        objectParticleEffect.SetGraphicsBuffer("ParticleBuffer", _objectParticle.status);
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnDestroy()
    {
        _globalParticle.Release();
        _objectParticle.Release();
    }
}
