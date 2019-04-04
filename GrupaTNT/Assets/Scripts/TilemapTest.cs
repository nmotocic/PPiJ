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

public class TilemapTest : MonoBehaviour
{
    private Tilemap map;
    private Grid grid;
    private Camera cam;

    private Grid createdGrid;
    
    private Vector3Int mousePosition;

    private LinkedList<Vector3Int> allTileLocations;
    private Vector3 tileAncor;
    private TileBase[] tileBases;

    // Start is called before the first frame update
    void Start()
    {
        map = gameObject.GetComponent<Tilemap>();
        grid = GameObject.FindWithTag("Grid").GetComponent<Grid>();
        cam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();

        GameObject gridObject = new GameObject("GridGenerated");
        createdGrid = gridObject.AddComponent<Grid>();
        
        GameObject tileObject = new GameObject("TilemapGenerated");
        Tilemap createdTile = tileObject.AddComponent<Tilemap>();
        tileObject.AddComponent<TilemapRenderer>();
        
        tileObject.transform.SetParent(gridObject.transform);
        
        allTileLocations = findAllPositions(map);
        tileAncor = map.tileAnchor;
        tileBases = map.GetTilesBlock(map.cellBounds);
        
        createdTile.SetTilesBlock(map.cellBounds, tileBases);
        


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
    
    public void SerializeTilemap() {;
        Stream writer = File.Open(Path.Combine(Application.persistentDataPath, "tilemap.txt"), FileMode.Create);
        BinaryFormatter b = new BinaryFormatter();  
        b.Serialize(writer, map);
        writer.Close();  
    }

    LinkedList<Vector3Int> findAllPositions(Tilemap tilemap)
    {
        LinkedList<Vector3Int> vector3List = new LinkedList<Vector3Int>();
        
        //get bounds
        tilemap.CompressBounds();
        
        foreach(Vector3Int tileLocation in tilemap.cellBounds.allPositionsWithin)
        {
            vector3List.AddLast(tileLocation);
        }

        return vector3List;
    }

}
