using System.Collections.Generic;
using DataTypes;

[System.Serializable]
public class TeamShips
{
	public List<Ship> ActiveShips;
	public List<Ship> InactiveShips;

	public TeamShips()
	{
		ActiveShips = new List<Ship>();
		InactiveShips = new List<Ship>();
	}

	public static ShipData[] PrepareDataForSave(TeamShips teamShips)
	{
		ShipData[] data = new ShipData[teamShips.ActiveShips.Count + teamShips.InactiveShips.Count];
		int dataIndex = 0;
		for (int i = 0; i < teamShips.ActiveShips.Count; i++, dataIndex++)
		{
			data[dataIndex] = teamShips.ActiveShips[i].GetData();
		}
		for (int i = 0; i < teamShips.InactiveShips.Count; i++, dataIndex++)
		{
			data[dataIndex] = teamShips.InactiveShips[i].GetData();
		}
		return data;
	}

	public static TeamShips LoadDataFromSave(ShipData[] ships)
	{
		TeamShips teamShips = new TeamShips();
		if (ships != null)
		{
			foreach (ShipData ship in ships)
			{
				if (SaveDictionary.ShipLookupDictionary.TryGetValue(ship.ShipName, out ShipProfile profile))
				{
					if (ship.OrbitingPlanet == "Docked")
					{
						teamShips.InactiveShips.Add(new Ship(profile, null, ship.TeamAffinity));
					}
					else if (GalaxyMap.Instance.TryGetTile(ship.OrbitingPlanet, out GalaxyTile tile))
					{
						Ship loadedShip = new Ship(profile, tile, ship.TeamAffinity);
						teamShips.ActiveShips.Add(loadedShip);
						tile.AddShip(loadedShip);
					}
				}
			}
		}
		return teamShips;
	}
}
