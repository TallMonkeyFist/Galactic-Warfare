using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyUIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject MovePanel = null;
    public GameObject ShopPanel = null;
    public GameObject ArmyPanel = null;
    public GameObject ShipPanel = null;
    
    [Header("Model References")]
    public GameObject GalaxyModel = null;
    public GameObject EquipmentModel = null;
    public GameObject LoadoutModel = null;
    public GameObject FleetModel = null;

    private GameObject[] panels = null;
    private GameObject[] models = null;

    private void Start()
    {
        panels = new GameObject[4] { MovePanel, ShopPanel, ArmyPanel, ShipPanel };
        models = new GameObject[4] { GalaxyModel, EquipmentModel, LoadoutModel, FleetModel };
    }

    public void OpenMoveUI()
    {
        foreach(GameObject go in panels)
        {
            go.SetActive(false);
        }
        panels[0].SetActive(true);

        foreach(GameObject go in models)
        {
            go.SetActive(false);
        }
        models[0].SetActive(true);
    }

    public void OpenShopUI()
    {
        foreach (GameObject go in panels)
        {
            go.SetActive(false);
        }
        panels[1].SetActive(true);

        foreach (GameObject go in models)
        {
            go.SetActive(false);
        }
        models[1].SetActive(true);
    }

    public void OpenArmyUI()
    {
        foreach (GameObject go in panels)
        {
            go.SetActive(false);
        }
        panels[2].SetActive(true);

        foreach (GameObject go in models)
        {
            go.SetActive(false);
        }
        models[2].SetActive(true);
    }

    public void OpenShipUI()
    {
        foreach (GameObject go in panels)
        {
            go.SetActive(false);
        }
        panels[3].SetActive(true);

        foreach (GameObject go in models)
        {
            go.SetActive(false);
        }
        models[3].SetActive(true);
    }
}
