using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EntityScript))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody2D))]
public class MinoBossAI : AiScriptBase
{

    private NavMeshAgent agent;
    private int state = 0; //State machine
    private float agentSpeed;
    private float agentAngleSpeed;
    private float agentAccel;
    //Attack stats
    [Header("Dash stats")]
    public double dashWindup = 2; //Seconds before attack
    public double dashDuration = 1; //Seconds attacking(moving)
    public double dashCooldown = 1;
    public int contactDamage = 3;
    public float dashSpeed = 6;
    public float dashAccel = 2;
    public float dashAngular = 5;
    [Header("Slash stats")]
    public double slashWindup = 2; //Seconds before attack
    public double slashDuration = 1; //Seconds attacking
    public double slashCooldown = 0.5;
    public int slashDamage = 2;
    public GameObject slashProjectile;
    public float slashProjectileSpeed=3;
    [Header("Spin stats")]
    public double spinWindup = 2; //Seconds before attack
    public double spinDuration = 0.5; //Seconds attacking
    public double spinCooldown = 1;
    [Header("Slam stats")]
    public double slamWindup = 2; //Seconds before attack
    public double slamDuration = 0.5; //Seconds attacking
    public double slamCooldown = 1;
    //Attack target
    [Header("Attack defaults")]
    public string targetObjectTag = GameDefaults.Player();
    public double autoTriggerAtackDist = 1;
    public double attackDelay = 2;

    //Stats
    public int health = 60;
    public int poise = 0;
    public int armor = 0;
    public float stunMod = 1;
    
    private bool danger = false;
    private int attackType=0;
    private int attackCombo = 0;
    private bool buff = false;

    private Vector2 targetDir, startPos;
    private Alarm alarm = new Alarm(0);
    private Alarm aiTimer;
    private Rigidbody2D rbody2d;
    private Animator anim;
    private EntityScript eScript;


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
        //Startup controller
        eScript = gameObject.GetComponent<EntityScript>();
        gameObject.GetComponent<EntityScript>().getController(null, new Vector2(0, 0));
        //Get defaults
        aiTimer = new Alarm(attackDelay);
        agentSpeed = agent.speed;
        agentAngleSpeed = agent.angularSpeed;
        agentAccel = agent.acceleration;
    }


    // Update is called once per frame
    void Update()
    {
        alarm.Update();
        aiTimer.Update();
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
        //Reset idle
        if (state == 5 && alarm.isActive())
        {
            state = 0;
        }
        //Reset attack alarm
        if (attackType != 0 && state==0)
        {
            attackType = 0;
            aiTimer.reset();
        }

        //LoS
        if (state == 0 && AiDefaults.getLineSight(my_pos, target))
        {   //Target obstructed
            agent.destination = target;
            rbody2d.velocity = new Vector2(0, 0);
        }
        else
        { //State machine
            //Pick state
            if (state == 0) // Stop moving if winding up an attack
            {
                agent.destination = target;
                rbody2d.velocity = new Vector2(0, 0);
            }
            else //Move
            {

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
            //Pick attack
            if (Vector2.Distance(my_pos, target) <= autoTriggerAtackDist && state == 0 && attackType==0) //Attack player immediately
            {
                pickAttack();
                while (attackType == -1 || attackType == 4) {
                    pickAttack();
                }
                Debug.Log("Picked attack:" + attackType.ToString());
            }
            else if (aiTimer.isActive() && attackType==0 && state==0) {
                pickAttack();
                while (buff && attackType == 4) pickAttack();
                Debug.Log("Picked attack:" + attackType.ToString());
            }
            //Attacks
            if (attackType > 0)
            {
                //Dash
                if (attackType == 1) {
                    //Windup
                    if (state == 0)
                    {
                        state = 1;
                        alarm.setMax(dashWindup);
                        alarm.reset();
                        //Stop moving
                        agent.destination = my_pos;
                    }
                    //Start
                    else if (state == 1 && alarm.isActive())
                    {
                        state = 2;
                        alarm.setMax(dashDuration);
                        alarm.reset();
                        danger = true;
                        //Set agent speeds
                        if (buff)
                        {
                            agent.speed = dashSpeed * 1.5f;
                            agent.angularSpeed = dashAngular * 4f;
                            agent.acceleration = dashAccel * 4f;
                            agent.destination = target;
                            buff = false;
                        }
                        else
                        {
                            agent.speed = dashSpeed;
                            agent.angularSpeed = dashAngular;
                            agent.acceleration = dashAccel;
                            agent.destination = target;
                        }

                    }
                    //End
                    else if ((state == 2) && (alarm.isActive() || !danger)) {
                        setState(3); //Stab state
                        alarm.setMax(dashCooldown/2);
                        alarm.reset();
                        danger = false;
                    }
                    //Cooldown
                    else if ((state == 3) && (alarm.isActive()))
                    {
                        state = 5; //IdleState
                        alarm.reset();
                    }
                }
                //Slash
                else if (attackType ==2) {
                    targetDir = -(my_pos - target);
                    targetDir.Normalize();
                    if (state == 0)
                    {
                        state = 1;
                        alarm.setMax(slashWindup);
                        alarm.reset();
                        //Stop moving
                        agent.destination = my_pos;
                    }
                    //Start
                    else if (state == 1 && alarm.isActive())
                    {
                        state = 2;
                        anim.SetInteger("State", state);
                        alarm.setMax(slashDuration);
                        alarm.reset();
                        var proj = eScript.DispenseObject(slashProjectile, my_pos, targetDir, slashProjectileSpeed);
                        proj.GetComponent<EntityScript>().controller.damage(slashDamage);
                        if (buff)
                        {
                            attackCombo = 2;
                            buff = false;
                        }
                        else {
                            attackCombo = 0;
                        }
                    }
                    //Swing
                    else if ((state == 2) && (alarm.isActive()))
                    {  
                        if (attackCombo > 0)
                        { //Moar swings
                            state = 1;
                            alarm.setMax(slashWindup / 4);
                            alarm.reset();
                        }
                        else
                        { //Cooldown
                            state = 5;
                            alarm.setMax(slashCooldown);
                            alarm.reset();
                        }
                    }
                }
                //Spin
                else if (attackType == 3) {

                }
                //Buff
                else if (attackType == 4){

                }

            }
        }

    }

    public void pickAttack() {
        var rand = Random.Range(0, 9);
        if (rand <= 1)
        {
            attackType = 1;
        }
        else if (rand <= 4)
        {
            attackType = 2;
        }
        else if (rand <= 7)
        {
            attackType = 3;
        }
        else if (rand <= 8)
        {
            attackType = 4;
        }
        else {
            attackType = -1;
        }

        // -1 Nothing
        // 1 Dash
        // 2 Slash
        // 3 Spin
        // 4 Buff
    }

    public override void updateAnimation(float flip)
    {
        var scale = gameObject.transform.localScale;
        if (flip != Mathf.Sign(scale.x) && state <= 1 && state>=0)
        {
            gameObject.transform.localScale = new Vector3(-scale.x, scale.y, scale.z);
        }
        anim.SetInteger("State", state);
        anim.SetInteger("AttackType", attackType);
    }

    public override void setState(int set)
    {
        state = set;
        agent.speed = agentSpeed;
        agent.angularSpeed = agentAngleSpeed;
        agent.acceleration = agentAccel;
    }

    public override void setAlarm(float duration)
    {
        if (state == GameDefaults.hitState())
        {
            duration *= stunMod;
        }
        alarm.setMax(Mathf.Abs(duration));
    }

    public override void getStats(ref int health, ref int armor, ref int poise, ref int meleeDamage)
    {
        health = this.health;
        armor = this.armor;
        poise = this.poise;
        meleeDamage = contactDamage;
    }

    public override bool isDangerous()
    {
        return danger;
    }

    public override void setDanger(bool level)
    {
        danger = level;
    }
}
