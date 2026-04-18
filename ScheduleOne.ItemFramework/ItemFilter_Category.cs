using System.Collections.Generic;
using ScheduleOne.Core.Items.Framework;

namespace ScheduleOne.ItemFramework;

public class ItemFilter_Category : ItemFilter
{
	public List<EItemCategory> AcceptedCategories = new List<EItemCategory>();

	public ItemFilter_Category(List<EItemCategory> acceptedCategories)
	{
		AcceptedCategories = acceptedCategories;
	}

	public override bool DoesItemMatchFilter(ItemInstance instance)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		if (!AcceptedCategories.Contains(((BaseItemInstance)instance).Category))
		{
			return false;
		}
		return base.DoesItemMatchFilter(instance);
	}
}
