using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AI/Actions/Look At Target")]
public class AILookAtAction : AIAction
{
    public override void Act(AIStateController controller)
    {
        lookAtTarget(controller);
    }

    private void lookAtTarget(AIStateController controller)
    {
        if(controller.sight.currentTarget == null) { return; }

        controller.navMeshAgent.updateRotation = false;

        Vector3 lookDirection = controller.sight.currentTarget.GetTargetPosition() - controller.navMeshAgent.transform.position;
        lookDirection.y = 0;

        lookDirection.x += controller.accuracy.x;

        if (lookDirection.x > 1) { lookDirection.x = lookDirection.x - 1; } 
        if(lookDirection.x < -1) { lookDirection.x = lookDirection.x + 1; }

        lookDirection.z += controller.accuracy.y;

        if(lookDirection.z > 1) { lookDirection.z = lookDirection.z - 1; } 
        if(lookDirection.z < -1) { lookDirection.z = lookDirection.z + 1; }

        Quaternion rot = Quaternion.LookRotation(lookDirection);

        controller.navMeshAgent.transform.rotation = Quaternion.Lerp(controller.transform.rotation, rot, controller.enemyStats.RotationSpeed * Time.fixedDeltaTime);
    }
}
