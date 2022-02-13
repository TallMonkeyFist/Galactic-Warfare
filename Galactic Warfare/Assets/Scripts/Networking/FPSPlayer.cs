using Mirror;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FPSPlayer : NetworkBehaviour
{
	public struct PlayerDisplayInfo
	{
		public string Name;
		public Texture2D Avatar;
		public int ID;
		public ulong SteamID;
	}

	[Header("References")]
	[Tooltip("FPS Player prefab to spawn")]
	[SerializeField] private GameObject fpsPlayerPrefab = null;
	[Tooltip("Spawn Manager to get selected equipment")]
	[SerializeField] private PlayerSpawnManager spawnManager = null;

	[Header("Team")]
	[Tooltip("Which team the player is on")]
	[SyncVar]
	[SerializeField] private int team = -1;

	[Header("UI")]
	[Tooltip("Canvas that displays when the player is not controlling an actor")]
	[SerializeField] private GameObject canvasSpawn = null;
	[Tooltip("UI for a game in progress")]
	[SerializeField] private UI_Game gameUI = null;

	private FPSNetworkManager nm;
	private NetworkConnectionToClient client;
	private int playerIndex;

	private bool m_IsAlive = false;
	private GameObject m_CurrentPlayer;

	public event Func<bool> CanPlayerSpawn;

	[SyncVar(hook = nameof(SyncIsHost))]
	private bool isHost = false;

	protected Callback<AvatarImageLoaded_t> avatarImageLoaded;

	private Texture2D displayImage;

	public static event Action<List<PlayerDisplayInfo>> ClientOnInfoUpdated;
	public static event Action<bool> AuthorityOnPartyOwnerStateUpdated;

	public event Action ServerOnDie;
	public event Action ServerOnSpawn;

	public bool IsHost { get { return isHost; } }
	public int PlayerTeam { get { return team; } }
	//public string DisplayName { get { return displayName; } }
	public Texture2D DisplayImage { get { return displayImage; } }
	public UI_Game GameUI { get { return gameUI; } }

	private int SpawnIndex = 0;

	private List<PlayerDisplayInfo> playerDisplayInfo = new List<PlayerDisplayInfo>();

	#region Server

	public override void OnStartServer()
	{
		nm = (FPSNetworkManager) NetworkManager.singleton;
		playerIndex = nm.spawnPrefabs.IndexOf(fpsPlayerPrefab);

		ServerOnSpawn += TargetDisableMouseCursor;

		RpcUpdateInfo();

		DontDestroyOnLoad(gameObject);

		if(isHost)
		{
			avatarImageLoaded = Callback<AvatarImageLoaded_t>.Create(OnAvatarImageLoaded);
		}
	}

	public override void OnStopServer()
	{
		playerDisplayInfo.Clear();

		base.OnStopServer();
	}

	[Server]
	public void SetDisplayName(string displayName)
	{
		//this.displayName = displayName;
	}

	[Server]
	public void SetSteamId(ulong id)
	{
		//m_SteamID = id;
	}

	[Server]
	public void SetPartyOwner(bool state)
	{
		isHost = state;
	}

	[Command]
	public void CmdStartGame(string level)
	{
		if(!isHost) { return; }

		((FPSNetworkManager)NetworkManager.singleton).StartGame(level);
	}

	[Command]
	private void CmdDespawn()
	{
		NetworkServer.Destroy(m_CurrentPlayer);
		m_CurrentPlayer = null;
		TargetSetPlayerAlive(client, false);
	}

	[Command]
	private void CmdSpawnPlayer(int index, SpawnData data, NetworkConnectionToClient conn = null)
	{
		client = conn;
		if (m_CurrentPlayer != null) { return; }

		if (CanPlayerSpawn == null || !CanPlayerSpawn.Invoke()) { return; }

		FPSNetworkManager networkManager = (FPSNetworkManager)NetworkManager.singleton;

		SpawnTransform spawnData = networkManager.ServerGetSpawnLocation(index, team);

		if(spawnData.position.Equals(Vector3.positiveInfinity)) { return; }

		GameObject playerInstance = Instantiate(nm.spawnPrefabs[playerIndex], spawnData.position, Quaternion.identity);
		playerInstance.transform.forward = spawnData.forwardDirection;

		m_CurrentPlayer = playerInstance;

		if (playerInstance.TryGetComponent(out Health health))
		{
			health.ServerOnDie += ServerHandleDie;
			health.ServerOnDie += ServerOnDie;
			health.ServerSetTeam(team);
		}

		NetworkServer.Spawn(playerInstance, connectionToClient);

		if (playerInstance.TryGetComponent(out PlayerInventoryManager inventory))
		{
			inventory.SetEquipment(data);
			inventory.team = team;
		}

		if(playerInstance.TryGetComponent(out PlayerMovement movement))
		{
			movement.ServerSetInput(true);
		}

		TargetSetPlayerAlive(conn, true);

		ServerOnSpawn?.Invoke();
	}

	[Command]
	public void CmdUpdatePlayerInfo()
	{
		RpcUpdateInfo();
	}

	[Server]
	private void ServerHandleDie()
	{
		NetworkServer.Destroy(m_CurrentPlayer);
		m_CurrentPlayer = null;
		TargetSetPlayerAlive(client, false);
	}

	[Server]
	public void ServerKillPlayer()
	{
		if(m_CurrentPlayer == null) { return; }

		if(m_CurrentPlayer.TryGetComponent<Health>(out Health health))
		{
			health.Kill();
		}
		NetworkServer.Destroy(m_CurrentPlayer);
		m_CurrentPlayer = null;
		TargetSetPlayerAlive(client, false);
	}

	[Server]
	public void ServerSetTeam(int team)
	{
		this.team = team;
		if(m_CurrentPlayer != null)
		{
			if(m_CurrentPlayer.TryGetComponent<Health>(out Health player))
			{
				player.ServerSetTeam(team);
			}
		}
	}

	#endregion

	#region Client

	public override void OnStartClient()
	{
		if (isHost) { return; }

		DontDestroyOnLoad(gameObject);

		if (!hasAuthority) { return; }

		DisableSpawnUI();
		Cursor.lockState = CursorLockMode.None;
	}

	public override void OnStopClient()
	{
		if(isHost) { return; }

		if(!hasAuthority) { return; }
	}

	private void SyncIsHost(bool oldState, bool newState)
	{
		if (!hasAuthority) { return; }

		AuthorityOnPartyOwnerStateUpdated?.Invoke(newState);
	}

	[TargetRpc]
	public void TargetResetSpawnManager()
	{
		spawnManager.InitDropdowns();
	}

	[TargetRpc]
	public void TargetSetPlayerAlive(NetworkConnection target, bool isAlive)
	{
		this.m_IsAlive = isAlive;

		if (!this.m_IsAlive)
		{
			EnableSpawnUI();
		}
		else
		{
			DisableSpawnUI();
		}
	}

	[Client]
	public void Spawn()
	{
		CmdSpawnPlayer(SpawnIndex, spawnManager.GetEquipment());
	}

	[TargetRpc]
	public void TargetEnableSpawnUI()
	{
		if (!hasAuthority) { return; }
		canvasSpawn.SetActive(true);
		Cursor.lockState = CursorLockMode.None;
		gameUI.enabled = true;
	}

	[TargetRpc]
	public void TargetDisableSpawnUI()
	{
		if (!hasAuthority) { return; }
		canvasSpawn.SetActive(false);
	}

	[Client]
	private void EnableSpawnUI()
	{
		if (!hasAuthority) { return; }
		canvasSpawn.SetActive(true);
		Cursor.lockState = CursorLockMode.None;
	}

	[Client]
	private void DisableSpawnUI()
	{
		if(!hasAuthority) { return; }

		canvasSpawn.SetActive(false);
	}

	[Client]
	private void ClientHandleDisplayNameUpdated(string oldName, string newName)
	{
		for(int i = 0; i < playerDisplayInfo.Count; i++)
		{
			if(playerDisplayInfo[i].ID == netIdentity.connectionToServer.connectionId)
			{
				PlayerDisplayInfo info = playerDisplayInfo[i];
				info.Name = newName;
				playerDisplayInfo[i] = info;
				ClientOnInfoUpdated?.Invoke(playerDisplayInfo);
			}
		}
	}

	[Client]
	private void ClientHandleSteamIDUpdated(ulong Oldid, ulong newId)
	{
		if (newId == ulong.MinValue)
		{
			displayImage = ((FPSNetworkManager)NetworkManager.singleton).defaultImage;
			return;
		}

		CSteamID id = new CSteamID(newId);
		Texture2D playerAvatar = GetSteamImageAsTexture(id);
		displayImage = playerAvatar;
	}

	private void OnAvatarImageLoaded(AvatarImageLoaded_t callback)
	{
		Texture2D image = GetSteamImageAsTexture(callback.m_steamID);
		for(int i = 0; i < playerDisplayInfo.Count; i++)
		{
			if(playerDisplayInfo[i].SteamID == callback.m_steamID.m_SteamID)
			{
				PlayerDisplayInfo info = playerDisplayInfo[i];
				info.Avatar = image;
				playerDisplayInfo[i] = info;
			}
		}
		ClientOnInfoUpdated?.Invoke(playerDisplayInfo);
	}

	private Texture2D GetSteamImageAsTexture(CSteamID id)
	{
		int iImage = SteamFriends.GetLargeFriendAvatar(id);

		if(iImage == -1)
		{
			avatarImageLoaded = Callback<AvatarImageLoaded_t>.Create(OnAvatarImageLoaded);
		}

		else if(SteamUtils.GetImageSize(iImage, out uint width, out uint height))
		{
			byte[] image = new byte[width * height * 4];

			if (SteamUtils.GetImageRGBA(iImage, image, (int)(width * height * 4)))
			{
				Texture2D texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
				texture.LoadRawTextureData(image);
				texture.Apply();
				return texture;
			}
		}

		return ((FPSNetworkManager)NetworkManager.singleton).defaultImage;
	}

	public void SetSpawnLocationIndex(int index)
	{
		SpawnIndex = index;
	}

	[ClientRpc]
	public void RpcUpdateInfo()
	{
		ClientOnInfoUpdated?.Invoke(playerDisplayInfo);
	}

	[TargetRpc]
	private void TargetDisableMouseCursor()
	{
		Cursor.lockState = CursorLockMode.Locked;
	}

	[ClientRpc]
	public void RemovePlayerInfo(int ID)
	{
		for (int i = playerDisplayInfo.Count - 1; i >= 0; i--)
		{
			if (playerDisplayInfo[i].ID == ID)
			{
				playerDisplayInfo.RemoveAt(i);
				return;
			}
		}
		ClientOnInfoUpdated?.Invoke(playerDisplayInfo);
	}

	[ClientRpc]
	public void RpcAddPlayerInfo(string name, ulong steamID, bool steam, int id)
	{
		AddPlayerInfo(name, steamID, steam, id);
	}

	[TargetRpc]
	public void TargetAddPlayerInfo(NetworkConnection conn, string name, ulong steamID, bool steam, int id)
	{
		AddPlayerInfo(name, steamID, steam, id);
	}

	[Client]
	private void AddPlayerInfo(string name, ulong steamID, bool steam, int id)
	{
		PlayerDisplayInfo info;
		info.Name = name;
		info.ID = id;
		info.SteamID = steamID;
		if (!steam)
		{
			info.Avatar = ((FPSNetworkManager)NetworkManager.singleton).defaultImage;
		}
		else
		{
			info.Avatar = GetSteamImageAsTexture(new CSteamID(steamID));
		}

		playerDisplayInfo.Add(info);
		ClientOnInfoUpdated?.Invoke(playerDisplayInfo);
	}

	[TargetRpc]
	public void TargetDisconnectSteamUser(NetworkConnection conn, ulong lobbyId)
	{
		SteamMatchmaking.LeaveLobby(new CSteamID(lobbyId));
	}

	#endregion
}
