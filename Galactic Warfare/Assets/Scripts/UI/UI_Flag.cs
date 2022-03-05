using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Flag : MonoBehaviour
{
	public FlagManager flagManager = null;
	public GameObject UIParent = null;
	public GameObject FlagUIPrefab = null;
	public Image[] flagSprites;

	private void Awake()
	{
		if(flagManager != null && flagManager.initialized)
		{
			InitUI();
		}
		else
		{
			FlagManager.OnFlagManagerInitialized += InitUI;
		}
	}

	private void HandleFlagValueChanged(int flagIndex, Color color)
	{
		if(flagIndex < 0 || flagIndex >= flagManager.FlagCount) { return; }
		flagSprites[flagIndex].color = color;
	}

	private void InitUI()
	{
		flagSprites = new Image[flagManager.FlagCount];
		for (int i = 0; i < flagManager.FlagCount; i++)
		{
			flagSprites[i] = Instantiate(FlagUIPrefab, UIParent.transform).GetComponent<Image>();
		}

		Flag.ClientOnFlagValueChanged += HandleFlagValueChanged;
		FlagManager.OnFlagManagerInitialized -= InitUI;
	}

	private void OnDestroy()
	{
		Flag.ClientOnFlagValueChanged -= HandleFlagValueChanged;
		FlagManager.OnFlagManagerInitialized -= InitUI;
	}
}
