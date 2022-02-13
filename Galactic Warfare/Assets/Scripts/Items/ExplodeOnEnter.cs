using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodeOnEnter : NetworkBehaviour
{
	[Tooltip("Time it takes for the mine to arm")]
	[SerializeField] private float armTimer = 1.0f;
	private float timeAtArm;
	private int team;

	public override void OnStartServer()
	{
		timeAtArm = Time.time + armTimer;
		team = GetComponent<ExplosiveItem>().team;
	}

	[ServerCallback]
	private void OnTriggerEnter(Collider other)
	{
		if(Time.time <= timeAtArm) { return; }
		if(other.TryGetComponent(out Health health) && health.GetTeam() != team)
		{
			NetworkServer.Destroy(gameObject);
		}
	}


	[ServerCallback]
	private void OnTriggerExit(Collider other)
	{
		if (Time.time <= timeAtArm) { return; }
		if (other.TryGetComponent(out Health health) && health.GetTeam() != team)
		{
			NetworkServer.Destroy(gameObject);
		}
	}
}
