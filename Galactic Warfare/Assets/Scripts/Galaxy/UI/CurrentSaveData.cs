using UnityEngine;
using DataTypes;
public class CurrentSaveData : MonoBehaviour
{
	public SaveData SaveData;

	public void StartNewGame()
	{
		StartNewGame("New Game");
	}

	public void StartNewGame(string saveGameName)
	{
		if (SaveSystem.FileExist(saveGameName))
		{
			int index = 1;
			while (SaveSystem.FileExist($"{saveGameName} {index}"))
			{
				index++;
			}
			saveGameName = $"New Game {index}";
		}
		SaveData = new SaveData(saveGameName);
		DontDestroyOnLoad(gameObject);
	}

	public void LoadData(SaveData saveData)
	{
		SaveData = new SaveData(saveData);
		DontDestroyOnLoad(gameObject);
	}

	public SaveData LoadGame()
	{
		Destroy(gameObject);
		return new SaveData(SaveData);
	}
}
