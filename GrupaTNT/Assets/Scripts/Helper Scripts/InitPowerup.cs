using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(EntityScript))]
public class InitPowerup : MonoBehaviour
{
    enum powerupList {
        health,
        shield,
        damage
    };
    public string type = powerupList.health.ToString();
    public float duration  = 10;
    public float increment  = 5;
    public int noExpiration = 0;
    private EntityScript parentScript;
    // Start is called before the first frame update
    void Start()
    {
        parentScript = gameObject.GetComponent<EntityScript>();
        if (type == "health")
        {
            parentScript.impactEffects.Add("health", new FSQI(new FloatStat("health", 1f), "baseValue", increment, duration, noExpiration));
        }
        else if (type == "shield") {
            parentScript.impactEffects.Add("armor", new FSQI(new FloatStat("armor", 1f), "baseValue", increment, duration, noExpiration));
        }
        else if (type == "damage")
        {
            parentScript.impactEffects.Add("ranged", new FSQI(new FloatStat("ranged", 1f), "baseValue", increment, duration, noExpiration));
        }
        else
        {
            Debug.LogWarning("Wut?");
        }
    }


}
