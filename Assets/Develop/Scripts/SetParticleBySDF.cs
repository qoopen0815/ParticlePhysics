using System.Collections;
using System.Linq;
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
    MeshToSDFBaker _baker;
    RenderTexture _sdf;

    ComputeShader _shader;
    GraphicsBuffer _buffer;

    Vector3 _boxCenter;
    Vector3 _boxSizeReference;
    Vector3 _actualBoxSize;
    int _maxResolution = 64;

    private Vector3 GetAbsolutePadding()
    {
        float maxExtent = Mathf.Max(_boxSizeReference.x, Mathf.Max(_boxSizeReference.y, _boxSizeReference.z));
        float voxelSize = maxExtent / _maxResolution;
        Vector3 absolutePadding = 2 * voxelSize * Vector3.one;
        return absolutePadding;
    }

    Vector3 SnapBoxToVoxels(int refAxis = 0)
    {
        float maxExtent = Mathf.Max(_boxSizeReference.x, Mathf.Max(_boxSizeReference.y, _boxSizeReference.z));
        int dimX, dimY, dimZ;

        if (refAxis == 0 || refAxis > 3) // Default behavior, choose largest dimension
        {
            if (maxExtent == _boxSizeReference.x)
            {
                refAxis = 1;
            }

            if (maxExtent == _boxSizeReference.y)
            {
                refAxis = 2;
            }

            if (maxExtent == _boxSizeReference.z)
            {
                refAxis = 3;
            }
        }

        if (refAxis == 1)
        {
            dimX = Mathf.Max(Mathf.RoundToInt(_maxResolution * _boxSizeReference.x / maxExtent), 1);
            dimY = Mathf.Max(Mathf.CeilToInt(_maxResolution * _boxSizeReference.y / maxExtent), 1);
            dimZ = Mathf.Max(Mathf.CeilToInt(_maxResolution * _boxSizeReference.z / maxExtent), 1);
            float voxelSize = _boxSizeReference.x / dimX;
            var tmpBoxSize = _boxSizeReference;
            tmpBoxSize.x = dimX * voxelSize;
            tmpBoxSize.y = dimY * voxelSize;
            tmpBoxSize.z = dimZ * voxelSize;
            return tmpBoxSize;
        }
        else if (refAxis == 2)
        {
            dimY = Mathf.Max(Mathf.RoundToInt(_maxResolution * _boxSizeReference.y / maxExtent), 1);
            dimX = Mathf.Max(Mathf.CeilToInt(_maxResolution * _boxSizeReference.x / maxExtent), 1);
            dimZ = Mathf.Max(Mathf.CeilToInt(_maxResolution * _boxSizeReference.z / maxExtent), 1);
            float voxelSize = _boxSizeReference.y / dimY;
            var tmpBoxSize = _boxSizeReference;
            tmpBoxSize.x = dimX * voxelSize;
            tmpBoxSize.y = dimY * voxelSize;
            tmpBoxSize.z = dimZ * voxelSize;
            return tmpBoxSize;
        }
        else
        {
            dimZ = Mathf.Max(Mathf.RoundToInt(_maxResolution * _boxSizeReference.z / maxExtent), 1);
            dimY = Mathf.Max(Mathf.CeilToInt(_maxResolution * _boxSizeReference.y / maxExtent), 1);
            dimX = Mathf.Max(Mathf.CeilToInt(_maxResolution * _boxSizeReference.x / maxExtent), 1);
            float voxelSize = _boxSizeReference.z / dimZ;
            var tmpBoxSize = _boxSizeReference;
            tmpBoxSize.x = dimX * voxelSize;
            tmpBoxSize.y = dimY * voxelSize;
            tmpBoxSize.z = dimZ * voxelSize;
            return tmpBoxSize;
        }
    }

    // Start is called before the first frame update
    void Hoge()
    {
        _meshFilter = this.GetComponent<MeshFilter>();

        _boxCenter = _meshFilter.mesh.bounds.center;
        _boxSizeReference = _meshFilter.mesh.bounds.extents * 2.0f;
        var absolutePadding = GetAbsolutePadding();
        _boxSizeReference += absolutePadding;
        _actualBoxSize = SnapBoxToVoxels();
        _boxSizeReference = _actualBoxSize;

        _baker = new MeshToSDFBaker(_boxSizeReference, _boxCenter, _maxResolution, _meshFilter.mesh);
        _baker.BakeSDF();
        _sdf = _baker.SdfTexture;

        Debug.Log("bounding box size: \t" + _boxSizeReference);
        Debug.Log("sdf size:" + "\tw: " + _sdf.width + "\th: " + _sdf.height + "\td: " + _sdf.volumeDepth);

        _shader = (ComputeShader)Resources.Load("SDF");
        int kernelID = _shader.FindKernel("SearchPixelCS");

        _shader.GetKernelThreadGroupSizes(kernelID, out var threadSizeX, out var threadSizeY, out var threadSizeZ);
        Vector3Int dispatchSize = Vector3Int.one * 10;
        Vector3Int resolution = new Vector3Int(
            x: (int)(dispatchSize.x * threadSizeX),
            y: (int)(dispatchSize.y * threadSizeY),
            z: (int)(dispatchSize.z * threadSizeZ));
        Debug.Log("Resolution: \t" + resolution);

        _buffer = new GraphicsBuffer(
            GraphicsBuffer.Target.Structured,
            resolution.x * resolution.y * resolution.z,
            Marshal.SizeOf(typeof(Vector4)));

        float[] threshold = new float[2] { 0, 0.000000001f };
        var bufferData = new Vector4[_buffer.count];
        var verts = new List<Vector4>();

        _shader.SetInts("resolution", new int[3] { resolution.x, resolution.y, resolution.z });
        _shader.SetTexture(kernelID, "tex3d", _sdf);
        _shader.SetBuffer(kernelID, "buffer", _buffer);

        for (int w = 0; w < _sdf.volumeDepth; w++)
        {
            for (int v = 0; v < 32; v++)
            {
                for (int u = 0; u <32; u++)
                {
                    _shader.SetInts("target_uvw", new int[3] { u, v, w });
                    _shader.Dispatch(kernelID, dispatchSize.x, dispatchSize.y, dispatchSize.z);
                    _buffer.GetData(bufferData);
                    
                    verts.AddRange(bufferData.OrderBy(data => data.w).Take(10000).Where(data => threshold[0] < data.w && data.w < threshold[1]));
                }
            }

            if (verts.Count > 10000)
            {
                verts = verts.OrderBy(data => data.w).Take(10000).ToList();
            }
        }

        var ratio = new Vector4(_boxSizeReference.x / _sdf.width, _boxSizeReference.y / _sdf.height, _boxSizeReference.z / _sdf.volumeDepth, 1);
        var move = new Vector4(_boxSizeReference.x / 2, _boxSizeReference.y / 2, _boxSizeReference.z / 2, 1);
        verts = verts.Select(data => Vector4.Scale(data, ratio) - move).ToList();

        _buffer = new GraphicsBuffer(
            GraphicsBuffer.Target.Structured,
            verts.Count,
            Marshal.SizeOf(typeof(Vector4)));
        _buffer.SetData(verts);

        effect.SetGraphicsBuffer("ParticleBuffer", _buffer);
        effect.SetUInt("ParticleNum", (uint)(resolution.x * resolution.y * resolution.z));
        effect.SetFloat("ParticleSize", particleRadius);
    }

    bool _frag = true;

    // Update is called once per frame
    void Update()
    {
        if(_frag)
        {
            Hoge();
            _frag = false;
        }
    }

    private void OnDestroy()
    {
        _baker.Dispose();
        _sdf.Release();
        _buffer.Release();
    }
}
