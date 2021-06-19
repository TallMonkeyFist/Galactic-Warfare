using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Weapon Profile to be applied to the weapon")]
    public WeaponProfile WeaponStats = null;
    [Tooltip("Transform for where the projectiles will spawn")]
    [SerializeField] private Transform muzzleTransform = null;
    [Tooltip("Layermask for shooting to the middle of the screen")]
    [SerializeField] private LayerMask shootMask = new LayerMask();
    [Tooltip("Audio source to play the sound to")]
    [SerializeField] private AudioSource audioSource = null;

    [HideInInspector] public float LastFireTime;
    [HideInInspector] public float FireDelay;
    [HideInInspector] public int ReserveAmmo;
    [HideInInspector] public int LoadedAmmo;

    private Camera playerCamera = null;
    private float radiansSpread = 0.0f;
    private bool reloading = false;

    private void Start()
    {
        playerCamera = Camera.main;
        radiansSpread = WeaponStats.BulletSpread * Mathf.PI / 180.0f;
        FireDelay = 1 / WeaponStats.FireRate;
    }

    public bool CanFire()
    {
        if(0 < LoadedAmmo && LastFireTime + FireDelay <= Time.time)
        {
            return true;
        }
        return false;
    }

    public void TryReload()
    {
        if (LoadedAmmo < WeaponStats.MagazineCapacity && ReserveAmmo > 0 && !reloading)
        {
            StartCoroutine(Reload());
        }
    }

    private IEnumerator Reload()
    {
        reloading = true;

        yield return new WaitForSeconds(WeaponStats.ReloadTime);

        int ammoToAdd = WeaponStats.MagazineCapacity - LoadedAmmo;
        ammoToAdd = Mathf.Min(ammoToAdd, ReserveAmmo);

        LoadedAmmo += ammoToAdd;
        ReserveAmmo -= ammoToAdd;
        reloading = false;
    }

    public Vector3 GetProjectileDirection()
    {
        Vector3 playerLookPos = playerCamera.transform.position + playerCamera.transform.forward * 1000.0f;

        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, 1000.0f, shootMask))
        {
            playerLookPos = playerCamera.transform.position + playerCamera.transform.forward * hit.distance;
        }

        Vector3 unclampedDirection = playerLookPos - muzzleTransform.position;

        Vector3 clampedDirection = Vector3.RotateTowards(muzzleTransform.up, unclampedDirection, radiansSpread, 0);

        return clampedDirection;
    }

    public Vector3 GetProjectileSpawnPosition()
    {
        return muzzleTransform.position;
    }
}
