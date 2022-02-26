using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_FPS : MonoBehaviour
{
	[Header("Player References")]
	[Tooltip("Player's inventory manager for weapon references")]
	[SerializeField] private PlayerInventoryManager inventory = null;
	[Tooltip("Player's movement for stamina reference")]
	[SerializeField] private PlayerMovement movement = null;
	[Tooltip("Player's health for health reference")]
	[SerializeField] private Health health = null;

	[Header("UI References")]
	[Tooltip("Text for weapon name")]
	[SerializeField] private TMP_Text textWeaponName = null;
	[Tooltip("Text for weapon magazine ammo count")]
	[SerializeField] private TMP_Text textMagazineAmmo = null;
	[Tooltip("Text for weapon reserve ammo count")]
	[SerializeField] private TMP_Text textReserveAmmo = null;
	[Tooltip("Text for item name")]
	[SerializeField] private TMP_Text textItemName = null;
	[Tooltip("Text for item count")]
	[SerializeField] private TMP_Text textItemCount = null;
	[Tooltip("Image for player's current health")]
	[SerializeField] private Image imageHealth = null;
	[Tooltip("Image for player's current stamina")]
	[SerializeField] private Image imageStamina = null;

	private void Start()
	{
		health.ClientOnHealthChanged += HandleHealthChanged;
		movement.ClientOnStaminaChanged += HandleStaminaChanged;
		imageHealth.fillAmount = health.GetHealth() / health.GetMaxHealth();
		imageStamina.fillAmount = movement.GetStamina() / movement.GetMaxStamina();
	}

	private void Update()
	{
		WeaponManager weapon = inventory.GetCurrentWeapon();
		textWeaponName.text = weapon.GetName();
		textMagazineAmmo.text = $"{weapon.GetCurrentAmmo()}/{weapon.GetMagazineCapacity()}";
		textReserveAmmo.text = $"{weapon.GetReserveAmmo()}";

		ItemManager item = inventory.GetCurrenItem();
		textItemName.text = item.GetName();
		textItemCount.text = $"{item.GetCurrentCount()}/{item.GetMaxCount()}";
	}

	private void HandleHealthChanged(float oldHealth, float newHealth)
	{
		imageHealth.fillAmount = health.GetHealth() / health.GetMaxHealth();
	}

	private void HandleStaminaChanged(float stamina, float maxStamina)
	{
		imageStamina.fillAmount = stamina / maxStamina;
	}
}
