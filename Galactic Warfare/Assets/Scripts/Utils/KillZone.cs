using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillZone : NetworkBehaviour
{
	[ServerCallback]
	private void OnTriggerEnter(Collider other)
	{
		if(other.TryGetComponent(out Health health))
		{
			health.Kill();
		}
	}

	public void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color(1.0f, 0.3f, 0.3f, 0.8f);
		Gizmos.DrawCube(transform.position, transform.localScale);
	}
}
