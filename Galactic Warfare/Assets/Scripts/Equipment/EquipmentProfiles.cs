using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Equipment Profile")]
public class EquipmentProfiles : ScriptableObject
{
    [Tooltip("Primary weapons that the player can equip")]
    public WeaponManager[] primaryWeapons;
    [Tooltip("Secondary weapons that the player can equip")]
    public WeaponManager[] secondaryWeapons;
    [Tooltip("Primary items that the player can equip")]
    public ItemManager[] primaryItems;
    [Tooltip("Secondary items that the player can equip")]
    public ItemManager[] secondaryItems;
}
