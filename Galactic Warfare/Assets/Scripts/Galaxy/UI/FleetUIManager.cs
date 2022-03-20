using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FleetUIManager : Shop
{
    [Header("Fleet Manager References")]
    [SerializeField] private TMP_Text itemNameText = null;
    [SerializeField] private TMP_Text itemDescText = null;
    [SerializeField] private TMP_Text itemCostText = null;

    public override void PurchaseItem()
    {
        ShopItemProfile currentProfile = itemToBuy[index].itemProfile;
        if(currentProfile.ItemPrefab.TryGetComponent(out ShipShopModel model))
        {
            teamManager.AddInactiveShip(model.Profile);
        }
        else
        {
            Debug.Log("Can't load ship to team.");
        }
    }
    protected override void UpdateUI() 
    {
        ShopItemProfile currentItem = itemToBuy[index].itemProfile;
        SetName(currentItem.ItemName);
        SetDescription(currentItem.ItemDescription);
        SetCost($"{currentItem.ItemCost} Units");
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
