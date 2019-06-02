using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : EntityControllerInterface
{
    Vector2 defaultFire = new Vector2(1.0f, 0.0f);
    EntityScript parentScript;
    Vector2 direction;
    float speed = 2f;
    float health = 20f;
    // Start is called before the first frame update
    public PlayerController(EntityScript ps,float speed) {
        parentScript = ps;
        this.speed = speed;
        FloatStat FS = new FloatStat("health", 100);
        FS.setFactor("current", 1f);
        parentScript.stats.Add("health",FS);
    }
    // Update is called once per frame
    public void Update()
    {
        //Debug.Log(parentScript.stats["health"].getCompoundValue());
        float X = Input.GetAxis("Horizontal");
        float Y = Input.GetAxis("Vertical");
        direction = new Vector2(X, Y);
        if (Input.GetMouseButtonDown(0)||Input.GetKeyDown("q"))
        {
            Vector2 position = parentScript.gameObject.transform.position;
            parentScript.DispenseObject(parentScript.projectileOptions[0], position, defaultFire.normalized,20f);
        }
    }
    public Vector2 getMovement() { return direction*speed; }
    public void OnCollisionEnter2D(Collision2D col) {
    }
}
