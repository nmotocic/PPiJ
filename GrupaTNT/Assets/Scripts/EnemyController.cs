using System.Collections;
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
    public int rangeDamage;
    public int stun { get; set; }
    private bool start = true;
    private bool dropped = false;
    
    private KillQuestController _killQuestController;
    private bool dead;

    public EnemyController(EntityScript ps) {
        parentScript = ps;
        parent = ps.gameObject;
        myAi = parent.GetComponent<AiScriptBase>();
        myAi.getStats(ref health, ref armor, ref poiseMax, ref meleeDamage, ref rangeDamage);
        stun = poiseMax;
        parentScript.stats.Add("projectileRange", new FloatStat("projectileRange", 10f));
        parentScript.stats["ranged"] = new FloatStat("ranged", (float)rangeDamage);
        parentScript.stats["health"] = new FloatStat("health", (float)health);
        parentScript.stats["armor"] = new FloatStat("armor", (float)armor);
        parentScript.stats["damage"] = new FloatStat("damage", (float)meleeDamage);
        if (meleeDamage != 0) {
            parentScript.rawInput.Add("EFFECT damage irrelevant " + meleeDamage.ToString() + " 0 1");
            parentScript.Input();
        }
    
        _killQuestController = GameObject.FindWithTag("Manager").GetComponent<KillQuestController>();
        dead = false;
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
        //if (start) Start();
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
        /*
        if (myAi.isDangerous())
        {
            Debug.Log("Udario nesto:" + col.gameObject);
            var es = col.gameObject.GetComponent<EntityScript>();
            myAi.setDanger(false);
        }
        */
    }

    public void damage(int dmg) {
        health = (int)parentScript.stats["health"].getCompoundValue();
        stun -= dmg;
        if (stun <= -1 && health>0) { //Stunned
            myAi.setState(-1);
            myAi.setAlarm(-stun);
            stun = poiseMax;
        }
    }
    public void death()//Death scriptd
    {
        myAi.setState(-2);
        parent.GetComponent<Collider2D>().enabled = false;
        parent.GetComponent<Rigidbody2D>().velocity = new Vector3(0,0,0);

        if (!dead)
        { 
            _killQuestController.EnemyDeath(parentScript.name);
            dead = true;
        }
        
        myAi.setDanger(false);
        if (!dropped) { lootDrop(); dropped = true; }
    }
    public void lootDrop()
    {
        Vector2 position = parentScript.gameObject.transform.position;
        string[] rawInput = { "EFFECT gold boop 1 -1 1", "EFFECT experience boop 1 -1 1" };
        parentScript.DispenseObject(parentScript.drop, position, new Vector3(), 0f, rawInput, "powerup");
    }
}
