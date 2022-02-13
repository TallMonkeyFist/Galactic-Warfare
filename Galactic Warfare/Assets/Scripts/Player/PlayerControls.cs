using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : NetworkBehaviour
{
	private PlayerMovement movement;
	private PlayerInventoryManager inventory;

	private void Awake()
	{
		movement = GetComponent<PlayerMovement>();
		inventory = GetComponent<PlayerInventoryManager>();
	}

	[TargetRpc]
	public void TargetDisableInput()
	{
		DisableInput();
	}

	[TargetRpc]
	public void TargetEnableInput()
	{
		EnableInput();
	}

	[ClientRpc]
	public void RpcDisableInput()
	{
		DisableInput();
	}

	[ClientRpc]
	public void RpcEnableInput()
	{
		EnableInput();
	}

	private void EnableInput()
	{
		movement.InputEnabled = true;
		inventory.InputEnabled = true;
	}

	private void DisableInput()
	{
		movement.InputEnabled = false;
		inventory.InputEnabled = false;
	}
}
