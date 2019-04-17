using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : EntityControllerInterface
{
    Vector2 direction;
    Rigidbody2D rb2d;
    static float defSpeed = 0f; //Disabled
    float speed = defSpeed;

    public EnemyController(Rigidbody2D rb) {
        rb2d = rb;
    }

    // Start is called before the first frame update
    public static void main()
    {
        return;
    }
    // Update is called once per frame
    public void Update()
    {
        Vector3 myPos = rb2d.position;
        direction = AiFollowPoint.simpleFollow(myPos, Camera.main.ScreenToWorldPoint(Input.mousePosition), false);
        //Stop moving
        if (direction.magnitude < defSpeed/4)
        {
            speed = 0;
        }
        else { //Continue moving
            speed = defSpeed;
        }
        direction = direction.normalized;
    }
    public Vector2 getMovement() {
        return direction*speed;
    }
}
