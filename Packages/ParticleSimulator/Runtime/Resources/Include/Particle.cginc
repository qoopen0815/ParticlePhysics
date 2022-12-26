#ifndef Granular_INCLUDED
#define Granular_INCLUDED

// ����̂��\�����闱�q�̕���
struct ParticleSubstance
{
	float radius;
	float mass;
	float3 offsetFromGranularCenter;
};

// ����Ԃ̃X�e�[�^�X
struct GranularParticle
{
	float3 position;
	float3 velocity;
	float3 acceleration;
	float4 orientation;
	float3 angularVelocity;
	float3 angularAcceleration;
};

// ����̂Ɋւ���萔
cbuffer GranularParticleCB
{
	uint _ParticleNum; // ����̂̐�
	uint _GranularParticleNum; // ����̂��\�����闱�q�̐�
	float _GranularTotalMass; // ����̂̑�����
	float4x4 _GranularInertialMoment; // ����̂̊������[�����g
	float _GranularMu; // ����̖̂��C�W��
};

// �����l�̃o�b�t�@
StructuredBuffer<ParticleSubstance> _ParticleSubstancesBuffer;

// ����̂̃o�b�t�@
StructuredBuffer<GranularParticle> _GranularsBufferRead;
RWStructuredBuffer<GranularParticle> _GranularsBufferWrite;

#endif
