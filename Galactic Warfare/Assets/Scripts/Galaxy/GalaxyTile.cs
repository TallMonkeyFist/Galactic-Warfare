using System;
using System.Collections.Generic;
using UnityEngine;
using DataTypes;

public class GalaxyTile : MonoBehaviour
{
	public static GalaxyTile SelectedTile = null;
	public static Action<GalaxyTile, GalaxyTile> OnTileSelected;

	public PlanetProfile profile;
	public float radius;
	public List<GalaxyTile> adjacentTiles = new List<GalaxyTile>();
	public List<Ship> OrbitingShips = new List<Ship>();
	public int TeamAffinity = -1;
	public ShipSpawnLocation[] ShipSpawnLocations;

	private HashSet<GalaxyTile> drawnTiles = new HashSet<GalaxyTile>();
	private GalaxyMap map;
	private GalaxyCamera cameraView;

	//Helper to verify only one line is drawn between any two tiles
	public static int DrawCount = 0;
	public static bool DrawConnections = true;
	public static bool DrawAdjecent = false;

	private void OnEnable()
	{
		cameraView = FindObjectOfType<GalaxyCamera>();
		map = FindObjectOfType<GalaxyMap>();
		map.AddTile(this);
	}

	private void Awake()
	{
		CreateDefaultSpawnLocations();
	}

	private void OnDrawGizmos()
	{
		if(DrawConnections)
		{
			foreach (GalaxyTile tile in adjacentTiles)
			{
				Gizmos.color = Color.blue;
				Vector3 dir = tile.transform.position - transform.position;
				Gizmos.DrawLine(transform.position + dir.normalized * radius, tile.transform.position - dir.normalized * tile.radius);
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		if(DrawAdjecent)
		{
			foreach (GalaxyTile tile in adjacentTiles)
			{
				Gizmos.color = Color.red;
				Vector3 dir = tile.transform.position - transform.position;
				Gizmos.DrawLine(transform.position + dir.normalized * radius, tile.transform.position - dir.normalized * tile.radius);
			}
		}
	}

	public void DrawLines(Material mat)
	{
		foreach (LineRenderer renderer in GetComponents<LineRenderer>())
		{
			Destroy(renderer);
		}

		int lineCount = 0;
		for (int i = adjacentTiles.Count - 1; i >= 0; i--)
		{
			GalaxyTile tile = adjacentTiles[i];
			if(tile == this)
			{
				adjacentTiles.RemoveAt(i);
				continue;
			}
			if(!tile.drawnTiles.Contains(this))
			{
				GameObject go = new GameObject($"Line ({lineCount++})");
				go.transform.parent = transform;
				LineRenderer line = go.AddComponent<LineRenderer>();
				line.material = mat;
				line.startWidth = line.endWidth = 0.05f;
				line.numCapVertices = 8;

				Vector3 dir = tile.transform.position - transform.position;

				line.SetPosition(0, transform.position + dir.normalized * radius);
				line.SetPosition(1, tile.transform.position - dir.normalized * tile.radius);
				drawnTiles.Add(tile);
				DrawCount++;
			}
		}
	}

	public Transform GetShipTransform(int shipNumber)
	{
		Transform ship = null;
		int number = 0;
		for(int i = 0; i < transform.childCount; i++)
		{
			if(!transform.GetChild(i).TryGetComponent(out ShipMover mover)) { continue; }
			if(!mover.gameObject.activeSelf) { continue; }
			if(number == shipNumber)
			{
				ship = transform.GetChild(i);
			}
			number++;
		}
		return ship;
	}

	public int GetShipCount(int team)
	{
		int shipCount = 0;
		//There should only ever be four total ships in orbiting ships
		foreach(Ship ship in OrbitingShips)
		{
			if(ship.TeamAffinity == team)
			{
				shipCount++;
			}
		}
		return shipCount;
	}

	public bool ContainsShip(Ship ship)
	{
		return OrbitingShips.Contains(ship);
	}

	public bool AddShip(Ship ship, bool shipMoving = false)
	{
		if(GetShipCount(ship.TeamAffinity) < 2)
		{
			OrbitingShips.Add(ship);
			CreateShipAtTile(ship, this, shipMoving);
			return true;
		}
		return false;
	}

	public bool RemoveShip(Ship ship)
	{
		if(OrbitingShips.Contains(ship))
		{
			RemoveShipAtTile(ship, this);
			OrbitingShips.Remove(ship);
			return true;
		}
		return false;
	}

	public bool IsTileAdjacent(GalaxyTile tile)
	{
		return adjacentTiles.Contains(tile) || tile == this;
	}

	private static void CreateShipAtTile(Ship ship, GalaxyTile tile, bool shipMoved = false)
	{
		int team = ship.TeamAffinity;
		ship.OrbitTile = tile;
		GameObject shipInstance = Instantiate(ship.Profile.ShipModel, tile.transform);
		shipInstance.name = ship.Profile.ShipName;
		if (!shipInstance.TryGetComponent(out ShipMover mover))
		{
			Logger.LogWarning("Ship model prefab is invalid. Missing a ShipMover Script. Adding one now.");
			mover = shipInstance.AddComponent<ShipMover>();
		}
		mover.SetShip(ship, shipMoved);

		switch (team)
		{
			case 0:
				HandleSpawnShip(shipInstance, tile.ShipSpawnLocations[0]);
				break;
			case 1:
				HandleSpawnShip(shipInstance, tile.ShipSpawnLocations[1]);
				break;
			default:
				Logger.LogWarning($"Team {team} is not supported", TeamManager.DisplayLogInfo);
				break;
		}
	}

	private static void HandleSpawnShip(GameObject shipInstance, ShipSpawnLocation spawnLocations)
	{
		if(!spawnLocations.Location1Active)
		{
			shipInstance.transform.parent = spawnLocations.ShipLocation1;
			shipInstance.transform.localPosition = Vector3.zero;
			spawnLocations.Location1Active = true;
		}
		else if(!spawnLocations.Location2Active)
		{
			shipInstance.transform.parent = spawnLocations.ShipLocation2;
			shipInstance.transform.localPosition = Vector3.zero;
			spawnLocations.Location2Active = true;
		}
	}

	private static void RemoveShipAtTile(Ship ship, GalaxyTile tile)
	{
		int team = ship.TeamAffinity;
		GameObject shipToDestroy = null;
		switch (team)
		{
			case 0:
				shipToDestroy = FindShipToRemove(ship, tile.ShipSpawnLocations[0]);
				break;
			case 1:
				shipToDestroy = FindShipToRemove(ship, tile.ShipSpawnLocations[1]);
				break;
			default:
				Logger.LogWarning($"Team {team} is not supported", TeamManager.DisplayLogInfo);
				break;
		}
		shipToDestroy.transform.parent = tile.transform;
		shipToDestroy.SetActive(false);
		Destroy(shipToDestroy);
	}

	private static GameObject FindShipToRemove(Ship ship, ShipSpawnLocation spawnLocations)
	{
		GameObject shipToDestroy = null;
		if (spawnLocations.Location1Active && spawnLocations.ShipLocation1.Find(ship.Profile.ShipName) != null)
		{
			shipToDestroy = spawnLocations.ShipLocation1.Find(ship.Profile.ShipName).gameObject;
			spawnLocations.Location1Active = false;
		}
		else if (spawnLocations.Location2Active && spawnLocations.ShipLocation2.Find(ship.Profile.ShipName) != null)
		{
			shipToDestroy = spawnLocations.ShipLocation2.Find(ship.Profile.ShipName).gameObject;
			spawnLocations.Location2Active = false;
		}
		return shipToDestroy;
	}

	private void OnMouseDown()
	{
		cameraView.SelectTile(this);
		OnTileSelected?.Invoke(SelectedTile, this);
		SelectedTile = this;
	}

	private void CreateDefaultSpawnLocations()
	{
		ShipSpawnLocations = null;
		ShipSpawnLocations = new ShipSpawnLocation[2];

		ShipSpawnLocation teamOne = new ShipSpawnLocation();
		teamOne.TeamAffinity = 0;
		GameObject spawnLocation = new GameObject("Team One Ship One");
		spawnLocation.transform.parent = transform;
		spawnLocation.transform.position = transform.position + (Vector3.right + Vector3.forward).normalized * (radius + 0.5f);
		teamOne.ShipLocation1 = spawnLocation.transform;
		spawnLocation = new GameObject("Team One Ship Two");
		spawnLocation.transform.parent = transform;
		spawnLocation.transform.position = transform.position + (Vector3.right - Vector3.forward).normalized * (radius + 0.5f);
		teamOne.ShipLocation2 = spawnLocation.transform;

		ShipSpawnLocation teamTwo = new ShipSpawnLocation();
		teamOne.TeamAffinity = 1;
		spawnLocation = new GameObject("Team Two Ship One");
		spawnLocation.transform.parent = transform;
		spawnLocation.transform.position = transform.position - (Vector3.right - Vector3.forward).normalized * (radius + 0.5f);
		teamTwo.ShipLocation1 = spawnLocation.transform;
		spawnLocation = new GameObject("Team Two Ship Two");
		spawnLocation.transform.parent = transform;
		spawnLocation.transform.position = transform.position - (Vector3.right + Vector3.forward).normalized * (radius + 0.5f);
		teamTwo.ShipLocation2 = spawnLocation.transform;

		ShipSpawnLocations[0] = teamOne;
		ShipSpawnLocations[1] = teamTwo;
	}

	public TileData GetSaveData()
	{
		TileData data = new TileData(profile.PlanetName, TeamAffinity);
		return data;
	}
}

[System.Serializable]
public class ShipSpawnLocation
{
	public int TeamAffinity = -1;
	public Transform ShipLocation1 = null;
	public Transform ShipLocation2 = null;
	public bool Location1Active = false;
	public bool Location2Active = false;
}
