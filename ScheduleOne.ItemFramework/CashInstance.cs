using System;
using FishNet.Serializing;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Storage;
using UnityEngine;

namespace ScheduleOne.ItemFramework;

[Serializable]
public class CashInstance : StorableItemInstance
{
	public const float MAX_BALANCE = 1E+09f;

	public float Balance { get; protected set; }

	public CashInstance(ItemDefinition definition, int quantity)
		: base(definition, quantity)
	{
	}

	public override ItemInstance GetCopy(int overrideQuantity = -1)
	{
		int quantity = ((BaseItemInstance)this).Quantity;
		if (overrideQuantity != -1)
		{
			quantity = overrideQuantity;
		}
		return new CashInstance(base.Definition, quantity);
	}

	public void ChangeBalance(float amount)
	{
		SetBalance(Balance + amount);
	}

	public void SetBalance(float newBalance, bool blockClear = false)
	{
		Balance = Mathf.Clamp(newBalance, 0f, 1E+09f);
		if (Balance <= 0f && !blockClear)
		{
			((BaseItemInstance)this).RequestClearSlot();
		}
		((BaseItemInstance)this).InvokeDataChange();
	}

	public override ItemData GetItemData()
	{
		return new CashData(((BaseItemInstance)this).ID, ((BaseItemInstance)this).Quantity, Balance);
	}

	public override float GetMonetaryValue()
	{
		return Balance;
	}

	public override void Write(Writer writer)
	{
		base.Write(writer);
		writer.WriteSingle(Balance, (AutoPackType)0);
	}

	public override void Read(Reader reader)
	{
		base.Read(reader);
		Balance = reader.ReadSingle((AutoPackType)0);
	}
}
