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
    public void Init(string entityType,Vector2 location,Vector2 direction,float speed)
    {
        gameObject.transform.position = location;
        gameObject.SetActive(true);
        this.entityType = entityType;
        controller = this.gameObject.GetComponent<EntityControllerInterface>();
        rb2d = gameObject.GetComponent<Rigidbody2D>();
        if (rb2d == null) { rb2d = gameObject.AddComponent<Rigidbody2D>(); }
        if (entityType.Equals("player")) { controller = new PlayerController(this,speed); }
        else if (!entityType.Equals("projectiles")) { controller = new ProjectileController(this,direction,speed); }
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
        
    }
    public void DispenseObject(GameObject dispensable, Vector2 location, Vector2 direction, float speed=0.2f)
    {
        Debug.Log("Dispensing...");
        GameObject x = Instantiate(dispensable);
        EntityScript y = x.AddComponent<EntityScript>();
        y.Init("projectile",location,direction,speed);
    }
    Vector2 GetLocation() {
        return gameObject.transform.position;
    }
}