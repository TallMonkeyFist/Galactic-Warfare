namespace DataTypes
{
	using UnityEngine;

	[System.Serializable]
	public struct SpawnTransform
	{
		public static SpawnTransform invalidSpawn = new SpawnTransform(Vector3.positiveInfinity);

		public Vector3 position;
		public Vector3 forwardDirection;

		public SpawnTransform(Vector3 _position)
		{
			position = _position;
			forwardDirection = Vector3.forward;
		}
	}
}