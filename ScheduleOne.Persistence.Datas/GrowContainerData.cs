using System;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.Growing;
using ScheduleOne.ItemFramework;
using ScheduleOne.Tiles;
using UnityEngine;

namespace ScheduleOne.Persistence.Datas;

public class GrowContainerData : GridItemData
{
	public string SoilID;

	public float SoilLevel;

	public int RemainingSoilUses;

	public float WaterLevel;

	public string[] AppliedAdditives;

	public GrowContainerData(Guid guid, ItemInstance item, int loadOrder, Grid grid, Vector2 originCoordinate, int rotation, string soilID, float soilLevel, int remainingSoilUses, float waterLevel, string[] appliedAdditives)
		: base(guid, item, loadOrder, grid, originCoordinate, rotation)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		SoilID = soilID;
		SoilLevel = soilLevel;
		RemainingSoilUses = remainingSoilUses;
		WaterLevel = waterLevel;
		AppliedAdditives = appliedAdditives;
	}

	public void ConvertOldAdditiveFormatToNew()
	{
		for (int i = 0; i < AppliedAdditives.Length; i++)
		{
			if (AppliedAdditives[i].Contains("/"))
			{
				Object obj = Resources.Load(AppliedAdditives[i]);
				Additive component = ((GameObject)((obj is GameObject) ? obj : null)).GetComponent<Additive>();
				if ((Object)(object)component == (Object)null)
				{
					Console.LogError("Failed to convert old additive format to new. Could not load additive prefab at path: " + AppliedAdditives[i]);
					continue;
				}
				Console.Log("Converted old additive format to new for additive: " + ((BaseItemDefinition)component.Definition).ID);
				AppliedAdditives[i] = ((BaseItemDefinition)component.Definition).ID;
			}
		}
	}
}
