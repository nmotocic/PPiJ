using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : EntityControllerInterface
{
    EntityScript parentScript;
    Vector2 direction;
    float speed = 2f;
    public float health = 100f;
    // Start is called before the first frame update
    public PlayerController(EntityScript ps,float speed=0f) {
        parentScript = ps;
        if (speed == 0f) { speed = this.speed; }
        parentScript.stats.Add("health", new FloatStat("health", health));
        parentScript.stats.Add("ranged", new FloatStat("ranged", 1));
        parentScript.stats.Add("damage", new FloatStat("ranged", 1));
        parentScript.stats.Add("gold", new FloatStat("gold", 0));
        parentScript.stats.Add("experience", new FloatStat("experience", 0));
        parentScript.stats.Add("armor", new FloatStat("armor", 5));
        parentScript.stats.Add("speed", new FloatStat("speed", speed));
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
            string[] rawInput= {"EFFECT damage boop 1 -1 1"};
            parentScript.DispenseObject(parentScript.projectileOptions[0], position, (target-position).normalized,20f,rawInput);
        }
    }
    public Vector2 getMovement() { return direction*parentScript.stats["speed"].getCompoundValue(); }
    public void OnCollisionEnter2D(Collision2D col) {
    }

    public void damage(int dmg)
    {
        health -= 0;
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
