using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : EntityControllerInterface
{
    Vector2 direction;
    EntityScript parent;
    static float defSpeed = 0f; //Disabled
    float speed = defSpeed;

    public EnemyController(EntityScript rb) {
        parent = rb;
    }

    // Start is called before the first frame update
    public static void main()
    {
        return;
    }
    // Update is called once per frame
    public void Update()
    {
        //parent.GetComponent<Rigidbody2D>().GetVector();
        direction = direction.normalized;
    }
    public Vector2 getMovement() {
        return direction*speed;
    }

    public void OnCollisionEnter2D(Collision2D col) {
        //Code
    }
}
