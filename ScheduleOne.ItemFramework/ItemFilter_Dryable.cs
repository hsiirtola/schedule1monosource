using ScheduleOne.Core.Items.Framework;
using ScheduleOne.Product;
using UnityEngine;

namespace ScheduleOne.ItemFramework;

public class ItemFilter_Dryable : ItemFilter
{
	public override bool DoesItemMatchFilter(ItemInstance instance)
	{
		if (!IsItemDryable(instance))
		{
			return false;
		}
		return base.DoesItemMatchFilter(instance);
	}

	public static bool IsItemDryable(ItemInstance instance)
	{
		if (instance == null)
		{
			return false;
		}
		if (instance is ProductItemInstance productItemInstance && (Object)(object)productItemInstance.AppliedPackaging == (Object)null && productItemInstance.Quality < EQuality.Heavenly)
		{
			switch ((productItemInstance.Definition as ProductDefinition).DrugType)
			{
			case EDrugType.Marijuana:
				return true;
			case EDrugType.Shrooms:
				return true;
			}
		}
		if (((BaseItemInstance)instance).ID == "cocaleaf" && (instance as QualityItemInstance).Quality < EQuality.Heavenly)
		{
			return true;
		}
		return false;
	}
}
