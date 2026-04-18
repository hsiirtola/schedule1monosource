using ScheduleOne.EntityFramework;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Temperature;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders;

public class AirConditionerLoader : GridItemLoader
{
	public override string ItemType => typeof(AirConditionerData).Name;

	public override void Load(DynamicSaveData data)
	{
		GridItem gridItem = null;
		if (data.TryExtractBaseData<GridItemData>(out var data2))
		{
			gridItem = LoadAndCreate(data2);
		}
		if ((Object)(object)gridItem == (Object)null)
		{
			Console.LogWarning("Failed to load grid item");
			return;
		}
		AirConditioner airConditioner = gridItem as AirConditioner;
		AirConditionerData data3;
		if ((Object)(object)airConditioner == (Object)null)
		{
			Console.LogWarning("Failed to cast grid item to AC");
		}
		else if (data.TryExtractBaseData<AirConditionerData>(out data3))
		{
			airConditioner.SetMode(data3.Mode);
		}
	}
}
