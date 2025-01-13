using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class VoronoiTilemapGenerator : MonoBehaviour
{
    public UnityEngine.Tilemaps.Tilemap landTilemap;
    public UnityEngine.Tilemaps.Tilemap waterTilemap;

    public TileBase grassTile;
    public TileBase desertTile;
    public TileBase pollutionTile;
    public TileBase waterTile;

    public int mapWidth = 100;
    public int mapHeight = 100;
    public int seedPointCount = 20;
    public float noiseScale = 0.1f;
    [Range(0f, 1f)]
    public float waterEdgeSize = 0.4f;
    public float edgeNoiseScale = 0.1f;
    public float edgeNoiseStrength = 0.2f;

    private enum BiomeType { Grass, Desert, Pollution }

    private struct SeedPoint
    {
        public Vector2 position;
        public BiomeType biome;
    }

    void Start()
    {
        
        GenerateVoronoiMap();
    }

    public void Clear()
    {
        landTilemap.ClearAllTiles();
        waterTilemap.ClearAllTiles();
    }

    public void GenerateVoronoiMap()
    {
        Clear();
        List<SeedPoint> seedPoints = GenerateSeedPoints();

        Vector2 mapCenter = new Vector2(0, 0);
        float maxDistance = Mathf.Sqrt(Mathf.Pow(mapWidth / 2, 2) + Mathf.Pow(mapHeight / 2, 2));

        for (int x = -mapWidth / 2; x < mapWidth / 2; x++)
        {
            for (int y = -mapHeight / 2; y < mapHeight / 2; y++)
            {
                Vector2 currentPos = new Vector2(x, y);
                float minDistance = float.MaxValue;
                BiomeType selectedBiome = BiomeType.Grass;

                foreach (var seed in seedPoints)
                {
                    float noise = Mathf.PerlinNoise((x + seed.position.x) * noiseScale, (y + seed.position.y) * noiseScale);
                    float jitter = Mathf.Lerp(-1f, 1f, noise) * 3f;

                    float distance = Vector2.Distance(currentPos, seed.position + new Vector2(jitter, jitter));

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        selectedBiome = seed.biome;
                    }
                }

                float distanceFromCenter = Vector2.Distance(currentPos, mapCenter) / maxDistance;

                float edgeNoise = Mathf.PerlinNoise(x * edgeNoiseScale, y * edgeNoiseScale) * edgeNoiseStrength;
                float adjustedThreshold = (1 - waterEdgeSize) + edgeNoise;

                if (distanceFromCenter > adjustedThreshold)
                {
                    waterTilemap.SetTile(new Vector3Int(x, y, 0), waterTile);  // �ٴ�
                }
                else
                {
                    landTilemap.SetTile(new Vector3Int(x, y, 0), GetTileForBiome(selectedBiome));  // ����
                }
            }
        }
    }

    List<SeedPoint> GenerateSeedPoints()
    {
        List<SeedPoint> seeds = new List<SeedPoint>();
        for (int i = 0; i < seedPointCount; i++)
        {
            Vector2 pos = new Vector2(Random.Range(-mapWidth / 2, mapWidth / 2), Random.Range(-mapHeight / 2, mapHeight / 2));
            BiomeType biome = (BiomeType)(i % 3);
            seeds.Add(new SeedPoint { position = pos, biome = biome });
        }
        return seeds;
    }

    TileBase GetTileForBiome(BiomeType biome)
    {
        switch (biome)
        {
            case BiomeType.Grass: return grassTile;
            case BiomeType.Desert: return desertTile;
            case BiomeType.Pollution: return pollutionTile;
            default: return grassTile;
        }
    }
}
