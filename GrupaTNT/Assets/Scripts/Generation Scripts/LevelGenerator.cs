using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField]
    public int roomGenNumber;
    
    private List<GameObject> _rooms;
    
    // Start is called before the first frame update
    void Start()
    {
        //Loading all the saved room prefabs
        _rooms = LoadGameObjectRooms();
        //Use rooms and flags to generate the level
        GenerateRooms(roomGenNumber);
        
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
            if(filename.Name.StartsWith("[ROOM]") && filename.Name.EndsWith(".prefab"))
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
}
