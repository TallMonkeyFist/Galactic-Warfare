using UnityEngine;
using DataTypes;

public class SpawnSystem : MonoBehaviour
{
	[Header("Spawn Settings")]
	[Tooltip("Which team the spawn system belongs to")]
	[SerializeField] public int team = -1;
	[Tooltip("Name of the spawn location")]
	[SerializeField] private string spawnLocationName = "Default Spawn Location";
	[Tooltip("Does the player spawn facing the spawn direction")]
	[SerializeField] private bool useSpawnDirection = false;
	[Tooltip("The spawn direction of the player")]
	[SerializeField] private Vector2 spawnDirection = Vector2.up;

	public string SpawnLocationName { get { return spawnLocationName; } }

	public SpawnTransform GetSpawnLocation()
	{
		SpawnTransform spawnTransform = new SpawnTransform();

		float xOffset = Random.Range(-transform.localScale.x / 2.0f, transform.localScale.x / 2.0f);
		float yOffset = Random.Range(-transform.localScale.y / 2.0f, transform.localScale.y / 2.0f);
		float zOffset = Random.Range(-transform.localScale.z / 2.0f, transform.localScale.z / 2.0f);

		spawnTransform.position = transform.position + new Vector3(xOffset, yOffset, zOffset);

		if(useSpawnDirection)
		{
			spawnTransform.forwardDirection = new Vector3(spawnDirection.x, 0.0f, spawnDirection.y).normalized;
		}
		else
		{
			float randomX = Random.Range(-1.0f, 1.0f);
			float randomY = Random.Range(-1.0f, 1.0f);

			spawnTransform.forwardDirection = new Vector3(randomX, 0.0f, randomY).normalized;
		}

		return spawnTransform;
	}

	public void OnDrawGizmos()
	{
		if (useSpawnDirection)
		{
			Gizmos.color = new Color(0.0f, 1.0f, 0.0f, 0.3f);
			Gizmos.DrawRay(transform.position, new Vector3(spawnDirection.x, 0.0f, spawnDirection.y).normalized * 5.0f);
		}

		Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.3f);
		Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
		Gizmos.DrawCube(Vector3.zero, Vector3.one);
	}
}
