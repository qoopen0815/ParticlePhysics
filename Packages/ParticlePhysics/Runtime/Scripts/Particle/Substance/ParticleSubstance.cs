using Unity.Mathematics;
using UnityEngine;

namespace ParticlePhysics.Substance
{
    /// <summary>
    /// The base class for particle substances.
    /// </summary>
    public abstract class ParticleSubstance
    {
        /// <summary>
        /// Graphics buffer for elements.
        /// </summary>
        protected GraphicsBuffer elements;

        /// <summary>
        /// Coefficient of friction.
        /// </summary>
        protected float mu;

        /// <summary>
        /// Total mass of the particle substance.
        /// </summary>
        protected float totalMass;

        /// <summary>
        /// Center of mass of the particle substance.
        /// </summary>
        protected float3 centerOfMass;

        /// <summary>
        /// Inertial moment of the particle substance.
        /// </summary>
        protected float3x3 inertialMoment;

        #region Accessor

        /// <summary>
        /// Accessor for the graphics buffer containing elements.
        /// </summary>
        public GraphicsBuffer Elements => elements;

        /// <summary>
        /// Accessor for the total mass of the particle substance.
        /// </summary>
        public float TotalMass => totalMass;

        /// <summary>
        /// Accessor for the coefficient of friction.
        /// </summary>
        public float Mu => mu;

        /// <summary>
        /// Accessor for the center of mass of the particle substance.
        /// </summary>
        public Vector3 CenterOfMass => centerOfMass;

        /// <summary>
        /// Accessor for the inertial moment of the particle substance.
        /// </summary>
        public float3x3 InertialMoment => inertialMoment;

        /// <summary>
        /// Accessor for the inertial moment as a 4x4 matrix.
        /// </summary>
        public Matrix4x4 InertialMoment4x4
        {
            get
            {
                Vector4 c0 = new Vector4(inertialMoment.c0.x, inertialMoment.c0.y, inertialMoment.c0.z, 0);
                Vector4 c1 = new Vector4(inertialMoment.c1.x, inertialMoment.c1.y, inertialMoment.c1.z, 0);
                Vector4 c2 = new Vector4(inertialMoment.c2.x, inertialMoment.c2.y, inertialMoment.c2.z, 0);
                Vector4 c3 = new Vector4(0, 0, 0, 1);
                return new Matrix4x4(c0, c1, c2, c3);
            }
        }

        #endregion

        /// <summary>
        /// Release the elements graphics buffer.
        /// </summary>
        public void Release()
        {
            elements.Release();
        }

        /// <summary>
        /// Calculate the mass of an element based on its radius and density.
        /// </summary>
        /// <param name="radius">Radius of the element.</param>
        /// <param name="density">Density of the element.</param>
        /// <returns>The mass of the element.</returns>
        protected float CalculateElementMass(float radius, float density)
        {
            float volume = (4.0f / 3.0f) * math.PI * math.pow(radius, 3);
            float mass = volume * density;
            return mass;
        }

        /// <summary>
        /// Calculate the inverse inertial moment of the particle substance based on its elements.
        /// </summary>
        /// <param name="elements">Array of particle elements.</param>
        /// <returns>The inverse inertial moment of the particle substance.</returns>
        protected float3x3 CalculateInverseInertialMoment(ParticleElement[] elements)
        {
            float3x3 inertialMoment = float3x3.zero;

            foreach (ParticleElement element in elements)
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

        /// <summary>
        /// Calculate the total mass of the particle substance based on its elements and density.
        /// </summary>
        /// <param name="elements">Array of particle elements.</param>
        /// <param name="density">Density of the particle substance.</param>
        /// <returns>The total mass of the particle substance.</returns>
        protected float CalculateTotalMass(ParticleElement[] elements, float density)
        {
            float totalMass = 0;
            foreach (ParticleElement element in elements)
            {
                totalMass += CalculateElementMass(element.radius, density);
            }
            return totalMass;
        }

        /// <summary>
        /// Calculate the center of mass of the particle substance based on its elements.
        /// </summary>
        /// <param name="elements">Array of particle elements.</param>
        /// <returns>The center of mass of the particle substance.</returns>
        protected float3 CalculateCenterOfMass(ParticleElement[] elements)
        {
            float3 centerOfMass = float3.zero;
            foreach (ParticleElement element in elements)
            {
                centerOfMass += new float3(element.offsetFromParticleCenter);
            }
            return centerOfMass / (float)elements.Length;
        }

        /// <summary>
        /// Abstract method to be implemented by derived classes to set the elements of the particle substance.
        /// </summary>
        /// <param name="particleRadius">Radius of the particles.</param>
        /// <param name="particleDensity">Density of the particles.</param>
        /// <returns>An array of particle elements.</returns>
        protected abstract ParticleElement[] SetElements(float particleRadius, float particleDensity);
    }
}
