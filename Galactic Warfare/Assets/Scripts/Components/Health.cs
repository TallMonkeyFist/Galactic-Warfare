using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : NetworkBehaviour
{
	[Header("Refernces")]
	[Tooltip("Player Renderer for team color")]
	[SerializeField] private Renderer playerRenderer = null;

	[Header("Health Settings")]
	[Tooltip("Max health of the actor")]
	[SerializeField] private int maxHealth = 100;

	[Header("Team")]
	[Tooltip("Which team is the player associated with")]
	[SyncVar (hook = nameof(SyncTeam))]
	[SerializeField] private int team = -1;

	public event Action ServerOnSpawn;
	public event Action ServerOnDie;
	public event Action<float, float> ClientOnHealthChanged;

	[SyncVar (hook =nameof(HandleHealthChanged))]
	private float currentHealth;

	public int GetTeam()
	{
		return team;
	}

	#region Server

	public override void OnStartServer()
	{
		currentHealth = maxHealth;
	}

	[Server]
	public void DealDamage(float damageAmount, int team, uint id)
	{
		if (currentHealth == 0) { return; }

		if(this.team == team && id != netId) { return; }

		currentHealth = Mathf.Clamp(currentHealth - damageAmount, 0, maxHealth);

		ClientOnHealthChanged?.Invoke(0.0f, currentHealth);

		if (currentHealth != 0) { return; }

		ServerOnDie?.Invoke();
	}

	[Server]
	public void Heal(float health, bool overheal)
	{
		if(!overheal)
		{
			currentHealth = Mathf.Clamp(currentHealth + health, 0, maxHealth);
		}
		else
		{
			currentHealth += health;
		}
	}

	[Server]
	public void ServerSetTeam(int teamNumber)
	{
		team = teamNumber;
		ServerOnSpawn?.Invoke();
	}

	[Server]
	public void Kill()
	{
		currentHealth = 0;

		ClientOnHealthChanged?.Invoke(0.0f, currentHealth);

		ServerOnDie?.Invoke();
	}

	#endregion

	#region Client

	public override void OnStartClient()
	{
		base.OnStartClient();

		if (playerRenderer == null) { return; }

		if (team == 1)
		{
			playerRenderer.material.color = Color.red;
		}
		else if(team == 2)
		{
			playerRenderer.material.color = Color.blue;
		}
		else
		{
			playerRenderer.material.color = Color.yellow;
		}
	}

	private void HandleHealthChanged(float oldHealth, float newHealth)
	{
		ClientOnHealthChanged?.Invoke(oldHealth, newHealth);
	}

	public float GetHealth()
	{
		return currentHealth;
	}

	public float GetMaxHealth()
	{
		return maxHealth;
	}

	private void SyncTeam(int oldTeam, int newTeam)
	{
		if(playerRenderer == null) { return; }

		if (newTeam == 1)
		{
			playerRenderer.material.color = Color.red;
		}
		else if (newTeam == 2)
		{
			playerRenderer.material.color = Color.blue;
		}
		else
		{
			playerRenderer.material.color = Color.yellow;
		}
	}

	#endregion
}
