using UnityEngine;

[CreateAssetMenu(menuName = "Profile/Shop Item", fileName = "New Shop Item")]
public class ShopItemProfile : SaveableEquipment
{
    [Header("Purchase Settings")]

    [Tooltip("Item purchase type")]
    public PurchaseType purchaseType = PurchaseType.Upgrade;
    [Tooltip("Cost to purchase item")]
    public int ItemCost = 0;
    [Tooltip("Extra cost per item already purchased")]
    public int ExtraCost = 0;

    [Header("Item Settings")]

    [Tooltip("Display name of item")]
    public string ItemName = "Default Name";
    [Tooltip("Preview object of item")]
    [SerializeField] private GameObject itemPrefab = null;
    [Tooltip("Display description of item")]
    [Multiline(3)]
    public string ItemDescription = "Default Description";
    public GameObject ItemPrefab
    {
        get
        {
            if(itemPrefab == null)
            {
                return Resources.Load("Prefabs/Default Purchase Item", typeof(GameObject)) as GameObject;
            }
            return itemPrefab;
        }
    }
}
