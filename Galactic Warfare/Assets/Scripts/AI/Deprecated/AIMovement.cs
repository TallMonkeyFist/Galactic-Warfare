using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIMovement : NetworkBehaviour
{
    [Header("References")]
    [Tooltip("Nav mesh agent for the ai")]
    [SerializeField] private NavMeshAgent agent = null;

    [Server]
    public void SetDestination(Vector3 _destination)
    {
        if(NavMesh.SamplePosition(_destination, out NavMeshHit hit, 1000.0f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
}
