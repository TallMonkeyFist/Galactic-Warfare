using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyMenu : MonoBehaviour
{
	[Header("References")]
	[Tooltip("Startup panel")]
	[SerializeField] private GameObject lobbyUI = null;
	[Tooltip("Button to start the game")]
	[SerializeField] private Button startGameButton = null;
	[Tooltip("Map Names")]
	[SerializeField] private string[] levelNames = null;
	[Tooltip("Map Images")]
	[SerializeField] private Texture2D[] levelImages = null;
	[Tooltip("Map Display Name")]
	[SerializeField] private TMP_Text levelDisplayName = null;
	[Tooltip("Map Display Image")]
	[SerializeField] private RawImage levelDisplayImage = null;
	[Tooltip("Text elements to display player names")]
	[SerializeField] private TMP_Text[] playerNameTexts = null;
	[Tooltip("Image elements to display player avatars")]
	[SerializeField] private RawImage[] playerAvatarImages = null;
	[Tooltip("Level to start")]
	[SerializeField] private string level = "Map_01";

	private int activeLevelIndex = 0;

	private void OnEnable()
	{
		FPSNetworkManager.ClientOnConnected += HandleClientConnected;
		FPSPlayer.AuthorityOnPartyOwnerStateUpdated += AuthorityHandlePartyOwnerStateUpdated;
		FPSPlayer.ClientOnInfoUpdated += ClientHandleInfoUpdated;
	}

	private void OnDestroy()
	{
		FPSNetworkManager.ClientOnConnected -= HandleClientConnected;
		FPSPlayer.AuthorityOnPartyOwnerStateUpdated -= AuthorityHandlePartyOwnerStateUpdated;
		FPSPlayer.ClientOnInfoUpdated -= ClientHandleInfoUpdated;
	}

	private void HandleClientConnected()
	{
		lobbyUI.SetActive(true);
	}

	private void AuthorityHandlePartyOwnerStateUpdated(bool state)
	{
		startGameButton.gameObject.SetActive(state);
		levelDisplayName.gameObject.SetActive(state);
		levelDisplayImage.gameObject.SetActive(state);
	}

	private void ClientHandleInfoUpdated(List<FPSPlayer.PlayerDisplayInfo> players)
	{
		FPSNetworkManager manager = (FPSNetworkManager)NetworkManager.singleton;

		for(int i = 0; i < players.Count; i++)
		{
			playerNameTexts[i].text = players[i].Name;
			playerAvatarImages[i].texture = players[i].Avatar;
		}

		for(int i = players.Count; i < playerNameTexts.Length; i++)
		{
			playerNameTexts[i].text = "Waiting for Player...";
			playerAvatarImages[i].texture = manager.defaultImage;
		}

		startGameButton.interactable = players.Count >= 1;
	}

	public void StartGame()
	{
		if(NetworkClient.connection.identity.TryGetComponent(out FPSPlayer player))
		{
			player.CmdStartGame(level);
		}
	}

	public void LeaveLobby()
	{
		if(NetworkServer.active && NetworkClient.isConnected)
		{
			NetworkManager.singleton.StopHost();
		}
		else
		{
			NetworkManager.singleton.StopClient();

			SceneManager.LoadScene(0);
		}
	}

	public void SetNextLevel()
	{
		activeLevelIndex = activeLevelIndex + 1 >= levelNames.Length ? 0 : activeLevelIndex + 1;
		SetLevelInfo(activeLevelIndex);
	}

	public void SetPrevLevel()
	{
		activeLevelIndex = activeLevelIndex - 1 < 0 ? levelNames.Length - 1 : activeLevelIndex - 1;
		SetLevelInfo(activeLevelIndex);
	}

	public void SetLevelInfo(int levelIndex)
	{
		activeLevelIndex = levelIndex;
		levelDisplayName.text = levelNames[levelIndex];
		levelDisplayImage.texture = levelImages[levelIndex];
		level = levelNames[levelIndex];
	}
}
