using Mirror;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{

    [Header("References")]
    [Tooltip("Startup panel")]
    [SerializeField] private GameObject landingPagePanel = null;

    private bool useSteam = false;

    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;

        FPSNetworkManager manager = (FPSNetworkManager)NetworkManager.singleton;
        useSteam = manager.useSteam;

        if(!useSteam) { return; }

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    public void HostLobby()
    {
        landingPagePanel.SetActive(false);

        if(useSteam)
        {
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 32);
            return;
        }

        NetworkManager.singleton.StartHost();
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            landingPagePanel.SetActive(true);
            return;
        }

        NetworkManager.singleton.StartHost();

        CSteamID lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        FPSNetworkManager.LobbyId = lobbyId.m_SteamID;

        SteamMatchmaking.SetLobbyData(
            lobbyId,
            "HostAddress",
            SteamUser.GetSteamID().ToString());
    }

    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {

        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        if(NetworkServer.active) { return; }

        CSteamID lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        FPSNetworkManager.LobbyId = lobbyId.m_SteamID;

        string hostAddress = SteamMatchmaking.GetLobbyData(
            lobbyId,
            "HostAddress");

        NetworkManager.singleton.networkAddress = hostAddress;
        NetworkManager.singleton.StartClient();

        landingPagePanel.SetActive(false);
    }
}
