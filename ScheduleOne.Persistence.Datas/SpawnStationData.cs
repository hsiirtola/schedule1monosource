using System;
using ScheduleOne.ItemFramework;
using ScheduleOne.Tiles;
using UnityEngine;

namespace ScheduleOne.Persistence.Datas;

[Serializable]
public class SpawnStationData : GridItemData
{
	public ItemSet Contents;

	public SpawnStationData(Guid guid, ItemInstance item, int loadOrder, Grid grid, Vector2 originCoordinate, int rotation, ItemSet contents)
		: base(guid, item, loadOrder, grid, originCoordinate, rotation)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Contents = contents;
	}
}
