using System;
using System.IO;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Trash;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders;

public class TrashLoader : Loader
{
	public override void Load(string mainPath)
	{
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		TrashData trashData = null;
		if (TryLoadFile(Path.Combine(mainPath, "Trash"), out var contents) || TryLoadFile(mainPath, out contents))
		{
			try
			{
				trashData = JsonUtility.FromJson<TrashData>(contents);
			}
			catch (Exception ex)
			{
				Debug.LogError((object)("Error loading data: " + ex.Message));
			}
			if (trashData != null && trashData.Items != null)
			{
				TrashItemData[] items = trashData.Items;
				foreach (TrashItemData trashItemData in items)
				{
					TrashItem trashItem = null;
					trashItem = ((!(trashItemData.DataType == "TrashBagData")) ? NetworkSingleton<TrashManager>.Instance.CreateTrashItem(trashItemData.TrashID, trashItemData.Position, trashItemData.Rotation, Vector3.zero, trashItemData.GUID, startKinematic: true) : NetworkSingleton<TrashManager>.Instance.CreateTrashBag(trashItemData.TrashID, trashItemData.Position, trashItemData.Rotation, trashItemData.Contents, Vector3.zero, trashItemData.GUID, startKinematic: true));
					if ((Object)(object)trashItem != (Object)null)
					{
						trashItem.HasChanged = false;
					}
				}
			}
		}
		else
		{
			string path = Path.Combine(mainPath, "Items");
			if (Directory.Exists(mainPath) && Directory.Exists(path))
			{
				string[] files = Directory.GetFiles(path);
				for (int j = 0; j < files.Length; j++)
				{
					if (!TryLoadFile(files[j], out var contents2, autoAddExtension: false))
					{
						continue;
					}
					TrashItemData trashItemData2 = null;
					try
					{
						trashItemData2 = JsonUtility.FromJson<TrashItemData>(contents2);
					}
					catch (Exception ex2)
					{
						Debug.LogError((object)("Error loading data: " + ex2.Message));
					}
					if (trashItemData2 == null)
					{
						continue;
					}
					TrashItem trashItem2 = null;
					if (trashItemData2.DataType == "TrashBagData")
					{
						TrashBagData trashBagData = null;
						try
						{
							trashBagData = JsonUtility.FromJson<TrashBagData>(contents2);
						}
						catch (Exception ex3)
						{
							Debug.LogError((object)("Error loading data: " + ex3.Message));
						}
						if (trashBagData != null)
						{
							trashItem2 = NetworkSingleton<TrashManager>.Instance.CreateTrashBag(trashBagData.TrashID, trashBagData.Position, trashBagData.Rotation, trashBagData.Contents, Vector3.zero, trashBagData.GUID, startKinematic: true);
						}
					}
					else
					{
						trashItem2 = NetworkSingleton<TrashManager>.Instance.CreateTrashItem(trashItemData2.TrashID, trashItemData2.Position, trashItemData2.Rotation, Vector3.zero, trashItemData2.GUID, startKinematic: true);
					}
					if ((Object)(object)trashItem2 != (Object)null)
					{
						trashItem2.HasChanged = false;
					}
				}
			}
		}
		if (trashData != null && trashData.Generators != null)
		{
			TrashGeneratorData[] generators = trashData.Generators;
			foreach (TrashGeneratorData trashGeneratorData in generators)
			{
				if (trashGeneratorData == null)
				{
					continue;
				}
				TrashGenerator trashGenerator = GUIDManager.GetObject<TrashGenerator>(new Guid(trashGeneratorData.GUID));
				if (!((Object)(object)trashGenerator != (Object)null))
				{
					continue;
				}
				for (int k = 0; k < trashGeneratorData.GeneratedItems.Length; k++)
				{
					TrashItem trashItem3 = GUIDManager.GetObject<TrashItem>(new Guid(trashGeneratorData.GeneratedItems[k]));
					if ((Object)(object)trashItem3 != (Object)null)
					{
						trashGenerator.AddGeneratedTrash(trashItem3);
					}
				}
				trashGenerator.HasChanged = false;
			}
			return;
		}
		Console.Log("Loading legacy trash generators at: " + mainPath);
		string path2 = Path.Combine(mainPath, "Generators");
		if (!Directory.Exists(mainPath) || !Directory.Exists(path2))
		{
			return;
		}
		string[] files2 = Directory.GetFiles(path2);
		for (int l = 0; l < files2.Length; l++)
		{
			if (!TryLoadFile(files2[l], out var contents3, autoAddExtension: false))
			{
				continue;
			}
			TrashGeneratorData trashGeneratorData2 = null;
			try
			{
				trashGeneratorData2 = JsonUtility.FromJson<TrashGeneratorData>(contents3);
			}
			catch (Exception ex4)
			{
				Debug.LogError((object)("Error loading data: " + ex4.Message));
			}
			if (trashGeneratorData2 == null)
			{
				continue;
			}
			TrashGenerator trashGenerator2 = GUIDManager.GetObject<TrashGenerator>(new Guid(trashGeneratorData2.GUID));
			if (!((Object)(object)trashGenerator2 != (Object)null))
			{
				continue;
			}
			for (int m = 0; m < trashGeneratorData2.GeneratedItems.Length; m++)
			{
				TrashItem trashItem4 = GUIDManager.GetObject<TrashItem>(new Guid(trashGeneratorData2.GeneratedItems[m]));
				if ((Object)(object)trashItem4 != (Object)null)
				{
					trashGenerator2.AddGeneratedTrash(trashItem4);
				}
			}
			trashGenerator2.HasChanged = false;
		}
	}
}
