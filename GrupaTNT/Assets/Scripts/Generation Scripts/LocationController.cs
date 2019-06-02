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
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Initialize(Vector2Int locationOnRoomGrid, LevelGenerator.Room[,] roomGrid)
    {
        this.locationOnRoomGrid = locationOnRoomGrid;
        this.roomGrid = roomGrid;

        boundsDict = new Dictionary<LevelGenerator.Room, Bounds>();
        foreach (var room in roomGrid)
        {
            if(room != null)
                boundsDict[room] = room.roomGameObject.GetComponent<Tilemap>().localBounds;
        }

        isInitalized = true;
    }

    public void MovePosition(string spriteName)
    {
        if (spriteName != lastDirectionName)
        {
            var deltaVector = FlagController.Instance.DirectionToDeltaGridVector(spriteName);
            locationOnRoomGrid.x += deltaVector.x;
            locationOnRoomGrid.y += deltaVector.y;
            lastDirectionName = spriteName;
        }
        
        Debug.Log("Location of new room" + locationOnRoomGrid.ToString());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isInitalized)
        {
            foreach (var tuple in boundsDict)
            {
                if (tuple.Value.Contains(playerObject.transform.position))
                {
                    locationOnRoomGrid = tuple.Key.GridPosition;
                    break;
                }
            }
        }
    }
    
    
}
