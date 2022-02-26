using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryManager : NetworkBehaviour
{
	[Header("References")]
	[Tooltip("Position for the player's weapons")]
	[SerializeField] private Transform weaponSocket = null;
	[Tooltip("Position for the player's items")]
	[SerializeField] private Transform itemSocket = null;
	[Tooltip("Transform for the player's head")]
	[SerializeField] private Transform headSocket = null;
	[Tooltip("Player camera reference for weapons")]
	[SerializeField] private Camera playerCamera = null;
	[Tooltip("Spawnable equipment for the player")]
	[SerializeField] private EquipmentProfiles equipmentProfile = null;
	[Tooltip("Collider to disable when shooting")]
	[SerializeField] private Collider playerHitCollider = null;

	[Header("Weapons")]
	[Tooltip("Primary Weapon")]
	[SerializeField] private WeaponManager primaryWeapon = null;
	[Tooltip("Secondary Weapon")]
	[SerializeField] private WeaponManager secondaryWeapon = null;

	[Header("Items")]
	[Tooltip("Primary Item")]
	[SerializeField] private ItemManager primaryItem = null;
	[Tooltip("Secondary Item")]
	[SerializeField] private ItemManager secondaryItem = null;

	[Header("Specialization")]
	[Tooltip("Specialization for the unit")]
	[SerializeField] private Specialization specialization = null;

	[SyncVar(hook =nameof(SyncPrimaryWeaponActive))]
	public bool primaryWeaponActive;
	[SyncVar(hook = nameof(SyncPrimaryItemActive))]
	public bool primaryItemActive;
	[SyncVar]
	[HideInInspector]
	public int team;

	public bool InputEnabled;

	private NetworkManager nm;
	private GameObject m_LastSpawn;
	private SpawnData equippedInventory;
	private bool prevInputState;

	private List<GameObject> m_SpawnedObjects;

	private WeaponSnapshot weaponOne;
	private WeaponSnapshot weaponTwo;
	private ItemSnapshot itemOne;
	private ItemSnapshot itemTwo;

	#region Structs

	[System.Serializable]
	private struct WeaponSnapshot
	{
		public readonly int currentAmmoCount;
		public readonly int reserveAmmoCount;

		public WeaponSnapshot(int current, int reserve)
		{
			currentAmmoCount = current;
			reserveAmmoCount = reserve;
		}
	}

	[System.Serializable]
	private struct ItemSnapshot
	{
		public readonly int currentItemCount;

		public ItemSnapshot(int current)
		{
			currentItemCount = current;
		}
	}

	#endregion

	#region Commands

	[Command]
	private void CmdWeaponUpdate(bool reload, bool fireDown, bool fireUp, PlayerLookData data)
	{
		WeaponManager weapon = primaryWeaponActive ? primaryWeapon : secondaryWeapon;
		if (reload)
		{
			if(weapon.TryReload())
			{
				TargetPlayReloadVfx();
			}
		}
		else if (fireDown)
		{
			if (weapon.TryFire())
			{
				if (nm == null)
				{
					nm = NetworkManager.singleton;
				}

				GameObject prefab = weapon.projectilePrefab;
				int projectileId = nm.spawnPrefabs.IndexOf(prefab);

				ServerSpawnProjectile(projectileId, weapon.ShootTransform.position, data.Forward, data.Up);
			}
		}
		else if (fireUp)
		{
			weapon.AlreadyShot = false;
		}
	}

	[Command]
	private void CmdItemUpdate(bool useKeyDown)
	{
		if (!useKeyDown) { return; }

		ItemManager item = primaryItemActive ? primaryItem : secondaryItem;

		if (nm == null)
		{
			nm = NetworkManager.singleton;
		}

		bool itemActive = m_LastSpawn == null ? false : true;
		bool destroyOnDie = item.DestroyOnDie;
		int condition = item.TrySpawn(itemActive);

		GameObject prefab = item.itemPrefab;
		int prefabId = nm.spawnPrefabs.IndexOf(prefab);

		switch (condition)
		{
			case -2:
				ServerDestroyLastItem(m_LastSpawn);
				m_LastSpawn = null;
				break;
			case -1:
				break;
			case 0:
				ServerSpawnItem(prefabId, item.SpawnLocation, item.SpawnRotation.eulerAngles, false, destroyOnDie);
				break;
			default:
				ServerSpawnItem(prefabId, item.SpawnLocation, item.SpawnRotation.eulerAngles, true, destroyOnDie);
				break;
		}
	}

	[Command]
	private void CmdSwapWeapons()
	{
		primaryWeaponActive = !primaryWeaponActive;
		TargetPlaySwapWeaponsVfx();
	}

	[Command]
	private void CmdSwapItems()
	{
		primaryItemActive = !primaryItemActive;
	}

	[Command]
	private void CmdDestroyLastItem(GameObject lastSpawned)
	{
		if (lastSpawned == null) { return; }
		
		NetworkServer.Destroy(lastSpawned);
	}

	[Command]
	private void CmdSetWeapons(int primaryIndex, int secondaryIndex)
	{
		if (primaryIndex < 0 || primaryIndex >= equipmentProfile.primaryWeapons.Length) { return; }
		if (secondaryIndex < 0 || secondaryIndex >= equipmentProfile.secondaryWeapons.Length) { return; }

		primaryWeapon = equipmentProfile.primaryWeapons[primaryIndex];
		secondaryWeapon = equipmentProfile.secondaryWeapons[secondaryIndex];

		TargetPlaySwapWeaponsVfx();
	}

	[Command]
	private void CmdSetItems(int primaryIndex, int secondaryIndex)
	{
		if (primaryIndex < 0 || primaryIndex >= equipmentProfile.primaryWeapons.Length) { return; }
		if (secondaryIndex < 0 || secondaryIndex >= equipmentProfile.secondaryWeapons.Length) { return; }

		primaryItem = equipmentProfile.primaryItems[primaryIndex];
		secondaryItem = equipmentProfile.secondaryItems[secondaryIndex];
	}

	#endregion

	#region Server

	public override void OnStartServer()
	{
		nm = NetworkManager.singleton;

		Health playerHealth = GetComponent<Health>();

		playerHealth.ServerOnDie += DestroyLastItem;
		playerHealth.ServerOnDie += DestroyAllItems;
		m_SpawnedObjects = new List<GameObject>();
		primaryWeaponActive = true;
		primaryItemActive = true;
	}

	[Server]
	public void SetEquipment(SpawnData data)
	{

		if (data.primaryWeapon < 0 || data.primaryWeapon >= equipmentProfile.primaryWeapons.Length) { return; }
		if (data.secondaryWeapon < 0 || data.secondaryWeapon >= equipmentProfile.secondaryWeapons.Length) { return; }
		if (data.primaryItem < 0 || data.primaryItem >= equipmentProfile.primaryWeapons.Length) { return; }
		if (data.secondaryItem < 0 || data.secondaryItem >= equipmentProfile.secondaryWeapons.Length) { return; }

		RpcDestroyEquipment();
		RpcSpawnEquipment(data);

		primaryWeapon = equipmentProfile.primaryWeapons[data.primaryWeapon];
		secondaryWeapon = equipmentProfile.secondaryWeapons[data.secondaryWeapon];
		primaryItem = equipmentProfile.primaryItems[data.primaryItem];
		secondaryItem = equipmentProfile.secondaryItems[data.secondaryItem];
		primaryItem.SetTransform(itemSocket, weaponSocket);
		secondaryItem.SetTransform(itemSocket, weaponSocket);

		equippedInventory = data;

		RpcSyncEquipped(primaryWeaponActive, primaryItemActive);
	}

	[Server]
	private void ServerSpawnProjectile(int objectId, Vector3 spawnPosition, Vector3 forward, Vector3 up)
	{
		GameObject projectileInstance = Instantiate(nm.spawnPrefabs[objectId], spawnPosition, Quaternion.LookRotation(forward, up));
		if(projectileInstance.TryGetComponent(out Projectile proj))
		{
			proj.team = team;
			proj.spawnedFromPlayer = netId;
		}

		NetworkServer.Spawn(projectileInstance);
		RpcPlayProjectileAudio();
		AISoundManager.RegisterSoundAtLocation(spawnPosition);
	}

	[Server]
	private void ServerSpawnItem(int objectId, Vector3 spawnPosition, Vector3 spawnRotation, bool setLast, bool destroyOnDie)
	{
		GameObject itemInstance = Instantiate(nm.spawnPrefabs[objectId], spawnPosition, Quaternion.Euler(spawnRotation));
		
		if(itemInstance.TryGetComponent(out ExplosiveItem item))
		{
			item.team = team;
			item.spawnedFromPlayer = netId;
		}

		NetworkServer.Spawn(itemInstance, (NetworkConnection)null);
		if (setLast)
		{
			TargetSetLastSpawnedItem(itemInstance);
			m_LastSpawn = itemInstance;
		}
		if (destroyOnDie)
		{
			m_SpawnedObjects.Add(itemInstance);
		}
	}

	[Server]
	private void ServerDestroyLastItem(GameObject lastSpawned)
	{
		if (lastSpawned != null)
			NetworkServer.Destroy(lastSpawned);
	}

	[Server]
	private void DestroyLastItem()
	{
		if (m_LastSpawn != null)
			NetworkServer.Destroy(m_LastSpawn);
	}

	[Server]
	private void DestroyAllItems()
	{
		foreach(GameObject go in m_SpawnedObjects)
		{
			NetworkServer.Destroy(go);
		}
	}

	[Server]
	private void ServerUpdatePlayerUI()
	{
		bool changed = false;

		if(weaponOne.currentAmmoCount != primaryWeapon.GetCurrentAmmo() || primaryWeapon.GetReserveAmmo() != weaponOne.reserveAmmoCount)
		{
			changed = true;
			weaponOne = new WeaponSnapshot(primaryWeapon.GetCurrentAmmo(), primaryWeapon.GetReserveAmmo());
		}
		if (weaponTwo.currentAmmoCount != secondaryWeapon.GetCurrentAmmo() || secondaryWeapon.GetReserveAmmo() != weaponTwo.reserveAmmoCount)
		{
			changed = true;
			weaponTwo = new WeaponSnapshot(secondaryWeapon.GetCurrentAmmo(), secondaryWeapon.GetReserveAmmo());
		}
		if (itemOne.currentItemCount != primaryItem.GetCurrentCount())
		{
			changed = true;
			itemOne = new ItemSnapshot(primaryItem.GetCurrentCount());
		}
		if (itemTwo.currentItemCount != secondaryItem.GetCurrentCount())
		{
			changed = true;
			itemTwo = new ItemSnapshot(secondaryItem.GetCurrentCount());
		}
		if(changed)
		{
			TargetUpdatePlayerUI(weaponOne, weaponTwo, itemOne, itemTwo);
		}
	}

	#endregion

	#region Client

	public WeaponManager GetCurrentWeapon()
	{
		if (primaryWeaponActive) { return primaryWeapon; }
		return secondaryWeapon;
	}

	public ItemManager GetCurrenItem()
	{
		if (primaryItemActive) { return primaryItem; }
		return secondaryItem;
	}

	public override void OnStartClient()
	{
		SpawnEquipment();

		if (!hasAuthority) { return; }

		primaryWeaponActive = true;
		primaryItemActive = true;
		m_LastSpawn = null;
		playerCamera = Camera.main;
		InputEnabled = true;
	}

	private void SpawnEquipment()
	{
		SpawnData data = equippedInventory;

		WeaponManager primary = Instantiate(equipmentProfile.primaryWeapons[data.primaryWeapon], weaponSocket);
		WeaponManager secondary = Instantiate(equipmentProfile.secondaryWeapons[data.secondaryWeapon], weaponSocket);
		ItemManager itemP = Instantiate(equipmentProfile.primaryItems[data.primaryItem], itemSocket);
		ItemManager itemS = Instantiate(equipmentProfile.secondaryItems[data.secondaryItem], itemSocket);

		primaryWeapon = primary;
		secondaryWeapon = secondary;
		primaryItem = itemP;
		secondaryItem = itemS;

		if(!hasAuthority) { return; }

		primary.SetHead(headSocket);
		secondary.SetHead(headSocket);

		itemP.SetTransform(itemSocket, weaponSocket);
		itemS.SetTransform(itemSocket, weaponSocket);
	}

	[ClientRpc]
	private void RpcDestroyEquipment()
	{
		if(primaryWeapon.gameObject != null)
		{
			Destroy(primaryWeapon.gameObject);
		}
		if (secondaryWeapon.gameObject != null)
		{
			Destroy(secondaryWeapon.gameObject);
		}
		if (primaryItem.gameObject != null)
		{
			Destroy(primaryItem.gameObject);
		}
		if (secondaryItem.gameObject != null)
		{
			Destroy(secondaryItem.gameObject);
		}
	}

	[ClientRpc]
	private void RpcSpawnEquipment(SpawnData data)
	{
		equippedInventory = data;

		WeaponManager primary = Instantiate(equipmentProfile.primaryWeapons[data.primaryWeapon], weaponSocket);
		WeaponManager secondary = Instantiate(equipmentProfile.secondaryWeapons[data.secondaryWeapon], weaponSocket);
		ItemManager itemP = Instantiate(equipmentProfile.primaryItems[data.primaryItem], itemSocket);
		ItemManager itemS = Instantiate(equipmentProfile.secondaryItems[data.secondaryItem], itemSocket);

		primaryWeapon = primary;
		secondaryWeapon = secondary;
		primaryItem = itemP;
		secondaryItem = itemS;

		if (!hasAuthority) { return; }

		primary.SetHead(headSocket);
		secondary.SetHead(headSocket);

		itemP.SetTransform(itemSocket, weaponSocket);
		itemS.SetTransform(itemSocket, weaponSocket);
	}

	[ClientRpc]
	private void RpcPlayProjectileAudio()
	{
		WeaponManager weapon = GetCurrentWeapon();
		weapon.AudioSource.PlayOneShot(weapon.FireSound);
	}

	[Client]
	private void ClientUpdate()
	{
		if (!hasAuthority || !InputEnabled)
		{
			return;
		}

		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			CmdSwapWeapons();
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			CmdSwapItems();
		}

		CmdWeaponUpdate(Input.GetKeyDown(KeyCode.R), Input.GetKey(KeyCode.Mouse0), Input.GetKeyUp(KeyCode.Mouse0), GetCurrentWeapon().GetProjectileDirection(playerHitCollider));
		CmdItemUpdate(Input.GetKeyDown(KeyCode.E));
	}

	[TargetRpc]
	private void TargetSyncEquipped(NetworkConnection conn, bool weapon, bool item)
	{
		SyncPrimaryItemActive(!item, item);
		SyncPrimaryWeaponActive(!weapon, weapon);
	}

	[ClientRpc]
	private void RpcSyncEquipped(bool weapon, bool item)
	{
		SyncPrimaryItemActive(!item, item);
		SyncPrimaryWeaponActive(!weapon, weapon);
	}

	[TargetRpc]
	private void TargetSyncEquipment(NetworkConnection conn, SpawnData data)
	{
		equippedInventory = data;
	}

	private void SyncPrimaryWeaponActive(bool oldValue, bool newValue)
	{
		if (newValue)
		{
			secondaryWeapon.gameObject.SetActive(false);
			primaryWeapon.gameObject.SetActive(true);
		}
		else
		{
			secondaryWeapon.gameObject.SetActive(true);
			primaryWeapon.gameObject.SetActive(false);
		}
	}

	private void SyncPrimaryItemActive(bool oldValue, bool newValue)
	{
		if (newValue)
		{
			secondaryItem.gameObject.SetActive(false);
			primaryItem.gameObject.SetActive(true);
		}
		else
		{
			secondaryItem.gameObject.SetActive(true);
			primaryItem.gameObject.SetActive(false);
		}
	}

	[TargetRpc]
	private void TargetSetLastSpawnedItem(GameObject lastSpawn)
	{
		m_LastSpawn = lastSpawn;
	}

	[TargetRpc]
	public void TargetDisableInput()
	{
		prevInputState = InputEnabled;
		InputEnabled = false;
	}

	[TargetRpc]
	public void TargetEnableInput()
	{
		InputEnabled = prevInputState;
	}

	[TargetRpc]
	private void TargetUpdatePlayerUI(WeaponSnapshot _primaryWeapon, WeaponSnapshot _secondaryWeapon, ItemSnapshot _primaryItem, ItemSnapshot _secondaryItem)
	{
		if(isServer) { return; }

		weaponOne = _primaryWeapon;
		weaponTwo = _secondaryWeapon;
		itemOne = _primaryItem;
		itemTwo = _secondaryItem;

		primaryWeapon.m_MagazineAmmo = weaponOne.currentAmmoCount;
		primaryWeapon.m_ReserveAmmo = weaponOne.reserveAmmoCount;

		secondaryWeapon.m_MagazineAmmo = weaponTwo.currentAmmoCount;
		secondaryWeapon.m_ReserveAmmo = weaponTwo.reserveAmmoCount;

		primaryItem.m_ItemCount = itemOne.currentItemCount;

		secondaryItem.m_ItemCount = itemTwo.currentItemCount;
	}

	[TargetRpc]
	private void TargetPlayReloadVfx()
	{
		WeaponManager weapon = GetCurrentWeapon();
		weapon.AudioSource.PlayOneShot(weapon.ReloadSound);
	}

	[TargetRpc]
	private void TargetPlaySwapWeaponsVfx()
	{
		WeaponManager weapon = GetCurrentWeapon();
		weapon.AudioSource.PlayOneShot(weapon.DrawWeaponsSound);
	}

	#endregion


	private void FixedUpdate()
	{
		if(isServer)
		{
			ServerUpdatePlayerUI();
		}
	}

	private void Update()
	{
		if(isClient)
		{
			ClientUpdate();
		}
	}
}
