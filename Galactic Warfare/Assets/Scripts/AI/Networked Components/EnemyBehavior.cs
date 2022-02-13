using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : NetworkBehaviour
{
	[SerializeField] private Health health = null;
	[SerializeField] private AIManager manager = null;
	[SerializeField] private AISight sight = null;

	public int team { get { return health.GetTeam(); } }

	[Server]
	public Health GetHealth()
	{
		return health;
	}

	public override void OnStartServer()
	{
		health.ServerOnDie += HandleDie;
	}

	[Server]
	private void HandleDie()
	{
		NetworkServer.Destroy(gameObject);
	}

	[Server]
	public void SetManager(AIManager _manager)
	{
		manager = _manager;
		health.ServerOnDie += manager.ServerSpawnAI;
	}

	[Server]
	public void SetTeam(int _team)
	{
		health.ServerSetTeam(_team);
		sight.SetTeam(_team);
	}
}
