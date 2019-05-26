using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EntityScript))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody2D))]
public class RangedAI : MonoBehaviour
{
    private NavMeshAgent agent;
    private int state = 0; //State machine

    //Attack range stats
    public int attackTriggerRange = 8; //Maxiumum range before attack windup
    public int attackSpeed = 5; //Speed of the attack
    public int maxAttackDist = 3; //Maximum distance this object will move while attacking (not implemented)
    //Attack timing stats (seconds)
    public double attackWindup = 2; //Seconds before attack
    public double attackDuration = 0.5; //Seconds attacking(moving)
    public double attackCooldown = 1;

    //Projectile
    public GameObject projectileObject;

    private Vector2 targetDir, startPos;
    //Components
    private Alarm alarm = new Alarm(0);
    private Rigidbody2D rbody2d;
    private EntityScript eScript;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        rbody2d = gameObject.GetComponent<Rigidbody2D>();
        eScript = gameObject.GetComponent<EntityScript>();
        if (projectileObject == null) {
            Debug.LogError("Object disabled, WRONG projectileObject!! Object id:"+this.GetInstanceID().ToString());
            this.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        alarm.Update();
        var my_pos = transform.position;
        var target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        target.z = 0;
        Debug.Log(state.ToString()+"\n"+alarm.getTimer());
        if (AiDefaults.getCircleSight(my_pos,projectileObject.GetComponent<CircleCollider2D>().radius, target, true) && state == 0)
        {   //Target obstructed
            agent.destination = target;
        }
        else
        { //State machine
            //Pick state
            if (state != 0) // Stop moving if winding up an attack
            {
                agent.destination = my_pos;
            }
            else //Move
            {
                agent.destination = target;
            }
            //--------------------------Attacking
            if (Vector2.Distance(my_pos, target) <= attackTriggerRange && state == 0)
            { //Prep attack
                agent.destination = my_pos;
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
                state = 3;
                alarm.setMax(attackDuration);
                alarm.reset();
            }
            else if ((state == 3 & alarm.isActive()))
            { //Attack over - stop
                rbody2d.velocity = new Vector2(0, 0);
                state = 4;
                alarm.setMax(attackCooldown);
                alarm.reset();
            }
            else if ((state == 4 & alarm.isActive()))
            { //Attack over - stop
                state = 0;
            }
            //--------------------------Attacking

        }
    }
}
