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


    //Helper scripts
    /// <summary>
    /// Turns a normalised V2 into degrees.
    /// </summary>
    /// <param name="vect">Normalised vector2</param>
    /// <returns>Angle</returns>
    public static float vectToAngle(Vector2 vect) {
        float angle = Vector2.Angle(vect, Vector2.right);
        if (vect.y < 0) angle *= -1;
        return angle;
    }

    public static Vector2 angleToVect(float angle) {
        angle *= Mathf.Deg2Rad;
        Vector2 vect = new Vector2(0,0);
        vect.y=Mathf.Sin(angle);
        vect.x = Mathf.Cos(angle);
        return vect;
    }

    public static Vector2 rotateVector(Vector2 vect, float angle) {
        float rads = angle * Mathf.Deg2Rad;
        float x = vect.x * Mathf.Cos(rads) - vect.y * Mathf.Sin(rads);
        float y = vect.x * Mathf.Sin(rads) - vect.y * Mathf.Cos(rads);
        return new Vector2(x, y);
    }
}
