﻿using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class SpawnController : MonoBehaviour
{
    private int gridHeightWidth;

    private LevelGenerator.Room[,] roomGrid;

    private List<string> enemyNames;
    private List<string> powerupNames;
    private List<string> bossNames;
    private string playerName;

    private FlagController _controller;
    private LevelManager _levelManager;

    // Start is called before the first frame update
    void Start()
    {
    }

    private void Awake()
    {
        enemyNames = GetPrefabsNames("EnemyData");
        bossNames = GetPrefabsNames("BossData");
        powerupNames = GetPrefabsNames("PowerupData");

        DirectoryInfo directoryInfoPlayer = new DirectoryInfo(Path.Combine(Application.dataPath, "Resources/PlayerData"));
        FileInfo[] filenamesPlayer = directoryInfoPlayer.GetFiles();

        playerName = filenamesPlayer[0].Name;
    }

    private List<string> GetPrefabsNames(string folderName)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(Application.dataPath, "Resources/" + folderName));
        FileInfo[] filenames = directoryInfo.GetFiles();

        //There could be other metafiles in the directory so we check how many room files we have.
        var names = new List<string>();
        foreach (FileInfo filename in filenames)
        {
            if (filename.Name.EndsWith(".prefab"))
            {
                names.Add(filename.Name);
            }
        }

        return names;
    }

    public void Initialize()
    {
        roomGrid = LevelGenerator.RoomGrid;
        gridHeightWidth = gameObject.GetComponent<LevelGenerator>().gridWidthHeight;
        _controller = GameObject.FindWithTag("Manager").GetComponent<FlagController>();
        _levelManager = LevelManager.Instance;
    }

    public static void MoveObjectToRoomCenter(GameObject player, LevelGenerator.Room room)
    {
        player.transform.position = new Vector3(room.roomGameObject.transform
            .Find("Floor").GetComponent<Tilemap>().localBounds.center.x, room.roomGameObject.transform
            .Find("Floor").GetComponent<Tilemap>().localBounds.center.y, 0);

    }
    
    public GameObject SpawnPlayerInRoomCenter(LevelGenerator.Room room)
    {
        string room_prefix = "PlayerData/";
        
            string pickedFile = playerName;

            GameObject loadedPlayer = Resources.Load<GameObject>(room_prefix +
                                                                  pickedFile.Substring(0,
                                                                      pickedFile.LastIndexOf(".")));

            loadedPlayer.transform.position = new Vector3(room.roomGameObject.transform
                .Find("Floor").GetComponent<Tilemap>().localBounds.center.x, room.roomGameObject.transform
                .Find("Floor").GetComponent<Tilemap>().localBounds.center.y, 0);


            var spawnedPlayer = Instantiate(loadedPlayer);

            return spawnedPlayer;
    }

    // TODO REPLACE WITH PER ROOM SPAWNING
    public void SpawnForAllRooms()
    {
        for (int i = 0; i < gridHeightWidth; i++)
        {
            for (int j = 0; j < gridHeightWidth; j++)
            {
                var vec = new Vector2Int(i, j);
                SpawnRoomEnemies(vec);
                SpawnRoomPowerUps(vec);
                SpawnBoss(vec);
            }
        }
    }
    
    public void SpawnForSingleRoom(Vector2Int gridPosition)
    {
        SpawnRoomEnemies(gridPosition);
        SpawnRoomPowerUps(gridPosition);
        SpawnBoss(gridPosition);
    }

    private void SpawnRoomPowerUps(Vector2Int position)
    {
        var flagWorldPositions = FindRoomPowerups(position);

        if (flagWorldPositions == null) return;

        List<GameObject> powerUpObjects = new List<GameObject>();

        string room_prefix = "PowerupData/";

        foreach (var realPosition in flagWorldPositions)
        {
            string pickedFile = powerupNames[Random.Range(0, powerupNames.Count)];

            GameObject loadedPowerup = Resources.Load<GameObject>(room_prefix +
                                                                pickedFile.Substring(0,
                                                                    pickedFile.LastIndexOf(".")));
            GameObject createdPowerup = Instantiate(loadedPowerup);
            createdPowerup.transform.Translate(roomGrid[position.y, position.x].roomGameObject.transform.position);
            createdPowerup.transform.Translate(realPosition);
            
            powerUpObjects.Add(createdPowerup);
        }
    }

    private void SpawnRoomEnemies(Vector2Int position)
    {
        if (roomGrid[position.y, position.x] == null) return;

        if (!roomGrid[position.y, position.x].bossRoom)
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
    }

    private void SpawnBoss(Vector2Int position)
    {
        if (roomGrid[position.y, position.x] == null) return;
        
        if (roomGrid[position.y, position.x].bossRoom)
        {
            var RoomGameObject = roomGrid[position.y, position.x].roomGameObject;
            var enemyWorldPositions = RoomGameObject.transform.Find("Floor")
                .GetComponent<Tilemap>().localBounds.center;
            
            Debug.LogWarning("Spawned boss in room:" + position.y.ToString() + " " + position.x.ToString() 
                      + " " + roomGrid[position.y, position.x].roomGameObject.name);
            
            GameObject boss;

            string room_prefix = "BossData/";

            string pickedFile = bossNames[Random.Range(0, bossNames.Count)];

            GameObject loadedBoss = Resources.Load<GameObject>(room_prefix +
                                                               pickedFile.Substring(0,
                                                                   pickedFile.LastIndexOf(".")));
            GameObject createdBoss = Instantiate(loadedBoss);
            createdBoss.transform.Translate(roomGrid[position.y, position.x].roomGameObject.transform.position);
            createdBoss.transform.Translate(enemyWorldPositions);

            boss = createdBoss;
        }
    }

    private List<Vector3> FindRoomFlags(Vector2Int position, string flagName)
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
        List<Vector3> foundFlags = new List<Vector3>();
        
        foreach (var tilePosition in tilemap.cellBounds.allPositionsWithin)
        {   
            TileBase tile = tilemap.GetTile(tilePosition);
            if (tile == null) 
                continue;
            
            var sprite = tilemap.GetSprite(tilePosition);

            // Check if the sprite is actually a door.
            if (!flagName.Equals(sprite.name))
                continue;
            
            foundFlags.Add(new Vector3(tilePosition.x * LevelGenerator.modifier.x, 
                tilePosition.y * LevelGenerator.modifier.y, 
                tilePosition.z * LevelGenerator.modifier.z));
        }

        return foundFlags;
    }

    private List<Vector3> FindRoomEnemies(Vector2Int position)
    {
        return FindRoomFlags(position, _controller.EnemySpawn.name);
    }

    private List<Vector3> FindRoomPowerups(Vector2Int position)
    {
        return FindRoomFlags(position, _controller.PowerUpSpawn.name);
    }

}
