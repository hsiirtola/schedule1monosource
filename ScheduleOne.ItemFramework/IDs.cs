using System.Collections.Generic;
using ScheduleOne.Core.Items.Framework;

namespace ScheduleOne.ItemFramework;

public class IDs : ItemFilter
{
	public List<string> AcceptedIDs = new List<string>();

	public override bool DoesItemMatchFilter(ItemInstance instance)
	{
		if (!AcceptedIDs.Contains(((BaseItemInstance)instance).ID))
		{
			return false;
		}
		return base.DoesItemMatchFilter(instance);
	}
}
