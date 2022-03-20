using Mirror;
using System;
using System.Collections.Generic;
using DataTypes;

public class SpawnManager : NetworkBehaviour
{
	public List<SpawnSystem> spawnSystems = new List<SpawnSystem>();

	public static event Action OnManagerInitialized;

	public override void OnStartServer()
	{
		((FPSNetworkManager)NetworkManager.singleton).spawnManager = this;
		OnManagerInitialized?.Invoke();
	}

	public override void OnStartClient()
	{
		if(isServer) { return; }

		((FPSNetworkManager)NetworkManager.singleton).spawnManager = this;
		OnManagerInitialized?.Invoke();
	}

	[Server]
	public SpawnTransform ServerGetSpawnLocation(int index, int team)
	{
		if(index < 0 || index >= spawnSystems.Count) { return SpawnTransform.invalidSpawn; }

		if(spawnSystems[index].team != team) { return SpawnTransform.invalidSpawn; }

		return spawnSystems[index].GetSpawnLocation();
	}

	public SpawnTransform ServerGetSpawnLocation(int team)
	{
		for(int i = 0; i < spawnSystems.Count; i++)
		{
			if(spawnSystems[i].team == team)
			{
				return spawnSystems[i].GetSpawnLocation();
			}
		}
		return SpawnTransform.invalidSpawn;
	}

	public SpawnTransform ServerGetRandomSpawnLocation(int team)
	{
		int index = UnityEngine.Random.Range(0, spawnSystems.Count);

		for(int i = 0; i < spawnSystems.Count; i++)
		{
			if(spawnSystems[index].team == team)
			{
				return spawnSystems[index].GetSpawnLocation();
			}
			index = (index + 1) % spawnSystems.Count;
		}
		return SpawnTransform.invalidSpawn;
	}
}
