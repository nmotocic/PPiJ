using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateZ : MonoBehaviour
{
    public bool randomStartAngle = false;
    public float turnSpeed = 10;
    public Vector3 rotVector = new Vector3(0,0,1);
    private Vector3 angle;
    
    // Start is called before the first frame update
    void Start()
    {
        angle = gameObject.transform.localEulerAngles;
        if(randomStartAngle) gameObject.transform.Rotate(rotVector, Random.Range(0,360f));
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.Rotate(rotVector, -turnSpeed);
    }
}
