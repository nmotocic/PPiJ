using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : EntityControllerInterface
{
    EntityScript parentScript;
    Vector2 direction;

    float speed;
    // Start is called before the first frame update
    public ProjectileController(EntityScript ps, Vector2 direction, float speed)
    {
        this.direction = direction;
        this.parentScript = ps;
        this.speed = speed;
    }
    // Update is called once per frame
    public void Update()
    {
    }
    public Vector2 getMovement() { return direction * speed; }
    public void OnCollisionEnter2D(Collision2D col)
    {
        EntityScript ES = col.otherCollider.gameObject.GetComponent<EntityScript>();
        Vector2 ve;
        foreach (ContactPoint2D e in col.contacts)
        {
            ve = (Vector2)parentScript.transform.position - e.point;
            if (Mathf.Abs(ve.x) > Mathf.Abs(ve.y))
            {
                if (direction.x * ve.x < 0) { direction.x = 0 - direction.x; }
            }
            else
            {
                if (direction.y * ve.y < 0) { direction.y = 0 - direction.y; }
            }
        }
    }


    public void OnTriggerEnter2D(Collider2D col)
    {
        var es = col.gameObject.GetComponent<EntityScript>();
        //Do stuff
    }
    public void death() { }
}