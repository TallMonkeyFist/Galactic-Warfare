using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "AI/Actions/Guard Action")]
public class AIGuardLocation : AIAction
{
    public override void Act(AIStateController controller)
    {
        guardLocation(controller);
    }

    private void guardLocation(AIStateController controller)
    {
        if (NavMesh.SamplePosition(controller.waypointList[controller.nextWaypoint].position, out NavMeshHit hit, controller.enemyStats.MaxTravelDistance, NavMesh.AllAreas))
        {
            controller.navMeshAgent.SetDestination(hit.position);
        }
    }
}
