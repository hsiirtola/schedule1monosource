using ScheduleOne.ItemFramework;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Items;

public class QualityItemUI : ItemUI
{
	public Image QualityIcon;

	protected QualityItemInstance qualityItemInstance;

	public override void Setup(ItemInstance item)
	{
		qualityItemInstance = item as QualityItemInstance;
		base.Setup(item);
	}

	public override void UpdateUI()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (!Destroyed)
		{
			((Behaviour)QualityIcon).enabled = true;
			((Graphic)QualityIcon).color = ItemQuality.GetColor(qualityItemInstance.Quality);
			base.UpdateUI();
		}
	}
}
