using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LocationController : MonoBehaviour
{
    private Vector2Int locationOnRoomGrid;
    private LevelGenerator.Room[,] roomGrid;
    private Dictionary<LevelGenerator.Room, Bounds> boundsDict;
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
                        break;
                    }
                }

                Debug.Log("Location of new room" + locationOnRoomGrid.ToString());
            }
        }
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
