using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Projectile : NetworkBehaviour
{
	[Header("References")]
	[Tooltip("Rigidbody for moving the projectile")]
	[SerializeField] private Rigidbody rb = null;

	[Header("Projectile Settings")]
	[Tooltip("How fast the projectile will travel")]
	[SerializeField] public float speed = 50.0f;
	[Tooltip("The projectiles life span (seconds)")]
	[SerializeField] public float lifeSpan = 5.0f;
	[Tooltip("The damage that the projectile deals")]
	[SerializeField] public float projetileDamage = 50.0f;
	[Tooltip("Raycast layers to prevent clipping through object")]
	[SerializeField] public LayerMask shootMask = new LayerMask();

	[Header("Explosive")]
	[Tooltip("Does the projectile deal splash damage")]
	[SerializeField] public bool isExplosive = false;
	[Tooltip("Prefab to spawn during explosion")]
	[SerializeField] public GameObject explosionPrefab = null;
	[Tooltip("Outer falloff radius of the explosion")]
	[SerializeField] public float outerFalloffRange = 10.0f;
	[Tooltip("Inner falloff radius of the explosion")]
	[SerializeField] public float innerFalloffRange = 0.0f;
	[Tooltip("Maximum damage dealt")]
	[SerializeField] public float maxDamage = 100.0f;
	[Tooltip("Mimum damage dealt")]
	[SerializeField] public float minDamage = 50.0f;

	[HideInInspector] public int team = -1;
	[HideInInspector] public uint spawnedFromPlayer;

	private float damageDiff;
	private float inverseFalloff;

	private void FixedUpdate()
	{
		if (!isServer) { return; }

		ServerMove();
	}

	private void Update()
	{
		if (isServer) { return; }

		ClientMove();
	}

	private void Start()
	{
		damageDiff = maxDamage - minDamage;
		inverseFalloff = 1 / outerFalloffRange;
		DestoryAfterSeconds();
	}


	private void DestoryAfterSeconds()
	{
		Destroy(gameObject, lifeSpan);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (isServer)
		{
			ServerDestroy(other);
		}
		if (isClient)
		{
			ClientDestroy(other);
		}
	}

	#region Server

	[Server]
	private void ServerDestroy(Collider other)
	{
		if (!other.TryGetComponent(out Health health) && other.isTrigger) { return; }

		if (health != null)
		{
			health.DealDamage(projetileDamage, team, spawnedFromPlayer);
		}

		AISoundManager.RegisterSoundAtLocation(transform.position, team);

		ServerExplode();

		if (!isClient)
		{
			Destroy(gameObject);
		}
	}

	[Server]
	private void ServerExplode()
	{
		if (!isExplosive) { return; }

		Collider[] hits = Physics.OverlapSphere(transform.position, outerFalloffRange);
		List<Health> damagedHealth = new List<Health>();
		foreach (Collider c in hits)
		{
			if (!Physics.Raycast(transform.position, c.transform.position - transform.position, out RaycastHit hit, outerFalloffRange)) { continue; }
			if (!hit.collider.TryGetComponent(out Health healthComp)) { continue; }
			if (damagedHealth.Contains(healthComp)) continue;

			damagedHealth.Add(healthComp);
			float damage = maxDamage;
			float distance = hit.distance;
			if (distance > innerFalloffRange && distance <= outerFalloffRange)
			{
				damage = minDamage + ((outerFalloffRange - distance) * inverseFalloff) * damageDiff;
			}
			healthComp.DealDamage(damage, team, spawnedFromPlayer);
		}
	}

	[Server]
	private void ServerMove()
	{
		Vector3 newPosition = transform.forward * speed * Time.fixedDeltaTime + transform.position;
		if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, speed * Time.fixedDeltaTime, shootMask))
		{
			newPosition = hit.point;
		}
		rb.MovePosition(newPosition);
	}

	#endregion


	#region Client

	[Client]
	private void ClientMove()
	{
		Vector3 newPosition = transform.forward * speed * Time.fixedDeltaTime + transform.position;
		transform.position = newPosition;
	}

	[Client]
	private void ClientDestroy(Collider other)
	{
		if (!other.TryGetComponent(out Health health) && other.isTrigger) { return; }

		ClientExplode();
		Destroy(gameObject);
	}
	
	[Client]
	private void ClientExplode()
	{
		PlayExplodeVFX();
	}

	#endregion

	private void PlayExplodeVFX()
	{
		if (explosionPrefab != null && isExplosive)
		{
			GameObject explosionInstance = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
		}
	}
}
