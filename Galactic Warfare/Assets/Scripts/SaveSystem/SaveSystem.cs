using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using DataTypes;

public static class SaveSystem
{
	private const string SaveFolder = "Saves";

	public static void SaveData(SaveData saveData)
	{
		string fileLocation = SaveNameToFileLocation(saveData.SaveFileName);
		CheckPath(SaveFolder);

		FileStream fs = new FileStream(fileLocation, FileMode.Create);
		BinaryFormatter formatter = new BinaryFormatter();

		formatter.Serialize(fs, saveData);
		fs.Close();
	}

	public static string[] GetAllSaveData()
	{
		CheckPath(SaveFolder);
		string saveFolderLocation = $"{Application.persistentDataPath}\\{SaveFolder}";
		string[] files = Directory.GetFiles(saveFolderLocation, "*.sav");
		for (int i = 0; i < files.Length; i++)
		{
			int lastIndex = files[i].LastIndexOf('\\');
			files[i] = files[i].Substring(lastIndex + 1);
			files[i] = files[i].Substring(0, files[i].Length - 4);
		}
		return files;
	}

	public static bool FileExist(string saveFileName)
	{
		string fileLocation = SaveNameToFileLocation(saveFileName);
		return File.Exists(fileLocation);
	}

	public static bool DeleteFile(string saveFileName)
	{
		if (!FileExist(saveFileName)) { return false; }
		string fileLocation = SaveNameToFileLocation(saveFileName);
		File.Delete(fileLocation);
		return true;
	}

	public static string SaveNameToFileLocation(string saveFileName)
	{
		return $"{Application.persistentDataPath}\\{SaveFolder}\\{saveFileName}.sav";
	}

	public static bool TryGetSaveData(string saveFileName, out SaveData saveData)
	{
		string fileLocation = SaveNameToFileLocation(saveFileName);
		saveData = new SaveData();
		if (File.Exists(fileLocation))
		{
			FileStream dataStream = new FileStream(fileLocation, FileMode.Open);
			BinaryFormatter converter = new BinaryFormatter();

			saveData = (SaveData)converter.Deserialize(dataStream);
			dataStream.Close();

			return true;
		}
		return false;
	}

	public static void CheckSaveData(SaveData data)
	{
		if (data.TeamOne.Equipment != null)
		{
			foreach (string s in data.TeamOne.Equipment)
			{
				Debug.Log(s);
			}
		}
	}

	public static string FormateDateTime(DateTime dateTime)
	{
		return dateTime.ToString("yyyy-MM-dd_HH-mm-ss");
	}

	private static void CheckPath(string subfolder)
	{
		string path = $"{Application.persistentDataPath}/{subfolder}";
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
	}
}