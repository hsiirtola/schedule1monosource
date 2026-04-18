using System;
using System.IO;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Storage;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders;

public class StorageLoader : Loader
{
	public override void Load(string mainPath)
	{
		if (Directory.Exists(mainPath))
		{
			string[] files = Directory.GetFiles(mainPath);
			for (int i = 0; i < files.Length; i++)
			{
				if (!TryLoadFile(files[i], out var contents, autoAddExtension: false))
				{
					continue;
				}
				WorldStorageEntityData worldStorageEntityData = null;
				try
				{
					worldStorageEntityData = JsonUtility.FromJson<WorldStorageEntityData>(contents);
				}
				catch (Exception ex)
				{
					Debug.LogError((object)("Error loading data: " + ex.Message));
				}
				if (worldStorageEntityData != null)
				{
					WorldStorageEntity worldStorageEntity = GUIDManager.GetObject<WorldStorageEntity>(new Guid(worldStorageEntityData.GUID));
					if ((Object)(object)worldStorageEntity != (Object)null)
					{
						worldStorageEntity.Load(worldStorageEntityData);
					}
				}
			}
		}
		if (!TryLoadFile(mainPath, out var contents2))
		{
			return;
		}
		WorldStorageEntitiesData worldStorageEntitiesData = JsonUtility.FromJson<WorldStorageEntitiesData>(contents2);
		if (worldStorageEntitiesData == null)
		{
			return;
		}
		Console.Log("Found world storage entities: " + worldStorageEntitiesData.Entities.Length);
		WorldStorageEntityData[] entities = worldStorageEntitiesData.Entities;
		foreach (WorldStorageEntityData worldStorageEntityData2 in entities)
		{
			if (worldStorageEntityData2 != null)
			{
				WorldStorageEntity worldStorageEntity2 = GUIDManager.GetObject<WorldStorageEntity>(new Guid(worldStorageEntityData2.GUID));
				if ((Object)(object)worldStorageEntity2 != (Object)null)
				{
					worldStorageEntity2.Load(worldStorageEntityData2);
				}
			}
		}
	}
}
