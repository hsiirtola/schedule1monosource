using System.Collections.Generic;
using ScheduleOne.Management;
using ScheduleOne.Management.UI;
using UnityEngine;

namespace ScheduleOne.UI.Management;

public class LabOvenConfigPanel : ConfigPanel
{
	[Header("References")]
	public ObjectFieldUI DestinationUI;

	protected override void BindInternal(List<EntityConfiguration> configs)
	{
		List<ObjectField> list = new List<ObjectField>();
		foreach (LabOvenConfiguration config in configs)
		{
			if (config == null)
			{
				Console.LogError("Failed to cast EntityConfiguration to LabOvenConfiguration");
				return;
			}
			list.Add(config.Destination);
		}
		DestinationUI.Bind(list);
	}
}
