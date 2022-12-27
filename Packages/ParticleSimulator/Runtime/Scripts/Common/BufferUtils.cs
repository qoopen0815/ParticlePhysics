using UnityEngine;

public class BufferUtils
{
    /// <summary>
    /// �����Ɏw�肳�ꂽ�o�b�t�@�̓���ւ�
    /// </summary>
    public static void SwapComputeBuffer(ref ComputeBuffer ping, ref ComputeBuffer pong)
    {
        ComputeBuffer temp = ping;
        ping = pong;
        pong = temp;
    }

    /// <summary>
    /// �o�b�t�@�̊J��
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
    /// �o�b�t�@�̃v�����g�f�o�b�O
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
