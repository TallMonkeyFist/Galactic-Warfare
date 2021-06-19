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
        updateTrackedPlayers();
    }

    private void updateTrackedPlayers()
    {
        for (int i = nearPlayers.Count - 1; i >= 0; i--)
        {
            if (nearPlayers[i] == null)
            {
                nearPlayers.RemoveAt(i);
            }
        }

        validTargets.Clear();

        foreach (Collider other in nearPlayers)
        {
            Vector3 targetDirection = (other.bounds.center - transform.position).normalized;

            if (Vector3.Angle(transform.forward, targetDirection) < FOV && (transform.position - other.transform.position).sqrMagnitude < sqrRangeDistance)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, targetDirection, out hit, maxEngagmentRange, sightMask))
                {
                    if (hit.collider == other && other.TryGetComponent(out Target target))
                    {
                        validTargets.Add(target);
                        targetSeen = true;
                    }
                }
            }
        }

        if (currentTarget != null && chaseTimer < timeBeforeTargetLost && !validTargets.Contains(currentTarget))
        {
            if (sqrMaxRange > (currentTarget.GetTargetPosition() - transform.position).sqrMagnitude)
            {
                validTargets.Insert(0, currentTarget);
                chaseTimer += Time.fixedDeltaTime;
                targetSeen = false;
            }
        }
        else
        {
            chaseTimer = 0;

        }

        Target newTarget = validTargets.Count > 0 ? validTargets[0] : null;

        if (currentTarget != newTarget)
        {
            ServerOnTargetChanged?.Invoke(newTarget);
            currentTarget = newTarget;
            chaseTimer = 0;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out Health health)) { return; }
        if(health.GetTeam() == aiTeam) { return; }

        nearPlayers.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
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
