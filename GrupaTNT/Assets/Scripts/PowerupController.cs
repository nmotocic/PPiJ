﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupController : EntityControllerInterface
{
    EntityScript parentScript;
    public PowerupController(EntityScript ps)
    {
        this.parentScript = ps;
    }
    public void Update() { }
    public Vector2 getMovement() { return new Vector2(0f, 0f); }
    public void OnCollisionEnter2D(Collision2D X) { }
}
