using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataTypes;

public class GalaxyMap : MonoBehaviour
{
	public GalaxyTile startTileTeam1;
	public GalaxyTile startTileTeam2;
	public Material lineMaterial;

	public static bool DisplayLogInfo = false;

	public static GalaxyMap Instance = null;
	public static event Action<GalaxyMap> LoadComplete;
	private List<GalaxyTile> tiles = new List<GalaxyTile>();

	private void Awake()
	{
		if(Instance == null)
		{
			Instance = this;
			LoadComplete?.Invoke(Instance);

		}
		else
		{
			Logger.Log("Galaxy Map already created! Deleting New One", DisplayLogInfo);
			Destroy(this);
		}
	}

	private void Start()
	{
		DrawLines();
	}

	private void OnDisable()
	{
		tiles.Clear();
	}

	public bool TryGetTile(string planetName, out GalaxyTile galaxyTile)
	{
		galaxyTile = null;
		foreach(GalaxyTile tile in tiles)
		{
			if(tile.profile.PlanetName == planetName)
			{
				galaxyTile = tile;
				return true;
			}
		}
		return false;
	}

	public void AddTile(GalaxyTile tile)
	{
		tiles.Add(tile);
	}

	public void RemoveTile(GalaxyTile tile)
	{
		tiles.Remove(tile);
	}

	public MapData GetMapData()
	{
		TileData[] tileData = new TileData[tiles.Count];
		for (int i = 0; i < tiles.Count; i++)
		{
			tileData[i] = tiles[i].GetSaveData();
		}
		MapData data = new MapData(tileData);
		return data;
	}

	public void LoadMapData(MapData data)
	{
		for(int i = 0; i < tiles.Count; i++)
		{
			GalaxyTile tile = tiles[i];
			for(int j = 0; j < data.TileData?.Length; j++)
			{
				TileData td = data.TileData[j];
				if(td.PlanetName == tile.profile.PlanetName)
				{
					tile.TeamAffinity = td.TeamAffinity;
				}
			}
		}
	}

	private void DrawLines()
	{
		GalaxyTile.DrawCount = 0;
		foreach(GalaxyTile tile in tiles)
		{
			tile.DrawLines(lineMaterial);
		}
		Logger.Log($"There are {GalaxyTile.DrawCount} lines between planets.", DisplayLogInfo);
	}
}
