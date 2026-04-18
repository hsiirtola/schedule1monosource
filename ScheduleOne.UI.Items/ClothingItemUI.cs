using ScheduleOne.Clothing;
using ScheduleOne.DevUtilities;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Items;

public class ClothingItemUI : ItemUI
{
	public Image ClothingTypeIcon;

	public override void UpdateUI()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		base.UpdateUI();
		ClothingInstance clothingInstance = itemInstance as ClothingInstance;
		if (itemInstance != null && (itemInstance.Definition as ClothingDefinition).Colorable)
		{
			((Graphic)IconImg).color = clothingInstance.Color.GetActualColor();
		}
		else
		{
			((Graphic)IconImg).color = Color.white;
		}
		if (itemInstance != null)
		{
			ClothingTypeIcon.sprite = Singleton<ClothingUtility>.Instance.GetSlotData((itemInstance.Definition as ClothingDefinition).Slot).Icon;
		}
		else
		{
			ClothingTypeIcon.sprite = null;
		}
	}
}
