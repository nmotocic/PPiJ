using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupController : EntityControllerInterface
{
    EntityScript parentScript;
    public PowerupController(EntityScript ps)
    {
        this.parentScript = ps;
        parentScript.impactEffects.Add("health", new FSQI(new FloatStat("health",10f),"baseValue",10f,-1,100000));
        parentScript.impactEffects.Add("ranged", new FSQI(new FloatStat("ranged", 10f), "POWAH", 9001f, 9001));
    }
    public void Update() { }
    public Vector2 getMovement() { return new Vector2(0f, 0f); }
    public void OnCollisionEnter2D(Collision2D X) { }

    public void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log(col.gameObject);
        //Do stuff
    }
    public void death() {
        GameObject.Destroy(parentScript.gameObject);
    }
}
