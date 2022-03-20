using Mirror;
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DataTypes;

public class FPSNetworkManager : NetworkManager
{
	public Texture2D defaultImage;
	public bool useSteam;

	public List<LobbyPlayer> Players = new List<LobbyPlayer>();

	private int LastAssignedTeam = 1;
	private GamemodeManager gameManager = null;

	public SpawnManager spawnManager = null;

	public bool IsGameInProgress { get; private set; } = false;

	public static event Action ClientOnConnected;
	public static event Action ClientOnDisconnected;
	public static bool DisplayLogInfo = true;

	private string lastLoadedScene;

	private List<PlayerData> playerInfoData = new List<PlayerData>();
	public Color TeamOneColor = Color.red;
	public Color TeamTwoColor = Color.blue;

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
		Players = new List<LobbyPlayer>();
		LastAssignedTeam = 1;
	}

	public override void OnStopServer()
	{
		Players.Clear();
		LastAssignedTeam = 1;
		playerInfoData.Clear();
		if(useSteam)
		{
			SteamMatchmaking.LeaveLobby(new CSteamID(LobbyId));
			LobbyId = ulong.MinValue;
		}
		IsGameInProgress = false;
	}

	public void StartGame(string level)
	{
		if(Players.Count < 1) { return; }

		IsGameInProgress = true;

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
			//Starting deathmatch
			gameManager.StartDeathmatch();
		}	
	}

	public override void OnServerAddPlayer(NetworkConnection conn)
	{
		base.OnServerAddPlayer(conn);

		ServerInitializePlayer(conn);
	}

	public override void OnServerConnect(NetworkConnection conn)
	{
		if(IsGameInProgress) { conn.Disconnect(); return; }
	}

	public override void OnServerDisconnect(NetworkConnection conn)
	{
		if(!conn.identity.gameObject.TryGetComponent(out LobbyPlayer player)) { base.OnServerDisconnect(conn);  return; }

		try
		{
			player.OnServerDisconnect();
			Players.Remove(player);
			if(IsGameInProgress)
			{
				gameManager.ServerHandlePlayerDisconnect(player.GamePlayer);
			}

			ServerHandlePlayerDisconnect(conn, player.PlayerTeam);

		}
		catch (NullReferenceException)
		{
			Logger.LogWarning("Caught null reference on server disconnect", DisplayLogInfo);
		}

		base.OnServerDisconnect(conn);
	}

	[Server]
	public void ServerAssignTeams()
	{
		foreach (KeyValuePair<int, NetworkConnectionToClient> entry in NetworkServer.connections)
		{
			Logger.Log(entry.Key, DisplayLogInfo);
		}
	}

	[Server]
	public void ServerAssignPlayerToTeam(NetworkConnection conn)
	{
		if (!conn.identity.TryGetComponent(out LobbyPlayer player)) { return; }

		if (LastAssignedTeam == 0)
		{
			LastAssignedTeam = 1;
			player.ServerSetTeam(1);
		}
		else
		{
			LastAssignedTeam = 0;
			player.ServerSetTeam(0);
		}
	}

	[Server]
	public void ServerRestartScene()
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
		if (!conn.identity.TryGetComponent(out LobbyPlayer player)) { conn.Disconnect(); return; }

		//Player already exist on the server
		if (Players.Contains(player)) { Players.Remove(player); }

		Players.Add(player);
		player.ServerSetPartyOwner(Players.Count == 1);

		PlayerData pd;

		if (!useSteam || !SteamManager.Initialized)
		{
			pd = GetPlayerData(conn, player);
		}
		else
		{
			pd = GetSteamPlayerData(conn, player);
		}

		player.ServerSetPlayerData(pd);
		ServerAddPlayerData(pd);
		ServerSyncPlayerList();
		ServerAssignPlayerToTeam(conn);
	}

	private PlayerData GetSteamPlayerData(NetworkConnection conn, LobbyPlayer player)
	{
		CSteamID steamId = SteamMatchmaking.GetLobbyMemberByIndex(new CSteamID(LobbyId), numPlayers - 1);
		player.ServerSetSteamID(steamId.m_SteamID);

		PlayerData pd;
		pd.ID = conn.identity.netId;
		pd.UseSteam = true;
		pd.Name = SteamFriends.GetFriendPersonaName(steamId);
		pd.SteamID = steamId.m_SteamID;
		pd.Team = player.PlayerTeam;

		return pd;
	}

	private PlayerData GetPlayerData(NetworkConnection conn, LobbyPlayer player)
	{
		player.ServerSetSteamID(ulong.MinValue);

		PlayerData pd;
		pd.ID = conn.identity.netId;
		pd.UseSteam = false;
		pd.Name = $"Player {Players.Count}";
		pd.SteamID = ulong.MinValue;
		pd.Team = player.PlayerTeam;

		return pd;
	}

	[Server]
	public void ServerUpdatePlayerData(LobbyPlayer player)
	{
		PlayerData newPlayerData;
		for(int i = 0; i < playerInfoData.Count; i++)
		{
			if (playerInfoData[i].ID == player.PlayerData.ID)
			{
				newPlayerData = playerInfoData[i];
				newPlayerData.Team = player.PlayerTeam;
				playerInfoData[i] = newPlayerData;
				ServerSyncPlayerList();
				return;
			}
		}
	}

	[Server]
	private void ServerSyncPlayerList()
	{
		for (int i = 0; i < Players.Count; i++)
		{
			LobbyPlayer player = Players[i];
			NetworkConnection conn = player.connectionToClient;
			player.TargetClearPlayerInfoList(conn);
			foreach(PlayerData data in playerInfoData)
			{
				player.TargetAddPlayerInfo(conn, data.Name, data.SteamID, data.UseSteam, data.ID, data.Team);
			}
			player.TargetSetLobbyChangeTeamButton(i);
		}
	}

	[Server]
	private void ServerAddPlayerData(PlayerData d)
	{
		playerInfoData.Add(d);
	}

	[Server]
	private void ServerHandlePlayerDisconnect(NetworkConnection conn, int team)
	{
		foreach (LobbyPlayer player in Players)
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
		ClientOnDisconnected?.Invoke();

		base.OnClientDisconnect(conn);
	}

	public override void OnStopClient()
	{
		Players.Clear();
		LastAssignedTeam = 1;
		if (useSteam)
		{
			SteamMatchmaking.LeaveLobby(new CSteamID(LobbyId));
			LobbyId = ulong.MinValue;
		}
		SceneManager.LoadScene(0);
	}

	#endregion

	public void LeaveGame()
	{

		if (NetworkServer.active && NetworkClient.isConnected)
		{
			StopHost();
		}
		else
		{
			StopClient();
		}
		LobbyId = ulong.MinValue;
	}
}
