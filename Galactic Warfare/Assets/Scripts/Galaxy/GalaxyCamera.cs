using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GalaxyCamera : MonoBehaviour
{
	public Transform galaxyCamera = null;
	public Transform targetRotation = null;
	public TMP_Text nameText = null;
	public TMP_Text descText = null;

	public float width = 10;
	public float height = 10;
	public float maxZoom = 10;
	public float cameraSpeed = 1;

	private float zoom = 3;
	private float x = 0;
	private float y = 0;
	private float z = 0;

	private void Update()
	{
		UpdateValues();

		Vector3 targetPos = new Vector3(this.x, this.y, this.z) - targetRotation.forward * zoom;
		galaxyCamera.position = Vector3.Lerp(galaxyCamera.position, targetPos, 5 * Time.deltaTime);
		galaxyCamera.rotation = Quaternion.Slerp(galaxyCamera.rotation,
			targetRotation.rotation, Time.deltaTime * 5);
	}

	public void SelectTile(GalaxyTile tile)
	{
		Vector3 pos = tile.transform.position;
		this.x = pos.x;
		this.y = pos.y;
		this.z = pos.z;
		nameText.text = $"{tile.profile.PlanetName}";
		descText.text = $"Victory Bonus: {tile.profile.VictoryUnits}\n" +
			$"Planet Bonus: {tile.profile.BonusUnits}";
		zoom = 5;
	}

	public void DeselectTile()
	{
		this.y = 0;
		nameText.text = $"";
		descText.text = $"Victory Bonus:";
	}

	private void UpdateValues()
	{
		float x = Input.GetAxis("Horizontal");
		float y = Input.GetAxis("Zoom");
		float z = Input.GetAxis("Vertical");

		Vector3 pos = new Vector3(this.x, 0, this.z);

		this.x = Mathf.Clamp(this.x + x * Time.deltaTime * cameraSpeed, -width, width);
		this.z = Mathf.Clamp(this.z + z * Time.deltaTime * cameraSpeed, -height, height);
		pos.x = this.x;
		pos.z = this.z;

		zoom = Mathf.Clamp(zoom - y * Time.deltaTime * cameraSpeed, 3, maxZoom - this.y);
	}
}
