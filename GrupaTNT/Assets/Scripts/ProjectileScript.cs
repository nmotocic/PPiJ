using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    public Vector2 direction;
    public float speed=0.0f;
    int TTL;
    // Start is called before the first frame update
    void Start()
    {
        TTL = 100;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(direction * speed);
        TTL--;
        if (TTL == 0 && speed!=0.0f) { GameObject.Destroy(gameObject); }
    }
    void OnCollisionEnter2D(Collision2D col)
    {
        EntityScript ES = col.otherCollider.gameObject.GetComponent<EntityScript>();
        Vector2 ve;
        foreach (ContactPoint2D e in col.contacts) {
            ve = (Vector2)transform.position - e.point;
            if (Mathf.Abs(ve.x) > Mathf.Abs(ve.y))
            {
                if (direction.x*ve.x < 0) { direction.x = 0 - direction.x; }
            }
            else
            {
                if (direction.y * ve.y < 0) { direction.y = 0 - direction.y; }
            }
        }
    }
}
