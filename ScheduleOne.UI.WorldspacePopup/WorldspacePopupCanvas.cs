using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.UI.WorldspacePopup;

public class WorldspacePopupCanvas : MonoBehaviour
{
	public const float WORLDSPACE_ICON_SCALE_MULTIPLIER = 0.4f;

	private const float HUDIconMaxOpacityAngle = 50f;

	private const float HUDIconMinOpacityAngle = 30f;

	[Header("References")]
	public RectTransform WorldspaceContainer;

	public RectTransform HudContainer;

	[Header("Prefabs")]
	public GameObject HudIconContainerPrefab;

	private List<WorldspacePopupUI> activeWorldspaceUIs = new List<WorldspacePopupUI>();

	private List<RectTransform> activeHUDUIs = new List<RectTransform>();

	private List<WorldspacePopup> popupsWithUI = new List<WorldspacePopup>();

	private void Update()
	{
		if (!PlayerSingleton<PlayerCamera>.InstanceExists)
		{
			return;
		}
		List<WorldspacePopup> list = new List<WorldspacePopup>();
		List<WorldspacePopup> list2 = new List<WorldspacePopup>();
		for (int i = 0; i < WorldspacePopup.ActivePopups.Count; i++)
		{
			if (!popupsWithUI.Contains(WorldspacePopup.ActivePopups[i]) && ShouldCreateUI(WorldspacePopup.ActivePopups[i]))
			{
				list.Add(WorldspacePopup.ActivePopups[i]);
			}
		}
		for (int j = 0; j < popupsWithUI.Count; j++)
		{
			if (!WorldspacePopup.ActivePopups.Contains(popupsWithUI[j]) || !ShouldCreateUI(popupsWithUI[j]))
			{
				list2.Add(popupsWithUI[j]);
			}
		}
		foreach (WorldspacePopup item in list)
		{
			CreateWorldspaceIcon(item);
			if (item.DisplayOnHUD)
			{
				CreateHUDIcon(item);
			}
		}
		foreach (WorldspacePopup item2 in list2)
		{
			DestroyWorldspaceIcon(item2);
			if (item2.DisplayOnHUD)
			{
				DestroyHUDIcon(item2);
			}
		}
	}

	private void LateUpdate()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		if (!PlayerSingleton<PlayerCamera>.InstanceExists)
		{
			return;
		}
		for (int i = 0; i < popupsWithUI.Count; i++)
		{
			if (((Component)PlayerSingleton<PlayerCamera>.Instance).transform.InverseTransformPoint(((Component)popupsWithUI[i]).transform.position).z > 0f)
			{
				Vector3 val = ((Component)popupsWithUI[i]).transform.position + popupsWithUI[i].WorldspaceOffset;
				Vector2 val2 = Vector2.op_Implicit(PlayerSingleton<PlayerCamera>.Instance.Camera.WorldToScreenPoint(val));
				float num = 1f;
				if (popupsWithUI[i].ScaleWithDistance)
				{
					float num2 = Vector3.Distance(val, ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position);
					num = 1f / Mathf.Sqrt(num2);
				}
				num *= popupsWithUI[i].SizeMultiplier;
				num *= 0.4f;
				((Transform)popupsWithUI[i].WorldspaceUI.Rect).position = Vector2.op_Implicit(val2);
				((Transform)popupsWithUI[i].WorldspaceUI.Rect).localScale = new Vector3(num, num, 1f);
				((Component)popupsWithUI[i].WorldspaceUI).gameObject.SetActive(true);
			}
			else
			{
				((Component)popupsWithUI[i].WorldspaceUI).gameObject.SetActive(false);
			}
		}
		for (int j = 0; j < popupsWithUI.Count; j++)
		{
			if ((Object)(object)popupsWithUI[j].HUDUI != (Object)null)
			{
				Vector3 val3 = Vector3.ProjectOnPlane(((Component)PlayerSingleton<PlayerCamera>.Instance).transform.forward, Vector3.up);
				Vector3 val4 = ((Component)popupsWithUI[j]).transform.position - ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position;
				float num3 = Vector3.SignedAngle(val3, ((Vector3)(ref val4)).normalized, Vector3.up);
				((Transform)popupsWithUI[j].HUDUI).localRotation = Quaternion.Euler(0f, 0f, 0f - num3);
				((Component)popupsWithUI[j].HUDUIIcon).transform.up = Vector3.up;
				float num4 = 1f;
				float num5 = Mathf.Abs(num3);
				if (num5 < 50f)
				{
					num4 = Mathf.Clamp01((num5 - 30f) / 20f);
				}
				popupsWithUI[j].HUDUICanvasGroup.alpha = Mathf.MoveTowards(popupsWithUI[j].HUDUICanvasGroup.alpha, num4, Time.unscaledDeltaTime * 5f);
			}
		}
	}

	private bool ShouldCreateUI(WorldspacePopup popup)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Distance(((Component)popup).transform.position, ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position) <= popup.Range;
	}

	private WorldspacePopupUI CreateWorldspaceIcon(WorldspacePopup popup)
	{
		WorldspacePopupUI worldspacePopupUI = popup.CreateUI(WorldspaceContainer);
		activeWorldspaceUIs.Add(worldspacePopupUI);
		popupsWithUI.Add(popup);
		popup.WorldspaceUI = worldspacePopupUI;
		return worldspacePopupUI;
	}

	private RectTransform CreateHUDIcon(WorldspacePopup popup)
	{
		RectTransform component = Object.Instantiate<GameObject>(HudIconContainerPrefab, (Transform)(object)HudContainer).GetComponent<RectTransform>();
		WorldspacePopupUI hUDUIIcon = popup.CreateUI(((Component)((Transform)component).Find("Container")).GetComponent<RectTransform>());
		popup.HUDUI = component;
		popup.HUDUIIcon = hUDUIIcon;
		popup.HUDUICanvasGroup = ((Component)component).GetComponent<CanvasGroup>();
		popup.HUDUICanvasGroup.alpha = 0f;
		activeHUDUIs.Add(component);
		return component;
	}

	private void DestroyWorldspaceIcon(WorldspacePopup popup)
	{
		for (int i = 0; i < activeWorldspaceUIs.Count; i++)
		{
			if ((Object)(object)activeWorldspaceUIs[i].Popup == (Object)(object)popup)
			{
				activeWorldspaceUIs[i].Destroy();
				activeWorldspaceUIs.RemoveAt(i);
				popupsWithUI.Remove(popup);
				break;
			}
		}
	}

	private void DestroyHUDIcon(WorldspacePopup popup)
	{
		for (int i = 0; i < activeHUDUIs.Count; i++)
		{
			if ((Object)(object)((Component)activeHUDUIs[i]).GetComponentInChildren<WorldspacePopupUI>().Popup == (Object)(object)popup)
			{
				((Component)activeHUDUIs[i]).GetComponentInChildren<WorldspacePopupUI>().Destroy();
				Object.Destroy((Object)(object)((Component)activeHUDUIs[i]).gameObject);
				activeHUDUIs.RemoveAt(i);
				break;
			}
		}
	}
}
