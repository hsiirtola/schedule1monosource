using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;

namespace ScheduleOne.Storage;

public class StorageManager : NetworkSingleton<StorageManager>, IBaseSaveable, ISaveable
{
	private StorageLoader loader = new StorageLoader();

	private bool NetworkInitialize___EarlyScheduleOne_002EStorage_002EStorageManagerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EStorage_002EStorageManagerAssembly_002DCSharp_002Edll_Excuted;

	public string SaveFolderName => "WorldStorageEntities";

	public string SaveFileName => "WorldStorageEntities";

	public Loader Loader => loader;

	public bool ShouldSaveUnderFolder => false;

	public List<string> LocalExtraFiles { get; set; } = new List<string>();

	public List<string> LocalExtraFolders { get; set; } = new List<string>();

	public bool HasChanged { get; set; } = true;

	public int LoadOrder { get; }

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EStorage_002EStorageManager_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public virtual void InitializeSaveable()
	{
		Singleton<SaveManager>.Instance.RegisterSaveable(this);
	}

	public virtual string GetSaveString()
	{
		List<WorldStorageEntityData> list = new List<WorldStorageEntityData>();
		for (int i = 0; i < WorldStorageEntity.All.Count; i++)
		{
			if (WorldStorageEntity.All[i].ShouldSave())
			{
				list.Add(WorldStorageEntity.All[i].GetSaveData());
			}
		}
		return new WorldStorageEntitiesData(list.ToArray()).GetJson();
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002EStorage_002EStorageManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EStorage_002EStorageManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EStorage_002EStorageManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EStorage_002EStorageManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	protected override void Awake_UserLogic_ScheduleOne_002EStorage_002EStorageManager_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		InitializeSaveable();
	}
}
