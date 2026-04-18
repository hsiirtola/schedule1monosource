using ScheduleOne.EntityFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence.Datas;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders;

public class JukeboxLoader : GridItemLoader
{
	public override string ItemType => typeof(JukeboxData).Name;

	public override void Load(string mainPath)
	{
		GridItem gridItem = LoadAndCreate(mainPath);
		if ((Object)(object)gridItem == (Object)null)
		{
			Console.LogWarning("Failed to load grid item");
			return;
		}
		Jukebox jukebox = gridItem as Jukebox;
		if ((Object)(object)jukebox == (Object)null)
		{
			Console.LogWarning("Failed to cast grid item to Jukebox");
			return;
		}
		JukeboxData data = GetData<JukeboxData>(mainPath);
		if (data == null)
		{
			Console.LogWarning("Failed to load jukebox data");
			return;
		}
		Console.Log($"Loaded jukebox data: {data}");
		jukebox.SetJukeboxState(null, data.State, setTrackTime: true, setSync: true);
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
		Jukebox jukebox = gridItem as Jukebox;
		JukeboxData data3;
		if ((Object)(object)jukebox == (Object)null)
		{
			Console.LogWarning("Failed to cast grid item to Jukebox");
		}
		else if (data.TryExtractBaseData<JukeboxData>(out data3))
		{
			jukebox.SetJukeboxState(null, data3.State, setTrackTime: true, setSync: true);
		}
	}
}
