using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomSaver : MonoBehaviour
{
   
    [SerializeField] GameObject gridObject;
    
    [SerializeField] public bool load_ONLY_DEBUG;

    [SerializeField] public bool save;
    
    private TileMapSerializer _serializer;

    
    // Start is called before the first frame update
    void Start()
    {
        //Creating of our serializer               
        _serializer = new TileMapSerializer();
        
        if(save)
            SaveRooms();
        
        if(load_ONLY_DEBUG)
            LoadRooms();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SaveRooms()
    {
        gridObject = GameObject.FindWithTag("Grid");
        int NumOfRooms = gridObject.transform.childCount; 
        GameObject[] _roomObjects = new GameObject[NumOfRooms];
        
        Tilemap[][] _tilemaps = new Tilemap[NumOfRooms][];
        int[] layerNumbers = null;
        
        for(int i = 0; i < NumOfRooms; i++)
        {
            _roomObjects[i] = gridObject.transform.GetChild(i).gameObject;
            int NumLayersInRoom = _roomObjects[i].transform.childCount;
            
            GameObject[][] tileMapObjects = new GameObject[NumOfRooms][];
            GameObject[] roomObjectChildren = new GameObject[NumLayersInRoom];
            Tilemap[] singleRoomTileMaps = new Tilemap[NumLayersInRoom];
            layerNumbers = new int[NumLayersInRoom];
            
            for (int j = 0; j < NumLayersInRoom; j++)
            {
                roomObjectChildren[j] = _roomObjects[i].transform.GetChild(j).gameObject;
                singleRoomTileMaps[j] = _roomObjects[i].transform.GetChild(j).gameObject.GetComponent<Tilemap>();
                layerNumbers[j] = _roomObjects[i].transform.GetChild(j).GetComponent<TilemapRenderer>().sortingOrder;
            }

            tileMapObjects[i] = roomObjectChildren;
            _tilemaps[i] = singleRoomTileMaps;
        }                           

        for (int i = 0; i < NumOfRooms; i++)
        {
            Tilemap[] map = _tilemaps[i];
            _serializer.SerializeRoom(map, layerNumbers,   i.ToString() + " -" + 
                                           _roomObjects[i].name + ".room");
        }
    }

    
    void LoadRooms()
    {
        
        gridObject = GameObject.FindWithTag("Grid");
        DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(Application.dataPath, "PremadeRooms"));
        FileInfo[] filenames = directoryInfo.GetFiles();

        //There could be other metafiles in the directory so we check how many room files we have.
        int trueRoomSize = 0;
        foreach (FileInfo filename in filenames)
        {
            if(filename.Name.EndsWith(".room"))
                trueRoomSize++;
        }
            
        //GameObject[] _roomObjects = new GameObject[trueRoomSize];

        for (int i = 0; i < trueRoomSize; i++)
        {
            string filename = filenames[i].Name;

            GameObject createdRoom = _serializer.DeserializeAndCreateRoom(filename);
            //_roomObjects[i] = createdRoom;
        }
    }
}
