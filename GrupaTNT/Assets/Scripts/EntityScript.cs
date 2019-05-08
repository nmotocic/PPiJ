using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityScript : MonoBehaviour
{
    public EntityControllerInterface controller;
    public Dictionary<string, string> stats; //Za sada public
    public List<GameObject> projectileOptions = new List<GameObject>();
    List<GameObject> firedProjectiles = new List<GameObject>();
    Rigidbody2D rb2d;
    public float speed=5;
    public string entityType;
    // Start is called before the first frame update
    public void Init(string entityType,Vector2 location,Vector2 direction,float speed)
    {
        gameObject.transform.position = location;
        gameObject.SetActive(true);
        this.entityType = entityType;
        rb2d = gameObject.GetComponent<Rigidbody2D>();
        if (rb2d == null) { rb2d = gameObject.AddComponent<Rigidbody2D>(); }
        if (entityType.Equals("player")) { controller = new PlayerController(this,speed); }
        else if (entityType.Equals("projectile")) { controller = new ProjectileController(this,direction,speed); }
        else if (entityType.Equals("powerup")) { controller = new ProjectileController(this, direction, speed); }
    }
    public void Start()
    {
        if (!entityType.Equals("player")) { return; }
        controller = this.gameObject.GetComponent<EntityControllerInterface>();
        rb2d = gameObject.GetComponent<Rigidbody2D>();
        if (rb2d == null) { rb2d = gameObject.AddComponent<Rigidbody2D>(); }
        controller = new PlayerController(this,speed);
    }

    // Update is called once per frame
    public void Update()
    {
        if (controller == null) { return; }
        controller.Update();
        Vector2 movement = controller.getMovement();
        rb2d.velocity = movement;
    }
    void OnCollisionEnter2D(Collision2D col)
    {
        print(col.otherCollider.gameObject);
    }
    public void DispenseObject(GameObject dispensable, Vector2 location, Vector2 direction, float speed=0.2f)
    {
        GameObject x = Instantiate(dispensable);
        EntityScript y = x.AddComponent<EntityScript>();
        Debug.Log(direction.ToString() + " " + speed.ToString());
        y.Init("projectile",location,direction,speed);
    }
    Vector2 GetLocation() {
        return gameObject.transform.position;
    }
}