using ScheduleOne.ItemFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Items;

public class QualityItemInfoContent : ItemInfoContent
{
	public Image Star;

	public TextMeshProUGUI QualityLabel;

	public override void Initialize(ItemInstance instance)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		base.Initialize(instance);
		if (!(instance is QualityItemInstance qualityItemInstance))
		{
			Console.LogError("QualityItemInfoContent can only be used with QualityItemInstance!");
			return;
		}
		((TMP_Text)QualityLabel).text = qualityItemInstance.Quality.ToString();
		((Graphic)QualityLabel).color = ItemQuality.GetColor(qualityItemInstance.Quality);
		((Graphic)Star).color = ItemQuality.GetColor(qualityItemInstance.Quality);
		((Component)QualityLabel).gameObject.SetActive(true);
	}
}
