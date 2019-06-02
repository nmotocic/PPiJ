using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LocationTracker : MonoBehaviour
{
    private Tilemap myTilemap;
    private LocationController _controller;

    // Start is called before the first frame update
    void Start()
    {
        myTilemap = gameObject.GetComponent<Tilemap>();
        _controller = GameObject.Find("Manager").GetComponent<LocationController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Sprite flagSprite = myTilemap.GetSprite(myTilemap.WorldToCell(other.transform.position));
        _controller.MovePosition(flagSprite.name);
        Debug.Log("TRIGGERED");
    }
}
