using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using Vector3 = UnityEngine.Vector3;

//TODO: - We need a way to mark places where doors/exits to be. So we know where to place rooms. Some kind of flag system is needed.

/// <summary>
/// Saves rooms, for each room it saves multiple layers.
/// </summary>
public class TileMapSerializer
{
    private Stream _stream;
    private BinaryFormatter _formatter;

    public TileMapSerializer()
    {
        _formatter = new BinaryFormatter();
        
    }

    /// <summary>
    /// Serializes rooms.
    /// </summary>
    /// <param name="tilemaps">Tilemaps from a single room. They should be ordered from bottom to top in layers.</param>
    /// <param name="FileName">Name of the serialized data file that will be saved.</param>
    public void SerializeRoom(Tilemap[] tilemaps, String FileName)
    {
        TilemapWrapper[] tilemapWrappers = new TilemapWrapper[tilemaps.Length];
        _stream = File.Open(Path.Combine(Application.dataPath, FileName), FileMode.OpenOrCreate);
        
        
        Vector3Int[] positions = new Vector3Int[tilemaps.Length];
        
        
        foreach (var tilemap in tilemaps)
        {
            var bounds = tilemap.cellBounds;
            var bases = tilemap.GetTilesBlock(bounds);
            
            TilemapWrapper tilemapWrapper = new TilemapWrapper(bases, bounds.position, bounds.size );
        }
        
        RoomWrapper roomWrapper = new RoomWrapper(tilemapWrappers);
        _formatter.Serialize(_stream, roomWrapper);
        _stream.Close();
    }

    public Tilemap[] DeserializeRoom(String filename)
    {
        _stream = File.Open(Path.Combine(Application.dataPath, filename), FileMode.OpenOrCreate);
        RoomWrapper roomWrapper = (RoomWrapper) _formatter.Deserialize(_stream);
        TilemapWrapper[] tilemapWrappers = roomWrapper._tilemapLayers;

        List<Tilemap> roomTileMaps = new List<Tilemap>(tilemapWrappers.Length);
        
        foreach (var tilemapWrapper in tilemapWrappers)
        {
            BoundsInt tileBounds = new BoundsInt(tilemapWrapper.LayerPosition, tilemapWrapper.LayerSize);
            TileBase[] tileBases = tilemapWrapper.LayerBases;
            
            Tilemap map = new Tilemap();
            map.SetTilesBlock(tileBounds, tileBases);
            
            roomTileMaps.Add(map);
        }

        return roomTileMaps.ToArray();
    }

    /// <summary>
    /// Stores relevant information about a tilemap to be serialized.
    /// </summary>
    [Serializable]
    public struct TilemapWrapper
    {
        public TilemapWrapper(TileBase[] roomBases, Vector3Int layerPosition, Vector3Int layerSize)
        {
            LayerBases = roomBases;
            LayerPosition = layerPosition;
            LayerSize = layerSize;
        }

        /// <summary>
        /// Used in SetTilesBlock.
        /// </summary>
        public TileBase[] LayerBases;
        
        /// <summary>
        /// Used in to create BoundsInt. BoundsInt will be used in SetTilesBlock.
        /// </summary>
        public SerializableVector3Int LayerPosition;

        /// <summary>
        /// Used in to create BoundsInt. BoundsInt will be used in SetTilesBlock.
        /// </summary>
        public SerializableVector3Int LayerSize;

    }
    
    /// <summary>
    /// Room creation, room has list of tilemap wrappers.
    /// </summary>
    [Serializable]
    public struct RoomWrapper
    {
        public RoomWrapper(TilemapWrapper[] tilemapLayers)
        {
            _tilemapLayers = tilemapLayers;
        }

        public TilemapWrapper[] _tilemapLayers;
        
        // ADD MORE ITEMS HERE IF NEEDED
    }
    
}
