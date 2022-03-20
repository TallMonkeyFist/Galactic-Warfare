using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : NetworkBehaviour
{
	[Header("Visuals")]
	[Tooltip("Flag color when teamo one holds the flag")]
	[SerializeField] private Color teamOneColor = Color.red;
	[Tooltip("Flag color when teamo two holds the flag")]
	[SerializeField] private Color teamTwoColor = Color.blue;
	[Tooltip("Renderer that changes color to represent team ownership")]
	[SerializeField] private Renderer render = null;

	[Header("AI")]
	[Tooltip("Patrol routes around the flag")]
	[SerializeField] private PatrolPattern[] patrolRoutes = null;

	[Header("Settings")]
	[Tooltip("Time it takes to capture a neutral flag")]
	public float FlagCaptureTime = 10.0f;
	[Tooltip("Bounds required to be occupied to capture the flag")]
	[SerializeField] private Collider captureTrigger = null;

	[Header("Spawning")]
	[Tooltip("Spawn system around the flag")]
	[SerializeField] private SpawnSystem flagSpawnSystem = null;

	[SyncVar(hook = nameof(SyncFlagValue))]
	private float flagValue;

	public static event Action<int, Color> ClientOnFlagValueChanged;
	public static event Action ServerOnFlagOwnerChanged;

	public SpawnSystem FlagSpawnSystem { get { return flagSpawnSystem; } }
	public float FlagValue { get { return flagValue; } }
	private List<Health> TeamOnePlayers = new List<Health>();
	private List<Health> TeamTwoPlayers = new List<Health>();

	private int teamOneCount;
	private int teamTwoCount;
	private int flagTeamOwner;
	[SyncVar]
	private int flagIndex = -1;
	private float flagCaptureDelta = 10.0f;

	public static bool DisplayLogInfo = true;

	public Vector3 GetRandomPosition(float xDelta, float yDelta, float zDelta)
	{
		Vector3 randomPosition = transform.position;

		randomPosition.x += UnityEngine.Random.Range(-xDelta, xDelta);
		randomPosition.y += UnityEngine.Random.Range(-yDelta, yDelta);
		randomPosition.z += UnityEngine.Random.Range(-zDelta, zDelta);

		return randomPosition;
	}

	public Vector3 GetRandomPosition()
	{
		Vector3 randomPosition = captureTrigger.transform.position;

		float xDelta = captureTrigger.bounds.size.x / 2.0f;
		float yDelta = captureTrigger.bounds.size.y / 2.0f;
		float zDelta = captureTrigger.bounds.size.z / 2.0f;

		randomPosition.x += UnityEngine.Random.Range(-xDelta, xDelta);
		randomPosition.y += UnityEngine.Random.Range(-yDelta, yDelta);
		randomPosition.z += UnityEngine.Random.Range(-zDelta, zDelta);

		return randomPosition;
	}

	public PatrolPattern GetRandomPatrol()
	{
		if(patrolRoutes.Length > 0)
		{
			int routeIndex = UnityEngine.Random.Range(0, patrolRoutes.Length);

			return patrolRoutes[routeIndex];
		}
		return null;
	}

	#region Server

	[Server]
	public void SetFlagIndex(int index)
	{
		flagIndex = index;
	}

	public override void OnStartServer()
	{
		flagValue = 0;
		ServerSetTeamSpawn(-1);
		teamOneColor = (NetworkManager.singleton as FPSNetworkManager).TeamOneColor;
		teamTwoColor = (NetworkManager.singleton as FPSNetworkManager).TeamTwoColor;
		flagCaptureDelta = 100.0f / FlagCaptureTime;
	}

	private void FixedUpdate()
	{
		if (!isServer) { return; }

		PruneNullPlayers();
		CalculateFlagValue();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!isServer) { return; }

		if (other.TryGetComponent(out Health player))
		{
			if(player.GetTeam() == 0 && !TeamOnePlayers.Contains(player))
			{
				Logger.Log($"Team one player entering flag {gameObject.name}", DisplayLogInfo);
				TeamOnePlayers.Add(player);
			}
			else if(player.GetTeam() == 1 && !TeamTwoPlayers.Contains(player))
			{
				Logger.Log($"Team two player entering flag {gameObject.name}", DisplayLogInfo);
				TeamTwoPlayers.Add(player);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (!isServer) { return; }

		if (other.TryGetComponent(out Health player))
		{
			if (player.GetTeam() == 0)
			{
				Logger.Log($"Team one player leaving flag {gameObject.name}", DisplayLogInfo);
				TeamOnePlayers.Remove(player);
			}
			else if (player.GetTeam() == 1)
			{
				Logger.Log($"Team one player leaving flag {gameObject.name}", DisplayLogInfo);
				TeamTwoPlayers.Remove(player);
			}
		}
	}

	[Server]
	private void PruneNullPlayers()
	{
		for(int i = TeamOnePlayers.Count - 1; i >= 0; i--)
		{
			if(TeamOnePlayers[i] == null) 
			{
				TeamOnePlayers.RemoveAt(i);
				Logger.Log($"Team one player leaving flag {gameObject.name}", DisplayLogInfo);
			}
		}
		for(int i = TeamTwoPlayers.Count - 1; i >= 0; i--)
		{
			if (TeamTwoPlayers[i] == null) 
			{
				TeamTwoPlayers.RemoveAt(i);
				Logger.Log($"Team two player leaving flag {gameObject.name}", DisplayLogInfo);
			}
		}
	}


	[Server]
	private void CalculateFlagValue()
	{
		teamOneCount = TeamOnePlayers.Count;
		teamTwoCount = TeamTwoPlayers.Count;

		//If team one has more players and hasn't captured the flag
		//  capture for team one
		if (teamOneCount > teamTwoCount && flagValue <= 100.0f)
		{
			flagValue = Mathf.Clamp(flagValue + (Time.deltaTime) * flagCaptureDelta, -100, 100);
			if (0 <= flagValue && flagValue < 100)
			{
				ServerSetTeamSpawn(-1);
			}
			if (100 <= flagValue)
			{
				ServerSetTeamSpawn(0);
			}
		}
		//If team two has more players and hasn't captured the flag
		//  capture for team two
		else if (teamOneCount < teamTwoCount && flagValue >= -100.0f)
		{
			flagValue = Mathf.Clamp(flagValue - (Time.deltaTime) * flagCaptureDelta, -100, 100);
			if (-100 < flagValue && flagValue <= 0)
			{
				ServerSetTeamSpawn(-1);
			}
			if (flagValue <= -100)
			{
				ServerSetTeamSpawn(1);
			}
		}
		//If team one and team two are trying to capture the flag
		//  decapture the flag for owning team
		else if (teamOneCount == teamTwoCount && teamTwoCount > 0 && flagValue != 0.0f)
		{
			if (flagValue > 0.0f)
			{
				flagValue = Mathf.Clamp(flagValue - (Time.deltaTime) * flagCaptureDelta, 0, 100);
			}
			if (flagValue < 0.0f)
			{
				flagValue = Mathf.Clamp(flagValue + (Time.deltaTime) * flagCaptureDelta, -100, 0);
			}
			if (flagValue == 0.0f)
			{
				ServerSetTeamSpawn(-1);
			}
		}
		//If no one is trying to capture the flag && flag is not captured
		//  Reset flag back to zero value
		else if (teamOneCount == 0 && teamTwoCount == 0 && flagTeamOwner == 0 && flagValue != 0.0f)
		{
			if (flagValue > 0.0f)
			{
				flagValue = Mathf.Clamp(flagValue - (Time.deltaTime) * flagCaptureDelta, 0, 100);
			}
			if (flagValue < 0.0f)
			{
				flagValue = Mathf.Clamp(flagValue + (Time.deltaTime) * flagCaptureDelta, -100, 0);
			}
		}
		//If no one is trying to capture the flag && flag is owned
		//  Reset flag back to team value
		else if (teamOneCount == 0 && teamTwoCount == 0 && flagTeamOwner != 0)
		{
			if (flagTeamOwner == 0 && flagValue <= 100.0f)
			{
				flagValue = Mathf.Clamp(flagValue + (Time.deltaTime) * flagCaptureDelta, -100, 100);
			}
			else if (flagTeamOwner == 1 && flagValue >= -100.0f)
			{
				flagValue = Mathf.Clamp(flagValue - (Time.deltaTime) * flagCaptureDelta, -100, 100);
			}
		}
	}

	[Server]
	private void ServerSetTeamSpawn(int newTeamOwner)
	{
		if(newTeamOwner == flagTeamOwner) { return; }

		flagTeamOwner = newTeamOwner;
		flagSpawnSystem.team = flagTeamOwner;

		ServerOnFlagOwnerChanged?.Invoke();
	}

	#endregion

	#region Client

	public override void OnStartClient()
	{
		base.OnStartClient();

		teamOneColor = (NetworkManager.singleton as FPSNetworkManager).TeamOneColor;
		teamTwoColor = (NetworkManager.singleton as FPSNetworkManager).TeamTwoColor;
	}

	private void SyncFlagValue(float oldValue, float newValue)
	{
		if(render == null) { return; }
		if(newValue >= 0)
		{
			render.material.color = Color.Lerp(Color.white, teamOneColor, newValue / 100);
		}
		else
		{
			render.material.color = Color.Lerp(Color.white, teamTwoColor, newValue / -100);
		}

		ClientOnFlagValueChanged?.Invoke(flagIndex, render.material.color);
	}

	#endregion
}
