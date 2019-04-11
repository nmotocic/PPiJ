﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : EntityControllerInterface
{
    Vector2 direction;
    float speed = 10f;
    // Start is called before the first frame update
    public static void main() {
        return;
    }
    // Update is called once per frame
    public void Update()
    {
        float X = Input.GetAxis("Horizontal");
        float Y = Input.GetAxis("Vertical");
        direction = new Vector2(X, Y);
    }
    public Vector2 getMovement() { return direction * speed; }
}
