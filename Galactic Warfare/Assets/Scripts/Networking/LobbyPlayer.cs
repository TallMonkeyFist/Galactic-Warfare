using DataTypes;
using Mirror;
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayer : NetworkBehaviour
{
	#region Server Variables

	public event Action ServerOnDisconnect;
	public GameObject GamePlayerPrefab;
	public FPSPlayer GamePlayer { get; private set; }
	private int GamePlayerIndex = -1;

	#endregion

	#region Authority Variables
	
	public static event Action<bool> AuthorityOnPartyOwnerStateUpdated;
	
	#endregion
	
	#region Client Variables

	public static event Action<List<PlayerDisplayInfo>> ClientOnPlayerInfoUpdated;
	protected List<Callback<AvatarImageLoaded_t>> avatarImageLoadedCallbacks;
	private List<PlayerDisplayInfo> PlayerDisplayInfo = new List<PlayerDisplayInfo>();

	#endregion

	#region Sync Variables

	[SyncVar(hook = nameof(ClientSyncIsHost))]
	private bool isHost = false;

	[SyncVar(hook = nameof(ClientSyncTeam))]
	private int playerTeam = -1;

	[SyncVar]
	private ulong steamID;

	[SyncVar]
	public PlayerData PlayerData;

	#endregion

	#region Variables

	public bool IsHost { get { return isHost; } }
	public int PlayerTeam { get { return playerTeam; } }
	public ulong SteamID { get { return steamID; } }

	public Button ChangeTeamsButton = null;

	private FPSNetworkManager NM = null;

	public static bool DisplayLogInfo = true;

	#endregion

	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}

	#region Server

	public override void OnStartServer()
	{
		base.OnStartServer();

		NM = NetworkManager.singleton as FPSNetworkManager;
		GamePlayerIndex = NM.spawnPrefabs.IndexOf(GamePlayerPrefab);
	}

	[Server]
	public void ServerSetTeam(int team)
	{
		Logger.Log($"Setting player {PlayerData.Name} team to {team}", DisplayLogInfo);
		playerTeam = team;
		if(GamePlayer != null) { GamePlayer.ServerSetTeam(team); }
		NM.ServerUpdatePlayerData(this);
	}

	[Server]
	public void ServerSetSteamID(ulong id)
	{
		steamID = id;
	}

	[Server]
	public void ServerSetPartyOwner(bool host)
	{
		isHost = host;
	}

	[Server]
	public void ServerSetPlayerData(PlayerData data)
	{
		PlayerData = data;
	}

	[Server]
	public void OnServerDisconnect()
	{
		ServerOnDisconnect?.Invoke();
	}

	[Server]
	public void ServerSpawnGamePlayer()
	{
		if(GamePlayer != null) { return; }
		if(!NM.IsGameInProgress) { return; }

		GameObject instance = Instantiate(NM.spawnPrefabs[GamePlayerIndex]);
		if(!instance.TryGetComponent(out FPSPlayer player)) { Destroy(instance); return; }
		NetworkServer.Spawn(instance, connectionToClient);
		GamePlayer = player;
		GamePlayer.ServerInitializePlayer(this);
	}

	#endregion

	#region Commands

	[Command]
	public void CmdStartGame(string level)
	{
		if(!isHost) { return; }

		NM.StartGame(level);
	}

	[Command]
	public void CmdSetTeam(int team)
	{
		ServerSetTeam(team);
	}

	#endregion

	#region Client

	public override void OnStartClient()
	{
		base.OnStartClient();

		NM = NetworkManager.singleton as FPSNetworkManager;

		Cursor.lockState = CursorLockMode.None;
	}

	public override void OnStopClient()
	{
		base.OnStopClient();
	}

	[Client]
	private void OnAvatarImageLoaded(AvatarImageLoaded_t callback)
	{
		Texture2D image = GetSteamImageAsTexture(callback.m_steamID);
		for(int i = 0; i < PlayerDisplayInfo.Count; i++)
		{
			if(PlayerDisplayInfo[i].SteamID == callback.m_steamID.m_SteamID)
			{
				PlayerDisplayInfo info = PlayerDisplayInfo[i];
				info.Avatar = image;
				PlayerDisplayInfo[i] = info;
			}
		}
		ClientOnPlayerInfoUpdated?.Invoke(PlayerDisplayInfo);
	}

	[Client]
	private Texture2D GetSteamImageAsTexture(CSteamID id)
	{
		int iImage = SteamFriends.GetLargeFriendAvatar(id);

		if(iImage == -1)
		{
			avatarImageLoadedCallbacks.Add(Callback<AvatarImageLoaded_t>.Create(OnAvatarImageLoaded));
		}
		else if (SteamUtils.GetImageSize(iImage, out uint width, out uint height))
		{
			byte[] image = new byte[width * height * 4];

			if (SteamUtils.GetImageRGBA(iImage, image, (int)(width * height * 4)))
			{
				Texture2D texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
				byte r, g, b, a;
				// I don't know why but the image is loaded upside down, this will flip it
				for (int i = 0; i < image.Length / 2; i += 4)
				{
					r = image[i];
					g = image[i + 1];
					b = image[i + 2];
					a = image[i + 3];
					image[i] = image[image.Length - i - 4];
					image[i + 1] = image[image.Length - i - 3];
					image[i + 2] = image[image.Length - i - 2];
					image[i + 3] = image[image.Length - i - 1];
					image[image.Length - i - 4] = r;
					image[image.Length - i - 3] = g;
					image[image.Length - i - 2] = b;
					image[image.Length - i - 1] = a;
				}
				texture.LoadRawTextureData(image);
				texture.Apply();
				return texture;
			}
		}

		return NM.defaultImage;
	}

	[Client]
	private void AddPlayerInfo(string name, ulong steamID, bool steam, uint id, int team)
	{
		PlayerDisplayInfo info = new PlayerDisplayInfo();
		info.Name = name;
		info.ID = id;
		info.SteamID = steamID;
		info.Team = team;
		if(!steam)
		{
			info.Avatar = NM.defaultImage;
		}
		else
		{
			info.Avatar = GetSteamImageAsTexture(new CSteamID(steamID));
		}

		PlayerDisplayInfo.Add(info);
		ClientOnPlayerInfoUpdated?.Invoke(PlayerDisplayInfo);
	}

	[Client]
	private void ClientChangeTeamLobbyButtonPressed()
	{
		CmdSetTeam((PlayerTeam + 1) % 2);
	}

	[TargetRpc]
	public void TargetSetLobbyChangeTeamButton(int buttonIndex)
	{
		if(ChangeTeamsButton != null) { ChangeTeamsButton.onClick.RemoveAllListeners(); ChangeTeamsButton.interactable = false; }
		ChangeTeamsButton = FindObjectOfType<LobbyMenu>().playerChangeTeamButtons[buttonIndex];
		ChangeTeamsButton.onClick.AddListener(ClientChangeTeamLobbyButtonPressed);
		ChangeTeamsButton.interactable = true;
	}

	[TargetRpc]
	private void TargetSetCursorLockState(CursorLockMode mode)
	{
		Cursor.lockState = mode;
	}

	[TargetRpc]
	public void TargetClearPlayerInfoList(NetworkConnection conn)
	{
		PlayerDisplayInfo.Clear();
	}

	[TargetRpc]
	public void TargetAddPlayerInfo(NetworkConnection conn, string name, ulong steamID, bool steam, uint id, int team)
	{
		AddPlayerInfo(name, steamID, steam, id, team);
	}

	[TargetRpc]
	public void TargetRemovePlayerInfo(NetworkConnection conn, int ID)
	{
		for(int i = PlayerDisplayInfo.Count - 1; i >= 0; i--)
		{
			if(PlayerDisplayInfo[i].ID == ID)
			{
				PlayerDisplayInfo.RemoveAt(i);
				break;
			}
		}
		ClientOnPlayerInfoUpdated?.Invoke(PlayerDisplayInfo);
	}

	[TargetRpc]
	public void TargetUnlockAchievement(AchievementName achievement)
	{
		if (SteamManager.Initialized)
		{
			SteamUserStats.SetAchievement(achievement.ToString());
		}
	}

	[TargetRpc]
	public void TargetSetAchievmentStatInt(AchievementName achievement, int value)
	{
		if (SteamManager.Initialized)
		{
			SteamUserStats.SetStat(achievement.ToString(), value);
		}
	}

	[TargetRpc]
	public void TargetSetAchievmentStatFloat(AchievementName achievement, float value)
	{
		if (SteamManager.Initialized)
		{
			SteamUserStats.SetStat(achievement.ToString(), value);
		}
	}

	[TargetRpc]
	public void TargetUpdateAverageStat(AchievementName achievement, float averageInSession, double sessionTime)
	{
		if (SteamManager.Initialized)
		{
			SteamUserStats.UpdateAvgRateStat(achievement.ToString(), averageInSession, sessionTime);
		}
	}

	#endregion

	#region Client Sync

	private void ClientSyncIsHost(bool oldIsHost, bool newIsHost)
	{
		AuthorityOnPartyOwnerStateUpdated?.Invoke(newIsHost);
	}

	private void ClientSyncTeam(int oldTeam, int newTeam)
	{

	}

	#endregion
}