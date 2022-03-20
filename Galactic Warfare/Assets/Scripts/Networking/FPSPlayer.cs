using Mirror;
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;
using DataTypes;

public class FPSPlayer : NetworkBehaviour
{
	[Header("References")]
	[Tooltip("FPS Player prefab to spawn")]
	[SerializeField] private GameObject FPSPlayerPrefab = null;
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

	private FPSNetworkManager NM;
	private NetworkConnectionToClient client;
	private int playerIndex;

	private bool m_IsAlive = false;
	private GameObject m_CurrentPlayer;

	public event Func<bool> CanPlayerSpawn;

	public event Action ServerOnDie;
	public event Action ServerOnSpawn;
	public int PlayerTeam { get { return team; } }
	public UI_Game GameUI { get { return gameUI; } }

	private int SpawnIndex = 0;

	public LobbyPlayer OwningPlayer;

	#region Server

	public override void OnStartServer()
	{
		base.OnStartServer();
		
		NM = (FPSNetworkManager) NetworkManager.singleton;
		playerIndex = NM.spawnPrefabs.IndexOf(FPSPlayerPrefab);
		ServerOnSpawn += TargetDisableMouseCursor;
		ClientHandleGameStart();
	}

	public override void OnStopServer()
	{
		OwningPlayer.ServerOnDisconnect -= ServerKillPlayer;

		base.OnStopServer();
	}

	[Server]
	public void ServerInitializePlayer(LobbyPlayer owner)
	{
		OwningPlayer = owner;
		OwningPlayer.ServerOnDisconnect += ServerKillPlayer;
		ServerSetTeam(owner.PlayerTeam);
		TargetHandleGameStart();
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

		SpawnTransform spawnData = NM.ServerGetSpawnLocation(index, team);

		if(spawnData.position.Equals(Vector3.positiveInfinity)) { return; }

		GameObject playerInstance = Instantiate(NM.spawnPrefabs[playerIndex], spawnData.position, Quaternion.identity);
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

		if(m_CurrentPlayer.TryGetComponent(out Health health))
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
			if(m_CurrentPlayer.TryGetComponent<Health>(out Health health))
			{
				health.ServerSetTeam(team);
			}
		}
	}

	#endregion

	#region Client

	public override void OnStartClient()
	{
		if (!hasAuthority) { return; }

		EnableSpawnUI();
		Cursor.lockState = CursorLockMode.None;
	}

	public override void OnStopClient()
	{
		if(!hasAuthority) { return; }
	}

	[TargetRpc]
	public void TargetResetSpawnManager()
	{
		ClientResetSpawnManager();
	}

	[Client]
	public void ClientResetSpawnManager()
	{
		spawnManager.InitDropdowns();
	}

	[TargetRpc]
	public void TargetSetPlayerAlive(NetworkConnection target, bool isAlive)
	{
		m_IsAlive = isAlive;

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
	public void TargetHandleGameStart()
	{
		ClientHandleGameStart();
	}

	[Client]
	private void ClientHandleGameStart()
	{
		if(!hasAuthority){ return; }
		EnableSpawnUI();
		GameUI.ClientEnableGameUI();
		ClientResetSpawnManager();
		if (TryGetComponent(out PlayerInventoryManager inventory))
		{
			inventory.ClientEnableInput();
		}
	}

	[TargetRpc]
	public void TargetEnableSpawnUI()
	{
		EnableSpawnUI();
	}

	[Client]
	public void EnableSpawnUI()
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
	private void DisableSpawnUI()
	{
		if(!hasAuthority) { return; }

		canvasSpawn.SetActive(false);
	}

	public void SetSpawnLocationIndex(int index)
	{
		SpawnIndex = index;
	}

	[TargetRpc]
	private void TargetDisableMouseCursor()
	{
		Cursor.lockState = CursorLockMode.Locked;
	}
	#endregion
}
