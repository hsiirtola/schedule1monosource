using System.Collections;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.UI.WorldspacePopup;

public class WorldspacePopup : MonoBehaviour
{
	public static List<WorldspacePopup> ActivePopups = new List<WorldspacePopup>();

	[Range(0f, 1f)]
	public float CurrentFillLevel = 1f;

	[Header("Settings")]
	public WorldspacePopupUI UIPrefab;

	public bool DisplayOnHUD = true;

	public bool ScaleWithDistance = true;

	public Vector3 WorldspaceOffset;

	public float Range = 50f;

	public float SizeMultiplier = 1f;

	[HideInInspector]
	public WorldspacePopupUI WorldspaceUI;

	[HideInInspector]
	public RectTransform HUDUI;

	[HideInInspector]
	public WorldspacePopupUI HUDUIIcon;

	[HideInInspector]
	public CanvasGroup HUDUICanvasGroup;

	private List<WorldspacePopupUI> UIs = new List<WorldspacePopupUI>();

	private Coroutine popupCoroutine;

	private void OnEnable()
	{
		if (!ActivePopups.Contains(this))
		{
			ActivePopups.Add(this);
		}
		((Component)this).GetComponentInParent<CopyPosition>()?.UpdateEnabledState();
	}

	private void OnDisable()
	{
		ActivePopups.Remove(this);
		((Component)this).GetComponentInParent<CopyPosition>()?.UpdateEnabledState();
	}

	public WorldspacePopupUI CreateUI(RectTransform parent)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		WorldspacePopupUI newUI = Object.Instantiate<WorldspacePopupUI>(UIPrefab, (Transform)(object)parent);
		newUI.Popup = this;
		newUI.SetFill(CurrentFillLevel);
		UIs.Add(newUI);
		newUI.onDestroyed.AddListener((UnityAction)delegate
		{
			UIs.Remove(newUI);
		});
		return newUI;
	}

	private void LateUpdate()
	{
		foreach (WorldspacePopupUI uI in UIs)
		{
			uI.SetFill(CurrentFillLevel);
		}
	}

	public void Popup()
	{
		if (popupCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(popupCoroutine);
		}
		popupCoroutine = ((MonoBehaviour)this).StartCoroutine(PopupCoroutine());
		IEnumerator PopupCoroutine()
		{
			((Behaviour)this).enabled = true;
			SizeMultiplier = 0f;
			float lerpTime = 0.25f;
			for (float i = 0f; i < lerpTime; i += Time.deltaTime)
			{
				SizeMultiplier = i / lerpTime;
				yield return (object)new WaitForEndOfFrame();
			}
			SizeMultiplier = 1f;
			yield return (object)new WaitForSeconds(0.6f);
			((Behaviour)this).enabled = false;
			popupCoroutine = null;
		}
	}
}
