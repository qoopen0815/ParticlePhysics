﻿using System.Runtime.InteropServices;
using UnityEngine;
using ParticleSimulator.Substance;

namespace ParticleSimulator
{
    public class ParticleBuffer
    {
        public GraphicsBuffer datas;
        public GraphicsBuffer substances;

        public ParticleBuffer(
            ParticleData[] particleDatas, 
            Element[] elementSubstances)
        {
            datas = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                particleDatas.Length,
                Marshal.SizeOf(typeof(ParticleData)));
            datas.SetData(particleDatas);

            substances = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                elementSubstances.Length,
                Marshal.SizeOf(typeof(Element)));
            substances.SetData(elementSubstances);
        }

        public void Release()
        {
            datas.Release();
            substances.Release();
        }
    }
}