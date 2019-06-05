using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] public int roomGenNumber;
    private int roomsToGen;
    [SerializeField] public int branchMaxLength;
    
    //Dont use
    [SerializeField] public GameObject mainRoom;
    
    public static Room[,] RoomGrid;
    private Vector2Int startingGridPostion;
    [SerializeField] public int gridWidthHeight;

    private GameObject _tempMainRoomInstantiation;
    private GameObject _gridGameObject;
    //private List<GameObject> _roomGameObjects;

    //index 0 -> 1 exit, 1 -> 2 exits, 2 -> 3 exits, 3 -> 4 exits
    private List<GameObject>[] GameObjectRoomsByExits;

    private Vector3Int roomMove = new Vector3Int(1000,1000,0);
    private Vector3Int ReverseRoomMove = new Vector3Int(-1000,-1000,0);

    public static Vector3 modifier;

    private Vector2Int[] deltaVectors;
    private Sprite[] directionsDelta;

    private bool madeBossRoom = false;
    
    // Start is called before the first frame update
    void Start()
    {
        //Grid init
        RoomGrid = new Room[gridWidthHeight, gridWidthHeight];
        
        roomsToGen = roomGenNumber;

        startingGridPostion = new Vector2Int(gridWidthHeight / 2, gridWidthHeight / 2);
        Debug.Log("Starting grid location " + startingGridPostion.ToString());
        
        deltaVectors = FlagController.Instance.deltaVectors;
        directionsDelta = FlagController.Instance.directionsDelta;
        
        //Create main room on grid
        var homeRoom = SetupMainRoom();

        var playerGameObject = GameObject.Find("Player");
        SpawnController.MoveObjectToRoomCenter(playerGameObject, homeRoom);

        //We get the modifier that we have to multiply delta movement by because of how Tiler scales things
        modifier = _gridGameObject.GetComponent<Grid>().cellSize;

        //Loading all the saved room prefabs
        var _roomGameObjects = LoadGameObjectRooms();
        GameObjectRoomsByExits = SortGameObjectRoomsBySize(_roomGameObjects);

        var actionList = new[]
        {
            new Action( () =>
            {
                GenerateRoomsNew(startingGridPostion.x + deltaVectors[0].x, 
                    startingGridPostion.y + deltaVectors[0].y, directionsDelta[0], homeRoom, branchMaxLength);  
            }),
            new Action( () =>
            {
                GenerateRoomsNew(startingGridPostion.x + deltaVectors[1].x, 
                    startingGridPostion.y + deltaVectors[1].y, directionsDelta[1], homeRoom, branchMaxLength);  
            }),
            new Action( () =>
            {
                GenerateRoomsNew(startingGridPostion.x + deltaVectors[2].x, 
                    startingGridPostion.y + deltaVectors[2].y, directionsDelta[2], homeRoom, branchMaxLength);  
            }),
            new Action( () =>
            {
                GenerateRoomsNew(startingGridPostion.x + deltaVectors[3].x, 
                    startingGridPostion.y + deltaVectors[3].y, directionsDelta[3], homeRoom, branchMaxLength);  
            })
        }.ToList();
        
        Shuffle(actionList);
        for (int i = 0; i < actionList.Count; i++)
        {
            var action = actionList[i];
            action();    
        }

        PatchLeftovers();

        /**
        //Pretty print
        int rowLength = RoomGrid.GetLength(0);
        int colLength = RoomGrid.GetLength(1);
        string arrayString = "";
        for (int i = 0; i < rowLength; i++)
        {
            for (int j = 0; j < colLength; j++)
            {
                arrayString += string.Format("{0} ", RoomGrid[i, j] == null ? "None" : RoomGrid[i,j].roomGameObject.name);
                arrayString += "     ";
            }
            arrayString += Environment.NewLine + Environment.NewLine;
        }
        **/

        AddNavMeshModifierToWalls();
        AddTagToEveryRoomWall();
        BakeNavMesh();
        
        this.gameObject.GetComponent<SpawnController>().Initialize();
        this.gameObject.GetComponent<SpawnController>().SpawnForAllRooms();

        this.gameObject.GetComponent<LocationController>().Initialize(startingGridPostion, RoomGrid);

        RemoveFlagRendering();

        Debug.Assert(madeBossRoom);
    }

    private void BakeNavMesh()
    {
        var navmeshObject = GameObject.Find("NavMesh Surface");
        var navmeshComponent = navmeshObject.GetComponent<NavMeshSurface2d>();
        
        navmeshComponent.BuildNavMesh();
    }

    private void AddNavMeshModifierToWalls()
    {
        AddComponentToRooms(typeof(NavMeshModifier), "Walls", SetUnwalkableOverride);
    }
    private void SetUnwalkableOverride(Component component)
    {
        var Modcomponent = component as NavMeshModifier;
        Modcomponent.overrideArea = true;
        Modcomponent.area = NavMesh.GetAreaFromName("Not Walkable");
    }

    private void AddTagToEveryRoomWall()
    {
        AddLayerTagToEveryRoomObject("Obstruction", "Walls");
    }

    private void RemoveFlagRendering()
    {
        foreach (var room in RoomGrid)
        {
            if (room == null) continue;

            room.roomGameObject.transform.Find("Flags").GetComponent<Renderer>().enabled = false;
        }
    }

    private void AddComponentToRooms(Type component, string layerName, Action<Component> componentSetup)
    {
        foreach (var room in RoomGrid)
        {
            if (room == null) continue;
            
            room.roomGameObject.transform.Find(layerName).gameObject.AddComponent(component);
            componentSetup(room.roomGameObject.transform.Find(layerName).gameObject.GetComponent(component));
        }
    }

    private void AddLayerTagToEveryRoomObject(string tag, string layerName)
    {
        foreach (var room in RoomGrid)
        {    
            if (room == null) continue;

            room.roomGameObject.transform.Find(layerName).gameObject.tag = tag;
        }
    }

    private void PatchLeftovers()
    {
        for(int j = 0; j < gridWidthHeight; j++)
        {
            for (int i = 0; i < gridWidthHeight; i++)
            {
                if (RoomGrid[j,i] != null)
                    continue;
                
                Dictionary<string, Vector2Int> positionDict = new Dictionary<string, Vector2Int>();

                foreach (var direction in directionsDelta)
                {
                    var deltaVector = FlagController.Instance.DirectionToDeltaGridVector(direction.name);
                    positionDict[direction.name] = new Vector2Int(i + deltaVector.x, j + deltaVector.y);
                }

                List<string> unconnectedNeighbors = FindUnconnectedNeighbors(new Vector2Int(i, j));
                
                if (unconnectedNeighbors.Count == 0)
                {
                    continue;
                }
                
                var availableRooms = FindGameObjectRoomsWithExits(unconnectedNeighbors);
                var pickedRoom = availableRooms[Random.Range(0, availableRooms.Count)];

                Door oldConnectionDoor;
                Room connectRoom;
                
                try
                {
                    var connectedRoomLocation = positionDict[unconnectedNeighbors[0]];
                    
                    connectRoom = RoomGrid[connectedRoomLocation.y, connectedRoomLocation.x];
                    oldConnectionDoor = FindConnectionDoor(connectRoom.Doors, 
                        FlagController.Instance.GetOppositeDirection(unconnectedNeighbors[0]));

                }
                catch (Exception e)
                {
                    return;
                }

                //create clone room from template room
                var clonedRoom = Instantiate(pickedRoom, _gridGameObject.transform, true);
                clonedRoom.transform.Find("Flags").gameObject.tag = "Flag";
                //reverse original transition
                clonedRoom.transform.Translate(ReverseRoomMove);

                var newDoors = DoorSearch(clonedRoom.transform.Find("Flags").GetComponent<Tilemap>());
                var oppositeSprite = FlagController.Instance.FindSpriteWithString(unconnectedNeighbors[0]);
                
                Door newConnectionDoor = null;
                try
                {
                    newConnectionDoor = FindConnectionDoor(newDoors, FlagController.Instance.FindSpriteWithString(unconnectedNeighbors[0]));
                }
                catch
                {
                    //Debug.LogWarning("Wanted connection for room not found!!!!");
                }

                var doorTilesNew = newConnectionDoor.Tiles[0];
                var doorTilesOld = oldConnectionDoor.Tiles[0];
                var deltaTiles = doorTilesOld - doorTilesNew;
            
                Vector3 deltaTilesMid = new Vector3(
                    Mathf.Clamp(deltaTiles.x, -1, 1)*(-1) + deltaTiles.x,
                    Mathf.Clamp(deltaTiles.y, -1, 1)*(-1) + deltaTiles.y,
                    deltaTiles.z);
       
                
                clonedRoom.transform.Translate(new Vector3(modifier.x*deltaTilesMid.x + connectRoom.RealPosition.x, 
                    modifier.y*deltaTilesMid.y + connectRoom.RealPosition.y,
                    modifier.z*deltaTilesMid.z + connectRoom.RealPosition.z), Space.World);

                bool bossRoom = false;
                if (madeBossRoom == false)
                {
                    //Is it a hallway?
                    if (!pickedRoom.name.EndsWith("_2"))
                    {
                        bossRoom = true;
                        Debug.Log("Made boss room");
                        madeBossRoom = true;
                    }
                }
                
                var newRoom = new Room(newDoors, clonedRoom, new Vector2Int(i, j), bossRoom);
                RoomGrid[newRoom.GridPosition.y, newRoom.GridPosition.x] = newRoom;
            }
        }
    }

    private List<GameObject> FindGameObjectRoomsWithExits(List<string> exits)
    {
        List<GameObject> listOfRoomsInCategory = null;
        try
        {
            listOfRoomsInCategory = GameObjectRoomsByExits[exits.Count - 1];
        }
        catch (Exception e)
        {
            throw new Exception("Error while looking for rooms, wrong index. " + e.Message);
        }

        List<GameObject> foundRooms = new List<GameObject>();
        foreach (var room in listOfRoomsInCategory)
        {
            if (RoomHasAllDirections(room, exits))
                foundRooms.Add(room);
        }

        return foundRooms;
    }
    
    

    private List<string> FindUnconnectedNeighbors(Vector2Int selfPosition)
    {
        Room foundRoom = RoomGrid[selfPosition.y, selfPosition.x];
        if (foundRoom != null) 
            return null;
        
        Dictionary<string, Vector2Int> positionDict = new Dictionary<string, Vector2Int>();

        foreach (var direction in directionsDelta)
        {
            var deltaVector = FlagController.Instance.DirectionToDeltaGridVector(direction.name);
            positionDict[direction.name] = new Vector2Int(selfPosition.x + deltaVector.x, selfPosition.y + deltaVector.y);
        }

        List<string> unconnectedDirections = new List<string>();
        
        foreach (var tuple in positionDict)
        {
            Room checkRoom = null;
            try
            {
                checkRoom = RoomGrid[tuple.Value.y, tuple.Value.x];
            }
            catch
            {
                continue;
                //throw new Exception("Went out off grid");
            }

            if(checkRoom == null)
                continue;

            if (RoomHasOppositeDirection(tuple.Key, checkRoom.roomGameObject))
            {
                unconnectedDirections.Add(tuple.Key);
            }
        }

        return unconnectedDirections;
    }

    void Awake () {
        _tempMainRoomInstantiation = Instantiate(mainRoom.gameObject);
    }

    private Room SetupMainRoom()
    {
        GameObject gridObject = new GameObject("Grid");
        GameObject roomHolder = new GameObject("ROOM_Main");
        
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
            roomHolder, startingGridPostion, false);
        RoomGrid[startingGridPostion.y, startingGridPostion.x] = room;
        _gridGameObject = gridObject;
        
        room.roomGameObject.transform.Find("Flags").gameObject.tag = "Flag";
        roomHolder.transform.Find("Flags").gameObject.AddComponent<LocationController>();
        roomHolder.transform.Find("Flags").gameObject.AddComponent<TilemapCollider>();
        //roomHolder.transform.Find("Flags").gameObject.GetComponent<TilemapCollider2D>().isTrigger = true;
        
        return room;
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
            if (filename.Name.StartsWith("ROOM") && filename.Name.EndsWith(".prefab"))
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
            createdRoom.transform.Find("Flags").gameObject.tag = "Flag";
            createdRoom.transform.Find("Flags").gameObject.AddComponent<LocationController>();
            createdRoom.transform.Find("Flags").gameObject.AddComponent<TilemapCollider>();
            //createdRoom.transform.Find("Flags").gameObject.GetComponent<TilemapCollider2D>().isTrigger = true;
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

    bool IsRoomOutOfGrid(int x, int y)
    {
        if ((x + 1) >= gridWidthHeight)
            return true;
        if ((x) <= 0)
            return true;
        if ((y + 1) >= gridWidthHeight)
            return true;
        if ((y) <= 0)
            return true;
    
        //Else inside
        return false;
    }

    void GenerateRoomsNew(int gridX, int gridY, Sprite direction, Room previousRoom, int n)
    {
        if (roomsToGen == 0)
        {
            //Debug.Log("Exited because room number ran out.");
            return;
        }
        
        if (IsRoomOutOfGrid(gridX, gridY))
        {
            //Debug.Log("Exited because went off of grid");
            return;
        }

        Room thisRoom = RoomGrid[gridY, gridX];

        if (thisRoom != null)
        {
            //Debug.Log("Exited because room already exists");
            return;
        }
        

        int numExitsMin = FindRequiredExits(new Vector2Int(gridX, gridY));
        int pickedExits;

        if (((float) n / branchMaxLength) >= (0.8))
        {
            pickedExits = Math.Max(numExitsMin, Random.Range(Math.Max(3, numExitsMin), 4));
        } else if (((float) n / branchMaxLength) >= 0.3)
        {
            pickedExits = Math.Max(numExitsMin, Random.Range(Math.Max(1, numExitsMin), 3));
        }
        else
        {
            pickedExits = Math.Max(numExitsMin, Random.Range(Math.Max(1, numExitsMin), 2));
        }
        
        //pickedExits = 3;

        int timeOutQueue = 40;
        
        // Information we randomly generate based on rules in while()
        GameObject pickedRoom = null;
        Room newRoom = null;
        GameObject clonedRoom = null;
        
        do
        {
            pickedRoom = null;
            newRoom = null;
            if (clonedRoom != null)
            {
                Destroy(clonedRoom);
                clonedRoom = null;
            }

            if (timeOutQueue == 0)
                return;

            List<GameObject> listOfRoomsInCategory = null;
            try
            {
                listOfRoomsInCategory = GameObjectRoomsByExits[pickedExits - 1];
            }
            catch (Exception e)
            {
                throw new Exception("Error while looking for rooms, wrong index. " + e.Message);
            }

            if (listOfRoomsInCategory == null)
                throw new Exception("No room with " + pickedExits + " exits was designed");

            pickedRoom = listOfRoomsInCategory[Random.Range(0, listOfRoomsInCategory.Count)];
            //pickedRoom = listOfRoomsInCategory[3];

            if(pickedRoom == null)
                throw new Exception("Room or Sprite is null in GenerateRooms");

            Door oldConnectionDoor;
            try
            {
                oldConnectionDoor = FindConnectionDoor(previousRoom.Doors, direction);
            }
            catch (Exception e)
            {
                return;
            }

            //create clone room from template room
            clonedRoom = Instantiate(pickedRoom, _gridGameObject.transform, true);
            clonedRoom.transform.Find("Flags").gameObject.tag = "Flag";
            //reverse original transition
            clonedRoom.transform.Translate(ReverseRoomMove);

            var newDoors = DoorSearch(clonedRoom.transform.Find("Flags").GetComponent<Tilemap>());
            var oppositeSprite = FlagController.Instance.GetOppositeDirection(direction.name);
            
            Door newConnectionDoor;
            try
            {
                newConnectionDoor = FindConnectionDoor(newDoors, oppositeSprite);
            }
            catch
            {
                //Debug.LogWarning("Wanted connection for room not found!!!!");
                timeOutQueue--;
                continue;
            }

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

            bool makeBossRoom = false;
            if ((roomsToGen < branchMaxLength/2 || n < branchMaxLength/2) && (madeBossRoom == false))
            {
                //Is it a hallway?
                if (!pickedRoom.name.EndsWith("_2"))
                {
                    makeBossRoom = true;
                    Debug.Log("MADE BOSS");
                    madeBossRoom = true;
                }
            }
            
            //Connect new room with old room 
            newRoom = new Room(newDoors, clonedRoom, new Vector2Int(gridX, gridY), makeBossRoom);

            timeOutQueue--;
        } while (newRoom == null || RoomCantConnectToAny(newRoom));

        if (timeOutQueue == 0)
        {
            Debug.Log("Exited with timeout");
            return;
            
        }

        RoomGrid[newRoom.GridPosition.y, newRoom.GridPosition.x] = newRoom;
        roomsToGen--;

        var actionList = new[]
        {
            new Action( () =>
            {
                if(newRoom.HasDoor[FlagController.Instance.DoorRight.name]) 
                    GenerateRoomsNew(gridX + 1, gridY, FlagController.Instance.DoorRight, newRoom,n - 1);
            }),
            new Action( () =>
            {
                if(newRoom.HasDoor[FlagController.Instance.DoorLeft.name])
                    GenerateRoomsNew(gridX - 1, gridY, FlagController.Instance.DoorLeft, newRoom, n - 1);
            }),
            new Action( () =>
            {
                if(newRoom.HasDoor[FlagController.Instance.DoorUp.name])
                    GenerateRoomsNew(gridX, gridY - 1, FlagController.Instance.DoorUp, newRoom, n - 1);
            }),
            new Action( () =>
            {
                if(newRoom.HasDoor[FlagController.Instance.DoorDown.name])
                    GenerateRoomsNew(gridX, gridY + 1, FlagController.Instance.DoorDown, newRoom, n - 1);
            })
        }.ToList();

        Shuffle(actionList);
        for (int i = 0; i < actionList.Count; i++)
        {
            var action = actionList[i];
                action();
        }
    }

    private int FindRequiredExits(Vector2Int gridPosition)
    {
        Dictionary<string, Vector2Int> positionDict = new Dictionary<string, Vector2Int>();

        foreach (var direction in directionsDelta)
        {
            var deltaVector = FlagController.Instance.DirectionToDeltaGridVector(direction.name);
            positionDict[direction.name] = new Vector2Int(gridPosition.x + deltaVector.x, gridPosition.y + deltaVector.y);
        }

        int counter = 0;
        
        foreach (var tuple in positionDict)
        {
            Room checkRoom = null;
            try
            {
                checkRoom = RoomGrid[tuple.Value.y, tuple.Value.x];
            }
            catch
            {
                throw new Exception("Went out off grid");
            }

            if(checkRoom == null)
                continue;

            if (RoomHasOppositeDirection(tuple.Key, checkRoom.roomGameObject))
            {
                counter++;
            }
        }

        return counter;
    }

    bool RoomCantConnectToAny(Room room)
    {
        var gridPosition = room.GridPosition;
        
        Dictionary<string, Vector2Int> positionDict = new Dictionary<string, Vector2Int>();

        foreach (var direction in directionsDelta)
        {
            
            var deltaVector = FlagController.Instance.DirectionToDeltaGridVector(direction.name);
            positionDict[direction.name] = new Vector2Int(gridPosition.x + deltaVector.x, 
                gridPosition.y + deltaVector.y);
        }

        foreach (var tuple in positionDict)
        {

            Room checkRoom;
            try
            {
                checkRoom = RoomGrid[tuple.Value.y, tuple.Value.x];
            }
            catch (IndexOutOfRangeException e)
            {
                // We check if we have to connect, if yes we give result.
                if (!room.CanConnect(tuple.Key))
                    return true;
                else 
                    continue;
            }

            if(checkRoom == null)
                continue;

            if (RoomHasOppositeDirection(tuple.Key, checkRoom.roomGameObject))
            {
                if (room.CanConnect(tuple.Key))
                    continue;
                else
                    return true;
            }
            else
            {
                return true;
            }
        }

        return false;

    }
    
    private Door FindConnectionDoor(List<Door> doors, Sprite randomDirectionSprite)
    {
        Door ConnectionDoor = null;

        foreach (var door in doors)
        {
            if (door.type.name.Equals(randomDirectionSprite.name))
            {
                ConnectionDoor = door;
                break;
            }
        }

        if (ConnectionDoor == null)
            throw new Exception("no connection found");
        return ConnectionDoor;
    }

    bool RoomHasOppositeDirection(string original, GameObject roomCompare)
    {
        var oppositeSprite = FlagController.Instance.GetOppositeDirection(original);
        return RoomHasDirection(roomCompare, oppositeSprite.name);
    }

    bool RoomHasAllDirections(GameObject room, List<string> sprites)
    {
        foreach (var direction in sprites)
        {
            if (!RoomHasDirection(room, direction))
            {
                return false;
            }
        }

        return true;
    }

    bool RoomHasDirection(GameObject room, string sprite)
    {
        var flags = FindFlagLayer(room);

        if (flags == null)
        {
           throw new Exception("No flag layer found. Maybe tag missing?");
        }

        var doors = DoorSearch(flags.GetComponent<Tilemap>());

        foreach (var door in doors)
        {
            if (sprite.Equals(door.type.name))
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

        public Dictionary<string, bool> HasDoor;
        public List<Door> Doors;
        public Vector3 RealPosition { get; set; }
        public Vector2Int GridPosition { get; set; }
        public GameObject roomGameObject { get; set; }
        private Dictionary<string, Room> ConnectedRoomDictionary;
        public bool bossRoom = false;
        
        public Room(List<Door> doors, GameObject container, Vector2Int gridPosition, bool bossRoom)
        {
            Doors = doors;
            RealPosition = container.transform.position;
            GridPosition = gridPosition;
            roomGameObject = container;
            ConnectedRoomDictionary = new Dictionary<string, Room>(4);
            HasDoor = new Dictionary<string, bool>(4);

            //Ugly manual setting for each type of entrance
            ConnectedRoomDictionary[FlagController.Instance.DoorUp.name] = null;
            HasDoor[FlagController.Instance.DoorUp.name] = false;
            ConnectedRoomDictionary[FlagController.Instance.DoorDown.name] = null;
            HasDoor[FlagController.Instance.DoorDown.name] = false;
            ConnectedRoomDictionary[FlagController.Instance.DoorLeft.name] = null;
            HasDoor[FlagController.Instance.DoorLeft.name] = false;
            ConnectedRoomDictionary[FlagController.Instance.DoorRight.name] = null;
            HasDoor[FlagController.Instance.DoorRight.name] = false;
            
            foreach (var door in doors)
            {
                HasDoor[door.type.name] = true;
            }

            this.bossRoom = bossRoom;
        }

        public void Connect(string sprite, Room other)
        {
            ConnectedRoomDictionary[sprite] = other;
        }

        public bool IsConnected(string sprite)
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

        public bool CanConnect(string sprite)
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
            
            var sprite = tilemap.GetSprite(tilePosition);

            // Check if the sprite is actually a door.
            if (!FlagController.Instance.directionsDelta.Contains(sprite))
                continue;

            var spriteName = sprite.name;
            
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
    public static void Shuffle<T>(IList<T> list)  
    {  
        int n = list.Count;  
        while (n > 1) {  
            n--;  
            int k = Random.Range(0, n + 1);  
            T value = list[k];  
            list[k] = list[n];  
            list[n] = value;  
        }  
    }

}


