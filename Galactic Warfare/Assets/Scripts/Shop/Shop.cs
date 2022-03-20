using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour
{
	[Header("Shop References")]
	public ShopItem[] itemToBuy = null;
	public Transform shopTransform = null;
	public Transform cameraLocation = null;
	public TeamManager teamManager = null;

	[Header("Shop Settings")]
	public float degreesPerItem;
	public float distanceFromOrigin;
	public bool inputEnabled = true;

	public int index = 0;
	protected Quaternion targetRotation;

	private float[] positionOffset;
	private float[] scaleOffset;
	private Quaternion startRotation;
	private Camera shopCam;

	protected void Start()
	{
		teamManager = TeamManager.Instance;
		Setup();
	}

	protected void Update()
	{
		if(Input.GetKeyDown(KeyCode.RightArrow))
		{
			SelectNext();
		}
		if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			SelectPrev();
		}
		Rotate();
		OffsetScale();

		shopCam.transform.position = Vector3.Lerp(shopCam.transform.position, cameraLocation.position, Time.deltaTime * 5);
		shopCam.transform.rotation = Quaternion.Slerp(shopCam.transform.rotation, cameraLocation.rotation, Time.deltaTime * 5);
	}

	public void SelectNext()
	{
		if(index < itemToBuy.Length - 1)
		{
			targetRotation = targetRotation * Quaternion.Euler(0, degreesPerItem, 0);
			positionOffset[index] = 0;
			scaleOffset[index] = 1;
			index++;
			positionOffset[index] = 1.5f;
			scaleOffset[index] = 1.5f;
			UpdateUI();
		}
	}

	public void SelectPrev()
	{
		if (index > 0)
		{
			targetRotation = targetRotation * Quaternion.Euler(0, -degreesPerItem, 0);
			positionOffset[index] = 0;
			scaleOffset[index] = 1;
			index--;
			positionOffset[index] = 1.5f;
			scaleOffset[index] = 1.5f;
			UpdateUI();
		}
	}

	public virtual void PurchaseItem()
	{
	}

	[ContextMenu("Setup")]
	public void Setup()
	{
		startRotation = shopTransform.rotation;
		float degrees = 0;
		index = 0;
		shopTransform.rotation = startRotation;

		for (int i = 0; i < itemToBuy.Length; i++)
		{
			itemToBuy[i].transform.localRotation = Quaternion.Euler(0, degrees + 180, 0);
			itemToBuy[i].transform.localPosition = itemToBuy[i].transform.forward * distanceFromOrigin;
			degrees -= degreesPerItem;
		}

		targetRotation = startRotation;

		shopCam = Camera.main;

		positionOffset = new float[itemToBuy.Length];
		scaleOffset = new float[itemToBuy.Length];

		positionOffset[0] = 1.5f;
		scaleOffset[0] = 1.5f;

		for(int i = 1; i < itemToBuy.Length; i++)
		{
			positionOffset[i] = 0;
			scaleOffset[i] = 1;
		}

		UpdateUI();
	}

	protected void Rotate()
	{
		shopTransform.rotation = Quaternion.Slerp(shopTransform.rotation, targetRotation, Time.deltaTime * 5);
	}

	protected void OffsetScale()
	{
		for (int i = 0; i < itemToBuy.Length; i++)
		{
			Vector3 currentPos = itemToBuy[i].transform.position;
			Vector3 targetPos = shopTransform.position + itemToBuy[i].transform.forward * (distanceFromOrigin + positionOffset[i]);
			Vector3 currentScale = itemToBuy[i].transform.localScale;
			Vector3 targetScale = new Vector3(scaleOffset[i], scaleOffset[i], scaleOffset[i]);
			itemToBuy[i].transform.position = Vector3.Lerp(currentPos, targetPos, Time.deltaTime * 5);
			itemToBuy[i].transform.localScale = Vector3.Lerp(currentScale, targetScale, Time.deltaTime * 5);
		}
	}

	protected virtual void UpdateUI() {}
}
