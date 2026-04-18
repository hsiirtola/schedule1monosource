using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ScheduleOne.Core;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.Levelling;
using ScheduleOne.Persistence;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne;

public class Registry : PersistentSingleton<Registry>
{
	[Serializable]
	public class ItemRegister
	{
		[HideInInspector]
		public string name;

		public string ID;

		public ItemDefinition Definition;
	}

	[SerializeField]
	private List<ItemRegister> ItemRegistry = new List<ItemRegister>();

	[SerializeField]
	private List<ItemRegister> ItemsAddedAtRuntime = new List<ItemRegister>();

	private Dictionary<int, ItemRegister> ItemDictionary = new Dictionary<int, ItemRegister>();

	private Dictionary<string, string> itemIDAliases = new Dictionary<string, string> { { "viagra", "viagor" } };

	private void OnValidate()
	{
		foreach (ItemRegister item in ItemRegistry)
		{
			if (string.IsNullOrEmpty(item.ID))
			{
				Console.LogError("Item ID is empty!");
			}
			else if ((Object)(object)item.Definition == (Object)null)
			{
				Console.LogError("Item Definition is null for ID: " + item.ID);
			}
			else
			{
				item.name = ((BaseItemDefinition)item.Definition).Name + " (" + ((object)System.Runtime.CompilerServices.Unsafe.As<EItemCategory, EItemCategory>(ref ((BaseItemDefinition)item.Definition).Category)/*cast due to .constrained prefix*/).ToString() + ")";
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if ((Object)(object)Singleton<Registry>.Instance == (Object)null || (Object)(object)Singleton<Registry>.Instance != (Object)(object)this)
		{
			return;
		}
		foreach (ItemRegister item in ItemRegistry)
		{
			if (ItemDictionary.ContainsKey(GetHash(item.ID)))
			{
				Console.LogError("Duplicate item ID: " + item.ID);
			}
			else
			{
				AddToItemDictionary(item);
			}
		}
	}

	public static ItemDefinition GetItem(string ID)
	{
		return Singleton<Registry>.Instance._GetItem(ID);
	}

	public static bool ItemExists(string ID)
	{
		return (Object)(object)Singleton<Registry>.Instance._GetItem(ID, warnIfNonExistent: false) != (Object)null;
	}

	public static T GetItem<T>(string ID) where T : ItemDefinition
	{
		return Singleton<Registry>.Instance._GetItem(ID) as T;
	}

	public ItemDefinition _GetItem(string ID, bool warnIfNonExistent = true)
	{
		if (string.IsNullOrEmpty(ID))
		{
			return null;
		}
		if (itemIDAliases.ContainsKey(ID.ToLower()))
		{
			ID = itemIDAliases[ID.ToLower()];
		}
		int hash = GetHash(ID);
		ItemRegister itemRegister = null;
		if (!ItemDictionary.ContainsKey(hash))
		{
			if (Singleton<LoadManager>.InstanceExists && !Singleton<LoadManager>.Instance.IsLoading && warnIfNonExistent)
			{
				Console.LogWarning("Item '" + ID + "' not found in registry! (Hash = " + hash + ")");
			}
			return null;
		}
		return ItemDictionary[hash]?.Definition;
	}

	private static int GetHash(string ID)
	{
		return ID.ToLower().GetHashCode();
	}

	private static string RemoveAssetsAndPrefab(string originalString)
	{
		int num = originalString.IndexOf("Assets/");
		if (num != -1)
		{
			originalString = originalString.Substring(num + "Assets/".Length);
		}
		int num2 = originalString.LastIndexOf(".prefab");
		if (num2 != -1)
		{
			originalString = originalString.Substring(0, num2);
		}
		return originalString;
	}

	protected override void Start()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		base.Start();
		Singleton<LoadManager>.Instance.onPreSceneChange.AddListener(new UnityAction(RemoveRuntimeItems));
	}

	public void AddToRegistry(ItemDefinition item)
	{
		ItemRegister itemRegister = new ItemRegister
		{
			Definition = item,
			ID = ((BaseItemDefinition)item).ID
		};
		ItemRegistry.Add(itemRegister);
		AddToItemDictionary(itemRegister);
		if (Application.isPlaying)
		{
			ItemsAddedAtRuntime.Add(new ItemRegister
			{
				Definition = item,
				ID = ((BaseItemDefinition)item).ID
			});
		}
	}

	public List<ItemDefinition> GetAllItems()
	{
		return ItemRegistry.ConvertAll((ItemRegister x) => x.Definition);
	}

	private void AddToItemDictionary(ItemRegister reg)
	{
		int hash = GetHash(reg.ID);
		if (ItemDictionary.ContainsKey(hash))
		{
			Console.LogError("Duplicate item ID: " + reg.ID);
		}
		else
		{
			ItemDictionary.Add(hash, reg);
		}
	}

	private void RemoveItemFromDictionary(ItemRegister reg)
	{
		int hash = GetHash(reg.ID);
		ItemDictionary.Remove(hash);
	}

	public void RemoveRuntimeItems()
	{
		foreach (ItemRegister item in new List<ItemRegister>(ItemsAddedAtRuntime))
		{
			RemoveFromRegistry(item.Definition);
		}
		ItemsAddedAtRuntime.Clear();
		Console.Log("Removed runtime items from registry");
	}

	public void RemoveFromRegistry(ItemDefinition item)
	{
		ItemRegister itemRegister = ItemRegistry.Find((ItemRegister x) => (Object)(object)x.Definition == (Object)(object)item);
		if (itemRegister != null)
		{
			ItemRegistry.Remove(itemRegister);
			RemoveItemFromDictionary(itemRegister);
		}
	}

	[Button]
	public void LogOrderedUnlocks()
	{
		List<ItemDefinition> list = new List<ItemDefinition>();
		for (int i = 0; i < ItemRegistry.Count; i++)
		{
			if ((ItemRegistry[i].Definition as StorableItemDefinition).RequiresLevelToPurchase)
			{
				list.Add(ItemRegistry[i].Definition);
			}
		}
		list.Sort((ItemDefinition x, ItemDefinition y) => (x as StorableItemDefinition).RequiredRank.CompareTo((y as StorableItemDefinition).RequiredRank));
		Console.Log("Ordered Unlocks:");
		foreach (ItemDefinition item in list)
		{
			string iD = ((BaseItemDefinition)item).ID;
			FullRank requiredRank = (item as StorableItemDefinition).RequiredRank;
			Console.Log(iD + " - " + requiredRank.ToString());
		}
	}
}
