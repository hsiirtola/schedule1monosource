using ScheduleOne.ItemFramework;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Items;

public class WaterContainerUI : ItemUI
{
	protected WaterContainerInstance wcInstance;

	public Text AmountLabel;

	public override void Setup(ItemInstance item)
	{
		wcInstance = item as WaterContainerInstance;
		base.Setup(item);
	}

	public override void UpdateUI()
	{
		base.UpdateUI();
		if (!Destroyed && wcInstance != null)
		{
			AmountLabel.text = (float)Mathf.RoundToInt(wcInstance.CurrentFillAmount * 10f) / 10f + "L";
		}
	}
}
