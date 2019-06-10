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
    public double spinWindup = 1.2; //Seconds before attack
    public int spinProjectiles = 10; //Number of projectiles launched
    public double spinInterval = 0.2; //Time between each projectile launch
    public double spinCooldown = 1;
    [Header("Slam stats")]
    public double slamWindup = 1; //Seconds before attack
    public double slamDuration = 0.5; //Seconds attacking
    public double slamCooldown = 1;
    //Attack target
    [Header("Attack defaults")]
    public string targetObjectTag = GameDefaults.Player();
    public double autoTriggerAtackDist = 1;
    public double attackDelay = 2;
    [Header("Perma buff?")]
    public bool permaBuff = false;

    //Stats
    public int health = 60;
    public int poise = 0;
    public int armor = 0;
    public float stunMod = 1;
    
    private bool danger = false;
    private int attackType=0;
    private int attackCombo = 0;
    private int attackCount = 1;
    private bool buff = false;

    private Vector2 targetDir, startPos;
    private Alarm alarm = new Alarm(0);
    private Alarm aiTimer;
    private Rigidbody2D rbody2d;
    private Animator anim;
    private EntityScript eScript;
    //Buff Icon
    [Header("Buff icon")]
    public GameObject buffIcon;
    private GameObject myBuff;

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
        //Startup buffIcon
        myBuff = Instantiate(buffIcon);
        myBuff.GetComponent<AI_Effect>().setParent(gameObject);
    }


    // Update is called once per frame
    void Update()
    {
        if (permaBuff) buff = true;
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
            setDestination(target, agent);
            rbody2d.velocity = new Vector2(0, 0);
        }
        else
        { //State machine
            //Pick state
            if (state == 0) // Move
            {
                setDestination(target, agent);
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
                GameObject.Destroy(myBuff);
                myBuff = null;
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
                //attackType = 4;
                //Dash
                if (attackType == 1) {
                    //Windup
                    if (state == 0)
                    {
                        state = 1;
                        alarm.setMax(dashWindup);
                        alarm.reset();
                        //Stop moving
                        setDestination(my_pos, agent);
                    }
                    //Start
                    else if (state == 1 && alarm.isActive())
                    {
                        state = 2;
                        danger = true;
                        //Set agent speeds
                        if (buff)
                        {
                            agent.speed = dashSpeed * 1.5f;
                            agent.angularSpeed = dashAngular * 4f;
                            agent.acceleration = dashAccel * 4f;
                            agent.destination = target;
                            buff = false;
                            if( myBuff!=null ) myBuff.GetComponent<AI_Effect>().deActivate();
                            alarm.setMax(dashDuration/2);
                            alarm.reset();
                        }
                        else
                        {
                            agent.speed = dashSpeed;
                            agent.angularSpeed = dashAngular;
                            agent.acceleration = dashAccel;
                            agent.destination = target;
                            alarm.setMax(dashDuration);
                            alarm.reset();
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
                        setDestination(my_pos, agent);

                        if (buff)
                        {
                            attackCombo = 2;
                            buff = false;
                            if (myBuff != null) myBuff.GetComponent<AI_Effect>().deActivate();
                        }
                        else
                        {
                            attackCombo = 0;
                        }
                    }
                    //Start
                    else if (state == 1 && alarm.isActive())
                    {
                        state = 2;
                        alarm.setMax(slashDuration);
                        alarm.reset();
                        eScript.DispenseObject(slashProjectile, my_pos, targetDir, slashProjectileSpeed);
                    }
                    //Swing
                    else if ((state == 2) && (alarm.isActive()))
                    {  
                        if (attackCombo > 0)
                        { //Moar swings
                            attackCombo--;
                            state = 1;
                            alarm.setMax(slashWindup / 6);
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
                    if (state == 0)
                    {
                        state = 1;
                        alarm.setMax(spinWindup);
                        alarm.reset();
                        //Stop moving
                        setDestination(my_pos, agent);
                    }
                    //Start
                    else if (state == 1 && alarm.isActive())
                    {
                        state = 2;
                        alarm.setMax(spinInterval);
                        alarm.reset();

                        if (buff)
                        {
                            attackCombo = spinProjectiles;
                            buff = false;
                            if (myBuff != null) myBuff.GetComponent<AI_Effect>().deActivate();
                            attackCount = 4;
                        }
                        else
                        {
                            attackCombo = spinProjectiles;
                            attackCount = 1;
                        }
                    }
                    //Spin and launch
                    else if ((state == 2) && (alarm.isActive()))
                    {
                        if (attackCombo > 0)
                        { //Keep launching
                            state = 2;
                            alarm.setMax(spinInterval);
                            alarm.reset();
                            attackCombo--;
                            for (int i = 0; i < attackCount; i++)
                            {
                                Vector2 randVector = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                                randVector.Normalize();
                                eScript.DispenseObject(slashProjectile, my_pos, randVector, slashProjectileSpeed);
                            }
                        }
                        else
                        { //Cooldown
                            state = 3;
                            alarm.setMax(spinCooldown / 3);
                            alarm.reset();
                        }
                    }
                    else if (state == 3 && alarm.isActive()) {
                        state = 5;
                        alarm.setMax(2*spinCooldown / 3);
                        alarm.reset();
                    }
                }
                //Buff
                else if (attackType == 4){
                    if (state == 0)
                    {
                        state = 1;
                        alarm.setMax(slamWindup);
                        alarm.reset();
                        //Stop moving
                        setDestination(my_pos, agent);
                    }
                    //Slam
                    else if (state == 1 && alarm.isActive())
                    {
                        state = 2;
                        alarm.setMax(slamDuration);
                        alarm.reset();
                        //Launch 4 projectiles
                        eScript.DispenseObject(slashProjectile, my_pos, Vector2.down, slashProjectileSpeed);
                        eScript.DispenseObject(slashProjectile, my_pos, Vector2.up, slashProjectileSpeed);
                        eScript.DispenseObject(slashProjectile, my_pos, Vector2.left, slashProjectileSpeed);
                        eScript.DispenseObject(slashProjectile, my_pos, Vector2.right, slashProjectileSpeed);
                        buff = true;
                        if (myBuff != null) myBuff.GetComponent<AI_Effect>().activate();
                    }
                    //Slam done
                    else if ((state == 2) && (alarm.isActive()))
                    {
                            state = 5;
                            alarm.setMax(slamCooldown);
                            alarm.reset();
                    }
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

    public override void getStats(ref int health, ref int armor, ref int poise, ref int meleeDamage, ref int rangeDamage)
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

    public void setDestination(Vector2 pos, NavMeshAgent agent)
    {
        if (agent.isOnNavMesh) agent.SetDestination(pos);
        else
        {
            //Do stuff
        }
    }
}
