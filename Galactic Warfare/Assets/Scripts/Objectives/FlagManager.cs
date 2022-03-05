using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagManager : NetworkBehaviour
{
	[SerializeField] private Flag[] flags = null;
	public bool initialized { get; private set; } = false;

	public Flag[] Flags
	{
		get
		{
			if(!initialized)
			{
				ForceInit();
			}
			return flags;
		}
	}

	public int FlagCount { get { return flags.Length; } }

	public static event Action OnFlagManagerInitialized;

	public override void OnStartServer()
	{
		ForceInit();
	}

	public override void OnStartClient()
	{
		if(isServer) { return; }

		initialized = true;
		OnFlagManagerInitialized?.Invoke();
	}

	private void ForceInit()
	{
		for (int i = 0; i < flags.Length; i++)
		{
			flags[i].SetFlagIndex(i);
		}
		initialized = true;
		OnFlagManagerInitialized?.Invoke();
	}
}
