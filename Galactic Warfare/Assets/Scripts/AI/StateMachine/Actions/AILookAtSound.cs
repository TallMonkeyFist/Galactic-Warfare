using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "AI/Actions/Look At Sound")]
public class AILookAtSound : AIAction
{
    public override void Act(AIStateController controller)
    {
        rotateBodyToSound(controller);
        rotateWeaponToSound(controller);
    }

    private void rotateBodyToSound(AIStateController controller)
    {
        if (controller.lastAudioPosition.Equals(Vector3.positiveInfinity))
        {
            return;
        }

        controller.navMeshAgent.updateRotation = false;

        Vector3 lookDirection = controller.lastAudioPosition - controller.navMeshAgent.transform.position;
        lookDirection.y = 0;

        lookDirection.x += controller.accuracy.x;

        if (lookDirection.x > 1) { lookDirection.x = lookDirection.x - 1; }
        if (lookDirection.x < -1) { lookDirection.x = lookDirection.x + 1; }

        lookDirection.z += controller.accuracy.y;

        if (lookDirection.z > 1) { lookDirection.z = lookDirection.z - 1; }
        if (lookDirection.z < -1) { lookDirection.z = lookDirection.z + 1; }

        Quaternion rot = Quaternion.LookRotation(lookDirection);

        controller.navMeshAgent.transform.rotation = Quaternion.Lerp(controller.transform.rotation, rot, controller.enemyStats.RotationSpeed * Time.fixedDeltaTime);
    }

    private void rotateWeaponToSound(AIStateController controller)
    {
        if (controller.lastAudioPosition.Equals(Vector3.positiveInfinity))
        {
            return;
        }

        Vector3 targetDirection = controller.lastAudioPosition - controller.inventory.WeaponSocket.position;

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
