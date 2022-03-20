using UnityEngine;
using Mirror;

public class LogDisplaySettings : MonoBehaviour
{
	public static LogDisplaySettings Instance { get; private set; } = null;

	public bool DisplayNetworkManagerLog = true;
	public bool DisplayAILog = true;
	public bool DisplayFlagInfo = true;
	public bool DisplayTeamManagerLog = true;
	public bool DisplayGalaxyMapLog = true;
	public bool DisplaySaveDictionaryLog = true;
	public bool DisplaySaveLoaderLog = true;
	public bool DisplayLobbyPlayerLog = true;

	private void Awake()
	{
		if (Instance != null)
		{
			Destroy(this);
			return;
		}
		Instance = this;
		SetLogSettings();
		DontDestroyOnLoad(gameObject);
	}

	public void SetLogSettings()
	{
		FPSNetworkManager.DisplayLogInfo = DisplayNetworkManagerLog;
		AIAction.DisplayLogInfo = DisplayAILog;
		Flag.DisplayLogInfo= DisplayFlagInfo;
		TeamManager.DisplayLogInfo = DisplayTeamManagerLog;
		GalaxyMap.DisplayLogInfo = DisplayGalaxyMapLog;
		SaveDictionaryLoader.DisplayLog = DisplaySaveLoaderLog;
		SaveDictionary.DisplayLog = DisplaySaveDictionaryLog;
		LobbyPlayer.DisplayLogInfo = DisplayLobbyPlayerLog;
	}

	private void OnValidate()
	{
		SetLogSettings();
	}
}