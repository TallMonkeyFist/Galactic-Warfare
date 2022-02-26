using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Mirror;

[System.Serializable]
public struct SpawnData
{
	public int primaryWeapon;
	public int secondaryWeapon;
	public int primaryItem;
	public int secondaryItem;
}

public class PlayerSpawnManager : MonoBehaviour
{
	[Header("Weapon UI References")]
	[Tooltip("Primary weapon dropdown")]
	[SerializeField] private TMP_Dropdown primaryWeapons = null;
	[Tooltip("Secondary weapon dropdown")]
	[SerializeField] private TMP_Dropdown secondaryWeapons = null;
	[Tooltip("Primary item dropdown")]
	[SerializeField] private TMP_Dropdown primaryItems = null;
	[Tooltip("Secondary item dropdown")]
	[SerializeField] private TMP_Dropdown secondaryItems = null;
	[Tooltip("Equipable player equipment")]
	[SerializeField] private EquipmentProfiles equipment = null;

	[Header("Spawn UI References")]
	[Tooltip("Dropdown to select spawn location")]
	[SerializeField] private TMP_Dropdown spawnLocationDropdown = null;

	private SpawnManager spawnManager = null;

	public void InitDropdowns()
	{
		InitWeaponDropdowns();
		InitSpawnLocationDropdown();
	}

	private void InitWeaponDropdowns()
	{
		primaryWeapons.ClearOptions();
		secondaryWeapons.ClearOptions();
		primaryItems.ClearOptions();
		secondaryItems.ClearOptions();

		List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

		foreach(WeaponManager go in equipment.primaryWeapons)
		{
			TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData(go.name);
			options.Add(data);
		}
		primaryWeapons.AddOptions(options);

		options.Clear();

		foreach (WeaponManager go in equipment.secondaryWeapons)
		{
			TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData(go.name);
			options.Add(data);
		}
		secondaryWeapons.AddOptions(options);

		options.Clear();

		foreach (ItemManager go in equipment.primaryItems)
		{
			TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData(go.name);
			options.Add(data);
		}
		primaryItems.AddOptions(options);

		options.Clear();

		foreach (ItemManager go in equipment.secondaryItems)
		{
			TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData(go.name);
			options.Add(data);
		}
		secondaryItems.AddOptions(options);

		options.Clear();
	}

	public void InitSpawnLocationDropdown()
	{
		spawnManager = ((FPSNetworkManager)NetworkManager.singleton).spawnManager;

		if(spawnManager == null)
		{
			SpawnManager.OnManagerInitialized += HandleSpawnManagerInitialized;
			return;
		}

		spawnLocationDropdown.ClearOptions();

		List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

		foreach (SpawnSystem spawnSystem in spawnManager.spawnSystems)
		{
			TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData(spawnSystem.SpawnLocationName);
			options.Add(data);
		}
		spawnLocationDropdown.AddOptions(options);
	}
	
	public SpawnData GetEquipment()
	{
		SpawnData data = new SpawnData();
		data.primaryWeapon = primaryWeapons.value;
		data.secondaryWeapon = secondaryWeapons.value;
		data.primaryItem = primaryItems.value;
		data.secondaryItem = secondaryItems.value;

		return data;
	}

	private void HandleSpawnManagerInitialized()
	{
		SpawnManager.OnManagerInitialized -= HandleSpawnManagerInitialized;

		spawnManager = ((FPSNetworkManager)NetworkManager.singleton).spawnManager;

		spawnLocationDropdown.ClearOptions();

		List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

		foreach (SpawnSystem spawnSystem in spawnManager.spawnSystems)
		{
			TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData(spawnSystem.SpawnLocationName);
			options.Add(data);
		}
		spawnLocationDropdown.AddOptions(options);
	}
}
