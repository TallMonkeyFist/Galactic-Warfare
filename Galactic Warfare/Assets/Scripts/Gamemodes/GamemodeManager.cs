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

    public static GamemodeManager singleton { get; private set; } = null;
    public static bool initialized { get; private set; } = false;
    public static event Action ServerOnManagerInitialized;

    [Server]
    public void StartDeathmatch()
    {
        UI_Game.RestartGame += Restart;

        if(deathmatch == null)
        {
            deathmatch = gameObject.AddComponent<Deathmatch>();
        }

        FPSNetworkManager networkManager = (FPSNetworkManager)NetworkManager.singleton;
        foreach(FPSPlayer player in networkManager.Players)
        {
            ServerHandlePlayerStartGame(player);

            player.CanSpawnPlayer += () => deathmatch.GetRemainingTickets(player.PlayerTeam) > 0;

            if (player.PlayerTeam == 1)
            {
                player.ServerOnSpawn += deathmatch.IncreaseTeamOneCount;
                player.ServerOnSpawn += deathmatch.DrainTicketOne;
                player.ServerOnDie += deathmatch.DecreaseTeamOneCount;
            }
            else if(player.PlayerTeam == 2)
            {
                player.ServerOnSpawn += deathmatch.IncreaseTeamTwoCount;
                player.ServerOnSpawn += deathmatch.DrainTicketTwo;
                player.ServerOnDie += deathmatch.DecreaseTeamTwoCount;
            }

            deathmatch.ServerOnTicketsChange += player.GameUI.TargetSetUITickets;
        }

        deathmatch.ServerOnGameOver += EndDeathmatch;

        deathmatch.StartGame(150, 150);

        ServerHandleGameStart();
    }

    [Server]
    public void EndDeathmatch(int winningTeam)
    {
        ServerHandleGameEnd();
        FPSNetworkManager networkManager = (FPSNetworkManager)NetworkManager.singleton;
        foreach (FPSPlayer player in networkManager.Players)
        {
            player.TargetDisableSpawnUI();
            bool teamOneWin = winningTeam == 1;
            if (player.PlayerTeam == 1)
            {
                player.ServerOnSpawn -= deathmatch.IncreaseTeamOneCount;
                player.ServerOnSpawn -= deathmatch.DrainTicketOne;
                player.ServerOnDie -= deathmatch.DecreaseTeamOneCount;

                ServerHandlePlayerEndGame(player, teamOneWin);
            }
            else if (player.PlayerTeam == 2)
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
        ((FPSNetworkManager)NetworkManager.singleton).RestartScene();
    }

    [Server]
    private void ServerHandlePlayerStartGame(FPSPlayer player)
    {
        player.TargetEnableSpawnUI();

        player.GameUI.TargetEnableGameUI();

        player.TargetResetSpawnManager();

        if (player.TryGetComponent(out PlayerInventoryManager inventory))
        {
            inventory.TargetEnableInput();
        }

        player.GameUI.isHost = player.IsHost;
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
        if (player.PlayerTeam == 1)
        {
            player.ServerOnSpawn -= deathmatch.IncreaseTeamOneCount;
            player.ServerOnSpawn -= deathmatch.DrainTicketOne;
            player.ServerOnDie -= deathmatch.DecreaseTeamOneCount;
        }
        else if (player.PlayerTeam == 2)
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
        initialized = true;
        singleton = this;
        ServerOnManagerInitialized?.Invoke();
    }

    [Server]
    private void ServerHandleGameEnd()
    {
        initialized = false;
        singleton = null;
    }
}
