using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyMenu : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Startup panel")]
    [SerializeField] private GameObject lobbyUI = null;
    [Tooltip("Button to start the game")]
    [SerializeField] private Button startGameButton = null;
    [Tooltip("Text elements to display player names")]
    [SerializeField] private TMP_Text[] playerNameTexts = null;
    [Tooltip("Image elements to display player avatars")]
    [SerializeField] private RawImage[] playerAvatarImages = null;

    private void Start()
    {
        FPSNetworkManager.ClientOnConnected += HandleClientConnected;
        FPSPlayer.AuthorityOnPartyOwnerStateUpdated += AuthorityHandlePartyOwnerStateUpdated;
        FPSPlayer.ClientOnInfoUpdated += ClientHandleInfoUpdated;
    }

    private void OnDestroy()
    {
        FPSNetworkManager.ClientOnConnected -= HandleClientConnected;
        FPSPlayer.AuthorityOnPartyOwnerStateUpdated -= AuthorityHandlePartyOwnerStateUpdated;
        FPSPlayer.ClientOnInfoUpdated -= ClientHandleInfoUpdated;
    }

    private void HandleClientConnected()
    {
        lobbyUI.SetActive(true);
    }

    private void AuthorityHandlePartyOwnerStateUpdated(bool state)
    {
        startGameButton.gameObject.SetActive(state);
    }

    private void ClientHandleInfoUpdated()
    {
        FPSNetworkManager manager = (FPSNetworkManager)NetworkManager.singleton;
        List<FPSPlayer> players = manager.Players;

        for(int i = 0; i < players.Count; i++)
        {
            playerNameTexts[i].text = players[i].DisplayName;
            playerAvatarImages[i].texture = players[i].DisplayImage;
        }

        for(int i = players.Count; i < playerNameTexts.Length; i++)
        {
            playerNameTexts[i].text = "Waiting for Player...";
            playerAvatarImages[i].texture = manager.defaultImage;
        }

        startGameButton.interactable = players.Count >= 1;
    }

    public void StartGame()
    {
        if(NetworkClient.connection.identity.TryGetComponent(out FPSPlayer player))
        {
            player.CmdStartGame();
        }
    }

    public void LeaveLobby()
    {
        if(NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else
        {
            NetworkManager.singleton.StopClient();

            SceneManager.LoadScene(0);
        }
    }
}
