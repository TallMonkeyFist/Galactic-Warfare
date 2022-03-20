namespace DataTypes
{
	using System;
	using System.Text;
	[Serializable]
	public struct SaveData
	{
		public string SaveFileName;

		public int CurrentTeam;
		public readonly DateTime SaveDate;
		public readonly TeamSaveData TeamOne;
		public readonly TeamSaveData TeamTwo;
		public readonly MapData MapData;

		public SaveData(string saveFileName)
		{
			this = new SaveData(saveFileName, new TeamSaveData("Team One", new string[0], new ShipData[0], 0), new TeamSaveData("Team Two", new string[0], new ShipData[0], 1), new MapData(), 0);
		}

		public SaveData(SaveData other)
		{
			this = new SaveData(other.SaveFileName, other.TeamOne, other.TeamTwo, other.MapData, other.CurrentTeam, other.SaveDate);
		}

		public SaveData(string saveFileName, TeamSaveData teamOne, TeamSaveData teamTwo, MapData mapData, int currentTeam)
		{
			SaveFileName = saveFileName;
			SaveDate = DateTime.Now;
			TeamOne = teamOne;
			TeamTwo = teamTwo;
			MapData = mapData;
			CurrentTeam = currentTeam;
		}

		public SaveData(string saveFileName, TeamSaveData teamOne, TeamSaveData teamTwo, MapData mapData, int currentTeam, DateTime time)
		{
			SaveFileName = saveFileName;
			SaveDate = time;
			TeamOne = teamOne;
			TeamTwo = teamTwo;
			MapData = mapData;
			CurrentTeam = currentTeam;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"{SaveFileName} {SaveDate}\n");
			sb.AppendLine(TeamOne.ToString() + '\n');
			sb.AppendLine(TeamTwo.ToString());
			return sb.ToString();

		}
	}
}


