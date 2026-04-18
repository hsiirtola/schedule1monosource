using ScheduleOne.Core.Items.Framework;
using ScheduleOne.ItemFramework;
using TMPro;
using UnityEngine;

namespace ScheduleOne.UI.Items;

public class ItemInfoContent : MonoBehaviour
{
	[Header("Settings")]
	public float Height = 90f;

	[Header("References")]
	public TextMeshProUGUI NameLabel;

	public TextMeshProUGUI DescriptionLabel;

	public virtual void Initialize(ItemInstance instance)
	{
		((TMP_Text)NameLabel).text = ((BaseItemInstance)instance).Name;
		((TMP_Text)DescriptionLabel).text = ((BaseItemInstance)instance).Description;
	}

	public virtual void Initialize(ItemDefinition definition)
	{
		((TMP_Text)NameLabel).text = ((BaseItemDefinition)definition).Name;
		((TMP_Text)DescriptionLabel).text = ((BaseItemDefinition)definition).Description;
	}
}
