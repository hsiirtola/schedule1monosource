using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Levelling;
using ScheduleOne.StationFramework;
using ScheduleOne.Storage;
using ScheduleOne.UI.Shop;
using UnityEngine;
using UnityEngine.Serialization;

namespace ScheduleOne.ItemFramework;

[Serializable]
[CreateAssetMenu(fileName = "StorableItemDefinition", menuName = "ScriptableObjects/StorableItemDefinition", order = 1)]
public class StorableItemDefinition : ItemDefinition
{
	[Header("Purchasing")]
	public float BasePurchasePrice = 10f;

	public List<ShopListing.CategoryInstance> ShopCategories = new List<ShopListing.CategoryInstance>();

	[Header("Unlocking")]
	public bool RequiresLevelToPurchase;

	public FullRank RequiredRank;

	[Header("Reselling")]
	[Range(0f, 1f)]
	public float ResellMultiplier = 0.5f;

	[Header("Storable Item")]
	public StoredItem StoredItem;

	[Range(0.1f, 5f)]
	public float PickpocketDifficultyMultiplier = 1f;

	[Tooltip("Optional station item if this item can be used at a station.")]
	public StationItem StationItem;

	[Header("Other Settings")]
	[FormerlySerializedAs("CombatUtilityForNPCs")]
	[Range(0f, 1f)]
	public float CombatUtility;

	public bool IsUnlocked => GetIsUnlocked();

	public override ItemInstance GetDefaultInstance(int quantity = 1)
	{
		return new StorableItemInstance(this, quantity);
	}

	protected virtual bool GetIsUnlocked()
	{
		if (RequiresLevelToPurchase)
		{
			return NetworkSingleton<LevelManager>.Instance.GetFullRank() >= RequiredRank;
		}
		return true;
	}
}
