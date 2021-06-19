using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFireBehaviour : NetworkBehaviour
{
    [Header("References")]
    [Tooltip("Inventory of the enemy")]
    [SerializeField] private AIInventory inventory = null;
    [Tooltip("Sight of the enemy")]
    [SerializeField] private AISight sight = null;

    public override void OnStartServer()
    {
        base.OnStartServer();

        sight.gameObject.SetActive(true);
        inventory.GetCurrentWeapon();
    }
}
