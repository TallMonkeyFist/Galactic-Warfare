using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct PlayerLookData
{
	public Vector3 Forward;
	public Vector3 Up;
}

public class WeaponManager : MonoBehaviour
{

	[Header("References")]
	[Tooltip("Position to spawn the projectiles at")]
	[SerializeField] private Transform muzzleTransform = null;
	[Tooltip("Position of the back of the weapon")]
	[SerializeField] private Transform shoulderTransform = null;
	[Tooltip("Position for the players head")]
	[SerializeField] private Transform headTransform = null;
	[Tooltip("Layer Mask for shooting to the center of the screen")]
	[SerializeField] private LayerMask shootMask = new LayerMask();

	[Header("UI")]
	[Tooltip("Name of the weapon")]
	[SerializeField] private string weaponName = "Default Weapon";

	[Header("Ammo Settings")]
	[Tooltip("Projectile to spawn on fire")]
	[SerializeField] public GameObject projectilePrefab = null;
	[Tooltip("Round per Magazine")]
	[SerializeField] private int magazineCapacity = 10;
	[Tooltip("Max Ammo Capacity")]
	[SerializeField] private int maxAmmo = 60;
	[Tooltip("Fire rate (bullets/second)")]
	[SerializeField] private float fireRate = 0.5f;
	[Tooltip("Automatic Weapon")]
	[SerializeField] private bool autoWeapon = false;
	[Tooltip("Max angle of the bullet spread")]
	[SerializeField] private float bulletSpread = 2.0f;

	[Header("Reload")]
	[Tooltip("Time it takes to reload")]
	[SerializeField] private float reloadTime = 1.0f;

	[Header("Audio")]
	[Tooltip("Sound to play on fire")]
	[SerializeField] private AudioClip fireSound = null;
	[Tooltip("Sound to play on fire")]
	[SerializeField] private AudioClip reloadSound = null;
	[Tooltip("Sound to play on fire")]
	[SerializeField] private AudioClip drawWeaponSound = null;
	[Tooltip("Audio source to play the sound to")]
	[SerializeField] private AudioSource audioSource = null;

	[HideInInspector] public bool AlreadyShot;
	[HideInInspector] public AudioClip FireSound { get { return fireSound; } }
	[HideInInspector] public AudioClip ReloadSound { get { return reloadSound; } }
	[HideInInspector] public AudioClip DrawWeaponsSound { get { return drawWeaponSound; } }
	[HideInInspector]  public AudioSource AudioSource { get { return audioSource; } }

	public Transform ShootTransform { get { return muzzleTransform; } }

	public int m_MagazineAmmo;
	public int m_ReserveAmmo;
	private float m_FireDelay;
	private float m_LastFireTime;
	private float radiansSpread;

	private bool reloading;

	private Collider[] colliders;
	public Collider[] Colliders {get 
		{ 
			if(colliders == null)
			{
				InitColliders();
			}
			return colliders;
		} }

	private void Start()
	{
		reloading = false;
		m_FireDelay = 1 / fireRate;
		m_LastFireTime = 0;
		m_ReserveAmmo = maxAmmo - magazineCapacity;
		m_MagazineAmmo = magazineCapacity;
		radiansSpread = bulletSpread * Mathf.PI / 180.0f;
		InitColliders();
	}

	private void InitColliders()
	{
		Collider[] myColliders = GetComponents<Collider>();
		Collider[] childColliders = GetComponents<Collider>();
		colliders = new Collider[myColliders.Length + childColliders.Length];
		int index = 0;
		foreach(Collider col in myColliders)
		{
			Colliders[index++] = col;
			index++;
		}
		foreach(Collider col in childColliders)
		{
			Colliders[index++] = col;
			index++;
		}
	}

	private void OnEnable()
	{
		reloading = false;
	}

	public bool TryReload()
	{
		if(m_MagazineAmmo < magazineCapacity && m_ReserveAmmo > 0 && !reloading)
		{
			reloading = true;
			StartCoroutine(Reload());
			return true;
		}

		return false;
	}

	private IEnumerator Reload()
	{
		AudioSource.PlayOneShot(reloadSound);

		yield return new WaitForSeconds(reloadTime);

		int ammoToAdd = magazineCapacity - m_MagazineAmmo;
		ammoToAdd = Mathf.Min(ammoToAdd, m_ReserveAmmo);

		m_MagazineAmmo += ammoToAdd;
		m_ReserveAmmo -= ammoToAdd;
		reloading = false;
	}

	public bool TryFire(Collider[] colliders)
	{
		if(!autoWeapon && AlreadyShot || reloading)
		{
			return false;
		}
		if(!IsValidShot(colliders))
		{
			return false;
		}
		if(m_MagazineAmmo >= 1 && Time.time > m_LastFireTime + (m_FireDelay))
		{
			 return Fire();
		}
		else if(m_MagazineAmmo <= 0)
		{
			TryReload();
		}

		return false;
	}

	private bool IsValidShot(Collider[] colliders)
	{
		bool valid = true;
		foreach (Collider col in colliders)
		{
			col.enabled = false;
		}

		// Make sure that the weapon is not clipping through wall and on same side of wall as player head
		Vector3 dir = muzzleTransform.position - shoulderTransform.position;
		if (Physics.Raycast(shoulderTransform.position, dir, out RaycastHit hit, dir.magnitude, shootMask))
		{
			valid = false;
		}
		dir = muzzleTransform.position - headTransform.position;
		if (Physics.Raycast(headTransform.position, dir, out hit, dir.magnitude, shootMask))
		{
			valid = false;
		}

		foreach (Collider col in colliders)
		{
			col.enabled = true;
		}
		return valid;
	}

	private bool Fire()
	{
		AlreadyShot = true;
		m_LastFireTime = Time.time;
		m_MagazineAmmo--;

		return true;
	}

	public PlayerLookData GetProjectileDirection(Collider[] colliders)
	{
		foreach(Collider col in colliders)
		{
			col.enabled = false;
		}
		PlayerLookData data;

		Vector3 playerLookPos = headTransform.transform.position + headTransform.transform.forward * 1000.0f;

		if (Physics.Raycast(headTransform.transform.position, headTransform.transform.forward, out RaycastHit hit, 1000.0f, shootMask))
		{
			playerLookPos = headTransform.transform.position + headTransform.transform.forward * hit.distance;
		}

		Vector3 unclampedDirection = (playerLookPos - muzzleTransform.position).normalized;


		Vector3 clampedDirection = Vector3.RotateTowards(muzzleTransform.up, unclampedDirection, radiansSpread, 0);

		data.Up = headTransform.up;
		data.Forward = clampedDirection;

		foreach (Collider col in colliders)
		{
			col.enabled = true;
		}

		return data;
	}

	public void AddAmmo(int ammoAmount)
	{
		m_ReserveAmmo = Mathf.Max(m_ReserveAmmo + ammoAmount, 0, maxAmmo - magazineCapacity);
	}

	public void SetHead(Transform head)
	{
		headTransform = head;
	}

	public int GetCurrentAmmo()
	{
		return m_MagazineAmmo;
	}

	public int GetMaxAmmo()
	{
		return maxAmmo;
	}

	public int GetMagazineCapacity()
	{
		return magazineCapacity;
	}

	public int GetReserveAmmo()
	{
		return m_ReserveAmmo;
	}

	public string GetName()
	{
		return weaponName;
	}
}
