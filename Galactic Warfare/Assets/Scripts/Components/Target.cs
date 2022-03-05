using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
	[Header("References")]
	[Tooltip("Where AI will fire at")]
	[SerializeField] private Collider bodyCollider = null;

	public Vector3 GetTargetPosition()
	{
		return bodyCollider.bounds.center;
	}
}
