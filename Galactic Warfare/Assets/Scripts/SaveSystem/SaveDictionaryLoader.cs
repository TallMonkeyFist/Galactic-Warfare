using System;
using System.Linq;
using UnityEngine;

public class SaveDictionaryLoader : MonoBehaviour
{
	public static SaveDictionaryLoader Instance { get; private set; } = null;
	public static event Action<SaveDictionaryLoader> LoadComplete;
	public string EquipmentShopProfilePath = "Equipment";
	public string ShipShopProfilePath = "Ships/Shop Profile";
	public string ShipProfilePath = "Ships/Profile";
	public string PlanetProfilePath = "Planets";

	public bool OverrideDictionaries = false;
	public static bool DisplayLog = false;

	private void Awake()
	{
		if (Instance != null)
		{
			Destroy(this);
			return;
		}
		LoadComplete?.Invoke(this);
		LoadDictionaryEntries();
		LoadDictionary();
	}

	private void LoadDictionaryEntries()
	{
		SaveableEquipment[] equipment = Resources.LoadAll(EquipmentShopProfilePath, typeof(SaveableEquipment)).Cast<SaveableEquipment>().ToArray();
		PlanetProfile[] planets = Resources.LoadAll(PlanetProfilePath, typeof(PlanetProfile)).Cast<PlanetProfile>().ToArray();
		ShipProfile[] ships = Resources.LoadAll(ShipProfilePath, typeof(ShipProfile)).Cast<ShipProfile>().ToArray();

		Logger.Log("Adding Entries to Dictionary Buffer", DisplayLog);

		foreach (SaveableEquipment saveableEquipment in equipment)
		{
			SaveDictionary.AddEntryToBuffer(saveableEquipment, SaveDictionary.DictionaryType.Equipment, OverrideDictionaries);
		}
		foreach (PlanetProfile planet in planets)
		{
			SaveDictionary.AddEntryToBuffer(planet, SaveDictionary.DictionaryType.Planet, OverrideDictionaries);
		}
		foreach (ShipProfile ship in ships)
		{
			SaveDictionary.AddEntryToBuffer(ship, SaveDictionary.DictionaryType.Ship, OverrideDictionaries);
		}
	}

	private void LoadDictionary()
	{
		Logger.Log("Loading Dictionaries", DisplayLog);
		SaveDictionary.CreateDictionaryFromBuffer(SaveDictionary.DictionaryType.Equipment, OverrideDictionaries);
		SaveDictionary.CreateDictionaryFromBuffer(SaveDictionary.DictionaryType.Planet, OverrideDictionaries);
		SaveDictionary.CreateDictionaryFromBuffer(SaveDictionary.DictionaryType.Ship, OverrideDictionaries);
	}
}
