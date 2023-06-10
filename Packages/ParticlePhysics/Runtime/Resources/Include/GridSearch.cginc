#ifndef GRIDSEARCH_INCLUDED
#define GRIDSEARCH_INCLUDED

// ---------------------
// Define Data structure (must be same as your particle data)
// ---------------------

#include "./Math.cginc"

cbuffer NearestNeighborCB
{
	float3 _GridResolution; // Number of grid cell.
	float _GridCellSize; // Size of grid cell.
};

StructuredBuffer<uint2> _GridIndicesBufferRead;
RWStructuredBuffer<uint2> _GridIndicesBufferWrite;

// Return hush number from particle position.
inline float3 GridCalculateCell(float3 position)
{
	return position / _GridCellSize;
}

inline float3 GridCalculateCell(float3 position, float4x4 _gridTF)
{
	return mul(_gridTF, float4(position, 1)).xyz / _GridCellSize;
}

// セルの3次元インデックスから1次元インデックスを返す
inline uint GridKey(uint3 xyz)
{
	return xyz.x + xyz.y * _GridResolution.x + xyz.z * _GridResolution.x * _GridResolution.y;
}

// (グリッドID, パーティクルID) のペアを作成する
inline uint2 MakeKeyValuePair(uint3 xyz, uint value)
{
	return uint2(GridKey(xyz), value);
}

// グリッドIDとパーティクルIDのペアからグリッドIDだけを抜き出す
inline uint GridGetKey(uint2 pair)
{
	return pair.x;
}

// グリッドIDとパーティクルIDのペアからパーティクルIDだけを抜き出す
inline uint GridGetValue(uint2 pair)
{
	return pair.y;
}

#define LOOP_AROUND_NEIGHBOR(pos, tf) int3 G_XYZ = (int3)GridCalculateCell(pos, tf); for(int Z = max(G_XYZ.z - 1, 0); Z <= min(G_XYZ.z + 1, _GridResolution.z - 1); Z++) for (int Y = max(G_XYZ.y - 1, 0); Y <= min(G_XYZ.y + 1, _GridResolution.y - 1); Y++)  for (int X = max(G_XYZ.x - 1, 0); X <= min(G_XYZ.x + 1, _GridResolution.x - 1); X++)

#endif
