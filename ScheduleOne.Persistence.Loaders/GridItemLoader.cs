using System;
using ScheduleOne.Building;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Tiles;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders;

public class GridItemLoader : BuildableItemLoader
{
	public override string ItemType => typeof(GridItemData).Name;

	public override void Load(string mainPath)
	{
		LoadAndCreate(mainPath);
	}

	public override void Load(DynamicSaveData data)
	{
		if (data.TryExtractBaseData<GridItemData>(out var data2))
		{
			LoadAndCreate(data2);
		}
	}

	protected GridItem LoadAndCreate(string mainPath)
	{
		if (TryLoadFile(mainPath, "Data", out var contents))
		{
			GridItemData data = null;
			try
			{
				data = JsonUtility.FromJson<GridItemData>(contents);
			}
			catch (Exception ex)
			{
				Console.LogError(GetType()?.ToString() + " error reading data: " + ex);
			}
			return LoadAndCreate(data);
		}
		return null;
	}

	protected GridItem LoadAndCreate(GridItemData data)
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		if (data != null)
		{
			ItemInstance itemInstance = ItemDeserializer.LoadItem(data.ItemString);
			if (itemInstance == null)
			{
				return null;
			}
			Grid grid = GUIDManager.GetObject<Grid>(new Guid(data.GridGUID));
			if ((Object)(object)grid == (Object)null)
			{
				Console.LogWarning("Failed to find grid for " + data.GridGUID);
				return null;
			}
			if (GUIDManager.IsGUIDAlreadyRegistered(new Guid(data.GUID)))
			{
				Console.LogWarning("Grid item with GUID " + data.GUID + " is already registered. Skipping creation. Property: " + grid.ParentProperty.PropertyName);
				return null;
			}
			return NetworkSingleton<BuildManager>.Instance.CreateGridItem(itemInstance, grid, data.OriginCoordinate, data.Rotation, data.GUID);
		}
		return null;
	}
}
