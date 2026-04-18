using System;
using System.Collections.Generic;
using FishNet.Object;
using ScheduleOne.Building;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Tiles;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders;

public class ProceduralGridItemLoader : BuildableItemLoader
{
	public override string ItemType => typeof(ProceduralGridItemData).Name;

	public override int LoadOrder => 100;

	public override void Load(string mainPath)
	{
		LoadAndCreate(mainPath);
	}

	public override void Load(DynamicSaveData data)
	{
		if (data.TryExtractBaseData<ProceduralGridItemData>(out var data2))
		{
			LoadAndCreate(data2);
		}
	}

	protected ProceduralGridItem LoadAndCreate(string mainPath)
	{
		if (TryLoadFile(mainPath, "Data", out var contents))
		{
			ProceduralGridItemData data = null;
			try
			{
				data = JsonUtility.FromJson<ProceduralGridItemData>(contents);
			}
			catch (Exception ex)
			{
				Console.LogError(GetType()?.ToString() + " error reading data: " + ex);
			}
			return LoadAndCreate(data);
		}
		return null;
	}

	protected ProceduralGridItem LoadAndCreate(ProceduralGridItemData data)
	{
		if (data != null)
		{
			ItemInstance itemInstance = ItemDeserializer.LoadItem(data.ItemString);
			if (itemInstance == null)
			{
				return null;
			}
			if (GUIDManager.IsGUIDAlreadyRegistered(new Guid(data.GUID)))
			{
				Console.LogWarning("Procedural grid item with GUID " + data.GUID + " is already registered. Skipping creation.");
				return null;
			}
			List<CoordinateProceduralTilePair> list = new List<CoordinateProceduralTilePair>();
			for (int i = 0; i < data.FootprintMatches.Length; i++)
			{
				CoordinateProceduralTilePair item = new CoordinateProceduralTilePair
				{
					coord = new Coordinate(Mathf.RoundToInt(data.FootprintMatches[i].FootprintCoordinate.x), Mathf.RoundToInt(data.FootprintMatches[i].FootprintCoordinate.y)),
					tileIndex = data.FootprintMatches[i].TileIndex
				};
				BuildableItem buildableItem = GUIDManager.GetObject<BuildableItem>(new Guid(data.FootprintMatches[i].TileOwnerGUID));
				if ((Object)(object)buildableItem == (Object)null)
				{
					Debug.LogError((object)("Failed to find tile parent for " + data.FootprintMatches[i].TileOwnerGUID));
					return null;
				}
				item.tileParent = ((NetworkBehaviour)buildableItem).NetworkObject;
				list.Add(item);
			}
			return NetworkSingleton<BuildManager>.Instance.CreateProceduralGridItem(itemInstance, data.Rotation, list, data.GUID);
		}
		return null;
	}
}
