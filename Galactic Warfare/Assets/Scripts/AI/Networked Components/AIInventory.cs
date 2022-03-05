using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIInventory : NetworkBehaviour
{
	[SerializeField] private LoadoutManager loadoutManager = null;
	[SerializeField] private Transform weaponSocket = null;
	[SerializeField] private Transform headSocket = null;
	[Tooltip("Collider to disable when shooting")]
	[SerializeField] private Collider playerHitCollider = null;

	private Loadout myLoadout;
	private WeaponManager currentWeapon = null;
	private NetworkManager nm;
	private Collider[] collidersToDisable = null;

	[SyncVar(hook = nameof(syncPrimaryIndex))]
	private int primaryIndex = -1;

	[HideInInspector] public int team;
	public Transform WeaponSocket { get { return weaponSocket; } }

	public override void OnStartServer()
	{
		base.OnStartServer();

		primaryIndex = loadoutManager.GetRandomLoadout(out myLoadout);
		SpawnEquipment(primaryIndex);
		SetupColliderArray();

		nm = NetworkManager.singleton;
	}

	[Server]
	private void SpawnEquipment(int loadoutIndex)
	{
		if (currentWeapon != null)
		{
			Destroy(currentWeapon.gameObject);
		}

		WeaponManager primary = Instantiate(loadoutManager.Loadouts[loadoutIndex].PrimaryWeapon, weaponSocket);
		primary.gameObject.SetActive(true);
		currentWeapon = primary;
	}

	[Client]
	private void syncPrimaryIndex(int oldIndex, int newIndex)
	{
		if(currentWeapon != null)
		{
			Destroy(currentWeapon.gameObject); 
		}

		WeaponManager primary = Instantiate(loadoutManager.Loadouts[newIndex].PrimaryWeapon, weaponSocket);
		primary.gameObject.SetActive(true);
		currentWeapon = primary;
	}

	public WeaponManager GetCurrentWeapon()
	{
		return currentWeapon;
	}

	[Server]
	public void FireWeaponInDirection(Vector3 direction)
	{
		if (nm == null)
		{
			nm = NetworkManager.singleton;
		}

		if(currentWeapon.TryFire(collidersToDisable))
		{
			GameObject prefab = currentWeapon.projectilePrefab;
			int projectileId = nm.spawnPrefabs.IndexOf(prefab);

			SpawnProjectile(projectileId, currentWeapon.ShootTransform.position, currentWeapon.ShootTransform.rotation, direction);

			currentWeapon.AlreadyShot = false;

			AISoundManager.RegisterSoundAtLocation(currentWeapon.ShootTransform.position, team);
		}
	}

	[Server]
	private void SpawnProjectile(int objectId, Vector3 spawnPosition, Quaternion spawnRotation, Vector3 forward)
	{
		GameObject projectileInstance = Instantiate(nm.spawnPrefabs[objectId], spawnPosition, spawnRotation);
		projectileInstance.transform.forward = forward;

		if(projectileInstance.TryGetComponent(out Projectile proj))
		{
			proj.team = team;
			proj.spawnedFromPlayer = netId;
		}
		NetworkServer.Spawn(projectileInstance);
		RpcPlayProjectileAudio();
	}

	[Server]
	public void SetMuzzleForward(Vector3 forward)
	{
		weaponSocket.forward = forward;
	}

	[ClientRpc]
	private void RpcPlayProjectileAudio()
	{
		WeaponManager weapon = GetCurrentWeapon();
		if(weapon == null)
		{
			Debug.LogError("Weapon should not be null");
			return;
		}
		weapon.AudioSource.PlayOneShot(weapon.FireSound);
	}

	private void SetupColliderArray()
	{
		List<Collider> colliders = new List<Collider>();
		GetCurrentWeapon().SetHead(headSocket);
		colliders.Add(playerHitCollider);
		foreach (Collider col in GetCurrentWeapon().Colliders)
		{
			colliders.Add(col);
		}
		collidersToDisable = colliders.ToArray();
	}
}
