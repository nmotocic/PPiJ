using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class RoomSaver : MonoBehaviour
{
    [FormerlySerializedAs("GridObject")] [SerializeField]
    GameObject gridObject;
    private GameObject[] _roomObjects;
    private GameObject[][] _tileMapObjects;
    
    // Start is called before the first frame update
    void Start()
    {
        gridObject = GameObject.FindWithTag("Grid");
        _roomObjects = new GameObject[gridObject.transform.childCount];
        
        for(int i = 0; i < gridObject.transform.childCount; i++)
        {
            _roomObjects[i] = gridObject.transform.GetChild(i).gameObject;

            _tileMapObjects = new GameObject[_roomObjects[i].transform.childCount][];
            for (int j = 0; j < _roomObjects[i].transform.childCount; j++)
            {
                _tileMapObjects[i] = new GameObject[_roomObjects[i].transform.childCount];
                _tileMapObjects[i][j] = _roomObjects[i].transform.GetChild(j).gameObject;
            }
        }
        
        
        Debug.Log("bla");
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
