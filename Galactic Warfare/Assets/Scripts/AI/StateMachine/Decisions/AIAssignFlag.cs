using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AI/Decisions/Assign New Flag")]
public class AIAssignFlag : AIDecision
{
	public override bool Decide(AIStateController controller)
	{
		return shouldAssignNewFlag(controller);
	}

	private bool shouldAssignNewFlag(AIStateController controller)
	{
		if(isFlagCaptured(controller))
		{
			controller.flagController.AssignRandomFlag(controller.team);

			if(controller.flagController.TryGetAssignedPatrol(out PatrolPattern route))
			{
				controller.waypointList = route.patrolPoints;
				controller.nextWaypoint = Random.Range(0, route.patrolPoints.Count);
			}

			return true;
		}
		return false;
	}

	private bool isFlagCaptured(AIStateController controller)
	{
		switch (controller.team)
		{
			case 1:
				return teamOneControlFlag(controller);

			case 2:
				return teamTwoControlFlag(controller);

			default:
				return false;
		}
	}

	private bool teamOneControlFlag(AIStateController controller)
	{
		if (controller.flagController.TryGetAssignedFlag(out Flag flag))
		{
			if (100 <= flag.FlagValue)
			{
				return true;
			}
			return false;
		}

		return true;
	}

	private bool teamTwoControlFlag(AIStateController controller)
	{
		if (controller.flagController.TryGetAssignedFlag(out Flag flag))
		{
			if (flag.FlagValue <= -100)
			{
				return true;
			}
			return false;
		}

		return true;
	}
}
