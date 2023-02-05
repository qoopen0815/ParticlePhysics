using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.VFX.SDF;
using UnityEditor;

using MarchingCubesProject;

[RequireComponent(typeof(MeshFilter))]
public class ParticleCollider : MonoBehaviour
{
    public float particleRadius = 0.1f;
    public int maxResolution = 64;

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
        MeshFilter mf = this.GetComponent<MeshFilter>();
        SetParticlesOnMeshSurface(mf.mesh);
    }

    private void OnDestroy()
    {
        ObjectParticles.Release();
    }

    public static Texture3D GetMeshSDF(Mesh mesh, int resolution)
    {
        Vector3 boxSize = mesh.bounds.extents * 2.0f;
        boxSize += GetAbsolutePadding(boxSize, resolution);

        var baker = new MeshToSDFBaker(
            sizeBox: SnapBoxToVoxels(boxSize, resolution),
            center: mesh.bounds.center,
            maxRes: resolution,
            mesh: mesh
            );

        baker.BakeSDF();
        RenderTexture sdf = baker.SdfTexture;

        baker.Dispose();
        return RenderTextureUtils.ConvertToTexture3D(sdf);
    }

    public static List<Vector3> GetVertsOnMeshSurface(Mesh mesh, int resolution)
    {
        Vector3 boxSize = mesh.bounds.extents * 2.0f;
        boxSize += GetAbsolutePadding(boxSize, resolution);
        boxSize = SnapBoxToVoxels(boxSize, resolution);

        var baker = new MeshToSDFBaker(
            sizeBox: boxSize,
            center: mesh.bounds.center,
            maxRes: resolution,
            mesh: mesh
            );

        baker.BakeSDF();
        var sdf = RenderTextureUtils.ConvertToTexture3D(baker.SdfTexture);

        baker.Dispose();

        Marching marching = new MarchingCubes { Surface = 0.005f };
        VoxelArray voxels = new VoxelArray(sdf.width, sdf.height, sdf.depth);

        //Fill voxels with values. Im using perlin noise but any method to create voxels will work.
        for (int x = 0; x < sdf.width; x++)
            for (int y = 0; y < sdf.height; y++)
                for (int z = 0; z < sdf.depth; z++)
                {
                    voxels[x, y, z] = sdf.GetPixel(x, y, z).r;
                }

        List<Vector3> verts = new();
        List<int> indices = new();

        //The mesh produced is not optimal. There is one vert for each index.
        //Would need to weld vertices for better quality mesh.
        marching.Generate(voxels.Voxels, verts, indices);

        var ratio = new Vector3(
            boxSize.x / sdf.width,
            boxSize.y / sdf.height,
            boxSize.z / sdf.depth);
        var move = boxSize * 0.5f - mesh.bounds.center;
        verts = verts.Select(data => Vector3.Scale(data, ratio) - move).ToList();

        return verts;
    }

    void SetParticlesOnMeshSurface(Mesh mesh)
    {
        _boxCenter = mesh.bounds.center;
        _boxSizeReference = mesh.bounds.extents * 2.0f;
        var absolutePadding = GetAbsolutePadding();
        _boxSizeReference += absolutePadding;
        _actualBoxSize = SnapBoxToVoxels();
        _boxSizeReference = _actualBoxSize;

        var baker = new MeshToSDFBaker(_boxSizeReference, _boxCenter, maxResolution, mesh);
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
    }

    private Vector3 GetAbsolutePadding()
    {
        float maxExtent = Mathf.Max(_boxSizeReference.x, Mathf.Max(_boxSizeReference.y, _boxSizeReference.z));
        float voxelSize = maxExtent / maxResolution;
        Vector3 absolutePadding = 2 * voxelSize * Vector3.one;
        return absolutePadding;
    }

    internal static Vector3 GetAbsolutePadding(Vector3 boxSize, int maxResolution)
    {
        float maxExtent = Mathf.Max(boxSize.x, Mathf.Max(boxSize.y, boxSize.z));
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

    internal static Vector3 SnapBoxToVoxels(Vector3 boxSize, int resolution, int refAxis = 0)
    {
        float maxExtent = Mathf.Max(boxSize.x, Mathf.Max(boxSize.y, boxSize.z));
        int dimX, dimY, dimZ;

        if (refAxis == 0 || refAxis > 3) // Default behavior, choose largest dimension
        {
            if (maxExtent == boxSize.x)
            {
                refAxis = 1;
            }

            if (maxExtent == boxSize.y)
            {
                refAxis = 2;
            }

            if (maxExtent == boxSize.z)
            {
                refAxis = 3;
            }
        }

        if (refAxis == 1)
        {
            dimX = Mathf.Max(Mathf.RoundToInt(resolution * boxSize.x / maxExtent), 1);
            dimY = Mathf.Max(Mathf.CeilToInt(resolution * boxSize.y / maxExtent), 1);
            dimZ = Mathf.Max(Mathf.CeilToInt(resolution * boxSize.z / maxExtent), 1);
            float voxelSize = boxSize.x / dimX;
            var tmpBoxSize = boxSize;
            tmpBoxSize.x = dimX * voxelSize;
            tmpBoxSize.y = dimY * voxelSize;
            tmpBoxSize.z = dimZ * voxelSize;
            return tmpBoxSize;
        }
        else if (refAxis == 2)
        {
            dimY = Mathf.Max(Mathf.RoundToInt(resolution * boxSize.y / maxExtent), 1);
            dimX = Mathf.Max(Mathf.CeilToInt(resolution * boxSize.x / maxExtent), 1);
            dimZ = Mathf.Max(Mathf.CeilToInt(resolution * boxSize.z / maxExtent), 1);
            float voxelSize = boxSize.y / dimY;
            var tmpBoxSize = boxSize;
            tmpBoxSize.x = dimX * voxelSize;
            tmpBoxSize.y = dimY * voxelSize;
            tmpBoxSize.z = dimZ * voxelSize;
            return tmpBoxSize;
        }
        else
        {
            dimZ = Mathf.Max(Mathf.RoundToInt(resolution * boxSize.z / maxExtent), 1);
            dimY = Mathf.Max(Mathf.CeilToInt(resolution * boxSize.y / maxExtent), 1);
            dimX = Mathf.Max(Mathf.CeilToInt(resolution * boxSize.x / maxExtent), 1);
            float voxelSize = boxSize.z / dimZ;
            var tmpBoxSize = boxSize;
            tmpBoxSize.x = dimX * voxelSize;
            tmpBoxSize.y = dimY * voxelSize;
            tmpBoxSize.z = dimZ * voxelSize;
            return tmpBoxSize;
        }
    }
}
