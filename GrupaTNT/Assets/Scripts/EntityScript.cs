using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityScript : MonoBehaviour
{
    EntityControllerInterface controller;
    private Rigidbody rb2d;
    public string entityType= "player";
    // Start is called before the first frame update
    public void Start()
    {
        rb2 = gameObject.GetComponent<Rigidbody>;
        if (entityType.Equals("player")) { controller = new PlayerController(); }
    }

    // Update is called once per frame
    public void Update()
    {
        controller.Update();
        Vector2 movement = controller.getMovement();
        rb2d.AddForce(movement);
    }
    void OnCollisionEnter2D(Collision2D col)
    {
        Debug.Log("Ow");
        Debug.Log(col.contacts);
    }
}
