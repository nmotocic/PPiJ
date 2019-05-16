using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEditor.VersionControl;
using UnityEngine;

public class FlagController : MonoBehaviour
{
    /// <summary>
    /// We manually set the sprites in the inspector
    /// </summary>
    [System.Serializable]
    public struct FlagTypes
    {
        
        public Sprite PowerUpSpawn;
        public Sprite EnemySpawn;
        public Sprite DoorRight;
        public Sprite DoorLeft;
        public Sprite DoorUp;
        public Sprite DoorDown;
    }    
    
    private FlagTypes types;
    private static List<FlagTypes> Flags = null;

    private void Awake()
    {
        Flags = new List<FlagTypes>();
        Flags.Add(types);
    }

    public static FlagTypes GetFlags()
    {
        return Flags[0];
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
