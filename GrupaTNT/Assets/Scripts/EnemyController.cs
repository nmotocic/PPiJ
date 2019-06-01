using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : EntityControllerInterface
{
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

    public EnemyController(GameObject obj) {
        parent = obj;
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
            es.controller.damage(meleeDamage);
        }
    }

    public void damage(int dmg) {
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
