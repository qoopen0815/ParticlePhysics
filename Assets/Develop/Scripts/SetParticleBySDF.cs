using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.SDF;

public class SetParticleBySDF : MonoBehaviour
{
    public float particleRadius;
    public VisualEffect effect;

    MeshFilter _meshFilter;

    //ComputeShader _shader;
    //GraphicsBuffer _resultBuffer;
    //GraphicsBuffer _debugBuffer;

    // Start is called before the first frame update
    void Start()
    {
        _meshFilter = this.GetComponent<MeshFilter>();
        //Vector3 sizeBox = _mesh.mesh.bounds.size * 1.1f;
        Vector3 sizeBox = new(2.578125f, 2.578125f, 0.604248f);
        Vector3 center = Vector3.zero;
        var baker = new MeshToSDFBaker(sizeBox, center, maxRes: 64, _meshFilter.mesh);
        baker.BakeSDF();
        var sdf = baker.SdfTexture;

        Debug.Log("bounding box size: \t" + sizeBox);
        Debug.Log("actual box size: \t" + baker.GetActualBoxSize());
        Debug.Log("sdf size:" + "\tw: " + sdf.height + "\th: " + sdf.height + "\td: " + sdf.depth);
        baker.Dispose();

        ////Vector3Int resolution = new Vector3Int(
        ////    x: (int)(sdf.width  / particleRadius),
        ////    y: (int)(sdf.height / particleRadius),
        ////    z: (int)(sdf.depth  / particleRadius));
        //Vector3Int resolution = new Vector3Int(
        //    x: (int)(sdf.width),
        //    y: (int)(sdf.height),
        //    z: (int)(sdf.depth));
        //Debug.Log("Resolution: \t" + resolution);

        //_shader = (ComputeShader)Resources.Load("SDF");
        //_resultBuffer = new GraphicsBuffer(
        //    GraphicsBuffer.Target.Structured,
        //    resolution.x * resolution.y * resolution.z,
        //    Marshal.SizeOf(typeof(Vector3)));
        //_debugBuffer = new GraphicsBuffer(
        //    GraphicsBuffer.Target.Structured,
        //    resolution.x * resolution.y * resolution.z,
        //    Marshal.SizeOf(typeof(Vector4)));

        //effect.SetGraphicsBuffer("ParticleBuffer", _resultBuffer);
        //effect.SetUInt("ParticleNum", (uint)(resolution.x * resolution.y * resolution.z));
        //effect.SetFloat("ParticleSize", particleRadius);

        //int kernelID = _shader.FindKernel("CSMain");
        //_shader.SetFloat("radius", particleRadius);
        //_shader.SetVector("resolution", new(resolution.x, resolution.y, resolution.z, 0));
        //_shader.SetVector("ratio", new(sdf.width/resolution.x, sdf.height / resolution.y, sdf.depth / resolution.z, 0));
        //_shader.SetVector("meshSize", new(sizeBox.x, sizeBox.y, sizeBox.z, 0));
        //_shader.SetTexture(kernelID, "sdf", sdf);
        //_shader.SetBuffer(kernelID, "result", _resultBuffer);
        //_shader.SetBuffer(kernelID, "debug", _debugBuffer);
        //_shader.Dispatch(kernelID, resolution.x, resolution.y, resolution.z);

        //BufferUtils.DebugBuffer<Vector4>(_debugBuffer, resolution.x * resolution.y * resolution.z, 10);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        //_resultBuffer.Release();
        //_debugBuffer.Release();
    }
}
