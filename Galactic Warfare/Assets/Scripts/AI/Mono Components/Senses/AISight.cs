using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISight : MonoBehaviour
{
	[SerializeField] private int aiTeam = 0;
	public List<Collider> nearPlayers = new List<Collider>();
	[SerializeField] private float autoEngagedRange = 2.0f;
	[SerializeField] private float FOV = 50.0f;
	[SerializeField] private float maxEngagmentRange = 20.0f;
	[SerializeField] private LayerMask sightMask = new LayerMask();

	public List<Target> validTargets = new List<Target>();

	public event Action<Target> ServerOnTargetChanged;

	private float timeBeforeTargetLost;
	private float sqrRangeDistance;
	private float sqrMaxRange;
	public Target currentTarget;
	public bool targetSeen;

	public float chaseTimer;

	private void Start()
	{
		sqrRangeDistance = autoEngagedRange * autoEngagedRange;
		sqrMaxRange = maxEngagmentRange * maxEngagmentRange;

		GetComponent<SphereCollider>().radius = maxEngagmentRange;
		chaseTimer = 0;
	}

	private void FixedUpdate()
	{
		UpdateTrackedPlayers();
	}

	private void UpdateTrackedPlayers()
	{
		UpdatePossibleTargets();
		UpdateCurrentTarget();
	}

	private void UpdatePossibleTargets()
	{
		RaycastHit hit;
		Collider col; 
		Vector3 targetDir;
		Target target;

		validTargets.Clear();
		for (int i = nearPlayers.Count - 1; i >= 0; i--)
		{
			col = nearPlayers[i];
			if(col == null) { nearPlayers.RemoveAt(i); continue; }
			targetDir = (col.bounds.center - transform.position);
			if(Vector3.Angle(transform.forward, targetDir) > FOV) { continue; }
			if((transform.position - col.transform.position).sqrMagnitude > sqrRangeDistance) { continue; }
			if(!Physics.Raycast(transform.position, targetDir.normalized, out hit, maxEngagmentRange, sightMask)) { continue; }
			if(hit.collider != col) { continue; }
			if(col.TryGetComponent(out target))
			{
				validTargets.Add(target);
				targetSeen = true;
			}
		}
	}

	private void UpdateCurrentTarget()
	{
		if(currentTarget == null) { chaseTimer = 0; }
		else if(validTargets.Contains(currentTarget)) { chaseTimer = 0; }
		else if(chaseTimer >= timeBeforeTargetLost) { chaseTimer = 0; }
		// Current Target is out of range, update chase timer
		else if(sqrMaxRange > (currentTarget.GetTargetPosition() - transform.position).sqrMagnitude)
		{
			validTargets.Add(currentTarget);
			chaseTimer += Time.fixedDeltaTime;
			targetSeen = false;
		}

		Target target = validTargets.Count > 0 ? validTargets[validTargets.Count - 1] : null;

		if(currentTarget != target)
		{
			ServerOnTargetChanged?.Invoke(target);
			currentTarget = target;
			chaseTimer = 0;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if(!other.TryGetComponent(out Target target)) { return; }
		if (!other.TryGetComponent(out Health health)) { return; }
		if(health.GetTeam() == aiTeam) { return; }
		if(nearPlayers.Contains(other)) { return; }

		nearPlayers.Add(other);
	}

	private void OnTriggerExit(Collider other)
	{
		if (!other.TryGetComponent(out Target target)) { return; }
		if (!other.TryGetComponent(out Health health)) { return; }
		if (health.GetTeam() == aiTeam) { return; }

		nearPlayers.Remove(other);
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.grey;
		Gizmos.DrawWireSphere(transform.position, maxEngagmentRange);
	}

	public void SetTeam(int team)
	{
		aiTeam = team;
	}

	public void SetFOV(float _fov)
	{
		FOV = _fov / 2.0f;
	}

	public void SetMaxRange(float _range)
	{
		maxEngagmentRange = _range;
	}

	public void SetRange(float _range)
	{
		autoEngagedRange = _range;
	}

	public void SetChaseTime(float _time)
	{
		timeBeforeTargetLost = _time;
	}
}
