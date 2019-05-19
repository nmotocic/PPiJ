using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] public int roomGenNumber;

    //Dont use
    [SerializeField] public GameObject mainRoom;
    
    private GameObject _tempMainRoomInstantiation;
    private GameObject _grid;
    
    private List<GameObject> _roomObjects;
    private List<Room> roomConnectionList = new List<Room>();
    
    private Vector3Int roomMove = new Vector3Int(1000,1000,0);
    private Vector3Int ReverseRoomMove = new Vector3Int(-1000,-1000,0);

    private Vector3 modifier;
    // Start is called before the first frame update
    void Start()
    {
        Tilemap flagMap = GameObject.FindWithTag("Flag").GetComponent<Tilemap>();

        //we get _grid
        SetupMainRoom();

        //We get the modifier that we have to multiply delta movement by because of how Tiler scales things
        modifier = _grid.GetComponent<Grid>().cellSize;
        
        //Loading all the saved room prefabs
        _roomObjects = LoadGameObjectRooms();
        List<Door> doors = DoorSearch(flagMap);
        
        //Get home room
        var homeRoom = _grid.transform.GetChild(0).gameObject;
        //Use rooms and flags to generate the level
        GenerateRooms(roomGenNumber, roomConnectionList[0]);

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
        
        Room room = new Room(DoorSearch(roomHolder.transform.Find("Flags").GetComponent<Tilemap>()), 
            roomHolder);
        roomConnectionList.Add(room);
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
            
            Destroy(createdRoom);
            
            transformedRoom.transform.Translate(roomMove);
            rooms.Add(transformedRoom);
        }

        return rooms;
    }

    void GenerateRooms(int roomGenNumber, Room previousRoom)
    {
        if (roomGenNumber == 0)
        {
            return;
        }

        GameObject pickedRoom = null;
        Sprite randomDirectionSprite = null;

        do
        {
            pickedRoom = _roomObjects[Random.Range(0, _roomObjects.Count)];
            randomDirectionSprite = FlagController.Instance.GetRandomDirection();
        } while (!RoomHasOppositeDirection(randomDirectionSprite, pickedRoom)
        || (previousRoom.IsConnected(randomDirectionSprite) && previousRoom.CanConnect(randomDirectionSprite)));

        if(pickedRoom == null || randomDirectionSprite == null)
            throw new Exception("Room or Sprite is null in GenerateRooms");

        var oldConnectionDoor = FindConnectionDoor(previousRoom.Doors, randomDirectionSprite);

        //create clone room from template room
        var clonedRoom = Instantiate(pickedRoom, _grid.transform, true);
        //reverse original transition
        clonedRoom.transform.Translate(ReverseRoomMove);

        var newDoors = DoorSearch(clonedRoom.transform.Find("Flags").GetComponent<Tilemap>());
        var oppositeSprite = FlagController.Instance.GetOppositeDirection(randomDirectionSprite.name);

        var newConnectionDoor = FindConnectionDoor(newDoors, oppositeSprite);

        var doorTilesNew = newConnectionDoor.Tiles[0];
        var doorTilesOld = oldConnectionDoor.Tiles[0];
        var deltaTiles = doorTilesOld - doorTilesNew;
        
        Vector3 deltaTilesMid = new Vector3(
            Mathf.Clamp(deltaTiles.x, -1, 1)*(-1) + deltaTiles.x,
            Mathf.Clamp(deltaTiles.y, -1, 1)*(-1) + deltaTiles.y,
            deltaTiles.z);
        
        clonedRoom.transform.Translate(new Vector3(modifier.x*deltaTilesMid.x + previousRoom.Position.x, 
            modifier.y*deltaTilesMid.y + previousRoom.Position.y,
            modifier.z*deltaTilesMid.z + previousRoom.Position.z), Space.World);

        //Connect new room with old room 
        Room newRoom = new Room(newDoors, clonedRoom);
        newRoom.Connect(newConnectionDoor.type, previousRoom);
        
        //Connect old room with new room
        previousRoom.Connect(oldConnectionDoor.type, newRoom);
        
        roomConnectionList.Add(newRoom);

        GenerateRooms(roomGenNumber - 1, newRoom);
    }

    private Door FindConnectionDoor(List<Door> doors, Sprite randomDirectionSprite)
    {
        Door ConnectionDoor = null;

        foreach (var door in doors)
        {
            if (door.type.name == randomDirectionSprite.name)
            {
                ConnectionDoor = door;
                break;
            }
        }

        if (ConnectionDoor == null)
            throw new Exception("no connection found");
        return ConnectionDoor;
    }

    bool RoomHasOppositeDirection(Sprite original, GameObject roomCompare)
    {
        var oppositeSprite = FlagController.Instance.GetOppositeDirection(original.name);
        return RoomHasDirection(roomCompare, oppositeSprite);
    }

    bool RoomHasDirection(GameObject room, Sprite sprite)
    {
        var flags = FindFlagLayer(room);

        if (flags == null)
        {
           throw new Exception("No flag layer found. Maybe tag missing?");
        }

        var doors = DoorSearch(flags.GetComponent<Tilemap>());

        foreach (var door in doors)
        {
            if (sprite.name.Equals(door.type.name))
            {
                return true;
            }
        }

        return false;
    }

    private GameObject FindFlagLayer(GameObject room)
    {
        GameObject flags = null;
        for (int i = 0; i < room.transform.childCount; i++)
        {
            var layer = room.transform.GetChild(i);
            if (layer.CompareTag("Flag"))
            {
                flags = layer.gameObject;
            }
        }

        return flags;
    }

    public class Room
    {
        public Room(List<Door> doors, GameObject container)
        {
            Doors = doors;
            Position = container.transform.position;
            Container = container;
            ConnectedRoomDictionary = new Dictionary<Sprite, Room>(4);
            HasDoor = new Dictionary<Sprite, bool>(4);

            foreach (var door in doors)
            {
                ConnectedRoomDictionary[door.type] = null;
                HasDoor[door.type] = true;
            }
        }

        public void Connect(Sprite sprite, Room other)
        {
            ConnectedRoomDictionary[sprite] = other;
        }

        public bool IsConnected(Sprite sprite)
        {
            return ConnectedRoomDictionary[sprite] != null;
        }

        public bool CanConnect(Sprite sprite)
        {
            return HasDoor[sprite] == true;
        }

        public Dictionary<Sprite, bool> HasDoor;
        public List<Door> Doors;
        public Vector3 Position { get; set; }
        public GameObject Container { get; set; }
        private Dictionary<Sprite, Room> ConnectedRoomDictionary;

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
        
        //Debug.Log(tilemap.GetSprite(start).name);
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
