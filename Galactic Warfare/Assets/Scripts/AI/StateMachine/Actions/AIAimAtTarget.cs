using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AI/Actions/Aim At Target")]
public class AIAimAtTarget : AIAction
{
    public override void Act(AIStateController controller)
    {
        aimAtTarget(controller);
    }

    private void aimAtTarget(AIStateController controller)
    {
        if (controller.sight.currentTarget == null)
        {
            Debug.LogWarning("Target is null, nothing to aim at");
            return;
        }

        Vector3 targetDirection = controller.sight.currentTarget.GetTargetPosition() - controller.inventory.WeaponSocket.position;

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);

        Vector3 targetEuler = targetRotation.eulerAngles;
        targetEuler.x += controller.fireAccuracy;

        targetRotation = Quaternion.Euler(targetEuler);

        Quaternion rotation = Quaternion.RotateTowards(controller.inventory.WeaponSocket.rotation, targetRotation, 90 * Time.fixedDeltaTime);

        controller.inventory.WeaponSocket.rotation = rotation;

        Vector3 euler = controller.inventory.WeaponSocket.localEulerAngles;

        euler.y = 0;
        euler.z = 0;

        controller.inventory.WeaponSocket.localEulerAngles = euler;
    }
}
