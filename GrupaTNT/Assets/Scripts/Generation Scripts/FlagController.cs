using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEditor.VersionControl;
using UnityEngine;

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
        return;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
