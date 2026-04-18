using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Growing;
using ScheduleOne.ItemFramework;
using ScheduleOne.UI.Management;
using UnityEngine;

namespace ScheduleOne.Management;

public class ManagementUtilities : Singleton<ManagementUtilities>
{
	public List<SeedDefinition> Seeds = new List<SeedDefinition>();

	public List<ShroomSpawnDefinition> MushroomSpawns = new List<ShroomSpawnDefinition>();

	[field: SerializeField]
	public Sprite StorageTypeIcon { get; private set; }

	[field: SerializeField]
	public StorageUIElement StorageUIElementPrefab { get; private set; }

	protected override void Awake()
	{
		base.Awake();
	}
}
