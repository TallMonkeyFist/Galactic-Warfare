using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Transform for the item to spawn")]
    [SerializeField] private Transform spawnTransform = null;
    [Tooltip("Rotation transform for the item")]
    [SerializeField] private Transform rotationTransform = null;

    [Header("UI")]
    [Tooltip("Name of the item")]
    [SerializeField] private string itemName = "Default item";
    [Tooltip("Weapon icon in the UI")]
    [SerializeField] private Image itemIcon = null;

    [Header("Item Settings")]
    [Tooltip("Item to spawn")]
    [SerializeField] public GameObject itemPrefab = null;
    [Tooltip("Max amount of items that can be carried")]
    [SerializeField] private int maxItemCount = 3;
    [Tooltip("Rate at which items can be spawned")]
    [SerializeField] private float spawnRate = 1.0f;
    [Tooltip("Can multiple instances of an item be spawned")]
    [SerializeField] private bool destroyPrevious = false;
    [Tooltip("Will the item be destroyed when the player dies")]
    [SerializeField] private bool destroyOnDie = false;

    public int m_ItemCount;
    private float m_LastSpawnTime;
    private float m_SpawnDelay;
    [HideInInspector]
    public Vector3 SpawnLocation { get { return spawnTransform.position; } }
    [HideInInspector]
    public Quaternion SpawnRotation { get { return rotationTransform.rotation; } }
    [HideInInspector]
    public bool DestroyOnDie { get { return destroyOnDie; } }

    private void Start()
    {
        m_ItemCount = maxItemCount;
        m_SpawnDelay = 1 / spawnRate;
    }

    public int TrySpawn(bool itemActive)
    {
        if(destroyPrevious && itemActive && Time.time > m_LastSpawnTime + m_SpawnDelay)
        {
            m_LastSpawnTime = Time.time;
            return -2;
        }
        else if(m_ItemCount > 0 && Time.time > m_LastSpawnTime + m_SpawnDelay)
        {
            return SpawnItem();
        }
        return -1;
    }

    private int SpawnItem()
    {
        m_ItemCount--;
        m_LastSpawnTime = Time.time;
        if(destroyPrevious)
        {
            return 1;
        }
        return 0;
    }

    public void AddItem(int itemAmount)
    {
        m_ItemCount = Mathf.Max(m_ItemCount + itemAmount, 0, maxItemCount);
    }

    public void SetTransform(Transform spawn, Transform rotation)
    {
        spawnTransform = spawn;
        rotationTransform = rotation;
    }

    public int GetCurrentCount()
    {
        return m_ItemCount;
    }

    public int GetMaxCount()
    {
        return maxItemCount;
    }

    public string GetName()
    {
        return itemName;
    }
}
