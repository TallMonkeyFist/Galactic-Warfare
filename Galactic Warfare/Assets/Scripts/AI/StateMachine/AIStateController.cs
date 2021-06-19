using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIStateController : NetworkBehaviour
{
    public EnemyStats enemyStats;
    public AIState currentState = null;
    public AIState remainState = null;
    public NavMeshAgent navMeshAgent = null;
    public AIFlagControl flagController = null;
    public AISight sight = null;
    public AIHearing hearing = null;
    public AIInventory inventory;
    public EnemyBehavior behavior = null;

    [HideInInspector] public List<Transform> waypointList = null;
    [HideInInspector] public int nextWaypoint = -1;
    [HideInInspector] public float stateTimeElapsed = 0.0f;
    [HideInInspector] public bool flagSet = false;
    [HideInInspector] public Vector2 accuracy;
    [HideInInspector] public float fireAccuracy;
    [HideInInspector] public float patrolTime;
    [HideInInspector] public int team { get { return behavior.team; } }
    [HideInInspector] public Vector3 lastAudioPosition;

    private bool aiActive;

    #region Server

    public override void OnStartServer()
    {
        ServerSetupAI(true);
    }

    [Server]
    public void ServerSetupAI(bool aiActivationFromAIManager)
    {
        aiActive = aiActivationFromAIManager;

        if(aiActive)
        {
            navMeshAgent.enabled = true;
        }
        else
        {
            navMeshAgent.enabled = false;
        }

        sight.gameObject.SetActive(true);

        sight.SetFOV(enemyStats.FOV);
        sight.SetMaxRange(enemyStats.MaxEngagementRange);
        sight.SetRange(Random.Range(enemyStats.MinEngagementRange, enemyStats.MaxEngagementRange));
        sight.SetChaseTime(enemyStats.ChaseTime);

        hearing.gameObject.SetActive(true);



        navMeshAgent.speed = Random.Range(enemyStats.MinMoveSpeed, enemyStats.MaxMoveSpeed);

        accuracy = new Vector2(enemyStats.LookAccuracy, enemyStats.LookAccuracy);
        fireAccuracy = Random.Range(-enemyStats.FireAccuracy, enemyStats.FireAccuracy);

        if(flagController.TryGetAssignedPatrol(out PatrolPattern route))
        {
            waypointList = route.patrolPoints;
            nextWaypoint = UnityEngine.Random.Range(0, route.patrolPoints.Count);
        }
        else
        {
            flagController.OnFlagInitialized += SetupWaypoint;
        }

        patrolTime = Random.Range(enemyStats.MinPatrolTime, enemyStats.MaxPatrolTime);

        hearing.team = behavior.team;
        inventory.team = behavior.team;
    }

    [ServerCallback]
    private void FixedUpdate()
    {
        if(!aiActive) { return; }

        currentState.UpdateState(this);
    }

    [Server]
    public void TransitionToState(AIState nextState)
    {
        if(nextState != null && nextState != remainState)
        {
            currentState = nextState;
            OnExitState();
        }
    }

    [Server]
    private void SetupWaypoint()
    {
        if (flagController.TryGetAssignedPatrol(out PatrolPattern route))
        {
            waypointList = route.patrolPoints;
            nextWaypoint = UnityEngine.Random.Range(0, route.patrolPoints.Count);
        }

        flagController.OnFlagInitialized -= SetupWaypoint;
    }

    #endregion

    public bool CheckIfCountDownElapsed(float duration)
    {
        stateTimeElapsed += Time.fixedDeltaTime;
        return (stateTimeElapsed >= duration);
    }

    private void OnExitState()
    {
        if(NavMesh.SamplePosition(navMeshAgent.transform.position, out NavMeshHit hit, enemyStats.MaxTravelDistance, NavMesh.AllAreas))
        {
            navMeshAgent.SetDestination(hit.position);
        }

        navMeshAgent.updatePosition = true;
        navMeshAgent.updateUpAxis = true;
        navMeshAgent.updateRotation = true;

        stateTimeElapsed = 0;
        flagSet = false;
    }
}
