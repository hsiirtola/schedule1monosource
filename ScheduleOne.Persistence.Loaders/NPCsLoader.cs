using System;
using System.Collections.Generic;
using System.IO;
using ScheduleOne.Persistence.Datas;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders;

public class NPCsLoader : Loader
{
	public virtual string NPCType => typeof(NPCCollectionData).Name;

	public override void Load(string mainPath)
	{
		NPCLoader nPCLoader = new NPCLoader();
		bool flag = false;
		if (TryLoadFile(mainPath, out var contents))
		{
			NPCCollectionData nPCCollectionData = null;
			try
			{
				nPCCollectionData = JsonUtility.FromJson<NPCCollectionData>(contents);
			}
			catch (Exception ex)
			{
				Console.LogError(GetType()?.ToString() + " error reading data: " + ex);
				return;
			}
			if (nPCCollectionData != null)
			{
				flag = true;
				DynamicSaveData[] nPCs = nPCCollectionData.NPCs;
				foreach (DynamicSaveData dynamicSaveData in nPCs)
				{
					if (dynamicSaveData != null)
					{
						nPCLoader.Load(dynamicSaveData);
					}
				}
			}
		}
		if (!flag)
		{
			Console.Log("Loading legacy NPC stuff");
			List<DirectoryInfo> directories = GetDirectories(mainPath);
			LegacyNPCLoader loader = new LegacyNPCLoader();
			for (int j = 0; j < directories.Count; j++)
			{
				new LoadRequest(directories[j].FullName, loader);
			}
		}
	}
}
