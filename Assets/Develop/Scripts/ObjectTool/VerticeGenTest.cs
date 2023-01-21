using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.SDF;
using UnityEditor;

using MarchingCubesProject;

public class VerticeGenTest : MonoBehaviour
{
    public float particleRadius;
    public VisualEffect effect;
    public int maxResolution = 32;

    MeshFilter _meshFilter;
    MeshToSDFBaker _baker;
    Texture3D _sdf;

    GraphicsBuffer _buffer;

    Vector3 _boxCenter;
    Vector3 _boxSizeReference;
    Vector3 _actualBoxSize;

    // Update is called once per frame
    void Start()
    {
        Hoge();
    }

    private void OnDestroy()
    {
        _baker.Dispose();
        _buffer.Release();
    }

    void Hoge()
    {
        _meshFilter = this.GetComponent<MeshFilter>();

        _boxCenter = _meshFilter.mesh.bounds.center;
        _boxSizeReference = _meshFilter.mesh.bounds.extents * 2.0f;
        var absolutePadding = GetAbsolutePadding();
        _boxSizeReference += absolutePadding;
        _actualBoxSize = SnapBoxToVoxels();
        _boxSizeReference = _actualBoxSize;

        _baker = new MeshToSDFBaker(_boxSizeReference, _boxCenter, maxResolution, _meshFilter.mesh);
        _baker.BakeSDF();
        
        _sdf = RenderTextureUtils.ConvertToTexture3D(_baker.SdfTexture);

        Debug.Log("Generate mesh: " + _meshFilter.mesh.name);
        Debug.Log("bounding box size: \t" + _boxSizeReference);
        Debug.Log("bounding box center: \t" + _boxCenter);
        Debug.Log("sdf size:" + "\tw: " + _sdf.width + "\th: " + _sdf.height + "\td: " + _sdf.depth);

        Marching marching = new MarchingCubes();
        marching.Surface = 0.005f;

        var voxels = new VoxelArray(_sdf.width, _sdf.height, _sdf.depth);

        //Fill voxels with values. Im using perlin noise but any method to create voxels will work.
        for (int x = 0; x < _sdf.width; x++)
            for (int y = 0; y < _sdf.height; y++)
                for (int z = 0; z < _sdf.depth; z++)
                {
                    voxels[x, y, z] = _sdf.GetPixel(x, y, z).r;
                }

        List<Vector3> verts = new List<Vector3>();
        List<int> indices = new List<int>();

        //The mesh produced is not optimal. There is one vert for each index.
        //Would need to weld vertices for better quality mesh.
        marching.Generate(voxels.Voxels, verts, indices);

        var ratio = new Vector3(
            _boxSizeReference.x / _sdf.width,
            _boxSizeReference.y / _sdf.height,
            _boxSizeReference.z / _sdf.depth);
        var move = _boxSizeReference * 0.5f - _boxCenter;
        verts = verts.Select(data => Vector3.Scale(data, ratio) - move).ToList();

        _buffer = new GraphicsBuffer(
            GraphicsBuffer.Target.Structured,
            verts.Count,
            Marshal.SizeOf(typeof(Vector3)));
        _buffer.SetData(verts);

        effect.SetGraphicsBuffer("ParticleBuffer", _buffer);
        effect.SetUInt("ParticleNum", (uint)verts.Count);
        effect.SetFloat("ParticleSize", particleRadius / 2);
    }

    private Vector3 GetAbsolutePadding()
    {
        float maxExtent = Mathf.Max(_boxSizeReference.x, Mathf.Max(_boxSizeReference.y, _boxSizeReference.z));
        float voxelSize = maxExtent / maxResolution;
        Vector3 absolutePadding = 2 * voxelSize * Vector3.one;
        return absolutePadding;
    }

    private Vector3 SnapBoxToVoxels(int refAxis = 0)
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
            dimX = Mathf.Max(Mathf.RoundToInt(maxResolution * _boxSizeReference.x / maxExtent), 1);
            dimY = Mathf.Max(Mathf.CeilToInt(maxResolution * _boxSizeReference.y / maxExtent), 1);
            dimZ = Mathf.Max(Mathf.CeilToInt(maxResolution * _boxSizeReference.z / maxExtent), 1);
            float voxelSize = _boxSizeReference.x / dimX;
            var tmpBoxSize = _boxSizeReference;
            tmpBoxSize.x = dimX * voxelSize;
            tmpBoxSize.y = dimY * voxelSize;
            tmpBoxSize.z = dimZ * voxelSize;
            return tmpBoxSize;
        }
        else if (refAxis == 2)
        {
            dimY = Mathf.Max(Mathf.RoundToInt(maxResolution * _boxSizeReference.y / maxExtent), 1);
            dimX = Mathf.Max(Mathf.CeilToInt(maxResolution * _boxSizeReference.x / maxExtent), 1);
            dimZ = Mathf.Max(Mathf.CeilToInt(maxResolution * _boxSizeReference.z / maxExtent), 1);
            float voxelSize = _boxSizeReference.y / dimY;
            var tmpBoxSize = _boxSizeReference;
            tmpBoxSize.x = dimX * voxelSize;
            tmpBoxSize.y = dimY * voxelSize;
            tmpBoxSize.z = dimZ * voxelSize;
            return tmpBoxSize;
        }
        else
        {
            dimZ = Mathf.Max(Mathf.RoundToInt(maxResolution * _boxSizeReference.z / maxExtent), 1);
            dimY = Mathf.Max(Mathf.CeilToInt(maxResolution * _boxSizeReference.y / maxExtent), 1);
            dimX = Mathf.Max(Mathf.CeilToInt(maxResolution * _boxSizeReference.x / maxExtent), 1);
            float voxelSize = _boxSizeReference.z / dimZ;
            var tmpBoxSize = _boxSizeReference;
            tmpBoxSize.x = dimX * voxelSize;
            tmpBoxSize.y = dimY * voxelSize;
            tmpBoxSize.z = dimZ * voxelSize;
            return tmpBoxSize;
        }
    }
}
