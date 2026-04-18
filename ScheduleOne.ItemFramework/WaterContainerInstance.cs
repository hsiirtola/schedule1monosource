using System;
using FishNet.Serializing;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Storage;
using UnityEngine;

namespace ScheduleOne.ItemFramework;

[Serializable]
public class WaterContainerInstance : StorableItemInstance
{
	public float CurrentFillAmount { get; private set; }

	public float NormalizedFillAmount => CurrentFillAmount / WaterContainerDefinition.Capacity;

	public WaterContainerDefinition WaterContainerDefinition => (WaterContainerDefinition)base.Definition;

	public WaterContainerInstance(ItemDefinition definition, int quantity, float fillAmount)
		: base(definition, quantity)
	{
		CurrentFillAmount = fillAmount;
	}

	public override ItemInstance GetCopy(int overrideQuantity = -1)
	{
		int quantity = ((BaseItemInstance)this).Quantity;
		if (overrideQuantity != -1)
		{
			quantity = overrideQuantity;
		}
		return new WaterContainerInstance(base.Definition, quantity, CurrentFillAmount);
	}

	public void ChangeFillAmount(float change)
	{
		CurrentFillAmount = Mathf.Clamp(CurrentFillAmount + change, 0f, WaterContainerDefinition.Capacity);
		((BaseItemInstance)this).InvokeDataChange();
	}

	public void ChangeFillAmountByPercentage(float percentage)
	{
		float change = WaterContainerDefinition.Capacity * percentage;
		ChangeFillAmount(change);
	}

	public void SetFillAmount(float amount)
	{
		CurrentFillAmount = Mathf.Clamp(amount, 0f, WaterContainerDefinition.Capacity);
		((BaseItemInstance)this).InvokeDataChange();
	}

	public override ItemData GetItemData()
	{
		return new WateringCanData(((BaseItemInstance)this).ID, ((BaseItemInstance)this).Quantity, CurrentFillAmount);
	}

	public override void Write(Writer writer)
	{
		base.Write(writer);
		writer.WriteSingle(CurrentFillAmount, (AutoPackType)0);
	}

	public override void Read(Reader reader)
	{
		base.Read(reader);
		CurrentFillAmount = reader.ReadSingle((AutoPackType)0);
	}
}
