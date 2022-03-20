using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShopUIManager : Shop
{
    [Header("Fleet Manager References")]
    [SerializeField] private TMP_Text itemNameText = null;
    [SerializeField] private TMP_Text itemDescText = null;
    [SerializeField] private TMP_Text itemCostText = null;

    //TODO
    //Add commander references to figure out if item is purchased

    public override void PurchaseItem()
    {
        ShopItemProfile currentProfile = itemToBuy[index].itemProfile;
        if (!teamManager.ActiveTeam.Equipment.currentEquipment.Contains(currentProfile))
        {
            teamManager.ActiveTeam.Equipment.currentEquipment.Add(currentProfile);
            UpdateUI();
        }
    }
    protected override void UpdateUI()
    {
        ShopItemProfile currentItem = itemToBuy[index].itemProfile;
        SetName(currentItem.ItemName);
        SetDescription(currentItem.ItemDescription);
        if (teamManager.ActiveTeam.Equipment.currentEquipment.Contains(currentItem))
        {
            SetCost("Purchased");
        }
        else
        {
            SetCost($"{currentItem.ItemCost} Units");
        }
    }

    private void SetName(string name)
    {
        itemNameText.text = name;
    }

    private void SetDescription(string desc)
    {
        itemDescText.text = desc;
    }

    private void SetCost(string cost)
    {
        itemCostText.text = cost;
    }
}
