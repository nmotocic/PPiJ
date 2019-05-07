using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : EntityControllerInterface
{
    EntityScript parentScript;
    Vector2 direction;
    float speed = 1f;
    // Start is called before the first frame update
    public ProjectileController(EntityScript ps, Vector2 directioninput)
    {
        this.direction = directioninput;
        parentScript = ps;
    }
    // Update is called once per frame
    public void Update()
    {
        float X = Input.GetAxis("Horizontal");
        float Y = Input.GetAxis("Vertical");
        direction = new Vector2(X, Y);
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
}