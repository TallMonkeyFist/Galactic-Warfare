using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataTypes;

public class AIManager : NetworkBehaviour
{
	[SerializeField] private int team = 0;
	[SerializeField] private int AICount = 10;
	[SerializeField] private float minTimeToSpawn = 2.0f;
	[SerializeField] private float maxTimeToSpawn = 10.0f;

	[SerializeField] private GameObject AIPrefab = null;

	public event Action ServerOnSpawnAI;
	public event Action ServerAIDie;
	private event Func<bool> CanSpawnAI;

	private Deathmatch deathmatch = null;
	private SpawnManager spawnManager = null;

	private bool IDMCallback = false;

	public override void OnStartServer()
	{
		base.OnStartServer();

		if(Deathmatch.singleton != null)
		{
			initDeathmatchManager();
		}
		else
		{
			Deathmatch.OnDeathmatchStarted += initDeathmatchManager;
			IDMCallback = true;
		}
		initSpawnManager();
	}

	[Server]
	private void initDeathmatchManager()
	{
		deathmatch = Deathmatch.singleton;
		if(IDMCallback)
		{
			Deathmatch.OnDeathmatchStarted -= initDeathmatchManager;
			IDMCallback = false;
		}
		if (team == 0)
		{
			ServerOnSpawnAI += deathmatch.DrainTicketOne;
			ServerOnSpawnAI += deathmatch.IncreaseTeamOneCount;
			ServerAIDie += deathmatch.DecreaseTeamOneCount;
		}
		else if(team == 1)
		{
			ServerOnSpawnAI += deathmatch.DrainTicketTwo;
			ServerOnSpawnAI += deathmatch.IncreaseTeamTwoCount;
			ServerAIDie += deathmatch.DecreaseTeamTwoCount;
		}

		CanSpawnAI = () => deathmatch.GetRemainingTickets(team) > 0;

		if (spawnManager != null)
		{
			spawnInitialAI();
		}
	}

	[Server]
	private void initSpawnManager()
	{
		if(((FPSNetworkManager)NetworkManager.singleton).spawnManager == null)
		{
			SpawnManager.OnManagerInitialized += spawnInitialAI;
		}
		else
		{
			spawnInitialAI();
		}
	}

	[Server]
	private void spawnInitialAI()
	{
		spawnManager = ((FPSNetworkManager)NetworkManager.singleton).spawnManager;
		if(deathmatch == null) { return; }

		for (int i = 0; i < AICount; i++)
		{
			StartCoroutine(spawnAIRoutine());
		}
		SpawnManager.OnManagerInitialized -= spawnInitialAI;
	}


	[Server]
	public void ServerSpawnAI()
	{
		StartCoroutine(spawnAIRoutine());
	}

	[Server]
	private void spawnAI()
	{
		if(CanSpawnAI == null || !CanSpawnAI.Invoke()) { return; }

		ServerOnSpawnAI?.Invoke();
		SpawnTransform spawnData = spawnManager.ServerGetRandomSpawnLocation(team);
		GameObject AIInstance = Instantiate(AIPrefab, spawnData.position, Quaternion.identity);
		AIInstance.transform.forward = spawnData.forwardDirection;

		if (AIInstance.TryGetComponent(out EnemyBehavior enemyBehavior))
		{
			enemyBehavior.SetManager(this);
			enemyBehavior.SetTeam(team);
			enemyBehavior.GetHealth().ServerOnDie += ServerAIDie;
		}

		NetworkServer.Spawn(AIInstance);
	}

	[Server]
	private IEnumerator spawnAIRoutine()
	{
		float spawnTime = UnityEngine.Random.Range(minTimeToSpawn, maxTimeToSpawn);

		yield return new WaitForSeconds(spawnTime);

		spawnAI();
	}

	public override void OnStopServer()
	{
		GamemodeManager.ServerOnManagerInitialized -= initDeathmatchManager;
		ServerOnSpawnAI = null;
	}
}
