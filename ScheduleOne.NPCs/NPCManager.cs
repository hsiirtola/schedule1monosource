using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ScheduleOne.Core;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Map;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.NPCs;

public class NPCManager : NetworkSingleton<NPCManager>, IBaseSaveable, ISaveable
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static UnityAction _003C_003E9__31_0;

		public static Predicate<NPCInventory.RandomInventoryItem> _003C_003E9__38_0;

		internal void _003CStart_003Eb__31_0()
		{
			NPCRegistry.Clear();
		}

		internal bool _003CGetNPCsWithSewerKey_003Eb__38_0(NPCInventory.RandomInventoryItem x)
		{
			return ((BaseItemDefinition)x.ItemDefinition).ID == "sewerkey";
		}
	}

	public static List<NPC> NPCRegistry = new List<NPC>();

	public Transform[] NPCWarpPoints;

	public Transform NPCContainer;

	[Header("Prefabs")]
	public NPCPoI NPCPoIPrefab;

	public NPCPoI PotentialCustomerPoIPrefab;

	public NPCPoI PotentialDealerPoIPrefab;

	private NPCsLoader loader = new NPCsLoader();

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ENPCManagerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ENPCManagerAssembly_002DCSharp_002Edll_Excuted;

	public string SaveFolderName => "NPCs";

	public string SaveFileName => "NPCs";

	public Loader Loader => loader;

	public bool ShouldSaveUnderFolder => false;

	public List<string> LocalExtraFiles { get; set; } = new List<string>();

	public List<string> LocalExtraFolders { get; set; } = new List<string>();

	public bool HasChanged { get; set; }

	public int LoadOrder { get; }

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ENPCs_002ENPCManager_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	protected override void Start()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		base.Start();
		UnityEvent onPreSceneChange = Singleton<LoadManager>.Instance.onPreSceneChange;
		object obj = _003C_003Ec._003C_003E9__31_0;
		if (obj == null)
		{
			UnityAction val = delegate
			{
				NPCRegistry.Clear();
			};
			_003C_003Ec._003C_003E9__31_0 = val;
			obj = (object)val;
		}
		onPreSceneChange.AddListener((UnityAction)obj);
	}

	public virtual void InitializeSaveable()
	{
		Singleton<SaveManager>.Instance.RegisterSaveable(this);
	}

	public static NPC GetNPC(string id)
	{
		foreach (NPC item in NPCRegistry)
		{
			if (item.ID.ToLower() == id.ToLower())
			{
				return item;
			}
		}
		return null;
	}

	public static List<NPC> GetNPCsInRegion(EMapRegion region)
	{
		List<NPC> list = new List<NPC>();
		foreach (NPC item in NPCRegistry)
		{
			if (!((Object)(object)item == (Object)null) && item.Region == region)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public virtual string GetSaveString()
	{
		List<DynamicSaveData> list = new List<DynamicSaveData>();
		for (int i = 0; i < NPCRegistry.Count; i++)
		{
			if (NPCRegistry[i].ShouldSave())
			{
				list.Add(NPCRegistry[i].GetSaveData());
			}
		}
		return new NPCCollectionData(list.ToArray()).GetJson();
	}

	public List<Transform> GetOrderedDistanceWarpPoints(Vector3 origin)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return new List<Transform>(NPCWarpPoints).OrderBy((Transform x) => Vector3.SqrMagnitude(x.position - origin)).ToList();
	}

	public virtual List<string> WriteData(string parentFolderPath)
	{
		List<string> list = new List<string>();
		string containerFolder = ((ISaveable)this).GetContainerFolder(parentFolderPath);
		for (int i = 0; i < NPCRegistry.Count; i++)
		{
			if (NPCRegistry[i].ShouldSave())
			{
				new SaveRequest(NPCRegistry[i], containerFolder);
				list.Add(NPCRegistry[i].SaveFolderName);
			}
		}
		return list;
	}

	[Button]
	public void GetNPCsWithSewerKey()
	{
		NPC[] array = Object.FindObjectsOfType<NPC>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Inventory.RandomInventoryItems.ToList().Exists((NPCInventory.RandomInventoryItem x) => ((BaseItemDefinition)x.ItemDefinition).ID == "sewerkey"))
			{
				Debug.Log((object)(array[i].fullName + " has the sewer key."), (Object)(object)array[i]);
			}
		}
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ENPCManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002ENPCManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ENPCManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002ENPCManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	protected override void Awake_UserLogic_ScheduleOne_002ENPCs_002ENPCManager_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		InitializeSaveable();
	}
}
