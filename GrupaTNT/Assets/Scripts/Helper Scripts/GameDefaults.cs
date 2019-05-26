using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameDefaults
{
    public static string Obstruction() {
        return "Obstruction";
    }
    public static string Projectile()
    {
        return "Projectile";
    }

    public static int layerWall()
    {
        return (1 << LayerMask.NameToLayer("Wall"));
    }

    public static string Player()
    {
        return "Player";
    }
}
