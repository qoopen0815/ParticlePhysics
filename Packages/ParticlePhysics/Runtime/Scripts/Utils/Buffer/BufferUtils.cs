using UnityEngine;

public class BufferUtils
{
    /// <summary>
    /// �����Ɏw�肳�ꂽ�o�b�t�@�̓���ւ�
    /// </summary>
    internal static void SwapComputeBuffer(ref ComputeBuffer ping, ref ComputeBuffer pong)
    {
        ComputeBuffer temp = ping;
        ping = pong;
        pong = temp;
    }

    /// <summary>
    /// �o�b�t�@�̊J��
    /// </summary>
    /// <param name="buffer"></param>
    internal static void ReleaseBuffer(ComputeBuffer buffer)
    {
        if (buffer != null)
        {
            buffer.Release();
            buffer = null;
        }
    }

    internal static T[] GetData<T>(GraphicsBuffer buffer)
    {
        var data = new T[buffer.count];
        buffer.GetData(data);
        return data;
    }

    /// <summary>
    /// �o�b�t�@�̃v�����g�f�o�b�O
    /// </summary>
    /// <param name="buffer"></param>
    internal static void DebugBuffer<T>(ComputeBuffer buffer, int threadGroups)
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
    internal static void DebugBuffer<T>(GraphicsBuffer buffer)
    {
        if (buffer != null)
        {
            var result = new T[buffer.count];
            buffer.GetData(result);
            foreach (var eachResult in result)
            {
                Debug.Log(eachResult);
            }
        }
    }
    public static void DebugBuffer<T>(GraphicsBuffer buffer, int index)
    {
        if (buffer != null)
        {
            var result = new T[buffer.count];
            buffer.GetData(result);
            if (index >= result.Length)
            {
                Debug.LogError("index out of range.");
                return;
            }

            Debug.Log(result[index]);
        }
    }
    internal static void DebugBuffer<T>(GraphicsBuffer buffer, int threadGroups, int startIndex, int endIndex)
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
