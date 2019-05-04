using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, EntityControllerInterface
{
    Vector2 direction;
    float speed = 2f;
    float health = 20f;
    // Start is called before the first frame update
    public PlayerController(float speed) {
        this.speed = speed;
    }
    // Update is called once per frame
    public void Update()
    {
        float X = Input.GetAxis("Horizontal");
        float Y = Input.GetAxis("Vertical");
        direction = new Vector2(X, Y);
    }
    public Vector2 getMovement() { return direction*speed; }
}
