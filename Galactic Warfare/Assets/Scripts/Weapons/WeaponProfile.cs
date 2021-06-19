using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu (menuName = "Equipment/Weapon")]
public class WeaponProfile : ScriptableObject
{
    [Header("UI")]
    [Tooltip("Name of the weapon")]
    public string WeaponName = "Default Weapon";
    [Tooltip("Weapon icon in the UI")]
    public Image WeaponIcon = null;

    [Header("Ammo")]
    [Tooltip("Projectile to spawn on weapon fire")]
    public GameObject ProjectilePrefab = null;
    [Tooltip("Round per Magazine")]
    public int MagazineCapacity = 10;
    [Tooltip("Max Ammo Capacity")]
    public int MaxAmmo = 60;

    [Header("Shooting")]
    [Tooltip("Fire rate (bullets/second)")]
    public float FireRate = 0.5f;
    [Tooltip("Automatic Weapon")]
    public bool AutoWeapon = false;
    [Tooltip("Max angle of the bullet spread")]
    public float BulletSpread = 2.0f;

    [Header("Reload")]
    [Tooltip("Time it takes to reload")]
    public float ReloadTime = 1.0f;

    [Header("Audio")]
    [Tooltip("Sound to play on fire")]
    public AudioClip FireSound = null;
    [Tooltip("Volume the fire sound will be played at")]
    [Range(0, 1)]
    public float FireVolume = 1.0f;
}