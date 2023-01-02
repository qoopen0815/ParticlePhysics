#ifndef Granular_INCLUDED
#define Granular_INCLUDED

// ����̂��\�����闱�q�̕���
struct ElementType
{
	float  radius;
	float  mass;
	float3 offsetFromParticleCenter;
};

// ����̂̃X�e�[�^�X
struct ParticleType
{
	float3 position;
	float3 velocity;
	float4 orientation;
	float3 angularVelocity;
};

// ����̂Ɋւ���萔
cbuffer ParticleCB
{
	uint  _ElementNum;
	uint  _ParticleNum;
	float _ParticleMu;
	float _ParticleTotalMass;
	float4x4 _ParticleInertialMoment;
};

// �����l�̃o�b�t�@
StructuredBuffer<ElementType> _ElementBuffer;

// ����̂̃o�b�t�@
StructuredBuffer<ParticleType> _ParticleBufferRead;
RWStructuredBuffer<ParticleType> _ParticleBufferWrite;

#endif
