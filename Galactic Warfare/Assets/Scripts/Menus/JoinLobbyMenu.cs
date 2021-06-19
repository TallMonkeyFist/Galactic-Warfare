using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinLobbyMenu : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Startup panel")]
    [SerializeField] private GameObject landingPagePanel = null;
    [Tooltip("IP Address Input field")]
    [SerializeField] private TMP_InputField addressInput = null;
    [Tooltip("Join game button")]
    [SerializeField] private Button joinButton = null;

    private void OnEnable()
    {
        FPSNetworkManager.ClientOnConnected += HandleClientConnected;
        FPSNetworkManager.ClientOnDisconnected += HandleClientDisconnected;
    }

    private void OnDisable()
    {
        FPSNetworkManager.ClientOnConnected -= HandleClientConnected;
        FPSNetworkManager.ClientOnDisconnected -= HandleClientDisconnected;
    }

    public void Join()
    {
        string address = addressInput.text;

        NetworkManager.singleton.networkAddress = address;
        NetworkManager.singleton.StartClient();

        joinButton.interactable = false;
    }

    private void HandleClientConnected()
    {
        joinButton.interactable = true;

        gameObject.SetActive(false);
        landingPagePanel.SetActive(false);
    }

    private void HandleClientDisconnected()
    {
        joinButton.interactable = true;
    }
}
