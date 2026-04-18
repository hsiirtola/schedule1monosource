using System;
using ScheduleOne.ItemFramework;
using ScheduleOne.Tiles;
using UnityEngine;

namespace ScheduleOne.Persistence.Datas;

public class MushroomBedData : GrowContainerData
{
	public ShroomColonyData ShroomColonyData;

	public MushroomBedData(Guid guid, ItemInstance item, int loadOrder, Grid grid, Vector2 originCoordinate, int rotation, string soilID, float soilLevel, int remainingSoilUses, float waterLevel, string[] appliedAdditives, ShroomColonyData colonyData)
		: base(guid, item, loadOrder, grid, originCoordinate, rotation, soilID, soilLevel, remainingSoilUses, waterLevel, appliedAdditives)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		ShroomColonyData = colonyData;
	}
}
