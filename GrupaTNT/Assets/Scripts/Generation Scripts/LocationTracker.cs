using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LocationTracker : MonoBehaviour
{
    [SerializeField] public int gridHeightWidth;

    private LevelGenerator.Room[,] roomGrid;
    
    
    // Start is called before the first frame update
    void Start()
    {
    }

    public void Initialize()
    {
        roomGrid = LevelGenerator.RoomGrid;
        gridHeightWidth = gameObject.GetComponent<LevelGenerator>().gridWidthHeight;
    }
    
    

}
