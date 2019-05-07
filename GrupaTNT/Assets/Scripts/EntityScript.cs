using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityScript : MonoBehaviour
{
    EntityControllerInterface controller;
    public List<GameObject> projectileOptions;
    List<GameObject> firedProjectiles = new List<GameObject>();
    Rigidbody2D rb2d;
    public float speed=5;
    public string entityType= "player";int i = 0;
    // Start is called before the first frame update
    public void Start()
    {
        controller = this.gameObject.GetComponent<EntityControllerInterface>();
        rb2d = gameObject.GetComponent<Rigidbody2D>();
        if (entityType.Equals("player")) { controller = new PlayerController(speed); }
    }

    // Update is called once per frame
    public void Update()
    {
        if (controller == null) { return; }
        controller.Update();
        Vector2 movement = controller.getMovement();
        rb2d.velocity = movement;
        print(movement);
    }
    void OnCollisionEnter2D(Collision2D col)
    {
        
    }
    void dispenseObject(GameObject dispensable, Vector2 direction, float speed=0.2f)
    {
        GameObject x = Instantiate(dispensable);
        ProjectileScript y = x.GetComponent<ProjectileScript>();
        Vector2 v = new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(1.0f, -1.0f));
        y.direction = v.normalized;
        y.speed = speed;
    }
    Vector2 getLocation() {
        return gameObject.transform.position;
    }
}