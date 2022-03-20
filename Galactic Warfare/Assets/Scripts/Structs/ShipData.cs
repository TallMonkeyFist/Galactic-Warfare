namespace DataTypes
{
	[System.Serializable]
	public struct ShipData
	{
		public string ShipName;
		public string OrbitingPlanet;
		public int TeamAffinity;

		public ShipData(ShipData other)
		{
			this = new ShipData(other.ShipName, other.OrbitingPlanet, other.TeamAffinity);
		}

		public ShipData(string name, string planet, int team)
		{
			ShipName = name;
			OrbitingPlanet = planet;
			TeamAffinity = team;
		}
	}
}