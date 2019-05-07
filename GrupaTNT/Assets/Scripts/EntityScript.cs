using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityScript : MonoBehaviour
{
    public EntityControllerInterface controller;
    public List<GameObject> projectileOptions = new List<GameObject>();
    List<GameObject> firedProjectiles = new List<GameObject>();
    Rigidbody2D rb2d;
    public float speed=5;
    public string entityType= "player";
    // Start is called before the first frame update
    public void init(string entityType,Vector2 direction,float speed)
    {
        this.entityType = entityType;
        controller = this.gameObject.GetComponent<EntityControllerInterface>();
        rb2d = gameObject.GetComponent<Rigidbody2D>();
        if (rb2d == null) { rb2d = gameObject.AddComponent<Rigidbody2D>(); }
        if (entityType.Equals("player")) { controller = new PlayerController(speed); }
        else if (entityType.Equals("projectile")) { controller = new ProjectileController(this,direction,speed); }
    }
    public void Start()
    {
        Debug.Log(this.ToString()+entityType);
        controller = this.gameObject.GetComponent<EntityControllerInterface>();
        rb2d = gameObject.GetComponent<Rigidbody2D>();
        if (rb2d == null) { rb2d = gameObject.AddComponent<Rigidbody2D>(); }
        if (entityType.Equals("player")) { controller = new PlayerController(speed); }
    }

    // Update is called once per frame
    public void Update()
    {
        if (controller == null) { return; }
        controller.Update();
        Vector2 movement = controller.getMovement();
        rb2d.velocity = movement;
        if (!entityType.Equals("player")) { print(movement); }
    }
    void OnCollisionEnter2D(Collision2D col)
    {
        
    }
    void DispenseObject(GameObject dispensable, Vector2 direction, float speed=0.2f)
    {
        GameObject x = Instantiate(dispensable);
        ProjectileScript y = x.GetComponent<ProjectileScript>();
        Vector2 v = new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(1.0f, -1.0f));
        y.direction = v.normalized;
        y.speed = speed;
    }
    Vector2 GetLocation() {
        return gameObject.transform.position;
    }
}