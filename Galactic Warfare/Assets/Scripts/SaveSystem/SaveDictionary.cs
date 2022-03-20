using System.Collections.Generic;

public static class SaveDictionary
{
	public static bool DisplayLog = true;
	public enum DictionaryType
	{
		Equipment,
		Planet,
		Ship
	}

	public static void AddEntryToBuffer<T>(T value, DictionaryType type, bool removeDuplicateEntry = true)
	{
		switch (type)
		{
			case DictionaryType.Equipment:
				AddEntryToDictionary(value as SaveableEquipment, (value as SaveableEquipment).DictionaryEntryName, EquipmentLookupBuffer, removeDuplicateEntry);
				break;
			case DictionaryType.Planet:
				AddEntryToDictionary(value as PlanetProfile, (value as PlanetProfile).PlanetName, PlanetLookupBuffer, removeDuplicateEntry);
				break;
			case DictionaryType.Ship:
				AddEntryToDictionary(value as ShipProfile, (value as ShipProfile).ShipName, ShipLookupBuffer, removeDuplicateEntry);
				break;
			default:
				Logger.LogWarning("Dictionary Not Implemented", DisplayLog);
				break;
		}
	}

	public static bool RemoveEntryFromBuffer(string key, DictionaryType type)
	{
		switch (type)
		{
			case DictionaryType.Equipment:
				return RemoveEntryFromDictionary(key, EquipmentLookupBuffer);
			case DictionaryType.Planet:
				return RemoveEntryFromDictionary(key, PlanetLookupBuffer);
			case DictionaryType.Ship:
				return RemoveEntryFromDictionary(key, ShipLookupBuffer);
			default:
				break;
		}
		return false;
	}

	public static void CreateDictionaryFromBuffer(DictionaryType type, bool overrideDictionary = false)
	{
		switch (type)
		{
			case DictionaryType.Equipment:
				if (CreateDictionaryFromBuffer(EquipmentLookupBuffer, ref EquipmentDictionaryLoaded, out Dictionary<string, SaveableEquipment> equipmentDictionary, overrideDictionary))
				{
					EquipmentLookupDictionary = equipmentDictionary;
				}
				break;
			case DictionaryType.Planet:
				if (CreateDictionaryFromBuffer(PlanetLookupBuffer, ref PlanetDictionaryLoaded, out Dictionary<string, PlanetProfile> planetDictionary, overrideDictionary))
				{
					PlanetLookupDictionary = planetDictionary;
				}
				break;
			case DictionaryType.Ship:
				if (CreateDictionaryFromBuffer(ShipLookupBuffer, ref ShipDictionaryLoaded, out Dictionary<string, ShipProfile> shipDictionary, overrideDictionary))
				{
					ShipLookupDictionary = shipDictionary;
				}
				break;
			default:
				Logger.LogWarning("No Matching Dictionary Type", DisplayLog);
				break;
		}
	}

	private static void AddEntryToDictionary<T>(T value, string key, Dictionary<string, T> dictionary, bool removeDuplicateEntry)
	{
		if (dictionary.ContainsKey(key))
		{
			if (removeDuplicateEntry)
			{
				Logger.LogWarning($"Overriding previous entry {key}", DisplayLog);
				dictionary.Remove(key);
				dictionary.Add(key, value);
			}
			else
			{
				Logger.LogWarning($"{key} already exist in Buffer", DisplayLog);
			}
		}
		else
		{
			dictionary.Add(key, value);
		}
	}

	private static bool RemoveEntryFromDictionary<T>(string key, Dictionary<string, T> dictionary)
	{
		return dictionary.Remove(key);
	}

	private static bool CreateDictionaryFromBuffer<T>(Dictionary<string, T> buffer, ref bool dictionaryLoaded, out Dictionary<string, T> dictionary, bool overrideDictionary = false)
	{
		dictionary = null;
		bool createdDictionary = false;
		if (!dictionaryLoaded || overrideDictionary)
		{
			dictionary = new Dictionary<string, T>(buffer);
			buffer.Clear();
			dictionaryLoaded = true;
			createdDictionary = true;
		}
		else
		{
			Logger.Log("Dictionary was already loaded", DisplayLog);
		}
		return createdDictionary;
	}

	#region Equipment

	private static Dictionary<string, SaveableEquipment> EquipmentLookupBuffer = new Dictionary<string, SaveableEquipment>();
	private static bool EquipmentDictionaryLoaded = false;
	public static Dictionary<string, SaveableEquipment> EquipmentLookupDictionary { private set; get; } = new Dictionary<string, SaveableEquipment>();

	#endregion

	#region Planet

	private static Dictionary<string, PlanetProfile> PlanetLookupBuffer = new Dictionary<string, PlanetProfile>();
	private static bool PlanetDictionaryLoaded = false;
	public static Dictionary<string, PlanetProfile> PlanetLookupDictionary { private set; get; } = new Dictionary<string, PlanetProfile>();

	#endregion

	#region Ship

	private static Dictionary<string, ShipProfile> ShipLookupBuffer = new Dictionary<string, ShipProfile>();
	private static bool ShipDictionaryLoaded = false;
	public static Dictionary<string, ShipProfile> ShipLookupDictionary { private set; get; } = new Dictionary<string, ShipProfile>();

	#endregion
}
