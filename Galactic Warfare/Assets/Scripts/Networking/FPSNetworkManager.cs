﻿using Mirror;
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FPSNetworkManager : NetworkManager
{
	public struct PlayerData
	{
		public string Name;
		public ulong SteamID;
		public bool UseSteam;
		public int ID;
	}

	public Texture2D defaultImage;
	public bool useSteam;

	public List<FPSPlayer> Players = new List<FPSPlayer>();

	public List<NetworkIdentity> teamOne = null;
	public List<NetworkIdentity> teamTwo = null;
	private GamemodeManager gameManager = null;

	public SpawnManager spawnManager = null;

	private bool isGameInProgress = false;

	public static event Action<FPSPlayer, string> ServerSetPlayerName;
	public static event Action ClientOnConnected;
	public static event Action ClientOnDisconnected;

	private string lastLoadedScene;

	private List<PlayerData> playerInfoData = new List<PlayerData>();

	public static ulong LobbyId { get; set; }

	public override void Start()
	{
		base.Start();
	}

	#region Server

	public override void OnStartServer()
	{
		base.OnStartServer();

		gameManager = gameManager == null ? gameObject.AddComponent<GamemodeManager>() : gameManager;
		Players = new List<FPSPlayer>();
		teamOne = new List<NetworkIdentity>();
		teamTwo = new List<NetworkIdentity>();
	}

	public override void OnStopServer()
	{
		Players.Clear();
		teamOne.Clear();
		teamTwo.Clear();
		playerInfoData.Clear();

		isGameInProgress = false;
	}

	public void StartGame(string level)
	{
		if(Players.Count < 1) { return; }

		isGameInProgress = true;

		ServerChangeScene(level);
	}

	public override void OnServerChangeScene(string newSceneName)
	{
		spawnManager = null;
	}

	public override void OnServerSceneChanged(string sceneName)
	{
		int index = sceneName.LastIndexOf('/');
		string mapName = sceneName.Substring(index + 1);

		lastLoadedScene = mapName;

		//If map name starts with Map_ then it is a valid map
		if (mapName.StartsWith("Map_"))
		{
			Debug.Log("Scene was changed");
			gameManager.StartDeathmatch();
		}
	}

	public override void OnServerAddPlayer(NetworkConnection conn)
	{
		base.OnServerAddPlayer(conn);

		Debug.Log("FPSNetworkManager Player added");

		ServerInitializePlayer(conn);

		Debug.Log($"Team one size: {teamOne.Count}");
		Debug.Log($"Team two size: {teamTwo.Count}");
		Debug.Log($"Player size:   {Players.Count}");
	}

	public override void OnServerConnect(NetworkConnection conn)
	{
		if(isGameInProgress) { conn.Disconnect(); return; }
	}

	public override void OnServerDisconnect(NetworkConnection conn)
	{
		if(!conn.identity.gameObject.TryGetComponent(out FPSPlayer player)) { return; }

		try
		{
			player.ServerKillPlayer();
			if(useSteam)
            {
				player.TargetDisconnectSteamUser(conn, LobbyId);
            }
			Players.Remove(player);
			if(isGameInProgress)
			{
				gameManager.ServerHandlePlayerDisconnect(player);
			}

			ServerHandlePlayerDisconnect(conn);

		}
		catch (NullReferenceException)
		{
			Debug.LogWarning("Caught null reference on server disconnect");
		}

		base.OnServerDisconnect(conn);
	}

	[Server]
	public void AssignTeams()
	{
		foreach (KeyValuePair<int, NetworkConnectionToClient> entry in NetworkServer.connections)
		{
			Debug.Log(entry.Key);
		}
	}

	[Server]
	public void ClearTeams()
	{
		teamOne.Clear();
		teamTwo.Clear();
	}

	[Server]
	public void AssignPlayerToTeam(NetworkConnection conn)
	{
		if (!conn.identity.TryGetComponent(out FPSPlayer player)) { return; }

		if (teamOne.Count > teamTwo.Count)
		{
			teamTwo.Add(conn.identity);
			player.ServerSetTeam(2);
		}
		else
		{
			teamOne.Add(conn.identity);
			player.ServerSetTeam(1);
		}
	}

	[Server]
	public void AssignPlayerToTeam(NetworkConnection conn, int team)
	{
		switch (team)
		{
			case 1:
				teamOne.Add(conn.identity);
				break;
			case 2:
				teamTwo.Add(conn.identity);
				break;
			default:
				Debug.LogWarning("Player is not allowed to join a team other than 1 or 2");
				break;
		}
	}

	[Server]
	public void RestartScene()
	{
		ServerChangeScene(lastLoadedScene);
	}

	[Server]
	public SpawnTransform ServerGetSpawnLocation(int index, int team)
	{
		if(spawnManager == null)
		{
			return SpawnTransform.invalidSpawn;
		}
		return spawnManager.ServerGetSpawnLocation(index, team);
	}

	[Server]
	private void ServerInitializePlayer(NetworkConnection conn)
	{

		// Invalid player prefab
		if (!conn.identity.TryGetComponent(out FPSPlayer player)) { conn.Disconnect(); return; }

		//Player already exist on the server
		if (Players.Contains(player)) { Players.Remove(player); }

		Players.Add(player);
		player.SetPartyOwner(Players.Count == 1);

		PlayerData pd;
		pd.ID = conn.connectionId;

		if (!useSteam || !SteamManager.Initialized)
		{
			player.SetSteamId(ulong.MinValue);
			player.SetDisplayName($"Player {Players.Count}");

			pd.UseSteam = false;
			pd.Name = $"Player {Players.Count}";
			pd.SteamID = ulong.MinValue;
		}
		else
		{
			CSteamID steamId = SteamMatchmaking.GetLobbyMemberByIndex(new CSteamID(LobbyId), numPlayers - 1);
			player.SetSteamId(steamId.m_SteamID);
			player.SetDisplayName(SteamFriends.GetFriendPersonaName(steamId));

			pd.UseSteam = true;
			pd.Name = SteamFriends.GetFriendPersonaName(steamId);
			pd.SteamID = steamId.m_SteamID;
		}

		ServerAddPlayerData(pd);
		ServerSyncPlayerList();
		AssignPlayerToTeam(conn);
	}

	[Server]
	private void ServerSyncPlayerList()
	{
		foreach (FPSPlayer player in Players)
		{
			NetworkConnection conn = player.connectionToClient;
			player.TargetClearPlayerList(conn);
			for (int i = 0; i < playerInfoData.Count; i++)
			{
				PlayerData d = playerInfoData[i];
				player.TargetAddPlayerInfo(conn, d.Name, d.SteamID, d.UseSteam, d.ID);
			}
		}
	}

	[Server]
	private void ServerAddPlayerData(PlayerData d)
	{
		playerInfoData.Add(d);
	}

	[Server]
	private void ServerHandlePlayerDisconnect(NetworkConnection conn)
    {
		foreach(FPSPlayer player in Players)
        {
			player.TargetRemovePlayerInfo(player.connectionToClient, conn.connectionId);
        }
    }

	#endregion

	#region Client

	public override void OnClientConnect(NetworkConnection conn)
	{
		base.OnClientConnect(conn);

		ClientOnConnected?.Invoke();
	}

	public override void OnClientDisconnect(NetworkConnection conn)
	{
		base.OnClientDisconnect(conn);

		ClientOnDisconnected?.Invoke();
	}

	public override void OnStopClient()
	{
		Players.Clear();
		teamOne.Clear();
		teamTwo.Clear();

		SceneManager.LoadScene(0);
	}

	#endregion

	public void LeaveGame()
	{

		if (NetworkServer.active && NetworkClient.isConnected)
		{
			if(useSteam)
			{
				foreach (FPSPlayer player in Players)
				{
					player.TargetDisconnectSteamUser(player.connectionToServer, LobbyId);
				}
			}
			StopHost();

		}
		else
		{
			if (useSteam)
			{
				SteamMatchmaking.LeaveLobby(new CSteamID(LobbyId));
			}
			StopClient();
			SceneManager.LoadScene(0);
		}
		LobbyId = ulong.MinValue;
		Debug.LogError(LobbyId);
	}
}
