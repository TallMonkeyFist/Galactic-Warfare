namespace DataTypes
{
	using UnityEngine;

	[System.Serializable]
	public struct PlayerDisplayInfo
	{
		public string Name;
		public Texture2D Avatar;
		public uint ID;
		public ulong SteamID;
		public int Team;
	}
}