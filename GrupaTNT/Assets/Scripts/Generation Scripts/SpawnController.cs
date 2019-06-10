using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;
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
    
    // Not going to be used
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
            string pickedFile = powerupNames[Random.Range(0, powerupNames.Count - 1)];

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

            string room_prefix = "EnemyData/";

            foreach (var realPosition in enemyWorldPositions)
            {
                //find all needed difficulty enemies
                var difficultyEnemies = enemyNames.FindAll(
                    new Predicate<string>(s => 
                        s.Substring(0, s.LastIndexOf(".prefab")).
                            EndsWith(_levelManager.DifficultyLevel.ToString())));
                
                Debug.LogWarning("different difficulties: " + difficultyEnemies.Count.ToString());
                
                string pickedFile = difficultyEnemies[Random.Range(0, difficultyEnemies.Count - 1)];
                
                GameObject loadedEnemy = Resources.Load<GameObject>(room_prefix +
                                                                    pickedFile.Substring(0,
                                                                        pickedFile.LastIndexOf(".")));
                GameObject createdEnemy = Instantiate(loadedEnemy);
                
                createdEnemy.transform.Translate(roomGrid[position.y, position.x].roomGameObject.transform.position);
                createdEnemy.transform.Translate(realPosition);

                roomGrid[position.y, position.x].enemies.Add(createdEnemy);
            }
        }
    }

    private void SpawnBoss(Vector2Int position)
    {
        if (roomGrid[position.y, position.x] == null) return;
        
        if (roomGrid[position.y, position.x].bossRoom)
        {
            var RoomGameObject = roomGrid[position.y, position.x].roomGameObject;
            var center = RoomGameObject.transform.Find("Floor")
                .GetComponent<Tilemap>().localBounds.center;
            
            Debug.LogWarning("Spawned boss in room:" + position.y.ToString() + " " + position.x.ToString() 
                      + " " + roomGrid[position.y, position.x].roomGameObject.name);
            
            string room_prefix = "BossData/";

            //find all needed difficulty enemies
            var difficultyBoss = bossNames.FindAll(
                new Predicate<string>(s => 
                    s.Substring(0, s.LastIndexOf(".prefab")).
                        EndsWith(_levelManager.DifficultyLevel.ToString())));
            
            string pickedFile = difficultyBoss[Random.Range(0, difficultyBoss.Count - 1)];

            GameObject loadedBoss = Resources.Load<GameObject>(room_prefix +
                                                               pickedFile.Substring(0,
                                                                   pickedFile.LastIndexOf(".")));
            GameObject createdBoss = Instantiate(loadedBoss);

            createdBoss.transform.Translate(roomGrid[position.y, position.x].roomGameObject.transform.position);
            createdBoss.transform.Translate(center);

            roomGrid[position.y, position.x].boss.Add(createdBoss);
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
