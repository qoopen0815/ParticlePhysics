using UnityEngine;

namespace ParticleSimulator
{
    public struct TerrainType
    {
        public float height;
        public Vector3 normal;

        public static TerrainType[] GenerateFromTerrain(Terrain terrain)
        {
            var terrainResolution = terrain.terrainData.heightmapResolution;
            var data = new TerrainType[terrainResolution * terrainResolution];
            // 将来的に必要最低限の範囲でのみデータを取得したほうがいい（軽量化のため）
            for (var i = 0; i < terrainResolution; i++)
            {
                for (var j = 0; j < terrainResolution; j++)
                {
                    data[i + j * terrainResolution].height = terrain.terrainData.GetHeight(i, j);
                    data[i + j * terrainResolution].normal = terrain.terrainData.GetInterpolatedNormal(i / (float)terrainResolution,
                                                                                                       j / (float)terrainResolution);
                }
            }
            return data;
        }
    };
}
