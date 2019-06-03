using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : EntityControllerInterface
{
    EntityScript parentScript;
    Vector2 direction;
    float speed = 2f;
    float health = 3f;
    // Start is called before the first frame update
    public PlayerController(EntityScript ps,float speed) {
        parentScript = ps;
        this.speed = speed;
        FloatStat FS = new FloatStat("health", 100);
        parentScript.stats.Add("health",FS);
        FloatStat FS2 = new FloatStat("ranged", 20);
        parentScript.stats.Add("ranged", FS2);
    }
    // Update is called once per frame
    public void Update()
    {
        //Debug.Log(parentScript.stats["health"].getCompoundValue());
        float X = Input.GetAxis("Horizontal");
        float Y = Input.GetAxis("Vertical");
        direction = new Vector2(X, Y);
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 position = parentScript.gameObject.transform.position;
            parentScript.DispenseObject(parentScript.projectileOptions[0], position, (target-position).normalized,20f);
        }
    }
    public Vector2 getMovement() { return direction*speed; }
    public void OnCollisionEnter2D(Collision2D col) {
    }

    public void damage(int dmg)
    {
        health -= Mathf.Abs(dmg);
        Debug.Log("Hp:"+health);
        if (health==0) { death(); }
    }

    public void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log(col.gameObject);
        //Do stuff
    }
    public void death() {
        Debug.Log("Oof");
    }
}
