#ifndef TERRAIN_INCLUDED
#define TERRAIN_INCLUDED

// ---------------------
// Define Data structure (must be same as your particle data)
// ---------------------

struct Terrain
{
	float height;
	float3 normal;
};

cbuffer TerrainCB
{
	uint _Resolution; // 地形の解像度
	float3 _Ratio; // 地形の解像度とサイズの比
	float _Friction; // 壁の摩擦
}

// Terrain情報を保持するバッファ
StructuredBuffer<Terrain> _TerrainBuffer;

// HeightmapをBilinearを使って補完する
inline float GetInterpolatedHeight(float pos_x, float pos_z)
{
	uint int_x = uint(pos_x);
	uint int_z = uint(pos_z);
	float2 dist_x = float2(float(int_x + 1) - pos_x, pos_x - float(int_x));
	float2 dist_z = float2(float(int_z + 1) - pos_z, pos_z - float(int_z));
	float2x2 heightmap = float2x2(
		_TerrainBuffer[int_x + int_z * _Resolution].height, _TerrainBuffer[(int_x + 1) + int_z * _Resolution].height,
		_TerrainBuffer[int_x + (int_z + 1) * _Resolution].height, _TerrainBuffer[(int_x + 1) + (int_z + 1) * _Resolution].height
		);
	float dst = mul(mul(dist_z, heightmap), dist_x);
	return dst + 0.1f;
}

// Normalmapを補完する
inline float3 GetInterpolatedNormal(float pos_x, float pos_z)
{
	uint int_x = uint(pos_x);
	uint int_z = uint(pos_z);

	float dist_x = float(int_x + 1) - pos_x;
	float dist_z = float(int_z + 1) - pos_z;

	float3 norm0 = _TerrainBuffer[int_x + int_z * _Resolution].normal * dist_x * dist_z;
	float3 norm1 = _TerrainBuffer[(int_x + 1) + int_z * _Resolution].normal * (1.0f - dist_x) * dist_z;
	float3 norm2 = _TerrainBuffer[int_x + (int_z + 1) * _Resolution].normal * dist_x * (1.0f - dist_z);
	float3 norm3 = _TerrainBuffer[(int_x + 1) + (int_z + 1) * _Resolution].normal * (1.0f - dist_x) * (1.0f - dist_z);

	float3 norm = norm0 + norm1 + norm2 + norm3;
	return norm / length(norm);
}

#endif
