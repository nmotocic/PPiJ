using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : EntityControllerInterface
{
    EntityScript parentScript;
    Vector2 direction;
    float speed = 2f;
    float health = 20f;
    // Start is called before the first frame update
    public PlayerController(EntityScript ps,float speed) {
        parentScript = ps;
        this.speed = speed;
    }
    // Update is called once per frame
    public void Update()
    {
        float X = Input.GetAxis("Horizontal");
        float Y = Input.GetAxis("Vertical");
        direction = new Vector2(X, Y);
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Dispensing...");
            Vector2 position = parentScript.gameObject.transform.position;
            Vector2 firingDirection = (Vector2)Input.mousePosition - position;
            parentScript.DispenseObject(parentScript.projectileOptions[0], position, firingDirection.normalized);
        }
    }
    public Vector2 getMovement() { return direction*speed; }
    public void OnCollisionEnter2D(Collision2D col) {
    }
}
