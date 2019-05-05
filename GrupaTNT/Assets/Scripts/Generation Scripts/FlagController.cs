using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;

public class FlagController : MonoBehaviour
{
    /// <summary>
    /// We manually set the sprites in the inspector
    /// </summary>
    [System.Serializable]
    public class FlagTypes
    {
        public Sprite PowerUpSpawn;
        public Sprite EnemySpawn;
        public Sprite DoorRight;
        public Sprite DoorLeft;
        public Sprite DoorUp;
        public Sprite DoorDown;
    }

    public FlagTypes flagTypes;

    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
