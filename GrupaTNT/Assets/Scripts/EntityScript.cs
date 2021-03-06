﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FSQI
{
    public FloatStat stat;
    public string modifier;
    public float value;
    public float time;
    public int mode;
    public FSQI(FloatStat stat, string modifier, float value=0.0f, float time=0.0f, int mode=0) {
        this.stat = stat;this.modifier = modifier;this.value = value;this.time = time;this.mode = mode;
    }
    public void ApplyTo(EntityScript ES)
    {
        string name = stat.getName();
        try
        {
            ES.applyPowerup(ES.stats[name], name, value, time, mode);
        }
        catch (System.Exception e)
        {
            Debug.Log("NOT FOUND:"+name+" IN "+ES);
            throw e;
        }
        finally { };
    }
}

public class EntityScript : MonoBehaviour
{
    public bool initFromStart = false;
    public List<string> rawInput = new List<string>();
    public const float TIMEBASE = 60f;
    public int time_period(float t, float period = TIMEBASE) { return (int)(t / TIMEBASE); }
    FSQI XX = new FSQI(null, "wasd", 1.0f);
    public GameObject parent;
    public EntityControllerInterface controller;
    public Dictionary<string, FloatStat> stats = new Dictionary<string, FloatStat>();
    public Dictionary<string, FSQI> impactEffects = new Dictionary<string, FSQI>();
    public Dictionary<int, List<FSQI>> queue = new Dictionary<int, List<FSQI>>();
    int currentTimePeriod = 0;
    public Dictionary<FSQI, FSQI> directAccess = new Dictionary<FSQI, FSQI>();
    public List<GameObject> projectileOptions = new List<GameObject>();
    List<GameObject> firedProjectiles = new List<GameObject>();
    public GameObject drop;
    Rigidbody2D rb2d;
    public float speed = 20f;
    public string entityType = null;
    public bool valsInit = false;

    public void Awake()
    {
        if (entityType=="player")
        {
            DontDestroyOnLoad(this);

            /*if (FindObjectsOfType(GetType()).Length > 1)
            {
                Destroy(gameObject);
            }*/
        }
    }

    // Start is called before the first frame update
    public void Init(string entityType, Vector2 location, Vector2 direction, float speed, GameObject parent = null)
    {
        this.parent = parent;
        gameObject.transform.position = location;
        gameObject.SetActive(true);
        this.entityType = entityType;
        getController(entityType, direction, speed);
    }
    public void Input()
    {
        foreach (string line in rawInput)
        {
            string[] split = line.Split(' ');
            Debug.Log(gameObject + line + split.Length.ToString());
            if (split.Length == 0) { continue; }
            if (split[0].Equals("STAT"))
            {
                stats.Add(split[1], new FloatStat(split[1], float.Parse(split[2])));
            }
            else if (split[0].Equals("EFFECT"))
            {
                FloatStat dfl = new FloatStat(split[1]);
                impactEffects[split[1]] = new FSQI(dfl,
                    split[2],
                    float.Parse(split[3]),
                    float.Parse(split[4]),
                    int.Parse(split[5])
                    );
            }
        }
    }
    public void InitLite(string entityType, Vector2 location, Vector2 direction, float speed, GameObject parent = null)
    {
        this.parent = parent;
        gameObject.transform.position = location;
        gameObject.SetActive(true);
        this.entityType = entityType;
        getController(entityType, direction, speed);
    }
    public void getController(string entityType, Vector2 direction, float speed = 0)
    {
        if (direction == null) direction = Vector2.zero;
        rb2d = gameObject.GetComponent<Rigidbody2D>();
        if (rb2d == null) { rb2d = gameObject.AddComponent<Rigidbody2D>(); }
        if (entityType != null)
        {
            if (entityType.Equals("player")) { controller = new PlayerController(this, speed); }
            else if (entityType.Equals("projectile")) { controller = new ProjectileController(this, direction, speed); }
            else if (entityType.Equals("powerup")) { controller = new PowerupController(this); }
            else if (entityType.Equals("enemy")) { controller = new EnemyController(this); }
        }
        else
        {
            if (gameObject.CompareTag(GameDefaults.Player())) { controller = new PlayerController(this, speed); }
            else if (gameObject.CompareTag(GameDefaults.Projectile())) { controller = new ProjectileController(this, direction, speed); }
            else if (gameObject.CompareTag(GameDefaults.Powerup())) { controller = new PowerupController(this); }
            else if (gameObject.CompareTag(GameDefaults.Enemy())) { controller = new EnemyController(this);}
        }
    }

    public void Start()
    {
        if (parent == null) parent = gameObject;
        if (entityType.Equals("powerup"))
        {
            Init(entityType, transform.position, new Vector2(), 0f, null);
        }
        if (!entityType.Equals("player"))
        {
            return;
        }
        controller = this.gameObject.GetComponent<EntityControllerInterface>();
        rb2d = gameObject.GetComponent<Rigidbody2D>();
        if (rb2d == null) { rb2d = gameObject.AddComponent<Rigidbody2D>(); }
        controller = new PlayerController(this, speed);
    }

    // Update is called once per frame
    public void Update()
    {
        if (gameObject.CompareTag(GameDefaults.Player())) {
            if (listAllPowerups().Count > 0) {
                checkPowerups();
                Debug.LogWarning("Pups:" + listAllPowerups().ToString());
            }
            if (controller == null) {
                getController("player", new Vector2(0,0), 0);
            }
        }
        if (controller == null) { return; }
        /*if (Input.GetMouseButtonDown(0) && gameObject.CompareTag(GameDefaults.Enemy()))
        {
            controller.damage(2);
            Debug.LogWarning("Napravio 2 dmga objektu:" + parent.GetInstanceID().ToString());
        }*/
        controller.Update();
        //DEBUG REMOVE AFTER TESTIIIING
        if (stats.ContainsKey("gold")&&CompareTag(GameDefaults.Player()))
        {
            Debug.Log("Gold:" + stats["gold"].getCompoundValue());
        }
        Vector2 movement = controller.getMovement();
        rb2d.velocity = movement;
        if (stats.ContainsKey("health")&&stats["health"].getCompoundValue()<=0.0f) { controller.death(); }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        GameObject other = collision.gameObject;
        EntityScript otherES = other.GetComponent<EntityScript>();
        //Debug.Log(gameObject + "-->" + other);
        if (gameObject.CompareTag(GameDefaults.Projectile()) && (other.CompareTag(GameDefaults.Obstruction())))
        {
            Destroy(gameObject);
            return;
        }
        if (gameObject.CompareTag(GameDefaults.Projectile()) && other.CompareTag(GameDefaults.Projectile()))
        {
            return;
        }
        if (gameObject.CompareTag(GameDefaults.Powerup()) && !other.CompareTag(GameDefaults.Player()))
        {
            return;
        }
        if (other.CompareTag(GameDefaults.Powerup()))
        {
            return;
        }
        if (other.CompareTag(GameDefaults.Obstruction()))
        {
            return;
        }

        if (otherES != null)
        {
            if (parent!=null)
            {
                if (other.gameObject.CompareTag(parent.tag) || otherES.parent != null && otherES.parent.gameObject.CompareTag(gameObject.tag))
                {
                    return;
                }
            }

            foreach (string effect in impactEffects.Keys)
            {
                //Debug.Log(effect);
                if (effect.Equals("damage"))
                {
                    if (gameObject.CompareTag(GameDefaults.Enemy())) {
                        if (!gameObject.GetComponent<AiScriptBase>().isDangerous()) continue;
                        else {
                            gameObject.GetComponent<AiScriptBase>().setDanger(false);
                        }
                    }
                    if (!otherES.stats.ContainsKey("health"))
                    {
                        continue;
                    }

                    float x = impactEffects["damage"].value;
                    if (otherES.stats.ContainsKey("armor"))
                    {
                        FloatStat FSA = otherES.stats["armor"];
                        if (FSA.getCompoundValue() >= 100)
                        {
                            x = 0;
                        }
                        else
                        x = Mathf.Max(x - FSA.getCompoundValue(), 1f);
                    }
                    FloatStat FSH = otherES.stats["health"];
                    otherES.controller.damage((int) x);
                    FSH.ChangeWithFactor("baseValue", 0 - x);
                    FSH = new FloatStat ("health",Mathf.RoundToInt(FSH.getCompoundValue()));
                }
                else if (effect.Equals("healthBoost"))
                {
                    if (!otherES.stats.ContainsKey("health"))
                    {
                        continue;
                    }

                    float x = impactEffects["health"].value;
                    FloatStat FSH = otherES.stats["health"];

                    FSH.ChangeWithFactor("baseValue", x);
                }
                else
                {
                    FSQI effectData = impactEffects[effect];
                    Debug.Log(otherES.stats.ContainsKey(effect).ToString()+effect);
                    if (!otherES.stats.ContainsKey(effect)) continue;
                    effectData.ApplyTo(otherES);
                }
            }
        }
        if (gameObject.CompareTag(GameDefaults.Powerup())&& other.CompareTag(GameDefaults.Player()))
        {
            Destroy(gameObject);
            return;
        }
        //Stari kod
        if (true) //Unity ima ugrađene tagove i layere, zasto si stvarao svoje?
        {
            //Projectile collisions
            if (gameObject.CompareTag(GameDefaults.Projectile()))
            {
                //Obstruction
                if (other.CompareTag(GameDefaults.Obstruction()))
                {
                    Destroy(gameObject);
                    return;
                }
                if (parent!=null &&! parent.CompareTag(other.gameObject.tag))
                {
                    controller.OnTriggerEnter2D(collision);
                    GameObject.Destroy(gameObject);
                    
                    // Wake up any script that is attached to the enemy + navmesh
                    var otherChaser = other.GetComponent<AiScriptBase>();
                    if (otherChaser != null && !otherChaser.enabled)
                    {
                        otherChaser.enabled = true;
                        other.GetComponent<NavMeshAgent>().enabled = true;
                    }

                }
            }
            //Enemy coll
            else if (gameObject.CompareTag(GameDefaults.Enemy()))
            {

                //Obstruction
                if (other.CompareTag(GameDefaults.Obstruction()))
                {
                    //Zid
                }
                //Enemy
                else if (other.CompareTag(GameDefaults.Enemy()))
                {
                    //var es = other.gameObject.GetComponent<EntityScript>();
                }
                //Player
                else if (other.gameObject.CompareTag(GameDefaults.Player()))
                {
                    if(controller !=null  && collision != null)
                    controller.OnTriggerEnter2D(collision);
                }
            }
            else if (gameObject.CompareTag(GameDefaults.Player()))
            {
                if (other.gameObject.CompareTag(GameDefaults.LevelExit()))
                {
                    if (controller != null && collision != null)
                        controller.OnTriggerEnter2D(collision);
                }
            }
        }
        
    }
    private void OnTriggerEnter2D(Collider2D collision) {
        OnTriggerStay2D(collision);
    }

    public void DispenseObject(GameObject dispensable, Vector2 location, Vector2 direction, float speed = 0.2f, string[] input=null, string type="projectile")
    {
        GameObject x = Instantiate(dispensable);
        EntityScript y = x.AddComponent<EntityScript>();
        y.Init(type, location, direction, speed, gameObject);
        if (input != null)
        {
            y.rawInput.AddRange(input);
        }
        if (type == "projectile")
        {
            float dmg = stats["ranged"].getCompoundValue();
            y.rawInput.Add("EFFECT damage irrelevant " + dmg.ToString() + " 0 1");
        }
        y.Input();
        Debug.Log(speed);
        y.Init(type,location,direction,speed,gameObject);
    }
    Vector2 GetLocation()
    {
        return gameObject.transform.position;
    }
    public void applyPowerup(FSQI fSQI) {
        applyPowerup(fSQI.stat, fSQI.modifier, fSQI.value, fSQI.time, fSQI.mode);
    }
    public void applyPowerup(FloatStat stat,string powName, float value=1.0f, float duration=-0.1f, int mode=0){
        float time = Time.time + duration;
        int timePeriod = time_period(time);
        FSQI powerup, template, existing;
        powerup = new FSQI(stat,powName,value,time);
        template = new FSQI(stat, powName);
        Debug.Log(stat.getName()+mode);
        if (directAccess.ContainsKey(template)) {
            existing = directAccess[template];
            queue[time_period(existing.time)].Remove(existing);
            directAccess.Remove(template);
        } else { queue[timePeriod] = new List<FSQI>(); }
        if (mode == 1) {
            stat.ChangeWithFactor(powName, value);return;
        }
        stat.setFactor(powName, value);

        if (duration > 0) {
            queue[timePeriod].Add(powerup);
            directAccess[template] = powerup;
        }
    }
    public void checkPowerups() {
        float time,TP;
        time = Time.time;
        TP = time_period(time);
        while (currentTimePeriod < TP) {
            if (queue.ContainsKey(currentTimePeriod)) {
                foreach (FSQI element in queue[currentTimePeriod]) {
                    applyPowerup(element.stat,element.modifier);
                }
                currentTimePeriod++;
            }
        }
        if(queue.ContainsKey(currentTimePeriod))
        foreach (FSQI element in queue[currentTimePeriod])
        {
            if (element.time <= time)
            {
                applyPowerup(element.stat, element.modifier);
            }
        }
    }
    public List<FSQI> listAllPowerups() {
        List<FSQI> returnList = new List<FSQI>();
        foreach (FSQI e in directAccess.Keys) {
            returnList.Add(directAccess[e]);
        }
        return returnList;
    }
}