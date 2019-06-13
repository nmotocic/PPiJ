using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : EntityControllerInterface
{
    EntityScript parentScript;
    PlayerSpriteRenderer spriteAnimator;
    Vector2 direction;
    float speed = 2f;
    public float health = 10f;
    Alarm attackDelay = new Alarm(0);
    // Start is called before the first frame update
    public PlayerController(EntityScript ps,float speed=0f) {
        parentScript = ps;
        spriteAnimator = parentScript.gameObject.GetComponent<PlayerSpriteRenderer>();
        if (speed == 0f) { speed = this.speed; }
        if (!parentScript.valsInit)
        {
            parentScript.stats.Add("health", new FloatStat("health", health));
            parentScript.stats.Add("ranged", new FloatStat("ranged", 1));
            parentScript.stats.Add("damage", new FloatStat("ranged", 1));
            parentScript.stats.Add("attackSpeed", new FloatStat("attackSpeed", 0.5f)); // Delay between attacks
            parentScript.stats.Add("gold", new FloatStat("gold", 0.01f));
            parentScript.stats.Add("experience", new FloatStat("experience", 0));
            parentScript.stats.Add("armor", new FloatStat("armor", 5));
            parentScript.stats.Add("speed", new FloatStat("speed", speed));
            parentScript.stats.Add("rangedCount", new FloatStat("rangedCount", 1)); //Broj projektila
            parentScript.stats.Add("rangedSpread", new FloatStat("rangedSpread", 15)); //Kut pucanja
            parentScript.valsInit = true;
        }
    }
    
    // Update is called once per frame
    public void Update()
    {
        attackDelay.Update();
        attackDelay.setMax(parentScript.stats["attackSpeed"].getCompoundValue());
        if (parentScript.stats["health"] != null) {
            if (parentScript.stats["health"].getCompoundValue() <= 0) {
                death();
            }
        }
        Debug.Log("Hp:"+parentScript.stats["health"].getCompoundValue()+"Delay:"+attackDelay.getTimer()+"/"+attackDelay.getMax());
        float X = Input.GetAxis("Horizontal");
        float Y = Input.GetAxis("Vertical");
        direction = new Vector2(X, Y);
        if (Input.GetMouseButton(0) && attackDelay.isActive() && parentScript.stats["health"].getCompoundValue()>0)
        {
            Vector2 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 position = parentScript.gameObject.transform.position;
            Vector2 shootVector = (target - position).normalized;
            string[] rawInput= {"EFFECT damage boop 1 -1 1"};
            float angle = 0,diff=0;
            int cnter = Mathf.RoundToInt(parentScript.stats["rangedCount"].getCompoundValue());
            if (cnter <= 1)
            { //Barem jedan projektil
                cnter = 1;
            }
            else { //Izracunaj kuteve
                diff = Mathf.Abs(parentScript.stats["rangedSpread"].getCompoundValue() / (parentScript.stats["rangedCount"].getCompoundValue()-1));
                angle = GameDefaults.vectToAngle(shootVector);
                angle -= Mathf.Abs(parentScript.stats["rangedSpread"].getCompoundValue()/2);
                shootVector = GameDefaults.angleToVect(angle);
                //Debug.LogWarning("Angle:" + angle.ToString());
            }
            for (int i = 0; i < cnter;i++){
                parentScript.DispenseObject(parentScript.projectileOptions[0], position, shootVector, 20f, rawInput);
                angle += diff;
                //if (cnter % 2 == 0 && cnter==2) angle += diff;
                shootVector = GameDefaults.angleToVect(angle);
                //Debug.LogWarning("ShootVector:" + target.ToString() + "-" + position.ToString() + "=" + shootVector.ToString());

            }
            if(spriteAnimator!=null)spriteAnimator.attack = true;
            attackDelay.reset();
        }
    }
    public Vector2 getMovement() { return direction*parentScript.stats["speed"].getCompoundValue(); }
    public void OnCollisionEnter2D(Collision2D col) {
    }

    public void damage(int dmg)
    {
        health -= 0;
        //Debug.Log("Hp:"+health);
        if (health==0) {
            death();
            spriteAnimator.dead = true;
        }
    }

    public void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log(col.gameObject);
        if (col.gameObject.tag.Equals("LevelExit"))
        {
            LevelManager.Instance.LoadLevel();
        }
    
        //Do stuff
    }
    public void death() {
        Debug.LogWarning("Oof");
        spriteAnimator.dead = true;
    }
}
