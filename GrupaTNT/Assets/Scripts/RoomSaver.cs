using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class RoomSaver : MonoBehaviour
{
    [FormerlySerializedAs("GridObject")] [SerializeField]
    GameObject gridObject;
    private GameObject[] _roomObjects;
    private GameObject[][] _tileMapObjects;

    private Tilemap[][] _tilemaps;
                                               
    private TileMapSerializer _serializer; 
    
    // Start is called before the first frame update
    void Start()
    {
        //Creating of our serializer               
        _serializer = new TileMapSerializer();
        
        SaveRooms();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SaveRooms()
    {
        gridObject = GameObject.FindWithTag("Grid");
        int NumOfRooms = gridObject.transform.childCount; 
        _roomObjects = new GameObject[NumOfRooms];
        
        for(int i = 0; i < NumOfRooms; i++)
        {
            _roomObjects[i] = gridObject.transform.GetChild(i).gameObject;
            int NumLayersInRoom = _roomObjects[i].transform.childCount;
            
            _tileMapObjects = new GameObject[NumOfRooms][];
            _tilemaps = new Tilemap[NumOfRooms][];
            GameObject[] roomObjectChildren = new GameObject[NumLayersInRoom];
            Tilemap[] SingleRoomTileMaps = new Tilemap[NumLayersInRoom];
            
            for (int j = 0; j < NumLayersInRoom; j++)
            {
                roomObjectChildren[j] = _roomObjects[i].transform.GetChild(j).gameObject;
                SingleRoomTileMaps[j] = _roomObjects[i].transform.GetChild(j).gameObject.GetComponent<Tilemap>();
            }

            _tileMapObjects[i] = roomObjectChildren;
            _tilemaps[i] = SingleRoomTileMaps;
        }                           

        for (int i = 0; i < NumOfRooms; i++)
        {
            Tilemap[] map = _tilemaps[i];
            _serializer.SerializeRoom(map, "PremadeRooms/Tilemap- " + i.ToString() + " -" + 
                                           _roomObjects[i].name + ".room");
        }
    }

    void LoadRooms()
    {
        _serializer.DeserializeRoom("Tilemap- 0 -Room.room");
        
        //TODO Create empty room object, insert players inside of children of empty room object
    }
}
