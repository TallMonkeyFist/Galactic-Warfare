using Mirror;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Game : NetworkBehaviour
{
	[Header("UI References")]
	[Tooltip("Game UI canvas")]
	[SerializeField] private GameObject canvasGame = null;
	[Tooltip("Panel for displaying current game information")]
	[SerializeField] private GameObject ticketsPanel = null;
	[Tooltip("Panel for displaying game over information")]
	[SerializeField] private GameObject gameOverPanel = null;
	[Tooltip("Text for team one's remaining tickets")]
	[SerializeField] private TMP_Text teamOneTickets = null;
	[Tooltip("Text for team two's remaining tickets")]
	[SerializeField] private TMP_Text teamTwoTickets = null;
	[Tooltip("Text for game over information")]
	[SerializeField] private TMP_Text gameOverText = null;
	[Tooltip("Restart button for host")]
	[SerializeField] private Button restartButton = null;

	public RawImage TeamOneBackground = null;
	public RawImage TeamTwoBackground = null;

	public static event Action RestartGame;

	public bool isHost;


	[TargetRpc]
	public void TargetSetUITickets(int teamOne, int teamTwo)
	{
		teamOneTickets.text = teamOne.ToString();
		teamTwoTickets.text = teamTwo.ToString();
	}

	[TargetRpc]
	public void TargetDisplayWin()
	{
		gameOverPanel.SetActive(true);
		gameOverText.text = "Ahhh, Victory";

		if(isHost)
		{
			restartButton.enabled = true;
			restartButton.gameObject.SetActive(true);
		}
	}

	[TargetRpc]
	public void TargetDisplayLose()
	{
		gameOverPanel.SetActive(true);
		gameOverText.text = "Mission failed, we'll get 'em next time";

		if (isHost)
		{
			restartButton.enabled = true;
			restartButton.gameObject.SetActive(true);
		}
	}

	public void Reset(int teamOne, int teamTwo)
	{
		teamOneTickets.text = teamOne.ToString();
		teamTwoTickets.text = teamTwo.ToString();
		gameOverPanel.SetActive(false);
	}

	public void ClientEnableGameUI()
	{
		canvasGame.SetActive(true);
		ticketsPanel.SetActive(true);
		gameOverPanel.SetActive(false);
		restartButton.enabled = false;
		restartButton.gameObject.SetActive(false);
		gameOverText.text = "";
	}

	public void Disable()
	{
		canvasGame.SetActive(false);
		ticketsPanel.SetActive(false);
		gameOverPanel.SetActive(false);
		restartButton.enabled = false;
		restartButton.gameObject.SetActive(false);
	}

	public void ButtonClicked()
	{
		RestartGame?.Invoke();
	}

	[TargetRpc]
	public void TargetEnableGameUI()
	{
		ClientEnableGameUI();
	}

	[TargetRpc]
	public void TargetDisableGameUI()
	{
		Disable();
	}

	public override void OnStartClient()
	{
		base.OnStartClient();

		TeamOneBackground.color = (NetworkManager.singleton as FPSNetworkManager).TeamOneColor;
		TeamTwoBackground.color = (NetworkManager.singleton as FPSNetworkManager).TeamTwoColor;
	}
}
