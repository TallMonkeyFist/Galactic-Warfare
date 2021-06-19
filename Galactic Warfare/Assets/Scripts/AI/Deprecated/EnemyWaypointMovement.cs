using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyWaypointMovement : NetworkBehaviour
{
    [Header("References")]
    [Tooltip("Nav mesh agent for the enemy")]
    [SerializeField] private NavMeshAgent agent = null;

    [Tooltip("Positions to move to")]
    [SerializeField] private Transform[] waypoints = null;

    private int currentPoint;
    private NavMeshHit NavHit;

    #region Server

    public override void OnStartServer()
    {
        currentPoint = -1;
        GetNextWaypoint();
        if (CheckPosition(waypoints[currentPoint].position))
        {
            agent.SetDestination(NavHit.position);
        }
    }

    [ServerCallback]
    private void Update()
    {
        if(agent.hasPath)
        {
            return;
        }
        GetNextWaypoint();
        if (CheckPosition(waypoints[currentPoint].position))
        {
            agent.SetDestination(NavHit.position);
        }
    }

    [Server]
    private void GetNextWaypoint()
    {
        currentPoint = (currentPoint + 1) % waypoints.Length;
    }

    [Server]
    private bool CheckPosition(Vector3 position)
    {
        if(NavMesh.SamplePosition(position, out NavHit, 1f, NavMesh.AllAreas))
        {
            return true;
        }
        return false;
    }
    #endregion

    #region Client



    #endregion
}
