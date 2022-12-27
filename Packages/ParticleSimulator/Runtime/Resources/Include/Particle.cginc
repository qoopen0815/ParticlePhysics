#ifndef Granular_INCLUDED
#define Granular_INCLUDED

// ����̂��\�����闱�q�̕���
struct ElementType
{
	float radius;
	float mass;
	float mu;
	float3 offsetFromParticleCenter;
};

// ����Ԃ̃X�e�[�^�X
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
	uint _ParticleNum; // ����̂̐�
	uint _ElementNum; // ����̂��\�����闱�q�̐�
	float _ParticleTotalMass; // ����̂̑�����
	float4x4 _ParticleInertialMoment; // ����̂̊������[�����g
};

// �����l�̃o�b�t�@
StructuredBuffer<ElementType> _ElementBuffer;

// ����̂̃o�b�t�@
StructuredBuffer<ParticleType> _ParticleBufferRead;
RWStructuredBuffer<ParticleType> _ParticleBufferWrite;

#endif
