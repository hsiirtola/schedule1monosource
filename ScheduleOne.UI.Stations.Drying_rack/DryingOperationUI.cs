using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.ObjectScripts;
using ScheduleOne.UI.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Stations.Drying_rack;

public class DryingOperationUI : MonoBehaviour
{
	[Header("References")]
	public RectTransform Rect;

	public Image Icon;

	public TextMeshProUGUI QuantityLabel;

	public Button Button;

	public Tooltip Tooltip;

	private float _dryMultiplier = 1f;

	public DryingOperation AssignedOperation { get; protected set; }

	public RectTransform Alignment { get; private set; }

	public void SetOperation(DryingOperation operation)
	{
		AssignedOperation = operation;
		Icon.sprite = ((BaseItemDefinition)Registry.GetItem(operation.ItemID)).Icon;
		RefreshQuantity();
		UpdatePosition();
	}

	public void SetAlignment(RectTransform alignment)
	{
		Alignment = alignment;
		((Component)this).transform.SetParent((Transform)(object)alignment);
		UpdatePosition();
	}

	public void RefreshQuantity()
	{
		((TMP_Text)QuantityLabel).text = AssignedOperation.Quantity + "x";
	}

	public void Start()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		((UnityEvent)Button.onClick).AddListener((UnityAction)delegate
		{
			Clicked();
		});
	}

	public void SetDryRate(float dryMultiplier)
	{
		_dryMultiplier = dryMultiplier;
	}

	public void UpdatePosition()
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Clamp01(AssignedOperation.Time / 720f);
		int num2 = (int)((float)(int)Mathf.Clamp(720f - AssignedOperation.Time, 0f, 720f) / _dryMultiplier);
		int num3 = num2 / 60;
		int num4 = num2 % 60;
		Tooltip.text = num3 + "h " + num4 + "m until next tier";
		float num5 = -62.5f;
		float num6 = 0f - num5;
		Rect.anchoredPosition = new Vector2(Mathf.Lerp(num5, num6, num), 0f);
	}

	private void Clicked()
	{
		Singleton<DryingRackCanvas>.Instance.Rack.TryEndOperation(Singleton<DryingRackCanvas>.Instance.Rack.DryingOperations.IndexOf(AssignedOperation), allowSplitting: true, AssignedOperation.GetQuality(), Random.Range(int.MinValue, int.MaxValue));
	}
}
