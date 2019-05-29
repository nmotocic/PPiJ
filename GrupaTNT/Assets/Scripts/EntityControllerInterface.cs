using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface EntityControllerInterface
{
    void Update();
    Vector2 getMovement();
    void OnCollisionEnter2D(Collision2D col);
    void OnTriggerEnter2D(Collider2D col);
    void damage(int dmg);

}
