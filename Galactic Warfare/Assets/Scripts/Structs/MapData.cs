namespace DataTypes
{
	[System.Serializable]
	public struct MapData
	{
		public readonly TileData[] TileData;

		public MapData(TileData[] tiles)
		{
			TileData = tiles;
		}
	}
}