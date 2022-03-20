using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopItem : MonoBehaviour
{
    public ShopItemProfile itemProfile;

    private void Awake()
    {
        GameObject instance = Instantiate(itemProfile.ItemPrefab, transform);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
    }
}
    