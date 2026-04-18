using ScheduleOne.EntityFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence.Datas;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders;

public class TrashContainerLoader : GridItemLoader
{
	public override string ItemType => typeof(TrashContainerData).Name;

	public override void Load(string mainPath)
	{
		GridItem gridItem = LoadAndCreate(mainPath);
		if ((Object)(object)gridItem == (Object)null)
		{
			Console.LogWarning("Failed to load grid item");
			return;
		}
		TrashContainerData data = GetData<TrashContainerData>(mainPath);
		if (data == null)
		{
			Console.LogWarning("Failed to load toggleableitem data");
			return;
		}
		TrashContainerItem trashContainerItem = gridItem as TrashContainerItem;
		if ((Object)(object)trashContainerItem != (Object)null)
		{
			trashContainerItem.Container.Content.LoadFromData(data.ContentData);
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
		TrashContainerItem trashContainerItem = gridItem as TrashContainerItem;
		TrashContainerData data3;
		if ((Object)(object)trashContainerItem == (Object)null)
		{
			Console.LogWarning("Failed to load trash container item");
		}
		else if (data.TryExtractBaseData<TrashContainerData>(out data3))
		{
			trashContainerItem.Container.Content.LoadFromData(data3.ContentData);
		}
	}
}
