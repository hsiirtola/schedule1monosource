using System;
using FishNet.Serializing.Helping;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.ItemFramework;
using UnityEngine;

namespace ScheduleOne.Storage;

[Serializable]
public class StorableItemInstance : ItemInstance
{
	[CodegenExclude]
	public virtual StoredItem StoredItem
	{
		get
		{
			if ((Object)(object)base.Definition != (Object)null && base.Definition is StorableItemDefinition)
			{
				return (base.Definition as StorableItemDefinition).StoredItem;
			}
			Console.LogError("StorableItemInstance has invalid definition: " + (object)base.Definition);
			return null;
		}
	}

	public StorableItemInstance(ItemDefinition definition, int quantity)
		: base(definition, quantity)
	{
		if ((Object)(object)(definition as StorableItemDefinition) == (Object)null)
		{
			Console.LogError("StoredItemInstance initialized with invalid definition!");
		}
	}

	public override ItemInstance GetCopy(int overrideQuantity = -1)
	{
		int quantity = ((BaseItemInstance)this).Quantity;
		if (overrideQuantity != -1)
		{
			quantity = overrideQuantity;
		}
		return new StorableItemInstance(base.Definition, quantity);
	}

	public override float GetMonetaryValue()
	{
		return (base.Definition as StorableItemDefinition).BasePurchasePrice * (base.Definition as StorableItemDefinition).ResellMultiplier * (float)((BaseItemInstance)this).Quantity;
	}
}
