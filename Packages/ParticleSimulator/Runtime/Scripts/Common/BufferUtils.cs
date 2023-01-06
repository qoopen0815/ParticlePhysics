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
}
