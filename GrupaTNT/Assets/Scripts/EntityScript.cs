using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityScript : MonoBehaviour
{
    EntityControllerInterface controller;
    Rigidbody2D rb2d;
    public float speed = 5;
    public string entityType= "player";int i = 0;
    // Start is called before the first frame update
    public void Start()
    {
        rb2d = gameObject.GetComponent<Rigidbody2D>();
        if (entityType.Equals("player")) { controller = new PlayerController(speed); }
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
