using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

public class LocationController : MonoBehaviour
{
    public Vector2Int locationOnRoomGrid;
    private LevelGenerator.Room[,] roomGrid;
    private Dictionary<LevelGenerator.Room, Bounds> boundsDict;
    
    // Maybe rename to alreadyAwoken, old name leftover
    private bool[,] alreadySpawned;

    private GameObject playerObject;

    private string lastDirectionName;
    private bool isInitalized = false;

    private float timeStep = 0.2f;
    private float time = 0.0f;

    private SpawnController _spawnController;

    public void Initialize(Vector2Int locationOnRoomGrid, LevelGenerator.Room[,] roomGrid, GameObject player,
        SpawnController spawnController)
    {
        this.locationOnRoomGrid = locationOnRoomGrid;
        this.roomGrid = roomGrid;
        alreadySpawned = new bool[roomGrid.GetLength(1), roomGrid.GetLength(0)];

        playerObject = player;

        boundsDict = new Dictionary<LevelGenerator.Room, Bounds>();
        foreach (var room in roomGrid)
        {
            if (room != null)
            {
                var tilemap = room.roomGameObject.transform.Find("Walls").GetComponent<Tilemap>();
                var bounds = tilemap.localBounds;
                var worldBounds = new Bounds(tilemap.LocalToWorld(bounds.center),
                    tilemap.localBounds.size);

                boundsDict[room] = worldBounds;
            }
        }

        _spawnController = spawnController;

        isInitalized = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isInitalized)
        {
            time += Time.deltaTime;
            if (time >= timeStep)
            {
                time = 0.0f;

                foreach (var tuple in boundsDict)
                {
                    var bounds = tuple.Value;
                    if (bounds.Contains(playerObject.transform.position))
                    {
                        locationOnRoomGrid = tuple.Key.GridPosition;

                        TurnOnMovementScriptsForEnemies(locationOnRoomGrid);
                        TurnOnMovementScriptsForBoss(locationOnRoomGrid);
                        break;
                    }
                }

                Debug.Log("Location of new room" + locationOnRoomGrid.ToString());
            }
        }
    }
    
    // Enemies and boss are split because of difference in the original implementation, can be combined now
    void TurnOnMovementScriptsForEnemies(Vector2Int position)
    {
        if (roomGrid[position.y, position.x] == null || alreadySpawned[position.y, position.x])
            return;

        if (!alreadySpawned[position.y, position.x] && !roomGrid[position.y, position.x].bossRoom)
        {
            foreach (var enemyGameObject in roomGrid[position.y, position.x].enemies)
            {
                if (enemyGameObject == null) continue;
                enemyGameObject.GetComponent<NavMeshAgent>().enabled = true;
                enemyGameObject.GetComponent<AiScriptBase>().enabled = true;
            }
            alreadySpawned[position.y, position.x] = true;
        }
    }
    
    void TurnOnMovementScriptsForBoss(Vector2Int position)
    {
        if (roomGrid[position.y, position.x] == null || alreadySpawned[position.y, position.x])
            return;

        // Not used anymore, we dont want the boss waking up when we enter the room
        /*
        if (!alreadySpawned[position.y, position.x] && roomGrid[position.y, position.x].bossRoom)
        {
            var BossGameObject = roomGrid[position.y, position.x].boss[0];
            //BossGameObject.GetComponent<NavMeshAgent>().enabled = true;
            BossGameObject.GetComponent<EntityScript>().enabled = true;
            BossGameObject.GetComponent<MinoBossAI>().enabled = true;
            alreadySpawned[position.y, position.x] = true;
        }*/
    }

    void CheckSpawn(Vector2Int position)
    {
        if (roomGrid[position.y, position.x] == null)
            return;
        
        if (!alreadySpawned[position.y, position.x])
        {
            _spawnController.SpawnForSingleRoom(position);
            alreadySpawned[position.y, position.x] = true;
        }
    }
}
