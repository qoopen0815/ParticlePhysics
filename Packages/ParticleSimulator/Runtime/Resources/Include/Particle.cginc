#ifndef Granular_INCLUDED
#define Granular_INCLUDED

// 粒状体を構成する粒子の物性
struct ParticleSubstance
{
	float radius;
	float mass;
	float3 offsetFromGranularCenter;
};

// 粒状態のステータス
struct GranularParticle
{
	float3 position;
	float3 velocity;
	float3 acceleration;
	float4 orientation;
	float3 angularVelocity;
	float3 angularAcceleration;
};

// 粒状体に関する定数
cbuffer GranularParticleCB
{
	uint _ParticleNum; // 粒状体の数
	uint _GranularParticleNum; // 粒状体を構成する粒子の数
	float _GranularTotalMass; // 粒状体の総質量
	float4x4 _GranularInertialMoment; // 粒状体の慣性モーメント
	float _GranularMu; // 粒状体の摩擦係数
};

// 物性値のバッファ
StructuredBuffer<ParticleSubstance> _ParticleSubstancesBuffer;

// 粒状体のバッファ
StructuredBuffer<GranularParticle> _GranularsBufferRead;
RWStructuredBuffer<GranularParticle> _GranularsBufferWrite;

#endif
