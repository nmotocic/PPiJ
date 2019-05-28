using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTowardsMotion : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        Vector3 dir = gameObject.GetComponent<Rigidbody2D>().velocity;
        var rotation = Quaternion.LookRotation(transform.position+dir,transform.TransformDirection(Vector3.up));

        transform.rotation = new Quaternion(0,0, rotation.z, rotation.w);

    }
}
