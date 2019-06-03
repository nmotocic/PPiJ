﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : EntityControllerInterface
{
    EntityScript parentScript;
    Vector2 direction;
    static float defSpeed = 0f; //Disabled
    float speed = defSpeed;
    //Control
    GameObject parent;
    private AiScriptBase myAi;
    //Enemy stats
    private int health;
    public int armor;
    public int poiseMax;
    public int meleeDamage;
    public int rangeDamage { get; set; }
    public int stun { get; set; }
    private bool start = true;

    public EnemyController(EntityScript ps) {
        parentScript = ps;
        parent = ps.gameObject;
        parentScript.stats.Add("ranged", new FloatStat("ranged", 20));
        parentScript.stats.Add("health", new FloatStat("health", (float)health));
        parentScript.stats.Add("armor", new FloatStat("armor", (float)armor));
        parentScript.stats.Add("damage", new FloatStat("damage", (float)meleeDamage));

    }
    public void Start()
    {
        myAi = parent.GetComponent<AiScriptBase>();
        if (myAi == null) {
            Debug.LogError("Nemam AI!Gasim se!");
            parent.SetActive(false);
        }
        myAi.getStats(ref health,ref armor,ref poiseMax, ref meleeDamage);
        stun = poiseMax;
        start = false;
    }

    // Update is called once per frame
    public void Update()
    {
        health = (int)parentScript.stats["health"].getCompoundValue();
        armor = (int)parentScript.stats["armor"].getCompoundValue();
        FloatStat MD = parentScript.stats["damage"];
        MD.removeFactor("isDangerous");
        meleeDamage = (int)MD.getCompoundValue();
        myAi = parent.GetComponent<AiScriptBase>();
        MD.setFactor("isDangerous", myAi.isDangerous() ? 1 : 0);
        if (start) Start();
        //parent.GetComponent<Rigidbody2D>().GetVector();
        direction = direction.normalized;
    }
    public Vector2 getMovement() {
        return parent.GetComponent<Rigidbody2D>().velocity;
    }

    public void OnCollisionEnter2D(Collision2D col) {
        //Code
    }
    public void OnTriggerEnter2D(Collider2D col) {
        
        if (myAi.isDangerous())
        {
            Debug.Log("Udario nesto:" + col.gameObject);
            var es = col.gameObject.GetComponent<EntityScript>();
            myAi.setDanger(false);
        }
    }

    public void temp(int dmg) {
        dmg = Mathf.Max(dmg - armor, 1);
        dmg = Mathf.Abs(dmg);
        health -= dmg;
        stun -= dmg;
        if (health <= 0) {
            death();
        }
        else if (stun <= -1) { //Stunned
            myAi.setState(-1);
            myAi.setAlarm(-stun);
            stun = poiseMax;
        }
    }
    public void death()//Death script
    {
        myAi.setState(-2);
        parent.GetComponent<Collider2D>().enabled = false;
        myAi.setDanger(false);
    }
}
