using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using Vector2 = UnityEngine.Vector2;
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
    public void SerializeRoom(Tilemap[] tilemaps, int[] layerNumbers, String FileName)
    {
        FileName = "PremadeRooms/" + FileName;
        List<TilemapWrapper> tilemapWrappers = new List<TilemapWrapper>();
        _stream = File.Open(Path.Combine(Application.dataPath, FileName), FileMode.OpenOrCreate);
        
        Vector3Int[] positions = new Vector3Int[tilemaps.Length];
        
        // HARDCODED TO 0.5, because unity treats it as half, it will always be half
        Vector2 halfPivot = new Vector2(0.5f, 0.5f);
        
        TilemapWrapper? tilemapFlag = null;

        int sizeOfTilemaps = 0;
        
        int iterator = 0;
        foreach (Tilemap tilemap in tilemaps)
        {
            tilemap.CompressBounds();
            
            List<SerializableVector3Int> tilePositionsInTilemap = new List<SerializableVector3Int>();
            List<string> namesInTilemap = new List<string>();
            List<float> xmins = new List<float>();
            List<float> ymins = new List<float>();
            List<float> widths = new List<float>();
            List<float> heights = new List<float>();
            float pixelsPerUnit = 0;
            int layerNumber = layerNumbers[iterator];
            
            foreach (Vector3Int tilePosition in tilemap.cellBounds.allPositionsWithin)
            {
                if (tilemap.GetTile(tilePosition) != null)
                {
                    tilePositionsInTilemap.Add(tilePosition);
                    
                    Sprite sprite = tilemap.GetSprite(tilePosition);
                    namesInTilemap.Add(sprite.texture.name);

                    var rect = sprite.rect;
                    xmins.Add(rect.xMin);
                    ymins.Add(rect.yMin);
                    widths.Add(rect.width);
                    heights.Add(rect.height);

                    pixelsPerUnit = sprite.pixelsPerUnit;
                }
            }

            if (tilemap.gameObject.tag == "Flag")
            {
                tilemapFlag = new TilemapWrapper(tilePositionsInTilemap.ToArray(),
                    namesInTilemap.ToArray(), xmins.ToArray(), ymins.ToArray(), widths.ToArray(),
                    heights.ToArray(), halfPivot, pixelsPerUnit, layerNumber);
            }
            else
            {
                tilemapWrappers.Add(new TilemapWrapper(tilePositionsInTilemap.ToArray(),
                    namesInTilemap.ToArray(), xmins.ToArray(), ymins.ToArray(), widths.ToArray(),
                    heights.ToArray(), halfPivot, pixelsPerUnit, layerNumber));   
            }
            
            iterator++;
        }

        // if we didnt manage to find the tileflag throw a warning
        if (tilemapFlag != null)
        {
            RoomWrapper roomWrapper = new RoomWrapper(tilemapWrappers.ToArray(), tilemapFlag.Value);
            _formatter.Serialize(_stream, roomWrapper);
        }
        else
        {
            Debug.LogError("No tilemap flag was added, make sure this is intentional." +
                             "Object: " + FileName);
        }

        _stream.Close();
    }

    public GameObject DeserializeAndCreateRoom(String FileName)
    {   
        GameObject gridObject = GameObject.FindWithTag("Grid");
        
        FileName = "PremadeRooms/" + FileName;
        _stream = File.Open(Path.Combine(Application.dataPath, FileName), FileMode.Open);
        RoomWrapper roomWrapper = (RoomWrapper) _formatter.Deserialize(_stream);
        TilemapWrapper[] tilemapWrappers = roomWrapper.tilemapLayers;
        TilemapWrapper? tilemapFlag = roomWrapper.tilemapFlag;

        GameObject roomObject = new GameObject(FileName);
        roomObject.transform.SetParent(gridObject.transform);
        
        int layerIndexer = 0;
        foreach (var tilemapWrapper in tilemapWrappers)
        {
            GameObject layerObject = new GameObject(layerIndexer.ToString());
            layerObject.transform.SetParent(roomObject.transform);
            Tilemap objectTilemap = layerObject.AddComponent<Tilemap>();
            layerObject.AddComponent<TilemapRenderer>().sortingOrder = tilemapWrappers[layerIndexer].layerNumber;
            
            int tileIndexer = 0;

            Tile[] singleLayerTiles = new Tile[tilemapWrapper.tilePositions.Length];
            foreach (var tilePosition in tilemapWrapper.tilePositions)
            {
                Rect rect = new Rect(tilemapWrapper.m_XMins[tileIndexer], tilemapWrapper.m_YMins[tileIndexer],
                    tilemapWrapper.m_Widths[tileIndexer], tilemapWrapper.m_Heights[tileIndexer]);
                Tile createdTile = ScriptableObject.CreateInstance<Tile>();
                
                Texture2D texture2D = Resources.Load<Texture2D>(tilemapWrapper.textureNames[tileIndexer]);
                Sprite recreatedSprite = Sprite.Create(texture2D, rect, 
                    tilemapWrapper.pivot, tilemapWrapper.pixelPerUnit);

                createdTile.sprite = recreatedSprite;
                objectTilemap.SetTile(tilePosition, createdTile);
                tileIndexer++;
            }
         
            layerIndexer++;
        }

        // we deal with the flagtile differently
        if (tilemapFlag != null)
        {
            //we know the flag is there so we get rid of the nullable
            TilemapWrapper tilemapFlagValue = tilemapFlag.Value;
            
            GameObject layerObject = new GameObject(layerIndexer.ToString());
            layerObject.transform.SetParent(roomObject.transform);
            Tilemap objectTilemap = layerObject.AddComponent<Tilemap>();
            
            //just for debugging
            layerObject.AddComponent<TilemapRenderer>().enabled = false;
            layerObject.tag = "Flag";
            
            int tileIndexer = 0;
            Tile[] singleLayerTiles = new Tile[tilemapFlagValue.tilePositions.Length];
            foreach (var tilePosition in tilemapFlagValue.tilePositions)
            {
                Rect rect = new Rect(tilemapFlagValue.m_XMins[tileIndexer], tilemapFlagValue.m_YMins[tileIndexer],
                    tilemapFlagValue.m_Widths[tileIndexer], tilemapFlagValue.m_Heights[tileIndexer]);
                Tile createdTile = ScriptableObject.CreateInstance<Tile>();
                
                Texture2D texture2D = Resources.Load<Texture2D>(tilemapFlagValue.textureNames[tileIndexer]);
                Sprite recreatedSprite = Sprite.Create(texture2D, rect, 
                    tilemapFlagValue.pivot, tilemapFlagValue.pixelPerUnit);

                createdTile.sprite = recreatedSprite;
                objectTilemap.SetTile(tilePosition, createdTile);
                tileIndexer++;
            }
        }

        _stream.Close();
        return roomObject;
    }

    /// <summary>
    /// Stores relevant information about a tilemap to be serialized.
    /// </summary>
    [Serializable]
    public struct TilemapWrapper
    {
        public TilemapWrapper(SerializableVector3Int[] tilePositions, string[] textureNames, float[] mXMins, 
            float[] mYMins, float[] mWidths, float[] mHeights, SerializableVector2 pivot, float pixelPerUnit, 
            int layerNumber)
        {
            this.tilePositions = tilePositions;
            this.textureNames = textureNames;
            m_XMins = mXMins;
            m_YMins = mYMins;
            m_Widths = mWidths;
            m_Heights = mHeights;
            this.pivot = pivot;
            this.pixelPerUnit = pixelPerUnit;
            this.layerNumber = layerNumber;
        }

        /// <summary>
        /// Positions of the tiles.
        /// </summary>
        public SerializableVector3Int[] tilePositions;

        /// <summary>
        /// Texture name storage, used to recreate Sprite
        /// Its an array, because we can use multiple textures to create a room
        /// </summary>
        public string[] textureNames;
        
        /// <summary>
        /// Recreation of Rect, Rect is used to recreate Sprite
        /// Could be replaced with two SerializableVector2s
        /// Maybe dosent require a array because they are the same for all sprites, but just to be sure.
        /// </summary>
        public float[] m_XMins;
        public float[] m_YMins;
        public float[] m_Widths;
        public float[] m_Heights;

        /// <summary>
        /// Used to recreate sprite, HAS TO BE FROM [0,1]! Will be set manually to 0.5 probably because unity is weird.
        /// </summary>
        public SerializableVector2 pivot;

        /// <summary>
        /// Pixel per units, will probably be a constant number, maybe change in future to store in RoomWrapper.
        /// </summary>
        public float pixelPerUnit;

        /// <summary>
        /// What we sort the layers by, lower means down, higher means up.
        /// </summary>
        public int layerNumber;

    }
    
    /// <summary>
    /// Room creation, room has list of tilemap wrappers.
    /// </summary>
    [Serializable]
    public struct RoomWrapper
    {
        public RoomWrapper(TilemapWrapper[] tilemapLayers, TilemapWrapper tilemapFlag)
        {
            this.tilemapLayers = tilemapLayers;
            this.tilemapFlag = tilemapFlag;
        }

        public TilemapWrapper[] tilemapLayers;

        public TilemapWrapper tilemapFlag;
        // ADD MORE ITEMS HERE IF NEEDED
    }
    
}
