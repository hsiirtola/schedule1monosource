using System;
using ScheduleOne.ItemFramework;
using ScheduleOne.Tiles;
using UnityEngine;

namespace ScheduleOne.Persistence.Datas;

public class ChemistryStationData : GridItemData
{
	public ItemSet InputContents;

	public ItemSet OutputContents;

	public string CurrentRecipeID;

	public EQuality ProductQuality;

	public Color StartLiquidColor;

	public float LiquidLevel;

	public int CurrentTime;

	public ChemistryStationData(Guid guid, ItemInstance item, int loadOrder, Grid grid, Vector2 originCoordinate, int rotation, ItemSet inputContents, ItemSet outputContents, string currentRecipeID, EQuality productQuality, Color startLiquidColor, float liquidLevel, int currentTime)
		: base(guid, item, loadOrder, grid, originCoordinate, rotation)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		InputContents = inputContents;
		OutputContents = outputContents;
		CurrentRecipeID = currentRecipeID;
		ProductQuality = productQuality;
		StartLiquidColor = startLiquidColor;
		LiquidLevel = liquidLevel;
		CurrentTime = currentTime;
	}
}
