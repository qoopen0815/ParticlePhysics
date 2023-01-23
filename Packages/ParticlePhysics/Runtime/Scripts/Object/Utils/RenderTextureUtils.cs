using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RenderTextureUtils
{
    private int _width, _height, _depth;
    private RenderTexture _renderTexture;
    private Texture3D _texture3D;
    private ComputeShader _shader;

    public RenderTextureUtils(RenderTexture texture)
    {
        this._renderTexture = texture;

        this._width = _renderTexture.width;
        this._height = _renderTexture.height;
        this._depth = _renderTexture.volumeDepth;

        this._texture3D = new Texture3D(_width, _height, _depth, TextureFormat.RHalf, true);
        _texture3D.filterMode = FilterMode.Bilinear;

        _shader = (ComputeShader)Resources.Load("TextureUtilsCS");
    }

    private int GetTotalVoxelCount()
    {
        return _width * _height * _depth;
    }

    public Texture3D ConvertToTexture3D(bool oneChannel = true)
    {
        ComputeBuffer tmpBufferVoxel = new ComputeBuffer(GetTotalVoxelCount(), 4 * sizeof(float));

        int kernelID = _shader.FindKernel("CopyToBuffer");
        _shader.SetBuffer(kernelID, "voxelsBuffer", tmpBufferVoxel);
        _shader.SetTexture(kernelID, "voxels", _renderTexture, 0);
        _shader.Dispatch(kernelID,
            threadGroupsX: Mathf.CeilToInt(_width / 8.0f),
            threadGroupsY: Mathf.CeilToInt(_height / 8.0f),
            threadGroupsZ: Mathf.CeilToInt(_depth / 8.0f));

        Texture3D outTexture = new Texture3D(_width, _height, _depth, oneChannel ? TextureFormat.RHalf : TextureFormat.RGBAHalf, false);
        outTexture.filterMode = FilterMode.Bilinear;
        outTexture.wrapMode = TextureWrapMode.Clamp;
        
        Color[] voxelArray = outTexture.GetPixels(0);
        tmpBufferVoxel.GetData(voxelArray);
        outTexture.SetPixels(voxelArray, 0);
        outTexture.Apply();

        tmpBufferVoxel.Release();

        return outTexture;
    }

    public static Texture3D ConvertToTexture3D(RenderTexture texture)
    {
        var utils = new RenderTextureUtils(texture);
        return utils.ConvertToTexture3D();
    }
}
