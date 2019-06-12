using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameDefaults
{
    //Tags
    public static string Obstruction() {
        return "Obstruction";
    }
    public static string Projectile()
    {
        return "Projectile";
    }
    public static string Enemy()
    {
        return "Enemy";
    }
    public static string Player()
    {
        return "Player";
    }
    public static string Powerup()
    {
        return "Powerup";
    }
    public static string LevelExit()
    {
        return "LevelExit";
    }
    //Layers
    public static int layerWall()
    {
        return (1 << LayerMask.NameToLayer("Wall"));
    }


    //States
    public static int deatState()
    {
        return -2;
    }
    public static int hitState()
    {
        return -1;
    }

    //Default timing
    public static int corpseTimeout() {
        return 10;
    }
}
