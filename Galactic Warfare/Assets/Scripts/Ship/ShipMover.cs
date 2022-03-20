using System;
using System.Collections.Generic;
using UnityEngine;

public class ShipMover : MonoBehaviour
{
	public static Action<ShipMover> OnShipSelected;
	public static ShipMover ActiveShip = null;
	public static List<ShipMover> Ships = new List<ShipMover>();

	public Ship Ship = null;
	public Renderer ShipRenderer;
	public Color SelectedColor;
	public Color TeamOneColor;
	public Color TeamTwoColor;
	private Color TeamColor;
	private bool HasMovedThisTurn = false;

	private void OnEnable()
	{
		Ships.Add(this);
		OnShipSelected += ShipSelected;
		GalaxyTile.OnTileSelected += MoveShip;
		TeamManager.OnEndTurn += (Team activeTeam) => { OnShipSelected?.Invoke(null); };
		TeamManager.OnStartTurn += (Team activeTeam) => { HasMovedThisTurn = false; };
	}

	public void SetShip(Ship ship, bool shipMoved = false)
	{
		HasMovedThisTurn = shipMoved;
		Ship = ship;
		TeamColor = Ship.TeamAffinity == 0 ? TeamOneColor : TeamTwoColor;
		ShipRenderer.material.color = TeamColor;
	}

	public static void ClearShip()
	{
		OnShipSelected?.Invoke(null);
	}

	private void OnMouseDown()
	{
		if (Ship.TeamAffinity != TeamManager.Instance.ActiveTeamIndex) { return; }
		if(HasMovedThisTurn) { return; }
		OnShipSelected?.Invoke(this);
	}

	private void ShipSelected(ShipMover ship)
	{
		ActiveShip = ship; 
		if(ship != this)
		{
			ShipRenderer.material.color = TeamColor;
		}
		else
		{
			ShipRenderer.material.color = SelectedColor;
		}
	}

	private void MoveShip(GalaxyTile previousTile, GalaxyTile newTile)
	{
		if(newTile == previousTile)
		{
			OnShipSelected?.Invoke(null);
			return;
		}
		if(ActiveShip != null && newTile != null && ActiveShip == this)
		{
			if(Ship.MoveShipTowardsTile(newTile))
			{
				OnShipSelected?.Invoke(null);
			}
		}
	}

	private void OnDisable()
	{
		TeamManager.OnStartTurn -= (Team activeTeam) => { HasMovedThisTurn = false; };
		TeamManager.OnEndTurn -= (Team activeTeam) => { OnShipSelected?.Invoke(null); };
		GalaxyTile.OnTileSelected -= MoveShip;
		OnShipSelected -= ShipSelected;
		Ships.Remove(this);
	}
}
