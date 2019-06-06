using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTowardsMotion : MonoBehaviour
{
    private bool stop = false;
    // Update is called once per frame
    void Update()
    {
        if (stop) return;
        var rbody2D = gameObject.GetComponent<Rigidbody2D>();
        Vector2 vel = rbody2D.velocity;
        var pos = rbody2D.position;
        if (vel != Vector2.zero) {
            var angle = Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg;
            gameObject.transform.Rotate(new Vector3(0, 0, angle), Space.World);
            stop = true;
        }
        /*
        var rotation = Quaternion.LookRotation((rbody2D.position+dir),transform.TransformDirection(Vector3.up));
        rotation.Normalize();
        transform.rotation = new Quaternion(0,0, -rotation.z, -rotation.w);
        */

    }
}
