using System.Collections.Generic;
using System.IO;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Property;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders;

public class PropertiesLoader : Loader
{
	public override void Load(string mainPath)
	{
		PropertyLoader propertyLoader = new PropertyLoader();
		bool flag = false;
		List<FileInfo> files = GetFiles(mainPath);
		bool flag2 = false;
		if (files.Count > 0)
		{
			foreach (FileInfo item in files)
			{
				if (item == null)
				{
					continue;
				}
				string fullName = item.FullName;
				Console.Log("Loading property file: " + fullName);
				if (TryLoadPropertyFile(fullName, out var propertyData))
				{
					flag2 = true;
					if (propertyData.PropertyCode == "seweroffice")
					{
						flag = true;
					}
				}
			}
		}
		if (!flag)
		{
			SewerOffice sewerOffice = Object.FindObjectOfType<SewerOffice>();
			if ((Object)(object)sewerOffice != (Object)null && TryLoadPropertyFile(sewerOffice.GetDefaultSaveFileFullPath(), out var _))
			{
				Console.Log("Loaded sewer office property data from default path.");
			}
		}
		if (!flag2 && Directory.Exists(mainPath))
		{
			List<DirectoryInfo> directories = GetDirectories(mainPath);
			for (int i = 0; i < directories.Count; i++)
			{
				new LoadRequest(directories[i].FullName, propertyLoader);
			}
		}
		bool TryLoadPropertyFile(string path, out PropertyData reference)
		{
			reference = null;
			if (TryLoadFile(path, out var contents, autoAddExtension: false) && Loader.TryDeserialize<PropertyData>(contents, out reference))
			{
				if (reference.DataType == "BusinessData")
				{
					Debug.LogWarning((object)("Skipped loading business data under property folder: " + path));
					return false;
				}
				propertyLoader.Load(reference, contents);
				return true;
			}
			return false;
		}
	}
}
