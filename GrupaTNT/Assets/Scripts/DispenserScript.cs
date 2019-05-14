using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DispenserScript : MonoBehaviour
{
    public int DispensingFrames = 20;
    int counter = 0;
    public GameObject dispensable;
    // Start is called before the first frame update
    void Start()
    {
        counter = 0;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 v;
        counter++;
        if (counter == DispensingFrames) {
            GameObject x = Instantiate(dispensable);
            v = new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
            EntityScript ES = x.GetComponent<EntityScript>();
            if (ES == null)
            {
                ES = x.AddComponent<EntityScript>();
                ES.Init("projectile", gameObject.transform.position, v.normalized, 20f,gameObject);
            }
            counter++;
            counter %= 4;
        }
    }
}
