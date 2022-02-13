using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
	[SerializeField] private PlayerMovement player = null;
	[SerializeField] private Camera playerCamera = null;

	private void Start()
	{
		playerCamera = Camera.main;
	}

	public void LateUpdate()
	{
		playerCamera.transform.position = player.LookTransform.position;
		playerCamera.transform.localRotation = Quaternion.Euler(player.xAxis, player.transform.rotation.eulerAngles.y, player.transform.rotation.eulerAngles.z);
	}
}
