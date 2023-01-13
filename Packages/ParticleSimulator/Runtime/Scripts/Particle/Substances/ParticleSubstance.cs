using Unity.Mathematics;
using UnityEngine;

namespace ParticleSimulator.Substance
{
    public abstract class ParticleSubstance
    {
        protected GraphicsBuffer elements;

        protected float mu;
        protected float totalMass;
        protected float3 centerOfMass;
        protected float3x3 inertialMoment;

        #region Accessor
        public GraphicsBuffer Elements => elements; 
        public float TotalMass => totalMass;
        public float Mu => mu;
        public Vector3 CenterOfMass => centerOfMass;
        public float3x3 InertialMoment => inertialMoment;
        public Matrix4x4 InertialMoment4x4
        {
            get
            {
                Vector4 c0 = new Vector4(inertialMoment.c0.x,   inertialMoment.c0.y,    inertialMoment.c0.z,    0);
                Vector4 c1 = new Vector4(inertialMoment.c1.x,   inertialMoment.c1.y,    inertialMoment.c1.z,    0);
                Vector4 c2 = new Vector4(inertialMoment.c2.x,   inertialMoment.c2.y,    inertialMoment.c2.z,    0);
                Vector4 c3 = new Vector4(0,                     0,                      0,                      1);
                return new Matrix4x4(c0, c1, c2, c3);
            }
        }

        #endregion

        public void Release()
        {
            elements.Release();
        }

        protected float CalculateElementMass(float radius, float density)
        {
            float volume = (4.0f / 3.0f) * math.PI * math.pow(radius, 3);
            float mass = volume * density;
            return mass;
        }

        protected float3x3 CalculateInverseInertialMoment(ElementType[] elements)
        {
            float3x3 inertialMoment = float3x3.zero;

            foreach (ElementType element in elements)
            {
                float3x3 elementInertialMoment = float3x3.identity * (2.0f / 5.0f) * element.mass * math.pow(element.radius, 2);

                float3 offset = element.offsetFromParticleCenter;
                float offsetSquared = math.dot(offset, offset);
                float3x3 offsetInertialMoment = elementInertialMoment + element.mass * (offsetSquared * float3x3.identity - math.mul(offset, offset));
                inertialMoment += offsetInertialMoment;
            }
            inertialMoment = math.transpose(inertialMoment);

            return inertialMoment;
        }

        protected float CalculateTotalMass(ElementType[] elements, float density)
        {
            float totalMass = 0;
            foreach (ElementType element in elements)
            {
                totalMass += CalculateElementMass(element.radius, density);
            }
            return totalMass;
        }

        protected float3 CalculateCenterOfMass(ElementType[] elements)
        {
            float3 centerOfMass = float3.zero;
            foreach (ElementType element in elements)
            {
                centerOfMass += new float3(element.offsetFromParticleCenter);
            }
            return centerOfMass / (float)elements.Length;
        }

        protected abstract ElementType[] SetElements(float particleRadius, float particleDensity);
    }
}
