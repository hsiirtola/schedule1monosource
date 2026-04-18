using System.Collections.Generic;
using ScheduleOne.Management;
using ScheduleOne.Management.UI;
using UnityEngine;

namespace ScheduleOne.UI.Management;

public class DryingRackConfigPanel : ConfigPanel
{
	[Header("References")]
	public QualityFieldUI QualityUI;

	public ObjectFieldUI DestinationUI;

	public NumberFieldUI StartThresholdUI;

	protected override void BindInternal(List<EntityConfiguration> configs)
	{
		List<QualityField> list = new List<QualityField>();
		List<ObjectField> list2 = new List<ObjectField>();
		List<NumberField> list3 = new List<NumberField>();
		foreach (DryingRackConfiguration config in configs)
		{
			if (config == null)
			{
				Console.LogError("Failed to cast EntityConfiguration to DryingRackConfiguration");
				return;
			}
			list.Add(config.TargetQuality);
			list2.Add(config.Destination);
			list3.Add(config.StartThreshold);
		}
		QualityUI.Bind(list);
		DestinationUI.Bind(list2);
		StartThresholdUI.Bind(list3);
	}
}
