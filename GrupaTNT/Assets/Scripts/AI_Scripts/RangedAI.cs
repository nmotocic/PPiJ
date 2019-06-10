using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EntityScript))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody2D))]
public class RangedAI : AiScriptBase
{

    private NavMeshAgent agent;
    private int state = 0; //State machine
    private EntityScript eScript;

    //Attack range stats
    public int attackTriggerRange = 4; //Maxiumum range before attack windup
    public int attackSpeed = 100; //Speed of the attack
    public int maxAttackDist = 3; //Maximum distance this object will move while attacking (not implemented)
    //Attack timing stats (seconds)
    public double attackWindup = 2; //Seconds before attack
    public double attackDuration = 0.5; //Seconds attacking(moving)
    public double attackCooldown = 1;
    //Attack target
    public string targetObjectTag = GameDefaults.Player();
    //Projectile
    public GameObject projectileObject;
    public int projectileDamage = 1;

    //Stats
    public int health = 0;
    public int poise = 0;
    public int armor = 0;
    public float stunMod = 1;
    public int contactDamage = 1;
    private bool danger = false;

    private Vector2 targetDir, startPos;
    private Alarm alarm = new Alarm(0);
    private Rigidbody2D rbody2d;
    private Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        //Startup agent
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        //Startup rbody2d
        rbody2d = gameObject.GetComponent<Rigidbody2D>();
        //Startup animation
        anim = gameObject.GetComponentInChildren<Animator>();
        //Startup es
        eScript = gameObject.GetComponent<EntityScript>();
        //Startup controller
        eScript.getController(null, new Vector2(0, 0));
    }



    // Update is called once per frame
    void Update()
    {
        alarm.Update();
        //Init
        var my_pos = transform.position;
        Vector3 target;
        //Pick target
        var targetObject = GameObject.FindGameObjectWithTag(targetObjectTag);
        if (targetObject == null) //Mouse
        {
            target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        else //Player
        {
            target = targetObject.transform.position;
        }
        //Update animation
        updateAnimation(Mathf.Sign(my_pos.x - target.x));
        target.z = 0;

        if (AiDefaults.getCircleSight(my_pos,projectileObject.GetComponent<CircleCollider2D>().radius, target, true) && state == 0)
        {   //Target obstructed
            setDestination(target, agent);
            rbody2d.velocity = new Vector2(0, 0);
        }
        else
        { //State machine

            //Pick state
            if (state != 0) // Stop moving if winding up an attack
            {
                setDestination(my_pos, agent);
            }
            else //Move
            {
                setDestination(target, agent);
                rbody2d.velocity = new Vector2(0, 0);
            }
            //Hurt
            if (state == GameDefaults.hitState())
            {
                if (alarm.isActive()) state = 0;
                danger = false;
            }
            //Death
            else if (state == GameDefaults.deatState())
            {
                //Drop stuff, get removed
                danger = false;
            }

            //--------------------------Attacking
            if (Vector2.Distance(my_pos, target) <= attackTriggerRange && state == 0)
            { //Prep attack
                setDestination(my_pos, agent);
                state = 1;
                targetDir = -(my_pos - target);
                targetDir.Normalize();
                startPos = my_pos;
                //Alarm
                alarm.setMax(attackWindup);
                alarm.reset();
            }
            else if (state == 1 && alarm.isActive())  //Wait for alarm
            { //Attack - shoot
                eScript.DispenseObject(projectileObject,my_pos, targetDir, attackSpeed);
                //Alarm
                state = 2;
                alarm.setMax(attackDuration);
                alarm.reset();
            }
            else if ((state == 2 & alarm.isActive()))
            { //Attack over - stop
                rbody2d.velocity = new Vector2(0, 0);
                state = 3;
                alarm.setMax(attackCooldown);
                alarm.reset();
            }
            else if ((state == 3 & alarm.isActive()))
            { //Attack over - stop
                state = 0;
            }
            //--------------------------Attacking

        }
    }

    //---------Implented methods
    public override void updateAnimation(float flip)
    {
        var scale = gameObject.transform.localScale;
        if (flip != Mathf.Sign(scale.x) && state == 0)
        {
            gameObject.transform.localScale = new Vector3(-scale.x, scale.y, scale.z);
        }
        anim.SetInteger("State", state);
    }

    public override void setState(int set)
    {
        state = set;
    }

    public override void setAlarm(float duration)
    {
        if (state == GameDefaults.hitState())
        {
            duration *= stunMod;
        }
        alarm.setMax(Mathf.Abs(duration));
    }

    public override void getStats(ref int health, ref int armor, ref int poise, ref int meleeDamage, ref int rangeDamage)
    {
        health = this.health;
        armor = this.armor;
        poise = this.poise;
        meleeDamage = contactDamage;
        rangeDamage = projectileDamage;
    }

    public override bool isDangerous()
    {
        return danger;
    }

    public override void setDanger(bool level)
    {
        danger = level;
    }

    public void setDestination(Vector2 pos, NavMeshAgent agent)
    {
        if (agent.isOnNavMesh) agent.SetDestination(pos);
        else
        {
            //Do stuff
        }
    }
}
