using System;
using FishNet.Serializing;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Storage;

namespace ScheduleOne.ItemFramework;

[Serializable]
public class QualityItemInstance : StorableItemInstance
{
	public EQuality Quality = EQuality.Standard;

	public QualityItemInstance(ItemDefinition definition, int quantity, EQuality quality)
		: base(definition, quantity)
	{
		Quality = quality;
	}

	public override bool CanStackWith(ItemInstance other, bool checkQuantities = true)
	{
		if (!(other is QualityItemInstance qualityItemInstance) || qualityItemInstance.Quality != Quality)
		{
			return false;
		}
		return base.CanStackWith(other, checkQuantities);
	}

	public override ItemInstance GetCopy(int overrideQuantity = -1)
	{
		int quantity = ((BaseItemInstance)this).Quantity;
		if (overrideQuantity != -1)
		{
			quantity = overrideQuantity;
		}
		return new QualityItemInstance(base.Definition, quantity, Quality);
	}

	public override ItemData GetItemData()
	{
		return new QualityItemData(((BaseItemInstance)this).ID, ((BaseItemInstance)this).Quantity, Quality.ToString());
	}

	public void SetQuality(EQuality quality)
	{
		Quality = quality;
		((BaseItemInstance)this).InvokeDataChange();
	}

	public override void Write(Writer writer)
	{
		base.Write(writer);
		writer.WriteUInt16((ushort)Quality);
	}

	public override void Read(Reader reader)
	{
		base.Read(reader);
		Quality = (EQuality)reader.ReadUInt16();
	}
}
