using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public struct Test
{
    public float x;
    public float y;
    public float z;
}

public class TestScript : MonoBehaviour
{
    uint _objNum = 10;

    ComputeShader _shader;
    GraphicsBuffer _buffer;

    // Start is called before the first frame update
    void Start()
    {
        _shader = (ComputeShader)Resources.Load("Test");
        _buffer = new GraphicsBuffer(
            GraphicsBuffer.Target.Structured,
            (int)_objNum,
            Marshal.SizeOf(typeof(Test)));

        DispatchShader(isDebugLog: true);
    }

    // Update is called once per frame
    void Update()
    {
        DispatchShader(isDebugLog: false);
    }

    private void OnDestroy()
    {
        _buffer.Release();
    }

    void DispatchShader(bool isDebugLog)
    {
        int kernelID = _shader.FindKernel("CSMain");
        _shader.SetBuffer(kernelID, "_test", _buffer);
        _shader.GetKernelThreadGroupSizes(kernelID, out var x, out var y, out var z);
        _shader.Dispatch(kernelID, 300, 1, 1);

        if (!isDebugLog) return;

        var result = new Test[_objNum];
        _buffer.GetData(result);
        foreach (var eachResult in result)
        {
            Debug.Log(eachResult.x);
        }
    }
}
