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
	[Tooltip("Position for the players head")]
	[SerializeField] private Transform headTransform = null;
	[Tooltip("Layer Mask for shooting to the center of the screen")]
	[SerializeField] private LayerMask shootMask = new LayerMask();

	[Header("UI")]
	[Tooltip("Name of the weapon")]
	[SerializeField] private string weaponName = "Default Weapon";
	[Tooltip("Weapon icon in the UI")]
	[SerializeField] private Image weaponIcon = null;

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
	[Tooltip("Volume the fire sound will be played at")]
	[Range(0, 1)]
	[SerializeField] private float fireVolume = 1.0f;
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

	private void Start()
	{
		reloading = false;
		m_FireDelay = 1 / fireRate;
		m_LastFireTime = 0;
		m_ReserveAmmo = maxAmmo - magazineCapacity;
		m_MagazineAmmo = magazineCapacity;
		radiansSpread = bulletSpread * Mathf.PI / 180.0f;
	}

	private void OnEnable()
	{
		reloading = false;
	}

	public bool TryReload()
	{
		if(m_MagazineAmmo < magazineCapacity && m_ReserveAmmo > 0 && !reloading)
		{
			StartCoroutine(Reload());
			return true;
		}

		return false;
	}

	private IEnumerator Reload()
	{
		reloading = true;
		AudioSource.PlayOneShot(reloadSound);

		yield return new WaitForSeconds(reloadTime);

		int ammoToAdd = magazineCapacity - m_MagazineAmmo;
		ammoToAdd = Mathf.Min(ammoToAdd, m_ReserveAmmo);

		m_MagazineAmmo += ammoToAdd;
		m_ReserveAmmo -= ammoToAdd;
		reloading = false;
	}

	public bool TryFire()
	{
		if(!autoWeapon && AlreadyShot || reloading)
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

	private bool Fire()
	{
		AlreadyShot = true;
		m_LastFireTime = Time.time;
		m_MagazineAmmo--;

		return true;
	}

	public PlayerLookData GetProjectileDirection()
	{
		PlayerLookData data;

		Vector3 playerLookPos = headTransform.transform.position + headTransform.transform.forward * 1000.0f;

		if (Physics.Raycast(headTransform.transform.position, headTransform.transform.forward, out RaycastHit hit, 1000.0f, shootMask))
		{
			playerLookPos = headTransform.transform.position + headTransform.transform.forward * hit.distance;
		}

		Vector3 unclampedDirection = playerLookPos - muzzleTransform.position;

		Vector3 clampedDirection = Vector3.RotateTowards(muzzleTransform.up, unclampedDirection, radiansSpread, 0);

		data.Up = headTransform.up;
		data.Forward = clampedDirection;

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
