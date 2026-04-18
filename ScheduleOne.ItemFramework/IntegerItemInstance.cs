using FishNet.Serializing;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Storage;

namespace ScheduleOne.ItemFramework;

public class IntegerItemInstance : StorableItemInstance
{
	public int Value;

	public IntegerItemInstance(ItemDefinition definition, int quantity, int value)
		: base(definition, quantity)
	{
		Value = value;
	}

	public override ItemInstance GetCopy(int overrideQuantity = -1)
	{
		int quantity = ((BaseItemInstance)this).Quantity;
		if (overrideQuantity != -1)
		{
			quantity = overrideQuantity;
		}
		return new IntegerItemInstance(base.Definition, quantity, Value);
	}

	public void ChangeValue(int change)
	{
		Value += change;
		((BaseItemInstance)this).InvokeDataChange();
	}

	public void SetValue(int value)
	{
		Value = value;
		((BaseItemInstance)this).InvokeDataChange();
	}

	public override ItemData GetItemData()
	{
		return new IntegerItemData(((BaseItemInstance)this).ID, ((BaseItemInstance)this).Quantity, Value);
	}

	public override void Write(Writer writer)
	{
		base.Write(writer);
		writer.WriteUInt16((ushort)Value);
	}

	public override void Read(Reader reader)
	{
		base.Read(reader);
		Value = reader.ReadUInt16();
	}
}
