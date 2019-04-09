using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class RoomSaver : MonoBehaviour
{
   
    [SerializeField] GameObject gridObject;

    private TileMapSerializer _serializer;

    [SerializeField] public bool load;

    [SerializeField] public bool save;
    
    // Start is called before the first frame update
    void Start()
    {
        //Creating of our serializer               
        _serializer = new TileMapSerializer();
        
        if(save)
            SaveRooms();
        
        if(load)
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
            
            GameObject[][] _tileMapObjects = new GameObject[NumOfRooms][];
            GameObject[] roomObjectChildren = new GameObject[NumLayersInRoom];
            Tilemap[] SingleRoomTileMaps = new Tilemap[NumLayersInRoom];
            layerNumbers = new int[NumLayersInRoom];
            
            for (int j = 0; j < NumLayersInRoom; j++)
            {
                roomObjectChildren[j] = _roomObjects[i].transform.GetChild(j).gameObject;
                SingleRoomTileMaps[j] = _roomObjects[i].transform.GetChild(j).gameObject.GetComponent<Tilemap>();
                layerNumbers[j] = _roomObjects[i].transform.GetChild(j).GetComponent<TilemapRenderer>().sortingOrder;
            }

            _tileMapObjects[i] = roomObjectChildren;
            _tilemaps[i] = SingleRoomTileMaps;
        }                           

        for (int i = 0; i < NumOfRooms; i++)
        {
            Tilemap[] map = _tilemaps[i];
            _serializer.SerializeRoom(map, layerNumbers, "Tilemap- " + i.ToString() + " -" + 
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
