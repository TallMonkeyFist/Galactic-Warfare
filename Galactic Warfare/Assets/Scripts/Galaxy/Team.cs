using System.Text;
using System.Collections.Generic;
using DataTypes;

[System.Serializable]
public class Team
{
	public static List<Team> ActiveTeams = new List<Team>();

	public string TeamName = "DefaultTeam";

	public TeamEquipment Equipment;
	public TeamShips Ships;
	public int TeamAffinity = -1;

	public Team(string teamName, TeamEquipment equipment, TeamShips ships, int team)
	{
		TeamName = teamName;
		Equipment = equipment;
		Ships = ships;
		TeamAffinity = team;
		ActiveTeams.Add(this);
	}

	public TeamSaveData RequestSaveData()
	{
		TeamSaveData saveData = new TeamSaveData(TeamName, TeamEquipment.PrepareDataForSave(Equipment), TeamShips.PrepareDataForSave(Ships), TeamAffinity);
		return saveData;
	}
}