using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using TMPro;
using DataTypes;

public class SaveUILoader : MonoBehaviour
{
	public CurrentSaveData SaveData;
	public GameObject MenuItemPrefab;
	public Transform SaveItemParentTransform;
	public Button NewGameButton;
	public TMP_InputField NewSaveGameInput;

	public void Start()
	{
		SaveData = Instantiate(Resources.Load("UI/Prefabs/SaveDataLoader", typeof(GameObject)) as GameObject).GetComponent<CurrentSaveData>();
		MenuItemPrefab = Instantiate(Resources.Load("UI/Prefabs/SaveMenuItem", typeof(GameObject)) as GameObject);
		NewGameButton.onClick.AddListener(StartNewGame);
		DisplayAllSaves();
	}

	public void DisplayAllSaves()
	{
		string[] saveFileNames = SaveSystem.GetAllSaveData();
		foreach(string fileName in saveFileNames)
		{
			GameObject SaveMenuItemObject = Instantiate(MenuItemPrefab);
			SaveMenuItemObject.GetComponent<RectTransform>().SetParent(SaveItemParentTransform, false);
			SaveSystem.TryGetSaveData(fileName, out SaveData saveData);
			SaveMenuItemObject.GetComponent<SaveMenuItem>().CreateSaveItem(saveData, this);
		}
	}

	public void StartNewGame()
	{
		if(!NewSaveGameInput || NewSaveGameInput.text.Trim().Length == 0)
		{
			SaveData.StartNewGame();
		}
		else
		{
			SaveData.StartNewGame(NewSaveGameInput.text.Trim());
		}
		SceneManager.LoadScene("Galaxy Scene");
	}

	public void LoadSave(SaveData saveData)
	{
		SaveData.LoadData(saveData);
		SceneManager.LoadScene("Galaxy Scene");
	}

	public void DeleteSave(SaveData saveData)
	{
		SaveSystem.DeleteFile(saveData.SaveFileName);
	}
}
