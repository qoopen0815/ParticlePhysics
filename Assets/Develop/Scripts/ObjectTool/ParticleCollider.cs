using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.SDF;
using UnityEditor;

using MarchingCubesProject;

[RequireComponent(typeof(MeshFilter))]
public class ParticleCollider : MonoBehaviour
{
    public float particleRadius = 0.1f;
    public int maxResolution = 64;
    public VisualEffect effect;

    Texture3D _sdf;
    GraphicsBuffer _buffer;

    Vector3 _boxCenter;
    Vector3 _boxSizeReference;
    Vector3 _actualBoxSize;

    public Texture3D MeshSdf { get => _sdf; private set => _sdf = value; }
    public GraphicsBuffer ObjectParticles { get => _buffer; private set => _buffer = value; }


    // Update is called once per frame
    void Start()
    {
        SetParticlesOnMeshSurface();
    }

    private void OnDestroy()
    {
        ObjectParticles.Release();
    }

    void SetParticlesOnMeshSurface()
    {
        var meshFilter = this.GetComponent<MeshFilter>();

        _boxCenter = meshFilter.mesh.bounds.center;
        _boxSizeReference = meshFilter.mesh.bounds.extents * 2.0f;
        var absolutePadding = GetAbsolutePadding();
        _boxSizeReference += absolutePadding;
        _actualBoxSize = SnapBoxToVoxels();
        _boxSizeReference = _actualBoxSize;

        var baker = new MeshToSDFBaker(_boxSizeReference, _boxCenter, maxResolution, meshFilter.mesh);
        baker.BakeSDF();

        MeshSdf = RenderTextureUtils.ConvertToTexture3D(baker.SdfTexture);
        baker.Dispose();

        Marching marching = new MarchingCubes
        {
            Surface = 0.005f
        };

        var voxels = new VoxelArray(MeshSdf.width, MeshSdf.height, MeshSdf.depth);

        //Fill voxels with values. Im using perlin noise but any method to create voxels will work.
        for (int x = 0; x < MeshSdf.width; x++)
            for (int y = 0; y < MeshSdf.height; y++)
                for (int z = 0; z < MeshSdf.depth; z++)
                {
                    voxels[x, y, z] = MeshSdf.GetPixel(x, y, z).r;
                }

        List<Vector3> verts = new();
        List<int> indices = new();

        //The mesh produced is not optimal. There is one vert for each index.
        //Would need to weld vertices for better quality mesh.
        marching.Generate(voxels.Voxels, verts, indices);

        var ratio = new Vector3(
            _boxSizeReference.x / MeshSdf.width,
            _boxSizeReference.y / MeshSdf.height,
            _boxSizeReference.z / MeshSdf.depth);
        var move = _boxSizeReference * 0.5f - _boxCenter;
        verts = verts.Select(data => Vector3.Scale(data, ratio) - move).ToList();

        ObjectParticles = new GraphicsBuffer(
            GraphicsBuffer.Target.Structured,
            verts.Count,
            Marshal.SizeOf(typeof(Vector3)));
        ObjectParticles.SetData(verts);

        effect.SetGraphicsBuffer("ParticleBuffer", ObjectParticles);
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
