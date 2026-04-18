using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Persistence.Datas;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders;

public class TimeLoader : Loader
{
	public override void Load(string mainPath)
	{
		if (TryLoadFile(mainPath, out var contents))
		{
			TimeData timeData = JsonUtility.FromJson<TimeData>(contents);
			NetworkSingleton<TimeManager>.Instance.Load(timeData);
		}
	}
}
