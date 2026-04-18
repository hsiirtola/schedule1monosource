using System.Collections.Generic;
using System.Linq;
using ScheduleOne.DevUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScheduleOne.UI.Tooltips;

public class TooltipManager : Singleton<TooltipManager>
{
	[Header("References")]
	public Canvas Canvas;

	[SerializeField]
	private RectTransform anchor;

	[SerializeField]
	private TextMeshProUGUI tooltipLabel;

	private List<Canvas> canvases = new List<Canvas>();

	private List<Canvas> sortedCanvases = new List<Canvas>();

	private List<GraphicRaycaster> raycasters = new List<GraphicRaycaster>();

	private EventSystem eventSystem;

	private bool tooltipShownThisFrame;

	private PointerEventData pointerEventData;

	private List<RaycastResult> rayResults = new List<RaycastResult>();

	protected override void Awake()
	{
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		base.Awake();
		eventSystem = EventSystem.current;
		sortedCanvases = (from canvas in canvases
			where (Object)(object)((Component)canvas).GetComponent<GraphicRaycaster>() != (Object)null
			orderby canvas.sortingOrder, ((Component)canvas).transform.GetSiblingIndex()
			select canvas).ToList();
		for (int num = 0; num < sortedCanvases.Count; num++)
		{
			raycasters.Add(((Component)sortedCanvases[num]).GetComponent<GraphicRaycaster>());
		}
		pointerEventData = new PointerEventData(eventSystem);
	}

	protected virtual void Update()
	{
		CheckForTooltipHover();
	}

	protected virtual void LateUpdate()
	{
		if (!tooltipShownThisFrame)
		{
			((Component)anchor).gameObject.SetActive(false);
		}
		tooltipShownThisFrame = false;
	}

	public void AddCanvas(Canvas canvas)
	{
		if ((Object)(object)canvas == (Object)null)
		{
			Console.LogWarning("TooltipManager: AddCanvas called with null canvas");
		}
		else if (!canvases.Contains(canvas))
		{
			canvases.Add(canvas);
			sortedCanvases = (from c in canvases
				where (Object)(object)c != (Object)null && (Object)(object)((Component)c).GetComponent<GraphicRaycaster>() != (Object)null
				orderby c.sortingOrder, ((Component)c).transform.GetSiblingIndex()
				select c).ToList();
			raycasters.Clear();
			for (int num = 0; num < sortedCanvases.Count; num++)
			{
				raycasters.Add(((Component)sortedCanvases[num]).GetComponent<GraphicRaycaster>());
			}
		}
	}

	private void CheckForTooltipHover()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		pointerEventData.position = Vector2.op_Implicit(GameInput.MousePosition);
		for (int i = 0; i < sortedCanvases.Count; i++)
		{
			if ((Object)(object)sortedCanvases[i] == (Object)null || !((Behaviour)sortedCanvases[i]).enabled || !((Component)sortedCanvases[i]).gameObject.activeSelf)
			{
				continue;
			}
			rayResults = new List<RaycastResult>();
			((BaseRaycaster)raycasters[i]).Raycast(pointerEventData, rayResults);
			if (rayResults.Count > 0)
			{
				RaycastResult val = rayResults[0];
				Tooltip componentInParent = ((RaycastResult)(ref val)).gameObject.GetComponentInParent<Tooltip>();
				if ((Object)(object)componentInParent != (Object)null && ((Behaviour)componentInParent).enabled)
				{
					ShowTooltip(componentInParent.text, Vector2.op_Implicit(componentInParent.labelPosition), componentInParent.isWorldspace);
				}
				break;
			}
		}
	}

	public void ShowTooltip(string text, Vector2 position, bool worldspace)
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		if (text == string.Empty || string.IsNullOrWhiteSpace(text))
		{
			Console.LogWarning("ShowTooltip: text is empty");
			return;
		}
		tooltipShownThisFrame = true;
		string text2 = ((TMP_Text)tooltipLabel).text;
		((TMP_Text)tooltipLabel).text = text;
		if (text2 != text)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(anchor);
			((TMP_Text)tooltipLabel).ForceMeshUpdate(true, true);
		}
		anchor.sizeDelta = new Vector2(((TMP_Text)tooltipLabel).renderedWidth + 4f, ((TMP_Text)tooltipLabel).renderedHeight + 1f);
		((Transform)anchor).position = Vector2.op_Implicit(position + new Vector2(anchor.sizeDelta.x / 2f, (0f - anchor.sizeDelta.y) / 2f) * Canvas.scaleFactor);
		Vector2 anchoredPosition = anchor.anchoredPosition;
		float num = Singleton<HUD>.Instance.canvasRect.sizeDelta.x * -0.5f - anchor.sizeDelta.x * anchor.pivot.x * -1f;
		float num2 = Singleton<HUD>.Instance.canvasRect.sizeDelta.x * 0.5f - anchor.sizeDelta.x * (1f - anchor.pivot.x);
		float num3 = Singleton<HUD>.Instance.canvasRect.sizeDelta.y * -0.5f - anchor.sizeDelta.y * anchor.pivot.y * -1f;
		float num4 = Singleton<HUD>.Instance.canvasRect.sizeDelta.y * 0.5f - anchor.sizeDelta.y * (1f - anchor.pivot.y);
		anchoredPosition.x = Mathf.Clamp(anchoredPosition.x, num, num2);
		anchoredPosition.y = Mathf.Clamp(anchoredPosition.y, num3, num4);
		anchor.anchoredPosition = anchoredPosition;
		((Component)anchor).gameObject.SetActive(true);
	}
}
