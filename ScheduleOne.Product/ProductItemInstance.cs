using System;
using System.Collections.Generic;
using System.Linq;
using FishNet.Serializing;
using FishNet.Serializing.Helping;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Effects;
using ScheduleOne.Equipping;
using ScheduleOne.ItemFramework;
using ScheduleOne.NPCs;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Product.Packaging;
using ScheduleOne.Storage;
using UnityEngine;

namespace ScheduleOne.Product;

[Serializable]
public class ProductItemInstance : QualityItemInstance
{
	public string PackagingID = string.Empty;

	[CodegenExclude]
	private PackagingDefinition packaging;

	[CodegenExclude]
	public PackagingDefinition AppliedPackaging
	{
		get
		{
			if ((Object)(object)packaging == (Object)null && PackagingID != string.Empty)
			{
				packaging = Registry.GetItem(PackagingID) as PackagingDefinition;
				if ((Object)(object)packaging == (Object)null)
				{
					Console.LogError("Failed to load packaging with ID (" + PackagingID + ")");
				}
			}
			return packaging;
		}
	}

	[CodegenExclude]
	public int Amount
	{
		get
		{
			if (!((Object)(object)AppliedPackaging != (Object)null))
			{
				return 1;
			}
			return AppliedPackaging.Quantity;
		}
	}

	public override string Name => ((BaseItemInstance)this).Name + (((Object)(object)packaging != (Object)null) ? (" (" + packaging.Quantity + ")") : " (Unpackaged)");

	[CodegenExclude]
	public override Equippable Equippable => GetEquippable();

	[CodegenExclude]
	public override StoredItem StoredItem => GetStoredItem();

	[CodegenExclude]
	public override Sprite Icon => GetIcon();

	public ProductItemInstance(ItemDefinition definition, int quantity, EQuality quality, PackagingDefinition _packaging = null)
		: base(definition, quantity, quality)
	{
		packaging = _packaging;
		if ((Object)(object)packaging != (Object)null)
		{
			PackagingID = ((BaseItemDefinition)packaging).ID;
		}
		else
		{
			PackagingID = string.Empty;
		}
	}

	public override bool CanStackWith(ItemInstance other, bool checkQuantities = true)
	{
		if (!(other is ProductItemInstance))
		{
			return false;
		}
		if ((Object)(object)(other as ProductItemInstance).AppliedPackaging != (Object)null)
		{
			if ((Object)(object)AppliedPackaging == (Object)null)
			{
				return false;
			}
			if (((BaseItemDefinition)(other as ProductItemInstance).AppliedPackaging).ID != ((BaseItemDefinition)AppliedPackaging).ID)
			{
				return false;
			}
		}
		else if ((Object)(object)AppliedPackaging != (Object)null)
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
		return new ProductItemInstance(base.Definition, quantity, Quality, AppliedPackaging);
	}

	public virtual void SetPackaging(PackagingDefinition def)
	{
		packaging = def;
		if ((Object)(object)packaging != (Object)null)
		{
			PackagingID = ((BaseItemDefinition)packaging).ID;
		}
		else
		{
			PackagingID = string.Empty;
		}
		((BaseItemInstance)this).InvokeDataChange();
	}

	private Equippable GetEquippable()
	{
		if ((Object)(object)AppliedPackaging != (Object)null)
		{
			return AppliedPackaging.Equippable_Filled;
		}
		return base.Equippable;
	}

	private StoredItem GetStoredItem()
	{
		if ((Object)(object)AppliedPackaging != (Object)null)
		{
			return AppliedPackaging.StoredItem_Filled;
		}
		return base.StoredItem;
	}

	private Sprite GetIcon()
	{
		if ((Object)(object)AppliedPackaging != (Object)null)
		{
			return Singleton<ProductIconManager>.Instance.GetIcon(((BaseItemInstance)this).ID, ((BaseItemDefinition)AppliedPackaging).ID);
		}
		return ((BaseItemInstance)this).Icon;
	}

	public override ItemData GetItemData()
	{
		return new ProductItemData(((BaseItemInstance)this).ID, ((BaseItemInstance)this).Quantity, Quality.ToString(), PackagingID);
	}

	public virtual float GetAddictiveness()
	{
		return (base.Definition as ProductDefinition).GetAddictiveness();
	}

	public float GetSimilarity(ProductDefinition other, EQuality otherQuality)
	{
		ProductDefinition productDefinition = base.Definition as ProductDefinition;
		float num = 0f;
		if (other.DrugType == productDefinition.DrugType)
		{
			num = 0.4f;
		}
		int num2 = 0;
		for (int i = 0; i < other.Properties.Count; i++)
		{
			if (productDefinition.HasProperty(other.Properties[i]))
			{
				num2++;
			}
		}
		for (int j = 0; j < productDefinition.Properties.Count; j++)
		{
			if (!other.HasProperty(productDefinition.Properties[j]))
			{
				num2--;
			}
		}
		float num3 = 0.3f;
		int num4 = Mathf.Max(productDefinition.Properties.Count, other.Properties.Count);
		if (num4 > 0)
		{
			num3 *= Mathf.Clamp01((float)num2 / (float)num4);
		}
		float num5 = 0.3f;
		if (otherQuality > EQuality.Trash)
		{
			num5 *= Mathf.Clamp01((float)Quality / (float)otherQuality);
		}
		return Mathf.Clamp01(num + num3 + num5);
	}

	public virtual void ApplyEffectsToNPC(NPC npc)
	{
		List<Effect> list = new List<Effect>();
		list.AddRange((base.Definition as ProductDefinition).Properties);
		list = list.OrderBy((Effect x) => x.Tier).ToList();
		for (int num = 0; num < list.Count; num++)
		{
			list[num].ApplyToNPC(npc);
		}
	}

	public virtual void ClearEffectsFromNPC(NPC npc)
	{
		List<Effect> list = new List<Effect>();
		list.AddRange((base.Definition as ProductDefinition).Properties);
		list = list.OrderBy((Effect x) => x.Tier).ToList();
		for (int num = 0; num < list.Count; num++)
		{
			list[num].ClearFromNPC(npc);
		}
	}

	public virtual void ApplyEffectsToPlayer(Player player)
	{
		List<Effect> list = new List<Effect>();
		list.AddRange((base.Definition as ProductDefinition).Properties);
		list = list.OrderBy((Effect x) => x.Tier).ToList();
		for (int num = 0; num < list.Count; num++)
		{
			list[num].ApplyToPlayer(player);
		}
	}

	public virtual void ClearEffectsFromPlayer(Player Player)
	{
		List<Effect> list = new List<Effect>();
		list.AddRange((base.Definition as ProductDefinition).Properties);
		list = list.OrderBy((Effect x) => x.Tier).ToList();
		for (int num = 0; num < list.Count; num++)
		{
			list[num].ClearFromPlayer(Player);
		}
	}

	public override float GetMonetaryValue()
	{
		if ((Object)(object)base.Definition == (Object)null)
		{
			Console.LogWarning("ProductItemInstance.GetMonetaryValue() - Definition is null");
			return 0f;
		}
		return (base.Definition as ProductDefinition).MarketValue * (float)((BaseItemInstance)this).Quantity * (float)Amount;
	}

	public override int GetTotalAmount()
	{
		return ((BaseItemInstance)this).GetTotalAmount() * Amount;
	}

	public override void Write(Writer writer)
	{
		base.Write(writer);
		writer.WriteString(PackagingID);
	}

	public override void Read(Reader reader)
	{
		base.Read(reader);
		PackagingID = reader.ReadString();
		packaging = null;
	}
}
