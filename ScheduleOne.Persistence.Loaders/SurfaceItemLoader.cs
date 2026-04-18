using System;
using ScheduleOne.Building;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence.Datas;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders;

public class SurfaceItemLoader : BuildableItemLoader
{
	public override string ItemType => typeof(SurfaceItemData).Name;

	public override void Load(string mainPath)
	{
		LoadAndCreate(mainPath);
	}

	public override void Load(DynamicSaveData data)
	{
		if (data.TryExtractBaseData<SurfaceItemData>(out var data2))
		{
			LoadAndCreate(data2);
		}
	}

	protected SurfaceItem LoadAndCreate(string mainPath)
	{
		if (TryLoadFile(mainPath, "Data", out var contents))
		{
			SurfaceItemData data = null;
			try
			{
				data = JsonUtility.FromJson<SurfaceItemData>(contents);
			}
			catch (Exception ex)
			{
				Console.LogError(GetType()?.ToString() + " error reading data: " + ex);
			}
			return LoadAndCreate(data);
		}
		return null;
	}

	protected SurfaceItem LoadAndCreate(SurfaceItemData data)
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		if (data != null)
		{
			ItemInstance itemInstance = ItemDeserializer.LoadItem(data.ItemString);
			if (itemInstance == null)
			{
				return null;
			}
			if (GUIDManager.IsGUIDAlreadyRegistered(new Guid(data.GUID)))
			{
				Console.LogWarning("Surface item with GUID " + data.GUID + " is already registered. Skipping creation.");
				return null;
			}
			Surface surface = GUIDManager.GetObject<Surface>(new Guid(data.ParentSurfaceGUID));
			if ((Object)(object)surface == (Object)null)
			{
				Console.LogWarning("Failed to find parent surface for " + data.ParentSurfaceGUID);
				return null;
			}
			return NetworkSingleton<BuildManager>.Instance.CreateSurfaceItem(itemInstance, surface, data.RelativePosition, data.RelativeRotation, data.GUID);
		}
		return null;
	}
}
