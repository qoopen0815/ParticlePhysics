using System.Runtime.InteropServices;
using UnityEngine;

public class ShaderDebug<T> where T : struct
{
    public GraphicsBuffer debugBuffer;

    public ShaderDebug(int objNum)
    {
        this.debugBuffer = new GraphicsBuffer(
            GraphicsBuffer.Target.Structured,
            objNum,
            Marshal.SizeOf(typeof(T)));
    }

    public void SetBuffer(ref ComputeShader shader, int kernelID, string name)
    {
        shader.SetBuffer(kernelID, name, debugBuffer);
    }

    public void Release()
    {
        debugBuffer.Release();
    }

    public void DebugLog()
    {
        if (debugBuffer != null)
        {
            var result = new T[debugBuffer.count];
            debugBuffer.GetData(result);
            foreach (var eachResult in result)
            {
                Debug.Log(eachResult);
            }
        }
    }
}
