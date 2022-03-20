using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DataTypes;

public class SaveMenuItem : MonoBehaviour
{
	SaveData SaveData;

	public TMP_Text SaveFileNameText = null;
	public TMP_Text SaveFileDateText = null;
	public TMP_Text SaveFileTimeText = null;

	public Button LoadSaveGameButton = null;
	public Button DeleteSaveGameButton = null;

	public void CreateSaveItem(SaveData saveData, SaveUILoader saveLoader)
	{
		SaveData = new SaveData(saveData);
		DisplaySave();
		LoadSaveGameButton.onClick.AddListener(() => { saveLoader.LoadSave(saveData); }) ;
		DeleteSaveGameButton.onClick.AddListener(() => { saveLoader.DeleteSave(saveData); Destroy(gameObject); });
	}

	public void DisplaySave()
	{
		string dateString = SaveData.SaveDate.ToString("MM/dd/yyyy");
		string timeString = SaveData.SaveDate.ToString("HH:mm:ss");
		SaveFileNameText.text = SaveData.SaveFileName;
		SaveFileDateText.text = dateString;
		SaveFileTimeText.text = timeString;
	}
}
