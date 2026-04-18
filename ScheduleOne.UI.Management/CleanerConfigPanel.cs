using System.Collections.Generic;
using ScheduleOne.Management;
using ScheduleOne.Management.UI;
using UnityEngine;

namespace ScheduleOne.UI.Management;

public class CleanerConfigPanel : ConfigPanel
{
	[Header("References")]
	public ObjectFieldUI BedUI;

	public ObjectListFieldUI BinsUI;

	protected override void BindInternal(List<EntityConfiguration> configs)
	{
		List<ObjectField> list = new List<ObjectField>();
		List<ObjectListField> list2 = new List<ObjectListField>();
		foreach (CleanerConfiguration config in configs)
		{
			if (config == null)
			{
				Console.LogError("Failed to cast EntityConfiguration to CleanerConfiguration");
				return;
			}
			list.Add(config.Home);
			list2.Add(config.Bins);
		}
		BedUI.Bind(list);
		BinsUI.Bind(list2);
	}
}
