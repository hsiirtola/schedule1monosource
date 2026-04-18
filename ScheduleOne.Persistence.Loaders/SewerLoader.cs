using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Map;
using ScheduleOne.Persistence.Datas;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders;

public class SewerLoader : Loader
{
	public override void Load(string mainPath)
	{
		if (TryLoadFile(mainPath, out var contents))
		{
			SewerData sewerData = null;
			try
			{
				sewerData = JsonUtility.FromJson<SewerData>(contents);
			}
			catch (Exception ex)
			{
				Console.LogError(GetType()?.ToString() + " error reading data: " + ex);
			}
			if (sewerData != null)
			{
				NetworkSingleton<SewerManager>.Instance.Load(sewerData);
			}
		}
		else
		{
			SewerData sewerData2 = new SewerData(isSewerUnlocked: false, isRandomWorldKeyCollected: false, Random.Range(0, NetworkSingleton<SewerManager>.Instance.RandomSewerKeyLocations.Length), hasSewerKingBeenDefeated: false, 999, -1, new List<int>());
			NetworkSingleton<SewerManager>.Instance.Load(sewerData2);
		}
	}
}
