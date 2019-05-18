﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] public int roomGenNumber;

    //Dont use
    [SerializeField] public GameObject mainRoom;
    
    private GameObject _tempMainRoomInstantiation;
    private GameObject _grid;
    
    private List<GameObject> _rooms;

    // Start is called before the first frame update
    void Start()
    {
        Tilemap flagMap = GameObject.FindWithTag("Flag").GetComponent<Tilemap>();

        SetupMainRoom();
            
        //Loading all the saved room prefabs
        
        _rooms = LoadGameObjectRooms();
        //List<Door> doors = DoorSearch(flagMap);
        //Use rooms and flags to generate the level
        //GenerateRooms(roomGenNumber);
        //GenerateRooms(roomGenNumber);

    }
    
    void Awake () {
        _tempMainRoomInstantiation = Instantiate(mainRoom.gameObject);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void SetupMainRoom()
    {
        GameObject gridObject = new GameObject("Grid");
        GameObject roomHolder = new GameObject("[ROOM]Main");
        
        roomHolder.transform.parent = gridObject.transform;
        
        for (int i = _tempMainRoomInstantiation.transform.childCount - 1; i >= 0; i--)
        {
            var layer = _tempMainRoomInstantiation.transform.GetChild(i);
            // transfer layers to holder
            layer.transform.SetParent(roomHolder.transform, false);
        }
        
        //transfer all components
        var components = _tempMainRoomInstantiation.GetComponents<Component>();
        foreach (var component in components)
        {
            UnityEditorInternal.ComponentUtility.CopyComponent(component);
            UnityEditorInternal.ComponentUtility.PasteComponentAsNew(gridObject);
        }
        
        Destroy(_tempMainRoomInstantiation);
        Destroy(mainRoom);
        gridObject.tag = "Grid";
        
        _grid = gridObject;
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
            GameObject loadedRoom = Resources.Load<GameObject>(room_prefix +
                                                                filename.Substring(0, 
                                                                    filename.LastIndexOf(".")));
            GameObject createdRoom = Instantiate(loadedRoom);
            GameObject transformedRoom = new GameObject(loadedRoom.name);
            
            for (int i = createdRoom.transform.childCount - 1; i >= 0; i--)
            {
                var layer = createdRoom.transform.GetChild(i);
                layer.transform.SetParent(transformedRoom.transform, false);
            }
            
            transformedRoom.transform.SetParent(_grid.transform, false);
            
            Destroy(createdRoom);
            rooms.Add(transformedRoom);
        }

        return rooms;
    }

    void GenerateRooms(int roomGenNumber)
    {
        
    }

    public class Door
    {
        public Door(List<Vector3Int> tiles, Sprite flagTypes)
        {
            Tiles = tiles;
            type = flagTypes;
        }
        public List<Vector3Int> Tiles { get; set; }
        public Sprite type { get; set; }
    }
    
    public List<Door> DoorSearch(Tilemap tilemap)
    {
        TileBase[] tiles = tilemap.GetTilesBlock(tilemap.cellBounds);
        List<string> foundFlags = new List<string>();
        Dictionary<string, List<Vector3Int>> doors = new Dictionary<string, List<Vector3Int>>();
        
        foreach (var tilePosition in tilemap.cellBounds.allPositionsWithin)
        {   
            TileBase tile = tilemap.GetTile(tilePosition);
            if (tile == null) 
                continue;
            
            string spriteName = tilemap.GetSprite(tilePosition).name;
            if (!foundFlags.Contains(spriteName))
            {
                if (!doors.ContainsKey(spriteName))
                {
                    doors.Add(spriteName, new List<Vector3Int>());
                }
                
                foundFlags.Add(spriteName);
                FloodSearchTileMap(tilemap, doors[spriteName], tilePosition, spriteName);
            }
        }

        List<Door> doorList = new List<Door>(doors.Keys.Count);

        foreach (var door in doors)
        {
            Door DoorHolder = new Door(door.Value, FlagController.Instance.FindSpriteWithString(door.Key));
            doorList.Add(DoorHolder);
        }

        return doorList;
    }

    private static void FloodSearchTileMap(Tilemap tilemap, List<Vector3Int> doors, 
        Vector3Int start, string spriteName)
    {
        if (tilemap.GetSprite(start) == null)
            return;
        
        Debug.Log(tilemap.GetSprite(start).name);
        if ((tilemap.GetSprite(start).name != spriteName) || (doors.Contains(start)))
        {
            return;
        }

        doors.Add(start);
        
        
        FloodSearchTileMap(tilemap, doors, new Vector3Int(start.x + 1, start.y, start.z), spriteName);
        FloodSearchTileMap(tilemap, doors, new Vector3Int(start.x - 1, start.y, start.z), spriteName);
        FloodSearchTileMap(tilemap, doors, new Vector3Int(start.x, start.y + 1, start.z), spriteName);
        FloodSearchTileMap(tilemap, doors, new Vector3Int(start.x, start.y - 1, start.z), spriteName);

    }

}
