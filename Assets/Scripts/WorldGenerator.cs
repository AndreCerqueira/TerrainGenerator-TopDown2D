using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGenerator : MonoBehaviour
{
    // Variables
    int width, height;

    public TileBase grassTile;
    public TileBase rockTile;
    public TileBase treeTile;
    public TileBase bushTile;
    public TileBase waterTile;
    public TileBase backgroundTile;
    public TileBase fogTile;

    public Tilemap backgroundTileMap;
    public Tilemap tileMap;

    float[,] noiseValues;

    int seed;
    const float FREQUENCY = 13;
    const float AMPLITUDE = 1;
    const float LACUNACY = 0.5f;
    const float PERSISTANCE = 0.7f;
    const int OCTAVES = 3;

    // Start is called before the first frame update
    void Awake()
    {
        // Get Previous Generated Map
        print("json - " + PlayerPrefs.GetString("mode"));

        if (PlayerPrefs.GetString("mode") == "new")
        {
            // Set data
            width = 100;
            height = 100;
            seed = UnityEngine.Random.Range(0, 1000);

            Noise noise = new Noise(seed, FREQUENCY, AMPLITUDE, LACUNACY, PERSISTANCE, OCTAVES);
            noiseValues = noise.GetNoiseValues(width, height);

            tileMap.ClearAllTiles();
            CreateBackground();
            GenerateRandomTiles();
            //CreateFog();
        }
        else
        {

            // Get data
            Map map = JsonUtility.FromJson<Map>(PlayerPrefs.GetString("mode"));

            // Set data
            width = map.width;
            height = map.height;

            tileMap.ClearAllTiles();
            CreateBackground();
            GenerateMapTiles(map);
            //CreateFog();
        }


    }


    public void GenerateRandomTiles()
    {
        for (int y = height; y > 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                bool haveWater = SetWater(x, y);

                if (!haveWater)
                    SetObstacles(x, y);

            }
        }
    }


    public void GenerateMapTiles(Map map)
    {
        for (int y = map.height; y > 0; y--)
        {
            for (int x = 0; x < map.width; x++)
            {
                string type = Map.GetTypeInPosition(x, y, map.obstacles);

                switch (type)
                {
                    case "Water":
                        SetWaterTile(x, y);
                        break;

                    case "Grass":
                        SetGrassTile(x, y);
                        break;

                    case "Bush":
                        SetBushTile(x, y);
                        break;

                    case "Rock":
                        SetRockTile(x, y);
                        break;

                    case "Tree":
                        SetTreeTile(x, y);
                        break;

                    default:
                        break;
                }

            }
        }
    }



    private bool SetWater(int x, int y)
    {

        if (noiseValues[x, y] < 0.30f)
        {
            SetWaterTile(x, y);
            return true;
        }

        return false;
    }


    float maxChance = 15f;
    private void SetObstacles(int x, int y)
    {

        if (noiseValues[x, y] < 0.60f)
        {
            float grassChance = UnityEngine.Random.Range(0f, 10f);

            if (grassChance > 7f)
            {

                float obstacleChance = UnityEngine.Random.Range(0f, maxChance);

                if (obstacleChance > maxChance - 1f)
                    SetTreeTile(x, y);
                else if (obstacleChance > maxChance - 2f)
                    SetBushTile(x, y);
                else if (obstacleChance > maxChance - 3f)
                    SetRockTile(x, y);
                else
                    SetGrassTile(x, y);
            }

        }
    }


    public void CreateBackground()
    {

        for (int y = height; y > 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                backgroundTileMap.SetTile(new Vector3Int(x, y, 0), backgroundTile);
            }
        }
    }


    public void CreateFog()
    {

        for (int y = 0; y < height*2; y++)
        {
            for (int x = 0; x < width*2; x++)
            {
                if (y >= height || x >= width)
                    tileMap.SetTile(new Vector3Int(x, y, 0), fogTile);
            }
        }

    }


    public Vector3 GetWorldCenter() 
    {
        Vector3 cellPos = tileMap.CellToWorld(new Vector3Int(width / 2, height / 2, -10));
        return new Vector3(cellPos.x, cellPos.y, -10);
    }


    public string GetMapToJson()
    {
        Map map = new Map(width, height);

        foreach (var pos in tileMap.cellBounds.allPositionsWithin)
        {
            Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
            if (tileMap.HasTile(localPlace))
                map.obstacles.Add(new Obstacle(pos.x, pos.y, tileMap.GetTile(localPlace).name));
        }

        string json = JsonUtility.ToJson(map);
        return json;
    }


    private void SetWaterTile(int x, int y) => tileMap.SetTile(new Vector3Int(x, y, 0), waterTile);
    private void SetGrassTile(int x, int y) => tileMap.SetTile(new Vector3Int(x, y, 0), grassTile);
    private void SetTreeTile(int x, int y) => tileMap.SetTile(new Vector3Int(x, y, 0), treeTile);
    private void SetBushTile(int x, int y) => tileMap.SetTile(new Vector3Int(x, y, 0), bushTile);
    private void SetRockTile(int x, int y) => tileMap.SetTile(new Vector3Int(x, y, 0), rockTile);

}


[Serializable]
public class Map
{
    public int width;
    public int height;
    public List<Obstacle> obstacles;

    public Map(int width, int height)
    {
        this.width = width;
        this.height = height;
        obstacles = new List<Obstacle>();
    }

    public static string GetTypeInPosition(int x, int y, List<Obstacle> obstacles)
    {
        foreach (Obstacle obstacle in obstacles)
        {
            if (obstacle.x == x && obstacle.y == y)
                return obstacle.type;
        }

        return "";
    }
}


[Serializable]
public class Obstacle
{
    public int x;
    public int y;
    public string type;

    public Obstacle(int x, int y, string type)
    {
        this.x = x;
        this.y = y;
        this.type = type;
    }
}
