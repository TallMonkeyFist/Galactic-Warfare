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

	public bool useSteam = false;
	private FPSNetworkManager manager;

	protected Callback<LobbyCreated_t> lobbyCreated;
	protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
	protected Callback<LobbyEnter_t> lobbyEntered;

	private void Start()
	{
		Cursor.lockState = CursorLockMode.None;

		manager = (FPSNetworkManager)NetworkManager.singleton;
		useSteam = manager.useSteam;

		if(!useSteam) { return; }

		lobbyCreated = Callback<LobbyCreated_t>.Create(OnSteamLobbyCreated);
		gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnSteamGameLobbyJoinRequested);
		lobbyEntered = Callback<LobbyEnter_t>.Create(OnSteamLobbyEntered);
	}

	public void HostLobby()
	{
		landingPagePanel.SetActive(false);

		if(useSteam)
		{
			SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 16);
			return;
		}

		manager.StartHost();
	}

	private void OnSteamLobbyCreated(LobbyCreated_t callback)
	{
		if (callback.m_eResult != EResult.k_EResultOK)
		{
			landingPagePanel.SetActive(true);
			return;
		}

		manager.StartHost();

		CSteamID lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
		FPSNetworkManager.LobbyId = lobbyId.m_SteamID;

		SteamMatchmaking.SetLobbyData(
			lobbyId,
			"HostAddress",
			SteamUser.GetSteamID().ToString());
	}

	private void OnSteamGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
	{
		SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
	}

	private void OnSteamLobbyEntered(LobbyEnter_t callback)
	{
		if(NetworkServer.active) { return; }

		CSteamID lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
		FPSNetworkManager.LobbyId = lobbyId.m_SteamID;

		string hostAddress = SteamMatchmaking.GetLobbyData(lobbyId, "HostAddress");
		manager.networkAddress = hostAddress;
		manager.StartClient();

		landingPagePanel.SetActive(false);
	}
}
