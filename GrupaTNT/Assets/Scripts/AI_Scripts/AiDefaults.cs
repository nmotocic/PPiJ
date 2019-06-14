using UnityEngine;

public abstract class AiDefaults
{
    /// <summary>
    /// Uses Physics2D.Linecast to check if the line between my_pos and target is clear of obstructions on the wall layer
    /// </summary>
    /// <param name="my_pos">My position in the world</param>
    /// <param name="target">End position in the world</param>
    /// <returns>Returns true if there is an obstruction. Otherwise returns false.</returns>
    public static bool getLineSight(Vector2 my_pos, Vector2 target) {
        return (Physics2D.Linecast(my_pos, target, GameDefaults.layerWall()));
    }

    /// <summary>
    /// Uses Physics2D.CircleCast to check if there is an obstruction in radius betwen my_pos and target
    /// </summary>
    /// <param name="my_pos">My position in the world</param>
    /// <param name="radius">Thickness of the collision check</param>
    /// <param name="target">End position</param>
    /// <param name="detailed">Make checks more strict</param>
    /// <param name="distance">Distance to target</param>
    /// <returns>Returns true if there is an obstruction. Otherwise returns false.</returns>
    public static bool getCircleSight(Vector2 my_pos, float radius, Vector2 target, bool detailed = true, float distance = 0)
    {
        if (distance == 0)
        {
            distance = (my_pos - target).magnitude;
        }
        distance = Mathf.Abs(distance);
        if (detailed)
        {
            radius += 0.1f;
            distance -= radius;
            if (distance < 0) {
                distance = 0;
                radius /= 2;
            }
        }
        
        var targetDir = (target - my_pos).normalized;
        return (Physics2D.CircleCast(my_pos, radius, targetDir, distance, GameDefaults.layerWall()));
    }



}
