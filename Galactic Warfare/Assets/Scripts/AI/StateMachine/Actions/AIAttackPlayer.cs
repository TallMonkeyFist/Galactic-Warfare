using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "AI/Actions/Attack Target")]
public class AIAttackPlayer : AIAction
{
	public override void Act(AIStateController controller)
	{
		attackPlayer(controller);
	}

	private void attackPlayer(AIStateController controller)
	{
		if(controller.sight.currentTarget == null)
		{
			Logger.LogWarning("Target is null, can't attack it", DisplayLogInfo);
			return;
		}

		Vector3 aiForward = controller.navMeshAgent.transform.forward;
		Vector3 fireDirection = controller.sight.currentTarget.GetTargetPosition() - controller.inventory.GetCurrentWeapon().ShootTransform.position;

		float angle = Vector3.Angle(aiForward, fireDirection);

		if(angle <= controller.enemyStats.FireFOV)
		{
			controller.inventory.FireWeaponInDirection(fireDirection);
		}

	}
}
