using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField]
    public int roomGenNumber;

    private TileMapSerializer _serializer;
    private List<GameObject> _rooms;
    
    // Start is called before the first frame update
    void Start()
    {
        _serializer = new TileMapSerializer();
        
        //Loading rooms into memory
        _rooms = LoadRooms();
        
        
        //Use rooms and flags to generate the level
        GenerateRooms(roomGenNumber);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    List<GameObject> LoadRooms()
    {
        GameObject gridObject = GameObject.FindWithTag("Grid");
        DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(Application.dataPath, "PremadeRooms"));
        FileInfo[] filenames = directoryInfo.GetFiles();

        //There could be other metafiles in the directory so we check how many room files we have.
        int trueRoomSize = 0;
        foreach (FileInfo filename in filenames)
        {
            if(filename.Name.EndsWith(".room"))
                trueRoomSize++;
        }
        
        List<GameObject> rooms = new List<GameObject>(trueRoomSize);

        for (int i = 0; i < trueRoomSize; i++)
        {
            string filename = filenames[i].Name;

            GameObject createdRoom = _serializer.DeserializeAndCreateRoom(filename);
            rooms[i] = createdRoom;
        }

        return rooms;
    }

    void GenerateRooms(int roomGenNumber)
    {
        
    }
}
