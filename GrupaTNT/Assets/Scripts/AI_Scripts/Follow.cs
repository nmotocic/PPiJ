using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Follow : MonoBehaviour
{

    private NavMeshAgent agent;
    private int state=0;
    private int attackDist = 4;
    private int attackSpeed = 300;
    private int maxAttackDist = 11;
    private Vector2 targetDir,startPos;
    private Alarm alarm = new Alarm(0);

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    // Update is called once per frame
    void Update()
    {
        alarm.Update();
        var my_pos = transform.position;
        var target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var rbody2d = gameObject.GetComponent<Rigidbody2D>();
        target.z = 0;
        Debug.Log("State:" + state.ToString() + "\nTimer:" + alarm.getTimer() + "\nTimer active:"+ alarm.isActive());
        if (Physics2D.Linecast(my_pos, target) && state==0)
        {   //Target obstructed
            agent.destination = target;
        }
        else { //State machine
            //Pick state
            if (state != 0)
            {
                agent.destination = my_pos;
            }
            if (Vector2.Distance(my_pos, target) <= attackDist && state == 0)
            { //Prep attack
                agent.destination = my_pos;
                state = 1;
                targetDir = -(my_pos - target);
                targetDir.Normalize();
                targetDir.Scale(new Vector2(attackSpeed, attackSpeed));
                startPos = my_pos;
                //Alarm
                alarm.setMax(2);
                alarm.reset();
            }
            else if (state == 1 && alarm.isActive()) { //Wait for alarm
                state = 2;
            }
            else if (state == 2)
            { //Attack
                rbody2d.AddForce(targetDir);
                state = 3;

                //Alarm
                alarm.setMax(1.5);
                alarm.reset();
            }
            else if ((state == 3 & alarm.isActive()))
            { //Attack over - stop
                rbody2d.velocity = new Vector2(0, 0);
                state = 4;
                alarm.setMax(1);
                alarm.reset();
            }
            else if ((state == 4 & alarm.isActive()))
            { //Attack over - stop
                state = 0;
            }
            else if (state == 0)
            {
                agent.destination = target;
            }
        }
    }

}
