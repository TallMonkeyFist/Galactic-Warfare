using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Profile/Ship", fileName = "New Ship Loadout")]
public class ShipProfile : ScriptableObject
{
    [Header("Ship Settings")]
    [Tooltip("Ship Name")]
    public string ShipName = "DefaultShip";
    [Tooltip("Count of troop spawns per match")]
    public int TroopCount = 75;
    [Tooltip("Count of ship spawns per match")]
    public int FleetCount = 25;
    [Tooltip("Count of vehicle spawns per match")]
    public int VehicleCount = 25;
    
    [Header("Gameplay Settings")]
    [Tooltip("Does the ship help in ground battles")]
    public bool GroundAssist = true;
    [Tooltip("Does the ship help in space battles")]
    public bool SpaceAssist = true;
    [Tooltip("Does the ship force space battle")]
    public bool PreventsInvasion = false;

    [Tooltip("Model")]
    [SerializeField] private GameObject shipModel = null;

    public GameObject ShipModel
    {
        get
        {
            if (shipModel == null)
            {
                return Resources.Load("Prefabs/Default Ship", typeof(GameObject)) as GameObject;
            }
            return shipModel;
        }
    }

    public override bool Equals(object other)
    {
        if(other is null)
        {
            return false;
        }
        if(ReferenceEquals(this, other))
        {
            return true;
        }
        if(other.GetType() != this.GetType())
        {
            return false;
        }

        return (other as ShipProfile) == this;
    }

    public override int GetHashCode()
    {
        return (ShipName, TroopCount, FleetCount, VehicleCount, PreventsInvasion, SpaceAssist, GroundAssist).GetHashCode();
    }

    public static bool operator==(ShipProfile lhs, ShipProfile rhs)
    {
        if (lhs.ShipName == rhs.ShipName)
            return true;
        return false;
    }

    public static bool operator !=(ShipProfile lhs, ShipProfile rhs) => !(lhs == rhs);
}
