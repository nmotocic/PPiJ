using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class StickToObject : MonoBehaviour
{
    public GameObject frend;
    Rigidbody2D rbody;
    void Start()
    {
        rbody = gameObject.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (frend != null) {
            if (frend.GetComponent<Rigidbody2D>() != null)
                rbody.position = frend.GetComponent<Rigidbody2D>().position;
        } 
    }

    public void setStick(GameObject fren) {
        frend = fren;
    }
}
