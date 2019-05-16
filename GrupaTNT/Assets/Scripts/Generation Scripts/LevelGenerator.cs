using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] public int roomGenNumber;

    [SerializeField] public GameObject grid;
    
    private List<GameObject> _rooms;

    // Start is called before the first frame update
    void Start()
    {
        GameObject flagChild = grid.transform.Find("Flags").gameObject;
        Tilemap flagMap = flagChild.GetComponent<Tilemap>();
    
        //Loading all the saved room prefabs
        _rooms = LoadGameObjectRooms();
        DoorSearch(flagMap);
        //Use rooms and flags to generate the level
        //GenerateRooms(roomGenNumber);

    }

    // Update is called once per frame
    void Update()
    {

    }

    List<GameObject> LoadGameObjectRooms()
    {
        GameObject gridObject = GameObject.FindWithTag("Grid");
        DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(Application.dataPath, "Resources/PremadeRooms"));
        FileInfo[] filenames = directoryInfo.GetFiles();

        //There could be other metafiles in the directory so we check how many room files we have.
        List<string> roomNames = new List<string>();
        foreach (FileInfo filename in filenames)
        {
            if (filename.Name.StartsWith("[ROOM]") && filename.Name.EndsWith(".prefab"))
                roomNames.Add(filename.Name);
        }

        List<GameObject> rooms = new List<GameObject>();

        string room_prefix = "PremadeRooms/";

        foreach (string filename in roomNames)
        {
            GameObject createdRoom = Resources.Load<GameObject>(room_prefix +
                                                                filename.Substring(0, filename.LastIndexOf(".")));
            rooms.Add(createdRoom);
        }

        return rooms;
    }

    void GenerateRooms(int roomGenNumber)
    {
    }

    public class Door
    {
        public Door(List<TileBase> tiles)
        {
            Tiles = tiles;
        }
        public List<TileBase> Tiles { get; set; }
    }
    
    public static List<Door> DoorSearch(Tilemap tilemap)
    {
        TileBase[] tiles = tilemap.GetTilesBlock(tilemap.cellBounds);
        List<string> foundFlags = new List<string>();
        Dictionary<string, List<TileBase>> doors = new Dictionary<string, List<TileBase>>();
        
        foreach (var tilePosition in tilemap.cellBounds.allPositionsWithin)
        {   
            TileBase tile = tilemap.GetTile(tilePosition);
            if (tile == null) continue;
            
            string spriteName = tilemap.GetSprite(tilePosition).name;
            if (!foundFlags.Contains(spriteName))
            {
                foundFlags.Add(spriteName);
                FloodSearchTileMap(tilemap, doors, tilePosition, spriteName);
            }
        }
        
        return null; 
    }

    private static void FloodSearchTileMap(Tilemap tilemap, Dictionary<string, List<TileBase>> doors, 
        Vector3Int start, string spriteName)
    {
        
        Debug.Log(tilemap.GetSprite(start).name);
        if (tilemap.GetSprite(start).name == spriteName)
        {
          
        }

        return;

        FloodSearchTileMap(tilemap, doors, new Vector3Int(start.x + 1, start.y, start.z), spriteName);
        FloodSearchTileMap(tilemap, doors, new Vector3Int(start.x - 1, start.y, start.z), spriteName);
        FloodSearchTileMap(tilemap, doors, new Vector3Int(start.x, start.y + 1, start.z), spriteName);
        FloodSearchTileMap(tilemap, doors, new Vector3Int(start.x, start.y - 1, start.z), spriteName);
        FloodSearchTileMap(tilemap, doors, new Vector3Int(start.x, start.y, start.z + 1), spriteName);
        FloodSearchTileMap(tilemap, doors, new Vector3Int(start.x, start.y, start.z - 1), spriteName);
        
    }

}
