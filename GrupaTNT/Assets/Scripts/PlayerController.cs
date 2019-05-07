using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, EntityControllerInterface
{
    Vector2 direction;
    float speed = 2f;
    float health = 20f;
    // Start is called before the first frame update
    public PlayerController(float v) {
        speed=v;
    }
    // Update is called once per frame
    public void Update()
    {
        float X = Input.GetAxis("Horizontal");
        float Y = Input.GetAxis("Vertical");
        if (Input.GetMouseButtonDown(0)) {

        }
        direction = new Vector2(X, Y);
    }
    public Vector2 getMovement() { return direction*speed; }
    public void OnCollisionEnter2D(Collision2D col) {
    }
}
