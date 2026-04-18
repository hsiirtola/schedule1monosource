using ScheduleOne.EntityFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence.Datas;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders;

public class SoilPourerLoader : GridItemLoader
{
	public override string ItemType => typeof(SoilPourerData).Name;

	public override void Load(string mainPath)
	{
		GridItem gridItem = LoadAndCreate(mainPath);
		if ((Object)(object)gridItem == (Object)null)
		{
			Console.LogWarning("Failed to load grid item");
			return;
		}
		SoilPourerData data = GetData<SoilPourerData>(mainPath);
		if (data == null)
		{
			Console.LogWarning("Failed to load toggleableitem data");
			return;
		}
		SoilPourer soilPourer = gridItem as SoilPourer;
		if ((Object)(object)soilPourer != (Object)null)
		{
			soilPourer.SendSoil(data.SoilID);
		}
	}

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
		SoilPourer soilPourer = gridItem as SoilPourer;
		SoilPourerData data3;
		if ((Object)(object)soilPourer == (Object)null)
		{
			Console.LogWarning("Failed to cast grid item to SoilPourer");
		}
		else if (data.TryExtractBaseData<SoilPourerData>(out data3))
		{
			soilPourer.SendSoil(data3.SoilID);
		}
	}
}
