using System;
using FishNet.Serializing;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.Equipping;
using ScheduleOne.Persistence.Datas;
using UnityEngine;

namespace ScheduleOne.ItemFramework;

[Serializable]
public abstract class ItemInstance : BaseItemInstance
{
	public ItemDefinition Definition
	{
		get
		{
			if ((Object)(object)base._definition == (Object)null)
			{
				base._definition = (BaseItemDefinition)(object)Registry.GetItem(((BaseItemInstance)this).ID);
				if ((Object)(object)base._definition == (Object)null)
				{
					Console.LogError("Failed to find definition with ID: " + ((BaseItemInstance)this).ID);
				}
			}
			return base._definition as ItemDefinition;
		}
	}

	public virtual Equippable Equippable => Definition.Equippable;

	public ItemInstance(ItemDefinition definition, int quantity)
		: base((BaseItemDefinition)(object)definition, quantity)
	{
		base._definition = (BaseItemDefinition)(object)definition;
		((BaseItemInstance)this).Quantity = quantity;
	}

	public virtual bool CanStackWith(ItemInstance other, bool checkQuantities = true)
	{
		if (other == null)
		{
			return false;
		}
		if (((BaseItemInstance)other).ID != ((BaseItemInstance)this).ID)
		{
			return false;
		}
		if (checkQuantities && ((BaseItemInstance)this).Quantity + ((BaseItemInstance)other).Quantity > ((BaseItemInstance)this).StackLimit)
		{
			return false;
		}
		return true;
	}

	public abstract ItemInstance GetCopy(int overrideQuantity = -1);

	public virtual ItemData GetItemData()
	{
		return new ItemData(((BaseItemInstance)this).ID, ((BaseItemInstance)this).Quantity);
	}

	public virtual void Write(Writer writer)
	{
		writer.WriteString(((BaseItemInstance)this).ID);
		writer.WriteUInt16((ushort)((BaseItemInstance)this).Quantity);
	}

	public virtual void Read(Reader reader)
	{
		((BaseItemInstance)this).Quantity = reader.ReadUInt16();
	}

	public static ItemInstance CreateInstanceAndRead(Reader reader)
	{
		string text = reader.ReadString();
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		ItemInstance defaultInstance = Registry.GetItem(text).GetDefaultInstance();
		defaultInstance.Read(reader);
		return defaultInstance;
	}
}
