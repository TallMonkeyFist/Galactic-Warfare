using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "AI/Actions/Patrol Waypoints")]
public class AIPatrolAction : AIAction
{
	public override void Act(AIStateController controller)
	{
		patrol(controller);
	}

	private void patrol(AIStateController controller)
	{
		if(NavMesh.SamplePosition(controller.waypointList[controller.nextWaypoint].position, out NavMeshHit hit, controller.enemyStats.MaxTravelDistance, NavMesh.AllAreas))
		{
			controller.navMeshAgent.SetDestination(hit.position);
		}

		if(controller.navMeshAgent.remainingDistance <= controller.navMeshAgent.stoppingDistance && !controller.navMeshAgent.pathPending)
		{
			controller.nextWaypoint = UnityEngine.Random.Range(0, controller.waypointList.Count);
		}
	}
}
