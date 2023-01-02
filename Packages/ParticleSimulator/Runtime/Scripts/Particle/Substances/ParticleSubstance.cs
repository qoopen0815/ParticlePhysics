using Unity.Mathematics;
using UnityEngine;

namespace ParticleSimulator.Substance
{
    public abstract class ParticleSubstance
    {
        protected ElementType[] elements;

        protected float mu;
        protected float totalMass;
        protected float3 centerOfMass;
        protected float3x3 inertialMoment;

        #region Accessor
        public ElementType[] Elements => elements;
        public float TotalMass => totalMass;
        public float Mu => mu;
        public Vector3 CenterOfMass => centerOfMass;
        public float3x3 InertialMoment => inertialMoment;
        #endregion

        protected ParticleSubstance(float radius = 0.04f, float density = 2000.0f, float mu = 0.05f)
        {
            this.mu = mu;
            this.elements = SetElements(radius, density);
            this.totalMass = CalculateTotalMass(elements, density);
            this.centerOfMass = CalculateCenterOfMass(elements);
            this.inertialMoment = CalculateInverseInertialMoment(elements);
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
            return centerOfMass / (float)this.elements.Length;
        }

        protected abstract ElementType[] SetElements(float particleRadius, float particleDensity);
    }
}
