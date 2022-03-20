using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeployShipUI : MonoBehaviour
{
	public Transform DeployShipPanel = null;
	public Button DeployShipButton = null;
	public Button CancelButton = null;
	public Button ExitModeButton = null;
	public ShipButtonItem ShipButtonItemPrefab = null;
	public Transform GridTransform = null;
	public Color EnabledButtonColor = Color.white;
	public Color DisabledButtonColor = Color.gray;
	private TeamManager TeamManager = null;
	private bool DeployMode = false;
	private bool MoveMode = false;
	private Ship ShipToDeploy = null;

	private void Awake()
	{
		DeployShipButton.onClick.AddListener(DispatchButton);
		CancelButton.onClick.AddListener(() => { HideList(TeamManager.ActiveTeam); });
		SetButtonEnabled(null, ExitModeButton, false);
		TeamManager.OnFirstTeamInactiveShipAdded += (Team team) => { SetButtonEnabled(team, DeployShipButton, true); };
		TeamManager.OnTeamInactiveShipCleared += (Team team) => { SetButtonEnabled(team, DeployShipButton, false); };
		TeamManager.OnTeamInactiveShipCleared += HideList;
		ShipMover.OnShipSelected += SetCancelButton;
		TeamManager = TeamManager.Instance;
		if(TeamManager == null)
		{
			TeamManager.LoadComplete += OnTeamManagerLoaded;
		}
	}

	private void OnTeamManagerLoaded(TeamManager manger)
	{
		TeamManager = manger;
	}

	private void SetCancelButton(ShipMover shipMover)
	{
		if(DeployMode)
		{
			CancelDeployMode();
		}
		if(shipMover == null)
		{
			MoveMode = false;
			SetButtonEnabled(TeamManager.ActiveTeam, ExitModeButton, false);
			ExitModeButton.onClick.RemoveListener(ShipMover.ClearShip);
		}
		else
		{
			MoveMode = true;
			SetButtonEnabled(TeamManager.ActiveTeam, ExitModeButton, true);
			ExitModeButton.onClick.AddListener(ShipMover.ClearShip);
		}
	}

	private void SetButtonEnabled(Team team, Button button, bool enable)
	{
		if (team != null && team != TeamManager.ActiveTeam)
		{
			return;
		}
		ColorBlock colorBlock = button.colors;
		if(enable)
		{
			colorBlock.normalColor = EnabledButtonColor;
			button.interactable = true;
		}
		else
		{
			colorBlock.normalColor = DisabledButtonColor;
			button.interactable = false;
		}
		button.colors = colorBlock;
	}

	private void DispatchButton()
	{
		if(DeployMode)
		{
			DeployShip(ShipToDeploy);
		}
		else
		{
			DisplayList();
		}
	}

	private void DisplayList()
	{
		for (int i = GridTransform.childCount - 1; i >= 0; i--)
		{
			Destroy(GridTransform.GetChild(i).gameObject);
		}
		DeployShipPanel.gameObject.SetActive(true);
		List<Ship> inactiveShips = TeamManager.ActiveTeam.Ships.InactiveShips;
		foreach(Ship ship in inactiveShips)
		{
			ShipButtonItem shipButtonItem = Instantiate(ShipButtonItemPrefab, GridTransform);
			shipButtonItem.ShipText.text = ship.Profile.ShipName;
			shipButtonItem.DeployButton.onClick.AddListener(() => { SetDeployMode(ship); });
		}
	}

	private void HideList(Team team)
	{
		DeployShipPanel.gameObject.SetActive(false);
	}

	private void SetDeployMode(Ship ship)
	{
		ShipToDeploy = ship;
		DeployMode = true;
		HideList(TeamManager.ActiveTeam);
		SetButtonEnabled(TeamManager.ActiveTeam, ExitModeButton, true);
		ExitModeButton.onClick.AddListener(CancelDeployMode);
		if (MoveMode)
		{
			MoveMode = false;
			ExitModeButton.onClick.RemoveListener(ShipMover.ClearShip);
		}
	}

	private void CancelDeployMode()
	{
		DeployMode = false;
		SetButtonEnabled(TeamManager.ActiveTeam, ExitModeButton, false);
		ExitModeButton.onClick.RemoveListener(CancelDeployMode);
	}

	private void DeployShip(Ship ship)
	{
		if(TeamManager.DeployInactiveShip(ship, GalaxyTile.SelectedTile))
		{
			CancelDeployMode();
		}
	}
}
