#ifndef Granular_INCLUDED
#define Granular_INCLUDED

// 粒状体を構成する粒子の物性
struct ElementType
{
	float radius;
	float mass;
	float mu;
	float3 offsetFromParticleCenter;
};

// 粒状態のステータス
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
	uint _ParticleNum; // 粒状体の数
	uint _ElementNum; // 粒状体を構成する粒子の数
	float _ParticleTotalMass; // 粒状体の総質量
	float4x4 _ParticleInertialMoment; // 粒状体の慣性モーメント
};

// 物性値のバッファ
StructuredBuffer<ElementType> _ElementBuffer;

// 粒状体のバッファ
StructuredBuffer<ParticleType> _ParticleBufferRead;
RWStructuredBuffer<ParticleType> _ParticleBufferWrite;

#endif
