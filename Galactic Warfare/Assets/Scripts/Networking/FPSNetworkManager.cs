using Mirror;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FPSNetworkManager : NetworkManager
{
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

    public Callback<AvatarImageLoaded_t> avatarImageLoaded;

    private List<AvatarInfo> unloadedPlayerAvatars;

    private string lastLoadedScene;

    public static ulong LobbyId { get; set; }

    public override void Start()
    {
        base.Start();
    }

    #region Server

    private struct AvatarInfo
    {
        public FPSPlayer player;
        public int playerIndex;
        public CSteamID SteamID;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        gameManager = gameObject.AddComponent<GamemodeManager>();
        Players = new List<FPSPlayer>();
        teamOne = new List<NetworkIdentity>();
        teamTwo = new List<NetworkIdentity>();
        unloadedPlayerAvatars = new List<AvatarInfo>();

        if (useSteam)
        {
            avatarImageLoaded = Callback<AvatarImageLoaded_t>.Create(PlayerAvatarLoaded);
        }
    }

    public override void OnStopServer()
    {
        Players.Clear();
        teamOne.Clear();
        teamTwo.Clear();
        unloadedPlayerAvatars.Clear();

        isGameInProgress = false;
    }

    public void StartGame()
    {
        if(Players.Count < 1) { return; }

        isGameInProgress = true;

        ServerChangeScene("Map_02");
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

        if(conn.identity.TryGetComponent(out FPSPlayer player))
        {
            Players.Add(player);

            player.SetDisplayName($"Player {Players.Count}");
            player.SetSteamId(ulong.MinValue);

            if (SteamManager.Initialized && useSteam)
            {
                CSteamID steamId = SteamMatchmaking.GetLobbyMemberByIndex(new CSteamID(LobbyId), numPlayers - 1);

                player.SetDisplayName($"{SteamFriends.GetFriendPersonaName(steamId)}");

                int imageIndex = SteamFriends.GetLargeFriendAvatar(steamId);

                if(imageIndex > -1)
                {
                    player.SetSteamId(steamId.m_SteamID);
                }
                else
                {
                    AvatarInfo info = new AvatarInfo();
                    info.player = player;
                    info.playerIndex = numPlayers - 1;
                    info.SteamID = steamId;

                    unloadedPlayerAvatars.Add(info);
                }
            }

            if(Players.Count == 1)
            {
                player.SetPartyOwner(true);
            }

            player.RpcUpdateInfo();
        }

        AssignPlayerToTeam(conn);
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        if(!isGameInProgress) { return; }

        conn.Disconnect();
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        try
        {
            if (conn.identity.gameObject.TryGetComponent<FPSPlayer>(out FPSPlayer player))
            {
                player.ServerKillPlayer();
                Players.Remove(player);
                if(isGameInProgress)
                {
                    gameManager.ServerHandlePlayerDisconnect(player);
                }
            }

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
        if (teamOne.Count > teamTwo.Count)
        {
            teamTwo.Add(conn.identity);
            if (conn.identity.TryGetComponent(out FPSPlayer player))
            {
                player.ServerSetTeam(2);
            }
        }
        else
        {
            teamOne.Add(conn.identity);
            if (conn.identity.TryGetComponent(out FPSPlayer player))
            {
                player.ServerSetTeam(1);
            }
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
    public void PlayerAvatarLoaded(AvatarImageLoaded_t callback)
    {
        for (int i = 0; i < unloadedPlayerAvatars.Count; i++)
        {
            AvatarInfo ai = unloadedPlayerAvatars[i];

            ai.player.SetSteamId(ai.SteamID.m_SteamID);
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
}
