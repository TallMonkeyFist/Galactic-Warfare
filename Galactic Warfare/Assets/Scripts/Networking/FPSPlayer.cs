using Mirror;
using Steamworks;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FPSPlayer : NetworkBehaviour
{
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

    private NetworkManager nm;
    private NetworkConnectionToClient client;
    private int playerIndex;

    private bool m_IsAlive = false;
    private GameObject m_CurrentPlayer;

    public event Func<bool> CanSpawnPlayer;

    [SyncVar(hook = nameof(SyncIsHost))]
    private bool isHost = false;
    [SyncVar(hook = nameof(ClientHandleDisplayNameUpdated))]
    private string displayName;
    [SyncVar(hook = nameof(ClientHandleSteamIDUpdated))]
    public ulong steamId;

    private Texture2D displayImage;

    public static event Action ClientOnInfoUpdated;
    public static event Action<bool> AuthorityOnPartyOwnerStateUpdated;

    public event Action ServerOnDie;
    public event Action ServerOnSpawn;

    public bool IsHost { get { return isHost; } }
    public int PlayerTeam { get { return team; } }
    public string DisplayName { get { return displayName; } }
    public Texture2D DisplayImage { get { return displayImage; } }
    public UI_Game GameUI { get { return gameUI; } }

    private int SpawnIndex = 0;

    #region Server

    public override void OnStartServer()
    {
        nm = NetworkManager.singleton;
        playerIndex = nm.spawnPrefabs.IndexOf(fpsPlayerPrefab);

        ServerOnSpawn += TargetDisableMouseCursor;

        RpcUpdateInfo();

        DontDestroyOnLoad(gameObject);
    }

    [Server]
    public void SetDisplayName(string displayName)
    {
        this.displayName = displayName;
    }

    [Server]
    public void SetSteamId(ulong id)
    {
        steamId = id;
    }

    [Server]
    public void SetPartyOwner(bool state)
    {
        isHost = state;
    }

    [Command]
    public void CmdStartGame()
    {
        if(!isHost) { return; }

        ((FPSNetworkManager)NetworkManager.singleton).StartGame();
    }

    private void SyncIsHost(bool oldState, bool newState)
    {
        if(!hasAuthority) { return; }

        AuthorityOnPartyOwnerStateUpdated?.Invoke(newState);
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

        if (CanSpawnPlayer == null || !CanSpawnPlayer.Invoke()) { return; }

        FPSNetworkManager networkManager = (FPSNetworkManager)NetworkManager.singleton;

        SpawnTransform spawnData = networkManager.ServerGetSpawnLocation(index, team);

        if(spawnData.position.Equals(Vector3.positiveInfinity)) { return; }

        GameObject playerInstance = Instantiate(nm.spawnPrefabs[playerIndex], spawnData.position, Quaternion.identity);
        playerInstance.transform.forward = spawnData.forwardDirection;

        m_CurrentPlayer = playerInstance;

        if (playerInstance.TryGetComponent<Health>(out Health health))
        {
            health.ServerOnDie += ServerHandleDie;
            health.ServerOnDie += ServerOnDie;
            health.ServerSetTeam(team);
        }

        NetworkServer.Spawn(playerInstance, connectionToClient);

        if (playerInstance.TryGetComponent<PlayerInventoryManager>(out PlayerInventoryManager inventory))
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

        if(NetworkServer.active) { return; }

        DontDestroyOnLoad(gameObject);


        ((FPSNetworkManager)NetworkManager.singleton).Players.Add(this);

        ClientOnInfoUpdated?.Invoke();

        if (!hasAuthority) { return; }

        DisableSpawnUI();
        Cursor.lockState = CursorLockMode.None;
    }

    public override void OnStopClient()
    {
        ClientOnInfoUpdated?.Invoke();

        ((FPSNetworkManager)NetworkManager.singleton).Players.Remove(this);

        if(!hasAuthority) { return; }
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
        ClientOnInfoUpdated?.Invoke();
    }

    [Client]
    private void ClientHandleSteamIDUpdated(ulong Oldid, ulong newId)
    {
        if (newId == ulong.MinValue)
        {

            displayImage = ((FPSNetworkManager)NetworkManager.singleton).defaultImage;

            ClientOnInfoUpdated?.Invoke();

            return;
        }

        CSteamID id = new CSteamID(newId);

        int imageIndex = SteamFriends.GetLargeFriendAvatar(id); 
        
        Texture2D ret = null;

        ret = ((FPSNetworkManager)NetworkManager.singleton).defaultImage;


        if (imageIndex > 0)
        {
            uint ImageWidth;
            uint ImageHeight;

            bool bIsValid = SteamUtils.GetImageSize(imageIndex, out ImageWidth, out ImageHeight);

            if (bIsValid)
            {
                byte[] Image = new byte[ImageWidth * ImageHeight * 4];

                bIsValid = SteamUtils.GetImageRGBA(imageIndex, Image, (int)(ImageWidth * ImageHeight * 4));

                if (bIsValid)
                {
                    ret = new Texture2D((int)ImageWidth, (int)ImageHeight, TextureFormat.RGBA32, false, true);
                    ret.LoadRawTextureData(Image);
                    ret.Apply();
                }
            }
        }

        displayImage = ret;

        ClientOnInfoUpdated?.Invoke();
    }

    public void SetSpawnLocationIndex(int index)
    {
        SpawnIndex = index;
    }

    [ClientRpc]
    public void RpcUpdateInfo()
    {
        ClientOnInfoUpdated?.Invoke();
    }

    [TargetRpc]
    private void TargetDisableMouseCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    #endregion
}
