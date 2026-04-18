using System.Collections.Generic;
using ScheduleOne.Management;
using ScheduleOne.Management.UI;
using UnityEngine;

namespace ScheduleOne.UI.Management;

public class PackagingStationConfigPanel : ConfigPanel
{
	[Header("References")]
	public ObjectFieldUI DestinationUI;

	protected override void BindInternal(List<EntityConfiguration> configs)
	{
		List<ObjectField> list = new List<ObjectField>();
		foreach (PackagingStationConfiguration config in configs)
		{
			if (config == null)
			{
				Console.LogError("Failed to cast EntityConfiguration to PackagingStationConfiguration");
				return;
			}
			list.Add(config.Destination);
		}
		DestinationUI.Bind(list);
	}
}
