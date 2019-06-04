using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LocationController : MonoBehaviour
{
    private Vector2Int locationOnRoomGrid;
    private LevelGenerator.Room[,] roomGrid;
    private Dictionary<LevelGenerator.Room, Bounds> boundsDict;

    [SerializeField] public GameObject playerObject;

    private string lastDirectionName;
    private bool isInitalized = false;

    public void Initialize(Vector2Int locationOnRoomGrid, LevelGenerator.Room[,] roomGrid)
    {
        this.locationOnRoomGrid = locationOnRoomGrid;
        this.roomGrid = roomGrid;
        
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

        isInitalized = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isInitalized)
        {
            foreach (var tuple in boundsDict)
            {
                var bounds = tuple.Value;
                if (tuple.Value.Contains(playerObject.transform.position))
                {
                    locationOnRoomGrid = tuple.Key.GridPosition;
                    break;
                }
            }
            
            Debug.Log("Location of new room" + locationOnRoomGrid.ToString());
        }
    }
    
    
}
