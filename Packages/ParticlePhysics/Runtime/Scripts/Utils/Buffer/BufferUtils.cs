using UnityEngine;

public class BufferUtils
{
    /// <summary>
    /// 引数に指定されたバッファの入れ替え
    /// </summary>
    public static void SwapComputeBuffer(ref ComputeBuffer ping, ref ComputeBuffer pong)
    {
        ComputeBuffer temp = ping;
        ping = pong;
        pong = temp;
    }

    /// <summary>
    /// バッファの開放
    /// </summary>
    /// <param name="buffer"></param>
    public static void ReleaseBuffer(ComputeBuffer buffer)
    {
        if (buffer != null)
        {
            buffer.Release();
            buffer = null;
        }
    }

    /// <summary>
    /// バッファのプリントデバッグ
    /// </summary>
    /// <param name="buffer"></param>
    public static void DebugBuffer<T>(ComputeBuffer buffer, int threadGroups)
    {
        if (buffer != null)
        {
            var result = new T[threadGroups];
            buffer.GetData(result);
            foreach (var eachResult in result)
            {
                Debug.Log(eachResult);
            }
        }
    }
    public static void DebugBuffer<T>(GraphicsBuffer buffer, int threadGroups)
    {
        if (buffer != null)
        {
            var result = new T[threadGroups];
            buffer.GetData(result);
            foreach (var eachResult in result)
            {
                Debug.Log(eachResult);
            }
        }
    }
    public static void DebugBuffer<T>(GraphicsBuffer buffer, int threadGroups, int index)
    {
        if (buffer != null)
        {
            var result = new T[threadGroups];
            buffer.GetData(result);
            if (index >= result.Length)
            {
                Debug.LogError("index out of range.");
                return;
            }

            Debug.Log(result[index]);
        }
    }
    public static void DebugBuffer<T>(GraphicsBuffer buffer, int threadGroups, int startIndex, int endIndex)
    {
        if (buffer != null)
        {
            var result = new T[threadGroups];
            buffer.GetData(result);
            if (endIndex >= result.Length)
            {
                Debug.LogError("index out of range.");
                return;
            }

            for (int index = startIndex; index <= endIndex; index++)
            {
                Debug.Log(result[index]);
            }
        }
    }
}
