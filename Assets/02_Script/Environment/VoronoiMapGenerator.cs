using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class VoronoiMapGenerator : MonoBehaviour
{
    [SerializeField] Tilemap landTilemap;
    [SerializeField] Tilemap waterTilemap;

    [SerializeField] List<Biome> landBiomes;
    [SerializeField] Biome waterBiome;

    public int mapWidth = 100;
    public int mapHeight = 100;

    /// <summary>
    /// Voronoi Digram�� ���� �� ����. Ŭ���� �� �߰� �ɰ�����.
    /// </summary>
    public int seedPointCount = 20;
    public float noiseScale = 0.1f;

    [Range(0f, 1f)]
    public float waterEdgeSize = 0.4f;
    public float edgeNoiseScale = 0.1f;
    public float edgeNoiseStrength = 0.2f;

    BiomeMap biomeMap;
    public ObjectMap objectMap;

    GameObject objectParent;

    /// <summary>
    /// Voronoi Diagram�� ���� ���. ������ ��ġ�� ������ ��.
    /// </summary>
    private struct SeedPoint
    {
        public Vector2 position;
        public Biome biome;
    }

    void Start()
    {
        Generate();
    }

    private void Update()
    {
        // --- Ÿ���� ���̿� ������ ������Ʈ ������ �� ������ �ִ��� ����� �ϴ� �κ�!! -- ���߿� �����
        if (Input.GetMouseButtonDown(0))
        {
            Vector3Int tilemapPos = landTilemap.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));

            TileBase tile = landTilemap.GetTile(tilemapPos);

            //GenerateTreeTmp(tilemapPos);

            DebugController.Log($"map[{tilemapPos.y}, {tilemapPos.x}] {biomeMap.GetTileBiomeByPosition(tilemapPos.x, tilemapPos.y)}");
            DebugController.Log($"{objectMap.Map[tilemapPos.y, tilemapPos.x]}");
        }

        // --- Damageable Resource �� �۵��ϴ��� üũ�ϴ� �κ�. ������ Grass Tree 2 ������ üũ

        if (Input.GetMouseButtonDown(1))
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero);

            if (hit && hit.transform.GetComponent<DamageableResourceNode>() != null)
            {
                hit.transform.GetComponent<DamageableResourceNode>().Hit(10);
            }
        }
    }

    /// <summary>
    /// ���� ��� ���� ��Ҹ� �����.
    /// </summary>
    public void Clear()
    {
        landTilemap.ClearAllTiles();
        waterTilemap.ClearAllTiles();

        if (objectParent != null)
        {
            DestroyImmediate(objectParent.gameObject);
            objectParent = null;
        }
    }

    public void Generate()
    {
        // ���� �����ϱ� �� ��� Ÿ���� �����Ѵ�.
        Clear();
        GenerateVoronoiMap();
        GenerateObjects();
    }

    /// <summary>
    /// ���̿ȿ� �´� Tree, Plant, Mineral�� �����Ѵ�.
    /// </summary>
    void GenerateObjects()
    {
        if (objectParent != null) // Generate�ϱ� ���� Clear ������ ������ ������ ����
        { 
            DestroyImmediate(objectParent.gameObject);
        }

        ObjectGenerator objectGenerator = new ObjectGenerator(biomeMap, mapWidth, mapHeight); // biome ������ ���缭 ������Ʈ�� �����ϱ� ������ �Ķ���ͷ� �ǳ��ش�.
        var objects = objectGenerator.Generate();
        objectMap = objectGenerator.objectMap;

        GameObject go = new GameObject("ObjectParent"); // ������Ʈ���� ��� �θ� ������Ʈ�� �����
        go.transform.parent = transform; // Map Generator�� �ڽ����� �����.  ���� : Map Generator - ObjectParent - Objects

        foreach (ResourceObject obj in objects)
        {
            InstantiateObject(obj, go.transform);
        }
    }

    /// <summary>
    /// ��ǥ�� ���� ������Ʈ�� �����Ѵ�.
    /// </summary>
    void InstantiateObject(ResourceObject obj, Transform parent)
    {
        Vector3 cellCenterPosition = landTilemap.GetCellCenterWorld(new Vector3Int(obj.position.x, obj.position.y));
        Vector3 cellPosition = landTilemap.CellToWorld(new Vector3Int(obj.position.x, obj.position.y));

        Vector3 position = cellPosition;

        if (obj.data.Width % 2 != 0)
        {
            position.x = cellCenterPosition.x;
        }
        if (obj.data.Height % 2 != 0)
        {
            position.y = cellCenterPosition.y;
        }

        GameObject go = Instantiate(obj.data.Prefab, position, Quaternion.identity, parent);
    }

    /// <summary>
    /// Voronoi Diagram�� ����Ͽ� �����ϰ� ���� �����Ѵ�.
    /// </summary>
    void GenerateVoronoiMap()
    {
        List<SeedPoint> seedPoints = GenerateSeedPoints();

        biomeMap = new BiomeMap(mapWidth, mapHeight);

        // ���� ���ʹ� �߾����� �д�
        Vector2 mapCenter = new Vector2(mapWidth / 2, mapHeight / 2);

        // ���� �߾ӿ��� ���� �ָ� ������ �Ÿ�. �ܰ��� �ٴٷ� �����ϱ� ���� �� �����̴�. 
        float maxDistance = Mathf.Sqrt(Mathf.Pow(mapWidth / 2, 2) + Mathf.Pow(mapHeight / 2, 2));

        // �� ��ü ��ȸ
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Vector2 currentPos = new Vector2(x, y);
                float minDistance = float.MaxValue;
                Biome selectedBiome = landBiomes[0];

                // ���� ��ġ�� Ÿ�Ͽ��� ���� ����� seed point�� biome type�� ���󰣴�.
                foreach (var seed in seedPoints)
                {
                    // �������� ���������� ���ڿ�������Ƿ� Perlin Noise�� �̿��� seed point���� ��ġ�� �ణ�� ����.
                    float noise = Mathf.PerlinNoise((x + seed.position.x) * noiseScale, (y + seed.position.y) * noiseScale);
                    float jitter = Mathf.Lerp(-1f, 1f, noise) * 3f;

                    float distance = Vector2.Distance(currentPos, seed.position + new Vector2(jitter, jitter));

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        selectedBiome = seed.biome;
                    }
                }

                // �߽ɿ������� ��� ���� ������ �ִ��� 0~1 ������ ������ ��ȯ�Ѵ�.
                float distanceFromCenter = Vector2.Distance(currentPos, mapCenter) / maxDistance;

                // �ڿ��������� ���� �ؾȼ��� �Ȱ��� Perlin Noise�� �����Ѵ�.
                float edgeNoise = Mathf.PerlinNoise(x * edgeNoiseScale, y * edgeNoiseScale) * edgeNoiseStrength;
                float adjustedThreshold = (1 - waterEdgeSize) + edgeNoise;

                if (distanceFromCenter > adjustedThreshold)
                {
                    waterTilemap.SetTile(new Vector3Int(x, y, 0), waterBiome.Tile);  // �ٴ�
                    biomeMap.MarkTile(x, y, waterBiome);
                }
                else
                {
                    landTilemap.SetTile(new Vector3Int(x, y, 0), selectedBiome.Tile);  // ����
                    biomeMap.MarkTile(x, y, selectedBiome);
                }
            }
        }
    }

    /// <summary>
    /// ��ü ���� ���� ����Ʈ�� ���� ��� biome�� �����ϰ� ���Ѵ�.
    /// </summary>
    /// <returns></returns>
    List<SeedPoint> GenerateSeedPoints()
    {
        List<SeedPoint> seeds = new List<SeedPoint>();
        for (int i = 0; i < seedPointCount; i++)
        {
            Vector2 pos = new Vector2(Random.Range(0, mapWidth), Random.Range(0, mapHeight));
            int randIdx = Random.Range(0, landBiomes.Count);
            seeds.Add(new SeedPoint { position = pos, biome = landBiomes[randIdx] });
        }
        return seeds;
    }
}
