﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEditor.VersionControl;
using UnityEngine;
using Random = System.Random;

public class FlagController : Singleton<FlagController>
{
    /// <summary>
    /// We manually set the sprites in the inspector
    /// </summary>
    [SerializeField]
    public Sprite PowerUpSpawn;
    [SerializeField]
    public Sprite EnemySpawn;
    [SerializeField]
    public Sprite DoorRight;
    [SerializeField]
    public Sprite DoorLeft;
    [SerializeField]
    public Sprite DoorUp;
    [SerializeField]
    public Sprite DoorDown;
    
    /// <summary>
    /// Probably not needed
    /// </summary>
    private void Awake()
    {
        Instance.DoorDown = DoorDown;
        Instance.DoorUp = DoorUp;
        Instance.DoorLeft = DoorLeft;
        Instance.DoorRight = DoorRight;
        Instance.EnemySpawn = EnemySpawn;
        Instance.PowerUpSpawn = PowerUpSpawn;
    }

    //Disallow constructor use
    protected FlagController() { }

    public Sprite GetRandomDirection()
    {
        var picker = UnityEngine.Random.Range(0, 4);
    
        switch (picker)
        {
            case 0: return DoorDown;
            case 1: return DoorUp;
            case 2: return DoorLeft;
            case 3: return DoorRight;
        }

        return null;
    }

    public Vector2Int DirectionToDeltaGridVector(string direction)
    {
        if (direction == DoorDown.name)
            return new Vector2Int(0, 1);
        if (direction == DoorUp.name)
            return new Vector2Int(0, -1);
        if (direction == DoorLeft.name)
            return new Vector2Int(-1, 0);
        if (direction == DoorRight.name)
            return new Vector2Int(1, 0);

        throw new Exception("Direction not valid" + direction);
    }

    public Sprite GetOppositeDirection(string str)
    {
        if (DoorDown.name == str)
            return DoorUp;
        if (DoorUp.name == str)
            return DoorDown;
        if (DoorLeft.name == str)
            return DoorRight;
        if (DoorRight.name == str)
            return DoorLeft;
        
        return null;
    }
    
    public Sprite FindSpriteWithString(string str)
    {
        if (DoorDown.name == str)
            return DoorDown;
        if (DoorUp.name == str)
            return DoorUp;
        if (DoorLeft.name == str)
            return DoorLeft;
        if (DoorRight.name == str)
            return DoorRight;
        if (PowerUpSpawn.name == str)
            return PowerUpSpawn;
        if (EnemySpawn.name == str)
            return EnemySpawn;

        Debug.LogError("No string passed in findSpriteWithString");
        return null;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
