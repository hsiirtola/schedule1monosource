using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Property;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders;

public class PropertyLoader : Loader
{
	public override void Load(string mainPath)
	{
		PropertyData propertyData = null;
		if (TryLoadFile(mainPath, "Property", out var contents) || TryLoadFile(mainPath, "Business", out contents))
		{
			try
			{
				propertyData = JsonUtility.FromJson<PropertyData>(contents);
			}
			catch (Exception ex)
			{
				Console.LogError(GetType()?.ToString() + " error reading data: " + ex);
			}
			if (propertyData != null)
			{
				Singleton<PropertyManager>.Instance.LoadProperty(propertyData, contents);
			}
		}
		string text = Path.Combine(mainPath, "Objects");
		if (Directory.Exists(text))
		{
			List<string> list = new List<string>();
			Dictionary<string, int> objectPriorities = new Dictionary<string, int>();
			BuildableItemLoader buildableItemLoader = new BuildableItemLoader();
			List<DirectoryInfo> directories = GetDirectories(text);
			for (int i = 0; i < directories.Count; i++)
			{
				BuildableItemData buildableItemData = buildableItemLoader.GetBuildableItemData(directories[i].FullName);
				if (buildableItemData != null)
				{
					list.Add(directories[i].FullName);
					objectPriorities.Add(directories[i].FullName, buildableItemData.LoadOrder);
				}
			}
			list = list.OrderBy((string x) => objectPriorities[x]).ToList();
			for (int num = 0; num < list.Count; num++)
			{
				new LoadRequest(list[num], buildableItemLoader);
			}
		}
		if (propertyData != null && propertyData.Employees != null)
		{
			DynamicSaveData[] employees = propertyData.Employees;
			foreach (DynamicSaveData dynamicSaveData in employees)
			{
				if (dynamicSaveData != null)
				{
					NPCLoader nPCLoader = Singleton<LoadManager>.Instance.GetNPCLoader(dynamicSaveData.DataType);
					if (nPCLoader == null)
					{
						Console.LogError("Failed to find loader for " + dynamicSaveData.DataType);
					}
					else
					{
						nPCLoader.Load(dynamicSaveData);
					}
				}
			}
			return;
		}
		string text2 = Path.Combine(mainPath, "Employees");
		if (!Directory.Exists(text2))
		{
			return;
		}
		List<DirectoryInfo> directories2 = GetDirectories(text2);
		for (int num3 = 0; num3 < directories2.Count; num3++)
		{
			if (TryLoadFile(directories2[num3].FullName, "NPC", out var contents2))
			{
				NPCData nPCData = null;
				try
				{
					nPCData = JsonUtility.FromJson<NPCData>(contents2);
				}
				catch (Exception ex2)
				{
					Console.LogWarning("Failed to load NPC data from " + directories2[num3].FullName + "\n Exception: " + ex2);
					continue;
				}
				LegacyNPCLoader legacyNPCLoader = Singleton<LoadManager>.Instance.GetLegacyNPCLoader(nPCData.DataType);
				if (legacyNPCLoader != null)
				{
					new LoadRequest(directories2[num3].FullName, legacyNPCLoader);
				}
			}
		}
	}

	public virtual void Load(PropertyData propertyData, string dataString)
	{
		if (propertyData == null)
		{
			return;
		}
		Debug.Log((object)("Loading property: " + propertyData.PropertyCode));
		Singleton<PropertyManager>.Instance.LoadProperty(propertyData, dataString);
		List<DynamicSaveData> list = new List<DynamicSaveData>();
		Dictionary<DynamicSaveData, BuildableItemLoader> objectLoaders = new Dictionary<DynamicSaveData, BuildableItemLoader>();
		DynamicSaveData[] objects = propertyData.Objects;
		foreach (DynamicSaveData dynamicSaveData in objects)
		{
			if (dynamicSaveData != null)
			{
				BuildableItemLoader objectLoader = Singleton<LoadManager>.Instance.GetObjectLoader(dynamicSaveData.DataType);
				if (objectLoader == null)
				{
					Console.LogError("Failed to find loader for " + dynamicSaveData.DataType);
					continue;
				}
				list.Add(dynamicSaveData);
				objectLoaders.Add(dynamicSaveData, objectLoader);
			}
		}
		list = list.OrderBy((DynamicSaveData x) => objectLoaders[x].LoadOrder).ToList();
		for (int num = 0; num < list.Count; num++)
		{
			try
			{
				objectLoaders[list[num]].Load(list[num]);
			}
			catch (Exception ex)
			{
				Console.LogError("Failed to load object of type " + list[num].DataType + ": " + ex);
			}
		}
		objects = propertyData.Employees;
		foreach (DynamicSaveData dynamicSaveData2 in objects)
		{
			if (dynamicSaveData2 == null)
			{
				continue;
			}
			NPCLoader nPCLoader = Singleton<LoadManager>.Instance.GetNPCLoader(dynamicSaveData2.DataType);
			if (nPCLoader == null)
			{
				Console.LogError("Failed to find loader for " + dynamicSaveData2.DataType);
				continue;
			}
			try
			{
				nPCLoader.Load(dynamicSaveData2);
			}
			catch (Exception ex2)
			{
				Console.LogError("Failed to load NPC of type " + dynamicSaveData2.DataType + ": " + ex2);
			}
		}
	}
}
