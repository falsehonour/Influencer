using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavAgent : MonoBehaviour
{
    [SerializeField] private Transform destination;
    private NavMeshAgent navMeshAgent;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent != null)
        {
            SetDestination();
        }
    }

    private void SetDestination()
    {
        if (destination != null)
        {
            Vector3 destinationPosition = destination.position;
            navMeshAgent.SetDestination(destinationPosition);
        }

    }
}
