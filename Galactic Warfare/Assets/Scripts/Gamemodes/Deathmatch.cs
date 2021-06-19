using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deathmatch : NetworkBehaviour
{
    public static Deathmatch singleton { get; private set; }
    public static event Action OnDeathmatchStarted;

    [SerializeField] private int defaultTeamOneTickets = 30;
    [SerializeField] private int defaultTeamTwoTickets = 30;

    public int teamOneTickets { get; private set; }
    public int teamTwoTickets { get; private set; }

    public int teamOneAlivePlayers { get; private set; }
    public int teamTwoAlivePlayers { get; private set; }

    public int winningTeam { get; private set; }
    public bool gameOver { get; private set; }

    public event Action<int> ServerOnGameOver;
    public event Action<int, int> ServerOnTicketsChange;

    private int startTicketsTeamOne;
    private int startTicketsTeamTwo;

    #region Server

    [Server]
    public int GetRemainingTickets(int team)
    {
        switch(team)
        {
            case 1:
                return teamOneTickets;
            case 2:
                return teamTwoTickets;
            default:
                return -1;
        }
    }

    [Server]
    public void DrainTicketOne()
    {
        teamOneTickets = Mathf.Clamp(teamOneTickets - 1, 0, startTicketsTeamOne);
        ServerOnTicketsChange?.Invoke(teamOneTickets, teamTwoTickets);
        CheckGameOver();
    }

    [Server]
    public void DrainTicketTwo()
    {
        teamTwoTickets = Mathf.Clamp(teamTwoTickets - 1, 0, startTicketsTeamTwo);
        ServerOnTicketsChange?.Invoke(teamOneTickets, teamTwoTickets);
        CheckGameOver();
    }

    [Server]
    public void StartGame()
    {
        StartGame(defaultTeamOneTickets, defaultTeamTwoTickets);
    }

    [Server]
    public void StartGame(int _teamOneTickets, int _teamTwoTickets)
    {
        startTicketsTeamOne = _teamOneTickets;
        startTicketsTeamTwo = _teamTwoTickets;
        teamOneTickets = _teamOneTickets;
        teamTwoTickets = _teamTwoTickets;
        winningTeam = -1;
        gameOver = false;
        teamOneAlivePlayers = 0;
        teamTwoAlivePlayers = 0;

        ServerOnTicketsChange?.Invoke(teamOneTickets, teamTwoTickets);
        OnDeathmatchStarted?.Invoke();

        singleton = this;
    }

    [Server]
    public void DecreaseTeamOneCount()
    {
        teamOneAlivePlayers--;
        CheckGameOver();
    }

    [Server]
    public void DecreaseTeamTwoCount()
    {
        teamTwoAlivePlayers--;
        CheckGameOver();
    }

    [Server]
    public void IncreaseTeamOneCount()
    {
        teamOneAlivePlayers++;
    }

    [Server]
    public void IncreaseTeamTwoCount()
    {
        teamTwoAlivePlayers++;
    }

    [Server]
    public void CheckGameOver()
    {
        if(teamOneTickets <= 0 && teamOneAlivePlayers <= 0)
        {
            SetGameOver(2);
        }
        if(teamTwoTickets <= 0 && teamTwoAlivePlayers <= 0)
        {
            SetGameOver(1);
        }
    }

    [Server]
    private void SetGameOver(int team)
    {
        gameOver = false;
        switch(team)
        {
            case 1:
                winningTeam = 1;
                break;

            case 2:
                winningTeam = 2;
                break;

            default:
                winningTeam = 3;
                break;
        }
        ServerOnGameOver?.Invoke(winningTeam);
        singleton = null;
    }

    #endregion
}
