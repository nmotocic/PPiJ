using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DispenserScript : MonoBehaviour
{
    int DispensingFrames = 20;
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
            ProjectileScript y = x.GetComponent<ProjectileScript>();
            v = new Vector2(Random.Range(-1.0f, 1 - 0f), Random.Range(-1.0f, 1 - 0f));
            y.direction = v.normalized;
            y.speed = 0.2f;
            counter++;
            counter %= 4;
        }
    }
}
