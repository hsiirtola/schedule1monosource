using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScheduleOne.Persistence.Datas;

[Serializable]
public class DynamicSaveData : SaveData
{
	[Serializable]
	public class AdditionalData
	{
		public string Name;

		public string Contents;
	}

	public string BaseData = string.Empty;

	public List<AdditionalData> AdditionalDatas = new List<AdditionalData>();

	public DynamicSaveData(SaveData baseData)
	{
		DataType = baseData.DataType;
		GameVersion = baseData.GameVersion;
		BaseData = baseData.GetJson(prettyPrint: false);
	}

	public void AddData(string name, string contents)
	{
		if (GetData(name) != string.Empty)
		{
			Console.LogWarning("DynamicSaveData already has data with the name " + name + ". Replacing original data.");
			for (int i = 0; i < AdditionalDatas.Count; i++)
			{
				if (AdditionalDatas[i].Name == name)
				{
					AdditionalDatas[i].Contents = contents;
					break;
				}
			}
		}
		else
		{
			AdditionalDatas.Add(new AdditionalData
			{
				Name = name,
				Contents = contents
			});
		}
	}

	public void AddData(string name, SaveData data)
	{
		AddData(name, data.GetJson(prettyPrint: false));
	}

	public string GetData(string name)
	{
		foreach (AdditionalData additionalData in AdditionalDatas)
		{
			if (additionalData.Name == name)
			{
				return additionalData.Contents;
			}
		}
		return string.Empty;
	}

	public bool TryGetData(string name, out string data)
	{
		data = GetData(name);
		if (data != string.Empty)
		{
			return true;
		}
		return false;
	}

	public T GetData<T>(string name, bool warn = true) where T : SaveData
	{
		string data = GetData(name);
		if (data != string.Empty)
		{
			return JsonUtility.FromJson<T>(data);
		}
		if (warn)
		{
			Console.LogWarning("DynamicSaveData does not contain data with the name " + name + ".");
		}
		return null;
	}

	public bool TryGetData<T>(string name, out T data) where T : SaveData
	{
		data = GetData<T>(name, warn: false);
		if (data != null)
		{
			return true;
		}
		return false;
	}

	public T ExtractBaseData<T>() where T : SaveData
	{
		if (BaseData != string.Empty)
		{
			return JsonUtility.FromJson<T>(BaseData);
		}
		Console.LogWarning("DynamicSaveData does not contain base data.");
		return null;
	}

	public bool TryExtractBaseData<T>(out T data) where T : SaveData
	{
		data = ExtractBaseData<T>();
		if (data != null)
		{
			return true;
		}
		Console.LogWarning("DynamicSaveData does not contain base data.");
		return false;
	}
}
