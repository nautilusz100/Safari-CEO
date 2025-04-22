using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Jeep : MonoBehaviour
{
    private NavMeshAgent agent;
    private Vector2 safariExit;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        safariExit = new Vector2(42.5f, 39.5f);
        agent.SetDestination(safariExit);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
