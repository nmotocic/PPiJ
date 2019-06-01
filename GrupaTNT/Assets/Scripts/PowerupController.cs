using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupController : EntityControllerInterface
{
    EntityScript parentScript;
    public PowerupController(EntityScript ps)
    {
        this.parentScript = ps;
    }
    public void Update() { }
    public Vector2 getMovement() { return new Vector2(0f, 0f); }
    public void OnCollisionEnter2D(Collision2D X) { }

    public void damage(int dmg)
    {
        //Do stuff
    }

    public void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log(col.gameObject);
        //Do stuff
    }
    public void death() {
        GameObject.Destroy(parentScript.gameObject);
    }
}
