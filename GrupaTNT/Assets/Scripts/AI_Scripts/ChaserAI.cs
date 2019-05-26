using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EntityScript))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody2D))]
public class ChaserAI : MonoBehaviour
{

    private NavMeshAgent agent;
    private int state = 0; //State machine

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

    private Vector2 targetDir,startPos;
    private Alarm alarm = new Alarm(0);
    private Rigidbody2D rbody2d;
    private Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        rbody2d = gameObject.GetComponent<Rigidbody2D>();
        anim = gameObject.GetComponentInChildren<Animator>();
    }



    // Update is called once per frame
    void Update()
    {
        alarm.Update();
        var my_pos = transform.position;
        Vector3 target;
        var targetObject = GameObject.FindGameObjectWithTag(targetObjectTag);

        if (targetObject == null)
        {
            target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        else
        {
            target = targetObject.transform.position;
        }
        updateAnimation(Mathf.Sign(my_pos.x-target.x));
        target.z = 0;
        if (AiDefaults.getLineSight(my_pos, target) && state ==0)
        {   //Target obstructed
            agent.destination = target;
        }
        else { //State machine
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
                targetDir = target-my_pos;
                targetDir.Normalize();
                targetDir.Scale(new Vector2(attackSpeed, attackSpeed));
                startPos = my_pos;
                //Alarm
                alarm.setMax(attackWindup);
                alarm.reset();
            }
            else if (state == 1 && alarm.isActive())  //Wait for alarm
            { //Attack
                rbody2d.AddForce(targetDir);
                state = 2;

                //Alarm
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

    void updateAnimation(float flip){
        var scale = gameObject.transform.localScale;
        if (flip != Mathf.Sign(scale.x) && state==0) {
            gameObject.transform.localScale = new Vector3(-scale.x, scale.y, scale.z);
        }
        anim.SetInteger("State", state);
    }
}
