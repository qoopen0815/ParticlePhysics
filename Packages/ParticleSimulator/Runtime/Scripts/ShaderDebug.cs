using System.Runtime.InteropServices;
using UnityEngine;

public class ShaderDebug
{
    public static void DebugLog<T>(GraphicsBuffer buffer, int threadGroups)
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
