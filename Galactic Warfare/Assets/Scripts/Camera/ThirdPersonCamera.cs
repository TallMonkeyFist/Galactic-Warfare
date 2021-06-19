using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public PlayerMovement player = null;
    public Camera playerCamera = null;
    public LayerMask cameraCollisionMask;
    public float cameraDistance = 5.0f;
    public float rightOffset = 0.5f;

    private void LateUpdate()
    {
        Vector3 offsetPosition = player.LookTransform.position + rightOffset * player.LookTransform.right;

        if (Physics.Raycast(player.LookTransform.position, player.LookTransform.right, out RaycastHit rightHit, rightOffset + 0.5f, cameraCollisionMask))
        {
            float distance = Mathf.Clamp(rightHit.distance - 0.5f, 0, rightOffset);
            offsetPosition = player.LookTransform.position + (distance) * player.LookTransform.right;
        }

        Vector3 position = offsetPosition + (-player.LookTransform.forward * cameraDistance);

        if (Physics.Raycast(offsetPosition, -player.LookTransform.forward, out RaycastHit hit, cameraDistance + 0.5f, cameraCollisionMask))
        {
            position = offsetPosition + (-player.LookTransform.forward * (hit.distance - 0.5f));
        }

        playerCamera.transform.position = position;
        playerCamera.transform.localRotation = Quaternion.Euler(player.xAxis, 0.0f, 0.0f);
    }
}
