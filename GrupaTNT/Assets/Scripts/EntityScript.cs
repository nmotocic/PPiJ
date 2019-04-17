using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EntityScript : MonoBehaviour
{
    EntityControllerInterface controller;
    Rigidbody2D rb2d;
    public string entityType= "player";
    // Start is called before the first frame update
    public void Start()
    {
        rb2d = gameObject.GetComponent<Rigidbody2D>();
        if (entityType.Equals("player")) { controller = new PlayerController(); }
        if (entityType.Equals("enemy")) { controller = new EnemyController(rb2d);  }
    }

    // Update is called once per frame
    public void Update()
    {
        controller.Update();
        Vector2 movement = controller.getMovement();
        rb2d.velocity = movement;
        print(movement);
    }
    void OnCollisionEnter2D(Collision2D col)
    {
        EntityScript ES = col.otherCollider.gameObject.GetComponent<EntityScript>();
        if (ES!=null)
        {
        }
        if (entityType.Equals("player")) {
        }
    }
}
