using System;
using System.Text;
using UnityEngine;
using DataTypes;

public class TeamManager : MonoBehaviour
{
	public static TeamManager Instance = null;
	public Team TeamOne;
	public Team TeamTwo;
	public string SaveFileName;

	public static bool DisplayLogInfo = false;

	public Team ActiveTeam { private set; get; }
	public int ActiveTeamIndex { private set; get; }

	public static Action<Team> OnStartTurn;
	public static Action<Team> OnFirstTeamInactiveShipAdded;
	public static Action<Team> OnTeamInactiveShipCleared;
	public static Action<Team> OnEndTurn;
	public static Action<TeamManager> LoadComplete;

	private void Awake()
	{
		if(Instance == null)
		{
			Instance = this;
			LoadComplete?.Invoke(this);
		}
		else
		{
			Logger.LogWarning("Team Manager already exist, deleting new manager.", DisplayLogInfo);
			Destroy(this);
		}
	}

	private void Start()
	{
		OnStartTurn += CheckForInactiveShipsEvent;
		CurrentSaveData currentSaveData = FindObjectOfType<CurrentSaveData>();
		SaveData data;
		if (currentSaveData)
		{
			data = currentSaveData.LoadGame();
			Logger.Log(data, DisplayLogInfo);
		}
		else
		{
			data = new SaveData("This should not occur in build. Please report this a bug");
		}
		LoadGame(data);
	}

	public void EndTurn()
	{
		OnEndTurn?.Invoke(ActiveTeam);
		ActiveTeam = ActiveTeam == TeamOne ? TeamTwo : TeamOne;
		ActiveTeamIndex = ActiveTeam == TeamOne ? 0 : 1;
		OnStartTurn?.Invoke(ActiveTeam);
	}

	public void AddInactiveShip(ShipProfile shipProfile)
	{
		ActiveTeam.Ships.InactiveShips.Add(new Ship(shipProfile, null, ActiveTeamIndex));
		if(ActiveTeam.Ships.InactiveShips.Count == 1)
		{
			OnFirstTeamInactiveShipAdded?.Invoke(ActiveTeam);
		}
	}

	public bool DeployInactiveShip(Ship ship, GalaxyTile tile)
	{
		if(tile == null || tile.TeamAffinity != ActiveTeam.TeamAffinity)
		{
			return false;
		}
		if(ship.PlaceShipAtTile(tile, true))
		{
			ActiveTeam.Ships.InactiveShips.Remove(ship);
			ActiveTeam.Ships.ActiveShips.Add(ship);
			if (ActiveTeam.Ships.InactiveShips.Count == 0)
			{
				OnTeamInactiveShipCleared?.Invoke(ActiveTeam);
			}
			return true;
		}
		return false;
	}

	[ContextMenu("Save Game")]
	private void SaveGame()
	{
		TeamSaveData teamOneData = TeamOne.RequestSaveData();
		TeamSaveData teamTwoData = TeamTwo.RequestSaveData();

		if(SaveFileName == "")
		{
			SaveFileName = SaveSystem.FormateDateTime(DateTime.Now);
		}

		SaveData dataToSave = new SaveData(SaveFileName, teamOneData, teamTwoData, GalaxyMap.Instance.GetMapData(), ActiveTeamIndex);
		SaveSystem.SaveData(dataToSave);

	}

	[ContextMenu("Load Game")]
	private void LoadGame()
	{
		if(SaveSystem.TryGetSaveData(SaveFileName, out SaveData saveData))
		{
			LoadGame(saveData);
		}
	}

	private void LoadGame(SaveData saveData)
	{
		GalaxyMap.Instance.LoadMapData(saveData.MapData);
		TeamOne = LoadTeam(saveData.TeamOne);
		TeamTwo = LoadTeam(saveData.TeamTwo);

		SaveFileName = saveData.SaveFileName;

		if (saveData.CurrentTeam == 0)
		{
			ActiveTeam = TeamOne;
		}
		else
		{
			ActiveTeam = TeamTwo;
		}
		ActiveTeamIndex = saveData.CurrentTeam;
		OnStartTurn?.Invoke(ActiveTeam);
	}

	private void CheckForInactiveShipsEvent(Team team)
	{
		if (team.Ships.InactiveShips.Count == 0)
		{
			OnTeamInactiveShipCleared?.Invoke(team);
		}
		else
		{
			OnFirstTeamInactiveShipAdded?.Invoke(team);
		}

	}

	private Team LoadTeam(TeamSaveData saveData)
	{
		TeamEquipment equipment = TeamEquipment.LoadDataFromSave(saveData.Equipment);
		TeamShips ships = TeamShips.LoadDataFromSave(saveData.Ships);
		Team team = new Team(saveData.TeamName, equipment, ships, saveData.TeamAffinity);
		return team;
	}
}