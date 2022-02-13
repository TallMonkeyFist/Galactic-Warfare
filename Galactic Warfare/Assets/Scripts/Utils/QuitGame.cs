using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuitGame : MonoBehaviour
{
	public static void Quit()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
		}

	public static void MainMenu()
	{
		FPSNetworkManager manager = NetworkManager.singleton as FPSNetworkManager;
		manager.LeaveGame();
	}
}
