using System;
using FishNet.Serializing;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Storage;

namespace ScheduleOne.Clothing;

[Serializable]
public class ClothingInstance : StorableItemInstance
{
	public EClothingColor Color;

	public override string Name => ((BaseItemInstance)this).Name + ((Color != EClothingColor.White) ? (" (" + Color.GetLabel() + ")") : string.Empty);

	public ClothingInstance(ItemDefinition definition, int quantity, EClothingColor color)
		: base(definition, quantity)
	{
		Color = color;
	}

	public override ItemInstance GetCopy(int overrideQuantity = -1)
	{
		int quantity = ((BaseItemInstance)this).Quantity;
		if (overrideQuantity != -1)
		{
			quantity = overrideQuantity;
		}
		return new ClothingInstance(base.Definition, quantity, Color);
	}

	public override ItemData GetItemData()
	{
		return new ClothingData(((BaseItemInstance)this).ID, ((BaseItemInstance)this).Quantity, Color);
	}

	public override void Write(Writer writer)
	{
		base.Write(writer);
		writer.WriteUInt16((ushort)Color);
	}

	public override void Read(Reader reader)
	{
		base.Read(reader);
		Color = (EClothingColor)reader.ReadUInt16();
	}
}
