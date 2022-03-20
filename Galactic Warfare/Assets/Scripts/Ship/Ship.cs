using DataTypes;

[System.Serializable]
public class Ship
{
	public ShipProfile Profile = null;
	public GalaxyTile OrbitTile = null;
	public int TeamAffinity = -1;

	public Ship(ShipProfile profile, GalaxyTile tile, int team)
	{
		Profile = profile;
		OrbitTile = tile;
		TeamAffinity = team;
	}

	public bool PlaceShipAtTile(GalaxyTile tile, bool shipMoving = false)
	{
		if(tile == null) { return false; }
		if(tile.GetShipCount(TeamAffinity) >= 2 && OrbitTile != tile) { return false; }

		OrbitTile?.RemoveShip(this);
		OrbitTile = tile;
		OrbitTile?.AddShip(this, shipMoving);
		return true;
		
	}

	public bool MoveShipTowardsTile(GalaxyTile tile)
	{
		// If tile is not adjacent, don't move towards it
		if(!OrbitTile.IsTileAdjacent(tile)) { return false; }
		
		return PlaceShipAtTile(tile, true);
	}

	public ShipData GetData()
	{
		string orbitingPlanet = OrbitTile == null ? "Docked" : OrbitTile.profile.PlanetName;
		ShipData data = new ShipData(Profile.ShipName, orbitingPlanet, TeamAffinity);
		return data;
	}
}
