using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Follow : MonoBehaviour
{

    private NavMeshAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    // Update is called once per frame
    void Update()
    {
        var target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        target.z = 0;
        agent.destination = target;
    }
}
