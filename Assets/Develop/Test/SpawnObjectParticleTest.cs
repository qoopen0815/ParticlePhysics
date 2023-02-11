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

    public GranularParticle particle;

    ComputeShader _shader;
    GraphicsBuffer _buffer;
    GraphicsBuffer _debug;
    Matrix4x4 _objectTF = Matrix4x4.identity;

    // Start is called before the first frame update
    void Start()
    {
        particle = GranularParticle.SetAsTetrahedronParticle(ParticleState.GenerateFromMesh(obj.GetComponent<MeshFilter>().mesh));

        _shader = (ComputeShader)Resources.Load("Test");
        _buffer = new GraphicsBuffer(
            GraphicsBuffer.Target.Structured,
            particle.num,
            Marshal.SizeOf(typeof(ParticleState)));
        _debug = new GraphicsBuffer(
            GraphicsBuffer.Target.Structured,
            particle.num,
            Marshal.SizeOf(typeof(Vector4)));

        effect.SetUInt("ParticleNum", (uint)particle.num);
        effect.SetFloat("ParticleSize", 0.1f);
        effect.SetGraphicsBuffer("ParticleBuffer", _buffer);
    }

    // Update is called once per frame
    void Update()
    {
        _objectTF.SetTRS(obj.transform.position, obj.transform.rotation, obj.transform.localScale);

        int kernelID = _shader.FindKernel("InitCS");
        //_shader.SetBuffer(kernelID, "_debug", _debug);
        _shader.GetKernelThreadGroupSizes(kernelID, out var x, out var _, out var _);
        _shader.Dispatch(kernelID, (int)(particle.num / x), 1, 1);

        kernelID = _shader.FindKernel("MainCS");
        _shader.SetMatrix("_ObjectTF", _objectTF);
        _shader.SetBuffer(kernelID, "_bufferRead", particle.state);
        _shader.SetBuffer(kernelID, "_bufferWrite", _buffer);
        _shader.SetBuffer(kernelID, "_debug", _debug);
        _shader.GetKernelThreadGroupSizes(kernelID, out x, out var _, out var _);
        _shader.Dispatch(kernelID, (int)(particle.num / x), 1, 1);

        BufferUtils.DebugBuffer<Vector4>(_debug, 10);
    }

    private void OnDestroy()
    {
        particle.Release();
        _buffer.Release();
        _debug.Release();
    }
}
