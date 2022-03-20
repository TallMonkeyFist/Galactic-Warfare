namespace DataTypes
{
	[System.Serializable]
	public struct TileData
	{
		public string PlanetName;
		public int TeamAffinity;

		public TileData(TileData other)
		{
			this = new TileData(other.PlanetName, other.TeamAffinity);
		}

		public TileData(string planet, int team)
		{
			PlanetName = planet;
			TeamAffinity = team;
		}
	}
}