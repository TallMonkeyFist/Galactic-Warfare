using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Loadout/Loadout Profile")]
public class Loadout : ScriptableObject
{
    public WeaponManager PrimaryWeapon;
    public WeaponManager SecondaryWeapon;
}
