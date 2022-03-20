using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoController : MonoBehaviour
{
	public bool DrawAdjecentTileLines = true;
	public bool DrawAllTileLines = false;

	public void SetGizmoSettings()
	{
		GalaxyTile.DrawAdjecent = DrawAdjecentTileLines;
		GalaxyTile.DrawConnections = DrawAllTileLines;
	}	

	private void OnValidate()
	{
		SetGizmoSettings();
	}
}
