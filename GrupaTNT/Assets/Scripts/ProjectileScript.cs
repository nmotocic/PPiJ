using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    public Vector2 direction;
    public float speed=0.0f;
    int TTL;
    // Start is called before the first frame update
    void Start()
    {
        TTL = 100;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(direction);
        transform.Translate(direction * speed);
        TTL--;
        if (TTL == 0 && speed!=0.0f) { GameObject.Destroy(gameObject); }
    }
}
