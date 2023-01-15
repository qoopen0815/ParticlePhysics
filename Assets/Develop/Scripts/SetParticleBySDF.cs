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

    ComputeShader _shader;
    GraphicsBuffer _resultBuffer;
    GraphicsBuffer _debugBuffer;

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
    void Start()
    {
        _meshFilter = this.GetComponent<MeshFilter>();

        _boxCenter = _meshFilter.mesh.bounds.center;
        _boxSizeReference = _meshFilter.mesh.bounds.extents * 2.0f;
        var absolutePadding = GetAbsolutePadding();
        _boxSizeReference += absolutePadding;
        _actualBoxSize = SnapBoxToVoxels();
        _boxSizeReference = _actualBoxSize;

        var baker = new MeshToSDFBaker(_boxSizeReference, _boxCenter, _maxResolution, _meshFilter.mesh);
        baker.BakeSDF();
        var sdf = baker.SdfTexture;

        Debug.Log("bounding box size: \t" + _boxSizeReference);
        Debug.Log("sdf size:" + "\tw: " + sdf.height + "\th: " + sdf.height + "\td: " + sdf.volumeDepth);

        //Vector3Int resolution = new Vector3Int(
        //    x: (int)(sdf.width  / particleRadius),
        //    y: (int)(sdf.height / particleRadius),
        //    z: (int)(sdf.depth  / particleRadius));

        Vector3Int resolution = new Vector3Int(
            x: sdf.width,
            y: sdf.height,
            z: sdf.volumeDepth);
        Debug.Log("Resolution: \t" + resolution);

        _shader = (ComputeShader)Resources.Load("SDF");
        _resultBuffer = new GraphicsBuffer(
            GraphicsBuffer.Target.Structured,
            resolution.x * resolution.y * resolution.z,
            Marshal.SizeOf(typeof(Vector3)));
        _debugBuffer = new GraphicsBuffer(
            GraphicsBuffer.Target.Structured,
            resolution.x * resolution.y * resolution.z,
            Marshal.SizeOf(typeof(Vector4)));

        effect.SetGraphicsBuffer("ParticleBuffer", _resultBuffer);
        effect.SetUInt("ParticleNum", (uint)(resolution.x * resolution.y * resolution.z));
        effect.SetFloat("ParticleSize", particleRadius);

        int kernelID = _shader.FindKernel("CSMain");

        _shader.SetVector("boxSize", new(_boxSizeReference.x, _boxSizeReference.y, _boxSizeReference.z, 0));
        _shader.SetVector("resolution", new(resolution.x, resolution.y, resolution.z, 0));
        _shader.SetFloat("particleRadius", particleRadius);
        _shader.SetVector("ratio", new(sdf.width / resolution.x, sdf.height / resolution.y, sdf.volumeDepth / resolution.z, 0));

        _shader.SetTexture(kernelID, "sdf", sdf);
        _shader.SetBuffer(kernelID, "result", _resultBuffer);
        _shader.SetBuffer(kernelID, "debug", _debugBuffer);
        _shader.Dispatch(kernelID, resolution.x, resolution.y, resolution.z);

        BufferUtils.DebugBuffer<Vector4>(_debugBuffer, resolution.x * resolution.y * resolution.z, 10);

        baker.Dispose();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        _resultBuffer.Release();
        _debugBuffer.Release();
    }
}
