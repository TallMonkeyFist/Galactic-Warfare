using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIFlagControl : NetworkBehaviour
{
	[SerializeField] private FlagManager flagManager = null;

	public int assignedFlagIndex { get; private set; } = -1;
	public PatrolPattern assignedPatrolPattern = null;

	public event Action OnFlagInitialized;

	public override void OnStartServer()
	{
		flagManager = FindObjectOfType<FlagManager>();

		assignedFlagIndex = UnityEngine.Random.Range(0, flagManager.FlagCount);

		OnFlagInitialized?.Invoke();
	}

	public bool TryGetAssignedFlag(out Flag flag)
	{
		flag = null;

		if(assignedFlagIndex == -1) { return false; }

		flag = flagManager.Flags[assignedFlagIndex];

		return true;
	}

	public bool TryGetAssignedPatrol(out PatrolPattern route)
	{
		route = null;

		if(assignedFlagIndex == -1) { return false; }

		route = flagManager.Flags[assignedFlagIndex].GetRandomPatrol();

		return route != null;
	}

	public void AssignRandomFlag(int team)
	{
		assignedFlagIndex = UnityEngine.Random.Range(0, flagManager.FlagCount);

		if(team == 1)
		{
			assignTeamOneFlag();
		}
		if(team == 2)
		{
			assignTeamTwoFlag();
		}
	}

	private void assignTeamOneFlag()
	{
		for(int i = 0; i < flagManager.FlagCount; i++)
		{
			if(100 <= flagManager.Flags[assignedFlagIndex].FlagValue)
			{
				assignedFlagIndex = (assignedFlagIndex + 1) % flagManager.FlagCount;
			}
			else
			{
				return;
			}
		}
	}

	private void assignTeamTwoFlag()
	{
		for (int i = 0; i < flagManager.FlagCount; i++)
		{
			if (flagManager.Flags[assignedFlagIndex].FlagValue <= -100)
			{
				assignedFlagIndex = (assignedFlagIndex + 1) % flagManager.FlagCount;
			}
			else
			{
				return;
			}
		}
	}
}
