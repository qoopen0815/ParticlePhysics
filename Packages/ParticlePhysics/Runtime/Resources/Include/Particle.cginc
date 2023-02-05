#ifndef Granular_INCLUDED
#define Granular_INCLUDED

// 粒状体を構成する粒子の物性
struct ElementType
{
	float  radius;
	float  mass;
	float3 offsetFromParticleCenter;
};

// 粒状体のステータス
struct ParticleType
{
	float3 position;
	float3 velocity;
	float4 orientation;
	float3 angularVelocity;
};

// 粒状体に関する定数
cbuffer ParticleCB
{
	uint  _ElementNum;
	uint  _ParticleNum;
	float _ParticleMu;
	float _ParticleTotalMass;
	float4x4 _ParticleInertialMoment;
};

// Main Particle Buffer
StructuredBuffer<ElementType> _ElementBuffer;
StructuredBuffer<ParticleType> _ParticleBufferRead;
RWStructuredBuffer<ParticleType> _ParticleBufferWrite;

// Collision Object Particle Buffer(Read Only)
StructuredBuffer<ElementType> _ObjectElementBuffer;
StructuredBuffer<ParticleType> _ObjectParticleBufferRead;

#endif
