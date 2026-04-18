using ScheduleOne.DevUtilities;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Tooltips;

public class Tooltip : MonoBehaviour
{
	[Header("Settings")]
	[TextArea(3, 10)]
	public string text;

	public Vector2 labelOffset;

	public RectTransform LabelOriginRect;

	private Canvas canvas;

	public Vector3 labelPosition
	{
		get
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			if (isWorldspace)
			{
				return Vector2.op_Implicit(RectTransformUtility.WorldToScreenPoint(Singleton<GameplayMenu>.Instance.OverlayCamera, ((Transform)LabelOriginRect).position));
			}
			return ((Transform)LabelOriginRect).position + new Vector3(labelOffset.x, labelOffset.y, 0f);
		}
	}

	public bool isWorldspace { get; private set; }

	protected virtual void Awake()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Invalid comparison between Unknown and I4
		if ((Object)(object)LabelOriginRect == (Object)null)
		{
			LabelOriginRect = ((Component)this).GetComponent<RectTransform>();
		}
		if ((Object)(object)((Component)this).GetComponentInParent<GraphicRaycaster>() == (Object)null)
		{
			Console.LogWarning("Tooltip has not parent GraphicRaycaster! Tooltip won't ever be activated");
		}
		canvas = ((Component)this).GetComponentInParent<Canvas>();
		if ((Object)(object)canvas != (Object)null)
		{
			isWorldspace = (int)canvas.renderMode == 2;
		}
	}
}
