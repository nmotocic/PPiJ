using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
            ES.applyPowerup(ES.stats[name], name, value, time);
        }
        catch (System.Exception e)
        {
            Debug.Log("NOT FOUND:"+name);
            throw e;
        }
        finally { };
    }
}

public class EntityScript : MonoBehaviour
{
    public List<string> input;
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
        getController(entityType,direction,speed);
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
        else {
            if (gameObject.CompareTag(GameDefaults.Player())) { controller = new PlayerController(this, speed); }
            else if (gameObject.CompareTag(GameDefaults.Projectile())) { controller = new ProjectileController(this, direction, speed); }
            else if (gameObject.CompareTag(GameDefaults.Powerup())) { controller = new PowerupController(this); }
            else if (gameObject.CompareTag(GameDefaults.Enemy())) { controller = new EnemyController(this);}
        }
    }

    public void Start()
    {
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
        controller = new PlayerController(this,speed);
    }

    // Update is called once per frame
    public void Update()
    {

        if (controller == null) { return; }
        /*if (Input.GetMouseButtonDown(0) && gameObject.CompareTag(GameDefaults.Enemy()))
        {
            controller.damage(2);
            Debug.LogWarning("Napravio 2 dmga objektu:" + parent.GetInstanceID().ToString());
        }*/
        controller.Update();
        //DEBUG REMOVE AFTER TESTIIIING
        if (stats.ContainsKey("health")&&CompareTag(GameDefaults.Player()))
        {
            Debug.Log("Hp:" + stats["health"].getCompoundValue());
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
        if (gameObject.CompareTag(GameDefaults.Projectile())&&(other.CompareTag(GameDefaults.Obstruction())))
        {
            Destroy(gameObject);
            return;
        }
        if (other.CompareTag(GameDefaults.Obstruction()))
        {
            return;
        }
        
        if (other.Equals(parent) || otherES.parent!=null&&otherES.parent.Equals(gameObject)) { return; }
        foreach (string effect in impactEffects.Keys)
        {
            //Debug.Log(gameObject.tag+other.tag);
            Debug.Log(effect);
            if (effect.Equals("damage"))
            {
                if (!otherES.stats.ContainsKey("health")) { continue; }
                float x = impactEffects["damage"].value;
                if (otherES.stats.ContainsKey("armor"))
                {
                    FloatStat FSA = otherES.stats["armor"];
                    x = Mathf.Max(x - FSA.getCompoundValue(), 1f);
                }
                FloatStat FSH = otherES.stats["health"];
                
                FSH.ChangeWithFactor("baseValue", 0 - x);
            }
            else if (effect.Equals("health"))
            {
                if (!otherES.stats.ContainsKey("health")) { continue; }
                float x = impactEffects["health"].value;
                FloatStat FSH = otherES.stats["health"];
                
                FSH.ChangeWithFactor("baseValue", x);
            }
            else
            {
                FSQI effectData = impactEffects[effect];
                effectData.ApplyTo(otherES);
            }
        }
        if (gameObject.CompareTag(GameDefaults.Powerup()))
        {
            Destroy(gameObject);
            return;
        }
        if (true) //Unity ima ugrađene tagove i layere, zasto si stvarao svoje?
        {
            //Projectile collisions
            if (gameObject.CompareTag(GameDefaults.Projectile())){
                //Obstruction
                if (other.CompareTag(GameDefaults.Obstruction()))
                {
                    Destroy(gameObject);
                    return;
                }
                if (!parent.CompareTag(other.gameObject.tag))
                {
                    controller.OnTriggerEnter2D(collision);
                    GameObject.Destroy(gameObject);
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
                    controller.OnTriggerEnter2D(collision);
                }
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision) {
        OnTriggerStay2D(collision);
    }

    public void DispenseObject(GameObject dispensable, Vector2 location, Vector2 direction, float speed=0.2f)
    {
        GameObject x = Instantiate(dispensable);
        EntityScript y = x.AddComponent<EntityScript>();
        y.Init("projectile",location,direction,speed,gameObject);
        float dmg = stats["ranged"].getCompoundValue();
        FloatStat FS = new FloatStat("damage", dmg);
        y.impactEffects.Add("damage",new FSQI(FS,"irrelevant",dmg,0,1));
    }
    Vector2 GetLocation() {
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
        if (directAccess.ContainsKey(template)) {
            existing = directAccess[template];
            queue[time_period(existing.time)].Remove(existing);
            directAccess.Remove(template);
        } else { queue[timePeriod] = new List<FSQI>(); }
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