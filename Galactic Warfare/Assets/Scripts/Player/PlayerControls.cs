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
        movement.InputEnabled = false;
        inventory.InputEnabled = false;
    }

    [TargetRpc]
    public void TargetEnableInput()
    {
        movement.InputEnabled = true;
        inventory.InputEnabled = true;
    }
}
