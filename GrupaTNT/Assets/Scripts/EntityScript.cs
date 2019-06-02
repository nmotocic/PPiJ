﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class EntityScript : MonoBehaviour
{
    public GameObject parent;
    public EntityControllerInterface controller;
    public Dictionary<string, FloatStat> stats = new Dictionary<string, FloatStat>();
    public List<GameObject> projectileOptions = new List<GameObject>();
    List<GameObject> firedProjectiles = new List<GameObject>();
    Rigidbody2D rb2d;
    public float speed=5;
    public string entityType;

    // Start is called before the first frame update
    public void Init(string entityType,Vector2 location,Vector2 direction,float speed,GameObject parent=null)
    {
        this.parent = parent;
        gameObject.transform.position = location;
        gameObject.SetActive(true);
        this.entityType = entityType;
        rb2d = gameObject.GetComponent<Rigidbody2D>();
        if (rb2d == null) { rb2d = gameObject.AddComponent<Rigidbody2D>(); }
        if (entityType.Equals("player")) { controller = new PlayerController(this, speed); }
        else if (entityType.Equals("projectile")) { controller = new ProjectileController(this, direction, speed); }
        else if (entityType.Equals("powerup")) { controller = new PowerupController(this); }
        else if (entityType.Equals("enemy")) { controller = new EnemyController(this); }
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
        GameObject other = col.collider.gameObject;
        EntityScript otherES = other.GetComponent<EntityScript>();
        Debug.Log(gameObject);
        if (otherES == null) //Unity ima ugrađene tagove i layere, zasto si stvarao svoje?
        {
            if (other.CompareTag(GameDefaults.Obstruction()) && gameObject.CompareTag(GameDefaults.Projectile()))
            {
                GameObject.Destroy(gameObject);
            }
        }
        else if (entityType == "projectile" && parent != other && parent != null)
        {
            if (otherES.entityType == "player")
            {
                FloatStat health = otherES.stats["health"];
                float x = health.getFactor("current", 1f);
                x -= 0.02f;
                Debug.Log(x);
                health.setFactor("current", (x > 0) ? x : 0f);
            }
            GameObject.Destroy(gameObject);
        }
        if (entityType == "powerup" && otherES.entityType == "player")
        {
            FloatStat health = otherES.stats["health"];
            float x = health.getFactor("current", 1f);
            x += 0.02f;
            Debug.Log(x);
            health.setFactor("current", (x > 0) ? x : 0f);
            Vector2 V2 = new Vector2(Random.Range(-5f, 5f), Random.Range(-2f, 2f));
            this.DispenseObject(gameObject, V2, new Vector2(), 0f);//za demonstraciju
            GameObject.Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject other = collision.gameObject;
        EntityScript otherES = other.GetComponent<EntityScript>();
        Debug.Log(gameObject);
        
        if (otherES == null) //Unity ima ugrađene tagove i layere, zasto si stvarao svoje?
        {
            if (other.CompareTag(GameDefaults.Obstruction()) && gameObject.CompareTag(GameDefaults.Projectile()))
            {
                GameObject.Destroy(gameObject);
            }
        }
    }

    public void DispenseObject(GameObject dispensable, Vector2 location, Vector2 direction, float speed=0.2f)
    {
        GameObject x = Instantiate(dispensable);
        EntityScript y = x.AddComponent<EntityScript>();
        y.Init("projectile",location,direction,speed,gameObject);
    }
    Vector2 GetLocation() {
        return gameObject.transform.position;
    }
}