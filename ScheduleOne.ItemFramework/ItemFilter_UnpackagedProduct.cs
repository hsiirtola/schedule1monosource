using System.Collections.Generic;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.Product;
using UnityEngine;

namespace ScheduleOne.ItemFramework;

public class ItemFilter_UnpackagedProduct : ItemFilter_Category
{
	public ItemFilter_UnpackagedProduct()
		: base(new List<EItemCategory> { (EItemCategory)0 })
	{
	}

	public override bool DoesItemMatchFilter(ItemInstance instance)
	{
		if (!(instance is ProductItemInstance productItemInstance))
		{
			return false;
		}
		if ((Object)(object)productItemInstance.AppliedPackaging != (Object)null)
		{
			return false;
		}
		return base.DoesItemMatchFilter(instance);
	}
}
