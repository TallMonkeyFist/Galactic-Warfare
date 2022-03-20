using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadoutUIManager : MonoBehaviour
{
    public GameObject previewPanel = null;
    public GameObject modifyPanel = null;
    public GameObject buttonPrefab = null;
    public GameObject scrollPanel = null;

    public TeamManager teamManager = null;

    private void Start()
    {
        teamManager = TeamManager.Instance;
    }

    public void OpenModifyPanel()
    {
        previewPanel.SetActive(false);
        modifyPanel.SetActive(true);
    }

    public void CloseModifyPanel()
    {
        previewPanel.SetActive(true);
        modifyPanel.SetActive(false);
    }

    public void SaveLoadout()
    {
        Debug.LogWarning("TODO: Save loadout");
    }

    public void OpenPrimaryWeapons()
    {
        Team activeTeam = teamManager.ActiveTeam;
        TeamEquipment equipment = activeTeam.Equipment;

        for(int i = scrollPanel.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(scrollPanel.transform.GetChild(i).gameObject);
        }

        for(int i = 0; i < equipment.currentEquipment.Count; i++)
        {
            ShopItemProfile item = equipment.currentEquipment[i];
            GameObject buttonInstance = Instantiate(buttonPrefab, scrollPanel.transform);
            Button button = buttonInstance.GetComponent<Button>();
            TMP_Text text = buttonInstance.GetComponentInChildren<TMP_Text>();
            text.text = $"{item.ItemName}";
        }
    }

    public void OpenSecondaryWeapons()
    {
        Team activeTeam = teamManager.ActiveTeam;
        TeamEquipment equipment = activeTeam.Equipment;

        for (int i = scrollPanel.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(scrollPanel.transform.GetChild(i).gameObject);
        }
    }

    public void OpenPrimaryIems()
    {
        Team activeTeam = teamManager.ActiveTeam;
        TeamEquipment equipment = activeTeam.Equipment;

        for (int i = scrollPanel.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(scrollPanel.transform.GetChild(i).gameObject);
        }
    }

    public void OpenSecondaryIems()
    {
        Team activeTeam = teamManager.ActiveTeam;
        TeamEquipment equipment = activeTeam.Equipment;

        for (int i = scrollPanel.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(scrollPanel.transform.GetChild(i).gameObject);
        }
    }


}
