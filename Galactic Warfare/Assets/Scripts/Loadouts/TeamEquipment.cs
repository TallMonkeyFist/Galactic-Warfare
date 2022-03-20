using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TeamEquipment
{
    public List<ShopItemProfile> currentEquipment;

    public TeamEquipment()
    {
        currentEquipment = new List<ShopItemProfile>();
    }

    public static string[] PrepareDataForSave(TeamEquipment currentEquipment)
    {
        List<string> equipmentToSave = new List<string>();
        foreach (ShopItemProfile item in currentEquipment.currentEquipment)
        {
            if (SaveDictionary.EquipmentLookupDictionary.ContainsKey(item.DictionaryEntryName))
            {
                equipmentToSave.Add(item.DictionaryEntryName);
            }
        }
        return equipmentToSave.ToArray();
    }

    public static TeamEquipment LoadDataFromSave(string[] equipment)
    {
        TeamEquipment teamEquipment = new TeamEquipment();
        if (equipment != null)
        {
            foreach (string s in equipment)
            {
                if (SaveDictionary.EquipmentLookupDictionary.ContainsKey(s))
                {
                    if (SaveDictionary.EquipmentLookupDictionary.TryGetValue(s, out SaveableEquipment item))
                    {
                        teamEquipment.currentEquipment.Add(item as ShopItemProfile);
                    }
                }
            }
        }
        return teamEquipment;
    }
}
