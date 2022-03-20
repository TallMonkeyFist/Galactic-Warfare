using UnityEngine;

public static class Logger
{
	public static void Log(string msg, bool display = true)
	{
		if (display)
		{
			Debug.Log(msg);
		}
	}

	public static void Log(object msg, bool display = true)
	{
		Log(msg.ToString(), display);
	}

	public static void LogWarning(string msg, bool display = true)
	{
		if (display)
		{
			Debug.LogWarning(msg);
		}
	}

	public static void LogWarning(object msg, bool display = true)
	{
		LogWarning(msg.ToString(), display);
	}

	public static void LogError(string msg, bool display = true)
	{
		if (display)
		{
			Debug.LogError(msg);
		}
	}

	public static void LogError(object msg, bool display = true)
	{
		LogError(msg.ToString(), display);
	}
}
