using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AI/Actions/Follow Target")]
public class AIFollowTarget : AIAction
{
	public override void Act(AIStateController controller)
	{
		FollowTarget(controller);
	}

	private void FollowTarget(AIStateController controller)
	{
		Target currentTarget = controller.sight.currentTarget;

		if (currentTarget != null)
		{
			controller.navMeshAgent.SetDestination(controller.sight.currentTarget.GetTargetPosition());
		}
		else
		{
			Logger.LogWarning("AI Target was null, should switch to new action", DisplayLogInfo);
		}
	}
}
