using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] Transform target; // The target to follow

    NavMeshAgent agent; // The NavMeshAgent component
    void Start()
    {
        agent = GetComponent<NavMeshAgent>(); // Get the NavMeshAgent component
        agent.updateRotation = false; // Disable rotation update
        agent.updateUpAxis = false; // Enable position update
    }

    // Update is called once per frame
    void Update()
    {
        agent.SetDestination(target.position); // Set the destination to the target's position
    }
}
