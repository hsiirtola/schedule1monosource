using System.Collections.Generic;
using ScheduleOne.Management;
using ScheduleOne.Management.UI;
using UnityEngine;

namespace ScheduleOne.UI.Management;

public class SpawnStationConfigPanel : ConfigPanel
{
	[Header("References")]
	public ObjectFieldUI DestinationUI;

	protected override void BindInternal(List<EntityConfiguration> configs)
	{
		base.BindInternal(configs);
		List<ObjectField> list = new List<ObjectField>();
		foreach (SpawnStationConfiguration config in configs)
		{
			if (config == null)
			{
				Console.LogError("Failed to cast EntityConfiguration to SpawnStationConfiguration");
				return;
			}
			list.Add(config.Destination);
		}
		DestinationUI.Bind(list);
	}
}
