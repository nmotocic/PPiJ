using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;


public class TileMapSerializer
{
    private TilemapStorage[][] rooms;
    private Stream _stream;

    public TileMapSerializer()
    {
        _stream = File.Open(Path.Combine(Application.persistentDataPath, "CharacterData.txt"), FileMode.Create);
    }

    //Stores relevant information about a tilemap to be serialized
    [SerializeField]
    private struct TilemapStorage
    {
        //TODO save all of the position, cant use shortcut of min max vector
        //TODO SAVE ALL OF THEM
        
        [SerializeField] List<Tile> tileList;

        [SerializeField] List<Vector3Int> positionList;
    }
    
    

    private void SerializeWrapper(TilemapStorage data)
    {
        
    }
    
}
