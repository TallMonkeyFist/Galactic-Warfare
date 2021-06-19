using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "AI/Actions/CaptureFlagAction")]
public class AICaptureFlagAction : AIAction
{
    public override void Act(AIStateController controller)
    {
        MoveToFlag(controller);
    }

    private void MoveToFlag(AIStateController controller)
    {
        if(controller.flagSet) { return; }

        if(controller.flagController.TryGetAssignedFlag(out Flag flag))
        {
            if (NavMesh.SamplePosition(flag.GetRandomPosition(), out NavMeshHit hit, controller.enemyStats.MaxTravelDistance, NavMesh.AllAreas))
            {
                controller.navMeshAgent.SetDestination(hit.position);
                controller.flagSet = true;
            }
        }
    }
}
