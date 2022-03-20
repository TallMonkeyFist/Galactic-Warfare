using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class ShopItemData
{
    public string profileLocation;
    public static string FolderLocation = "Equipment/";
    public ShopItemData(ShopItemProfile profile)
    {
        profileLocation = FolderLocation + profile.ItemName;
    }

    public ShopItemProfile GetAssset()
    {
        return Resources.Load(profileLocation) as ShopItemProfile;
    }

    public static bool operator==(ShopItemData lhs, ShopItemData rhs)
    {
        return lhs.profileLocation == rhs.profileLocation;
    }
    
    public static bool operator!=(ShopItemData lhs, ShopItemData rhs)
    {
        return lhs.profileLocation != rhs.profileLocation;
    }

    public override bool Equals(object obj) => this.Equals(obj as ShopItemData);

    public bool Equals(ShopItemData data)
    {
        if(data is null)
        {
            return false;
        }
        if(ReferenceEquals(this, data))
        {
            return true;
        }
        if(this.GetType() != data.GetType())
        {
            return false;
        }
        return profileLocation == data.profileLocation;
    }

    public override int GetHashCode()
    {
        return profileLocation.GetHashCode();
    }
}

public enum PurchaseType
{ Ship, Upgrade, Equipment }
