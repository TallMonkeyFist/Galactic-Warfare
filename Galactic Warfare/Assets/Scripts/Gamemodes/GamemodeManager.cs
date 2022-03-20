using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GamemodeManager : NetworkBehaviour
{
	public Deathmatch deathmatch { get; private set; } = null;

	public bool GameRunning { get; private set; }
	public FPSNetworkManager NM;

	public static GamemodeManager singleton { get; private set; } = null;
	public static event Action ServerOnManagerInitialized;
	public static event Action ServerOnGameStart;
	public static event Action ServerOnGameEnd;

	[Server]
	public void StartDeathmatch()
	{
		ServerHandleGameStart();
		
		UI_Game.RestartGame += Restart;

		deathmatch = Deathmatch.singleton;
		if(deathmatch == null)
		{
			deathmatch = gameObject.AddComponent<Deathmatch>();
		}

		Debug.Log($"Player count {NM.Players.Count}");
		foreach (LobbyPlayer lp in NM.Players)
		{
			FPSPlayer player = lp.GamePlayer;
			ServerHandlePlayerStartGame(player);

			player.CanPlayerSpawn += () => deathmatch.GetRemainingTickets(lp.PlayerTeam) > 0;

			if (player.PlayerTeam == 0)
			{
				player.ServerOnSpawn += deathmatch.IncreaseTeamOneCount;
				player.ServerOnSpawn += deathmatch.DrainTicketOne;
				player.ServerOnDie += deathmatch.DecreaseTeamOneCount;
			}
			else if(player.PlayerTeam == 1)
			{
				player.ServerOnSpawn += deathmatch.IncreaseTeamTwoCount;
				player.ServerOnSpawn += deathmatch.DrainTicketTwo;
				player.ServerOnDie += deathmatch.DecreaseTeamTwoCount;
			}

			deathmatch.ServerOnTicketsChange += player.GameUI.TargetSetUITickets;
		}

		deathmatch.ServerOnGameOver += EndDeathmatch;

		deathmatch.StartGame(50, 50);
		ServerOnGameStart?.Invoke();
	}

	[Server]
	public void EndDeathmatch(int winningTeam)
	{
		ServerHandleGameEnd(winningTeam);
		foreach (LobbyPlayer lp in NM.Players)
		{
			FPSPlayer player = lp.GamePlayer;
			player.TargetDisableSpawnUI();
			bool teamOneWin = winningTeam == 0;
			if (player.PlayerTeam == 0)
			{
				player.ServerOnSpawn -= deathmatch.IncreaseTeamOneCount;
				player.ServerOnSpawn -= deathmatch.DrainTicketOne;
				player.ServerOnDie -= deathmatch.DecreaseTeamOneCount;

				ServerHandlePlayerEndGame(player, teamOneWin);
			}
			else if (player.PlayerTeam == 1)
			{
				player.ServerOnSpawn -= deathmatch.IncreaseTeamTwoCount;
				player.ServerOnSpawn -= deathmatch.DrainTicketTwo;
				player.ServerOnDie -= deathmatch.DecreaseTeamTwoCount;

				ServerHandlePlayerEndGame(player, !teamOneWin);
			}
			deathmatch.ServerOnTicketsChange -= player.GameUI.TargetSetUITickets;
		}
	}

	[Server]
	private void Restart()
	{
		UI_Game.RestartGame -= Restart;
		NM.ServerRestartScene();
	}

	[Server]
	private void ServerHandlePlayerStartGame(FPSPlayer player)
	{
		player.GameUI.isHost = player.OwningPlayer.IsHost;
		player.TargetHandleGameStart();
	}

	[Server]
	private void ServerHandlePlayerEndGame(FPSPlayer player, bool win)
	{
		player.TargetDisableSpawnUI();

		if (player.TryGetComponent(out PlayerInventoryManager inventory))
		{
			inventory.TargetDisableInput();
		}

		if (win)
		{
			player.GameUI.TargetDisplayWin();
		}
		else
		{
			player.GameUI.TargetDisplayLose();
		}
	}

	[Server]
	public void ServerHandlePlayerDisconnect(FPSPlayer player)
	{
		if (player.PlayerTeam == 0)
		{
			player.ServerOnSpawn -= deathmatch.IncreaseTeamOneCount;
			player.ServerOnSpawn -= deathmatch.DrainTicketOne;
			player.ServerOnDie -= deathmatch.DecreaseTeamOneCount;
		}
		else if (player.PlayerTeam == 1)
		{
			player.ServerOnSpawn -= deathmatch.IncreaseTeamTwoCount;
			player.ServerOnSpawn -= deathmatch.DrainTicketTwo;
			player.ServerOnDie -= deathmatch.DecreaseTeamTwoCount;
		}
		deathmatch.ServerOnTicketsChange -= player.GameUI.TargetSetUITickets;
	}

	[Server]
	private void ServerHandleGameStart()
	{
		singleton = this;
		NM = NetworkManager.singleton as FPSNetworkManager;
		ServerOnManagerInitialized?.Invoke();
		for(int i = 0; i < NM.Players.Count; i++)
		{
			NM.Players[i].ServerSpawnGamePlayer();
		}
	}

	[Server]
	private void ServerHandleGameEnd(int winningTeam)
	{
		ServerOnGameEnd?.Invoke();
		CheckGamemodeAchievmentProgress(winningTeam);
		deathmatch.ServerOnGameOver -= EndDeathmatch;
	}

	[Server]
	private void CheckGamemodeAchievmentProgress(int winningTeam)
	{
		if(!NM.useSteam) { return; }
		List<LobbyPlayer> players = NM.Players;
		LobbyPlayer player;
		for(int i = 0; i < players.Count; i++)
		{
			player = players[i];
			if(player.PlayerTeam == winningTeam)
			{
				player.TargetUnlockAchievement(AchievementName.WIN_1_GAME);
			}
			else
			{
				player.TargetUnlockAchievement(AchievementName.LOSE_1_GAME);
			}
		}
	}
}
