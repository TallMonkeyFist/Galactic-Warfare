using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkUtilities
{
	private static NetworkUtilities instance = null;

	public static NetworkUtilities Instance
	{
		get
		{
			return instance == null ? instance = new NetworkUtilities() : instance;
		}
	}

	public float ServerAISyncTime = 1.0f / 30.0f;
}
