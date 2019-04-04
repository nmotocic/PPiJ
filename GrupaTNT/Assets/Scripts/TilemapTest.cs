using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Testing script for Tilemaps and Serialization
/// </summary>
public class TilemapTest : MonoBehaviour
{
    private Tilemap map;
    private Grid grid;
    private Camera cam;
    
    private Vector3Int mousePosition;

    private LinkedList<Vector3Int> allTileLocations;
    private TileBase[] tileBases;

    private TileMapSerializer _serializer;

    // Start is called before the first frame update
    void Start()
    {
        //Creating of our serializer
        _serializer = new TileMapSerializer();
        
        //We assume the script is on the Tilemap Object so we get the Tilemap Component
        map = gameObject.GetComponent<Tilemap>();
        
        //We find the grid and camera objects, grab the components we need.
        grid = GameObject.FindWithTag("Grid").GetComponent<Grid>();
        cam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();

        
        //TEST ONE
        TestingProgramaticallyCopyAndSerialize();

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 screenToWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D rayHit = Physics2D.Raycast(screenToWorld, Vector2.zero);
        //if (map.GetSprite(mousePosition) != null)
        if (rayHit)
        {
            mousePosition = Vector3Int.CeilToInt(new Vector3(rayHit.point.x, rayHit.point.y, 0));
            Debug.Log(mousePosition.ToString() + map.GetSprite(mousePosition).name + map.GetTile(mousePosition).name);
        }
    }

    private int SmoothRound(float input)
    {
        return (int) (input + 0.5);
    }
    
    /// <summary>
    /// TESTING PURPOSES
    /// We can copy a handmade tilemap into a empty one with this script.
    /// Then we save it via serialization.
    /// TODO MOVE THIS TO A GAME MANAGER OBJECT WHERE IT GRABS ALL TILES FROM ONE ROOM
    /// </summary>
    public void TestingProgramaticallyCopyAndSerialize()
    {
        GameObject gridObject = new GameObject("GridGenerated");                                       
        Grid createdGrid = gridObject.AddComponent<Grid>();                                                 
                                                                                                 
        GameObject tileObject = new GameObject("TilemapGenerated");                                    
        Tilemap createdTile = tileObject.AddComponent<Tilemap>();                                      
        tileObject.AddComponent<TilemapRenderer>();                                                    
                                                                                                 
        tileObject.transform.SetParent(gridObject.transform);

        tileBases = map.GetTilesBlock(map.cellBounds);                                                 
                                                                                                 
        createdTile.SetTilesBlock(map.cellBounds, tileBases);                                          
                                                                                                 
        _serializer.SerializeRoom(new Tilemap[] {createdTile}, "SerializeTilemapData_TEST.room");      
    }
    
}
