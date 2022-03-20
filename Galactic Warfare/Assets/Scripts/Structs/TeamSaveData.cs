namespace DataTypes
{
	using System.Text;

	[System.Serializable]
	public struct TeamSaveData
	{
		public readonly string TeamName;
		public readonly string[] Equipment;
		public readonly ShipData[] Ships;
		public readonly int TeamAffinity;

		public TeamSaveData(string teamName, string[] equipment, ShipData[] ships, int team)
		{
			Equipment = equipment;
			Ships = ships;
			TeamName = teamName;
			TeamAffinity = team;
		}

		public TeamSaveData(TeamSaveData other)
		{
			this = new TeamSaveData(other.TeamName, other.Equipment, other.Ships, other.TeamAffinity);
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append($"{TeamName} Equipment: ");
			foreach (string s in Equipment)
			{
				sb.Append($"[{s}] ");
			}
			sb.Append("\nShips: ");
			foreach (ShipData ship in Ships)
			{
				sb.Append($"[{ship.ShipName}: {ship.OrbitingPlanet}] ");
			}
			return sb.ToString();
		}
	}
}
