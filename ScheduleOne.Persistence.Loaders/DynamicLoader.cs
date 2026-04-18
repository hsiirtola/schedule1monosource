using System;
using ScheduleOne.Persistence.Datas;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders;

public class DynamicLoader
{
	public void Load(string serializedDynamicSaveData)
	{
		if (string.IsNullOrEmpty(serializedDynamicSaveData))
		{
			Console.LogError("DynamicLoader: No data to load.");
			return;
		}
		DynamicSaveData dynamicSaveData = null;
		try
		{
			dynamicSaveData = JsonUtility.FromJson<DynamicSaveData>(serializedDynamicSaveData);
		}
		catch (Exception ex)
		{
			Console.LogError(GetType()?.ToString() + " error reading data: " + ex);
		}
		if (dynamicSaveData != null)
		{
			Load(dynamicSaveData);
		}
	}

	public virtual void Load(DynamicSaveData saveData)
	{
	}

	public static T ExtractBaseData<T>(DynamicSaveData saveData) where T : SaveData
	{
		if (saveData == null)
		{
			Console.LogError("DynamicLoader: No data to extract.");
			return null;
		}
		T result = null;
		try
		{
			result = JsonUtility.FromJson<T>(saveData.BaseData);
			return result;
		}
		catch (Exception ex)
		{
			Console.LogError("DynamicLoader: Error extracting base data: " + ex);
		}
		return result;
	}

	public static bool TryExtractBaseData<T>(DynamicSaveData saveData, out T baseData) where T : SaveData
	{
		baseData = null;
		if (saveData == null)
		{
			Console.LogError("DynamicLoader: No data to extract.");
			return false;
		}
		try
		{
			baseData = JsonUtility.FromJson<T>(saveData.BaseData);
		}
		catch (Exception ex)
		{
			Console.LogError("DynamicLoader: Error extracting base data: " + ex);
			return false;
		}
		if (baseData == null)
		{
			return false;
		}
		return true;
	}
}
