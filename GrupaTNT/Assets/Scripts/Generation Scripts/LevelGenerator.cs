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
    private int roomsToGen;
    
    //Dont use
    [SerializeField] public GameObject mainRoom;
    
    private Room[,] _roomGrid;
    private Vector2Int startingGridPostion;
    [SerializeField] public int gridWidthHeight;

    private GameObject _tempMainRoomInstantiation;
    private GameObject _gridGameObject;
    //private List<GameObject> _roomGameObjects;

    //index 0 -> 1 exit, 1 -> 2 exits, 2 -> 3 exits, 3 -> 4 exits
    private List<GameObject>[] GameObjectRoomsByExits;

    private Vector3Int roomMove = new Vector3Int(1000,1000,0);
    private Vector3Int ReverseRoomMove = new Vector3Int(-1000,-1000,0);

    private Vector3 modifier;
    // Start is called before the first frame update
    void Start()
    {
        //Grid init
        _roomGrid = new Room[gridWidthHeight, gridWidthHeight];

        roomsToGen = roomGenNumber;
        
        startingGridPostion = new Vector2Int(gridWidthHeight/2, gridWidthHeight/2);
        
        Tilemap flagMap = GameObject.FindWithTag("Flag").GetComponent<Tilemap>();

        //Create main room on grid
        var homeRoom = SetupMainRoom();

        //We get the modifier that we have to multiply delta movement by because of how Tiler scales things
        modifier = _gridGameObject.GetComponent<Grid>().cellSize;
        
        //Loading all the saved room prefabs
        var _roomGameObjects = LoadGameObjectRooms();
        GameObjectRoomsByExits = SortGameObjectRoomsBySize(_roomGameObjects);
        

        //Use rooms and flags to generate the level
        GenerateRoomsNew(startingGridPostion.x, startingGridPostion.y);

        //GenerateRooms(_roomGrid[startingGridPostion.x, startingGridPostion.y]);

    }
    
    void Awake () {
        _tempMainRoomInstantiation = Instantiate(mainRoom.gameObject);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private GameObject SetupMainRoom()
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
            roomHolder, startingGridPostion);
        _roomGrid[startingGridPostion.x, startingGridPostion.y] = room;
        _gridGameObject = gridObject;

        return _gridGameObject.transform.GetChild(0).gameObject;
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

    bool IsRoomOnGridEdge(Room room)
    {
        if ((room.GridPosition.x + 1) == gridWidthHeight)
            return true;
        if ((room.GridPosition.x) == 0)
            return true;
        if ((room.GridPosition.y + 1) == gridWidthHeight)
            return true;
        if ((room.GridPosition.y) == 0)
            return true;
    
        //Else not on edge
        return false;
    }

    void GenerateRoomsNew(int gridX, int gridY)
    {
        
    }
    
    /*
    void GenerateRooms(Room previousRoom)
    {
        if (roomsToGen == 0)
        {
            return;
        }

        if (IsRoomOnGridEdge(previousRoom))
        {
            return;
        }

        GameObject pickedRoom = null;
        Sprite randomDirectionSprite = null;

        do
        {
            pickedRoom = _roomGameObjects[Random.Range(0, _roomGameObjects.Count)];
            randomDirectionSprite = FlagController.Instance.GetRandomDirection();
        } while (!RoomHasOppositeDirection(randomDirectionSprite, pickedRoom)
        || previousRoom.IsConnected(randomDirectionSprite) 
        || !previousRoom.CanConnect(randomDirectionSprite));

        if(pickedRoom == null || randomDirectionSprite == null)
            throw new Exception("Room or Sprite is null in GenerateRooms");

        var oldConnectionDoor = FindConnectionDoor(previousRoom.Doors, randomDirectionSprite);

        //create clone room from template room
        var clonedRoom = Instantiate(pickedRoom, _gridGameObject.transform, true);
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
        
        clonedRoom.transform.Translate(new Vector3(modifier.x*deltaTilesMid.x + previousRoom.RealPosition.x, 
            modifier.y*deltaTilesMid.y + previousRoom.RealPosition.y,
            modifier.z*deltaTilesMid.z + previousRoom.RealPosition.z), Space.World);

        // Calculating the new room gridPosition
        Vector2Int newRoomGridPositionDelta = FlagController.Instance.DirectionToDeltaVector(randomDirectionSprite);
        Vector2Int newRoomGridPosition = previousRoom.GridPosition + newRoomGridPositionDelta;
        
        //Connect new room with old room 
        Room newRoom = new Room(newDoors, clonedRoom, newRoomGridPosition);
        newRoom.Connect(newConnectionDoor.type, previousRoom);
        
        //Connect old room with new room
        previousRoom.Connect(oldConnectionDoor.type, newRoom);

        _roomGrid[newRoomGridPosition.x, newRoomGridPosition.y] = newRoom;
        roomsToGen--;
        GenerateRooms(newRoom);
    }
    */

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

        public Dictionary<Sprite, bool> HasDoor;
        public List<Door> Doors;
        public Vector3 RealPosition { get; set; }
        public Vector2Int GridPosition { get; set; }
        public GameObject roomGameObject { get; set; }
        private Dictionary<Sprite, Room> ConnectedRoomDictionary;
        
        public Room(List<Door> doors, GameObject container, Vector2Int gridPosition)
        {
            Doors = doors;
            RealPosition = container.transform.position;
            GridPosition = gridPosition;
            roomGameObject = container;
            ConnectedRoomDictionary = new Dictionary<Sprite, Room>(4);
            HasDoor = new Dictionary<Sprite, bool>(4);

            //Ugly manual setting for each type of entrance
            ConnectedRoomDictionary[FlagController.Instance.DoorUp] = null;
            HasDoor[FlagController.Instance.DoorUp] = false;
            ConnectedRoomDictionary[FlagController.Instance.DoorDown] = null;
            HasDoor[FlagController.Instance.DoorDown] = false;
            ConnectedRoomDictionary[FlagController.Instance.DoorLeft] = null;
            HasDoor[FlagController.Instance.DoorLeft] = false;
            ConnectedRoomDictionary[FlagController.Instance.DoorRight] = null;
            HasDoor[FlagController.Instance.DoorRight] = false;
            
            foreach (var door in doors)
            {
                HasDoor[door.type] = true;
            }
        }

        public void Connect(Sprite sprite, Room other)
        {
            ConnectedRoomDictionary[sprite] = other;
        }

        public bool IsConnected(Sprite sprite)
        {
            try
            {
                return ConnectedRoomDictionary[sprite] != null;
            }
            catch (KeyNotFoundException e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public bool CanConnect(Sprite sprite)
        {
            return HasDoor[sprite] == true;
        }
        
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

    private List<GameObject>[] SortGameObjectRoomsBySize(List<GameObject> roomsGameObjects)
    {
        var sortedArray = new List<GameObject>[4];
        
        foreach (var roomGameObject in roomsGameObjects)
        {
            var flagTilemap = roomGameObject.transform.Find("Flags").GetComponent<Tilemap>();
            List<Door> doors = DoorSearch(flagTilemap);
            
            if (doors.Count == 0) throw new Exception("Room with 0 exits");
            
            if (sortedArray[doors.Count - 1] == null)
                sortedArray[doors.Count - 1] = new List<GameObject>();
                
            sortedArray[doors.Count - 1].Add(roomGameObject);
        }

        return sortedArray;
    }

    
}
