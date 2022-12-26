using Unity.Mathematics;
using UnityEngine;

namespace ParticleSimulator.Substance
{
    public abstract class ParticleSubstance
    {
        protected Element[] elements;

        protected float totalMass;
        protected float3 centerOfMass;
        protected float3x3 inertialMoment;

        #region Accessor
        public Element[] Elements => elements;
        public float TotalMass => totalMass;
        public Vector3 CenterOfMass => centerOfMass;
        public float3x3 InertialMoment => inertialMoment;
        #endregion

        protected ParticleSubstance(float radius = 0.04f, float density = 2000.0f)
        {
            this.elements = SetElements(radius, density);
            totalMass = CalculateTotalMass(elements, density);
            centerOfMass = CalculateCenterOfMass(elements);
            inertialMoment = CalculateInverseInertialMoment(elements);
        }

        protected float CalculateElementMass(float radius, float density)
        {
            float volume = (4.0f / 3.0f) * math.PI * math.pow(radius, 3);
            float mass = volume * density;
            return mass;
        }

        protected float3x3 CalculateInverseInertialMoment(Element[] elements)
        {
            float3x3 inertialMoment = float3x3.zero;

            foreach (Element element in elements)
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

        protected float CalculateTotalMass(Element[] elements, float density)
        {
            float totalMass = 0;
            foreach (Element element in elements)
            {
                totalMass += CalculateElementMass(element.radius, density);
            }
            return totalMass;
        }

        protected float3 CalculateCenterOfMass(Element[] elements)
        {
            float3 centerOfMass = float3.zero;
            foreach (Element element in elements)
            {
                centerOfMass += new float3(element.offsetFromParticleCenter);
            }
            return centerOfMass / (float)this.elements.Length;
        }

        protected abstract Element[] SetElements(float particleRadius, float particleDensity);
    }
}
