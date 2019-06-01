using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class SpawnController : Singleton<SpawnController>
{
    [SerializeField] public int gridHeightWidth;

    private LevelGenerator.Room[,] roomGrid;

    private List<string> enemyNames;
    
    // Start is called before the first frame update
    void Start()
    {
    }

    private void Awake()
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(Application.dataPath, "Resources/EnemyData"));
        FileInfo[] filenames = directoryInfo.GetFiles();

        //There could be other metafiles in the directory so we check how many room files we have.
        enemyNames = new List<string>();
        foreach (FileInfo filename in filenames)
        {
            if (filename.Name.EndsWith(".prefab"))
                enemyNames.Add(filename.Name);
        }

    }

    public void Initialize()
    {
        roomGrid = LevelGenerator.RoomGrid;   
    }
    
    public void SpawnForAllRooms()
    {
        for (int i = 0; i < gridHeightWidth; i++)
        {
            for (int j = 0; j < gridHeightWidth; j++)
            {
                SpawnRoomEnemies(new Vector2Int(i,j));
            }
        }
    }

    private void SpawnRoomEnemies(Vector2Int position)
    {
        var enemyWorldPositions = FindRoomEnemies(position);

        if (enemyWorldPositions == null) return;

        List<GameObject> enemies = new List<GameObject>();

        string room_prefix = "EnemyData/";

        foreach (var realPosition in enemyWorldPositions)
        {
            string pickedFile = enemyNames[Random.Range(0, enemyNames.Count)];

            GameObject loadedEnemy = Resources.Load<GameObject>(room_prefix +
                                                                pickedFile.Substring(0,
                                                                    pickedFile.LastIndexOf(".")));
            GameObject createdEnemy = Instantiate(loadedEnemy);
            createdEnemy.transform.Translate(roomGrid[position.y, position.x].roomGameObject.transform.position);
            createdEnemy.transform.Translate(realPosition);
            
            enemies.Add(createdEnemy);   
        }
    }

    private List<Vector3> FindRoomEnemies(Vector2Int position)
    {
        LevelGenerator.Room room;
        try
        {
            room = roomGrid[position.y, position.x];
        }
        catch
        {
            return null;
        }

        if (room == null) return null; 

        var tilemap = room.roomGameObject.transform.Find("Flags").GetComponent<Tilemap>();
        
        TileBase[] tiles = tilemap.GetTilesBlock(tilemap.cellBounds);
        List<Vector3> foundSpawns = new List<Vector3>();
        
        foreach (var tilePosition in tilemap.cellBounds.allPositionsWithin)
        {   
            TileBase tile = tilemap.GetTile(tilePosition);
            if (tile == null) 
                continue;
            
            var sprite = tilemap.GetSprite(tilePosition);

            // Check if the sprite is actually a door.
            if (!FlagController.Instance.EnemySpawn.name.Equals(sprite.name))
                continue;
            
            foundSpawns.Add(new Vector3(tilePosition.x * LevelGenerator.modifier.x, 
                tilePosition.y * LevelGenerator.modifier.y, 
                tilePosition.z * LevelGenerator.modifier.z));
        }

        return foundSpawns;
    }
    
}
