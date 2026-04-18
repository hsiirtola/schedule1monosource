using System.Collections.Generic;
using System.IO;
using ScheduleOne.Persistence.Datas;

namespace ScheduleOne.Persistence.Loaders;

public class BusinessesLoader : Loader
{
	public override void Load(string mainPath)
	{
		BusinessLoader businessLoader = new BusinessLoader();
		List<FileInfo> files = GetFiles(mainPath);
		bool flag = false;
		if (files.Count > 0)
		{
			flag = true;
			foreach (FileInfo item in files)
			{
				string fullName = item.FullName;
				if (TryLoadFile(fullName, out var contents, autoAddExtension: false) && Loader.TryDeserialize<BusinessData>(contents, out var data))
				{
					businessLoader.Load(data, contents);
				}
			}
		}
		if (!flag && Directory.Exists(mainPath))
		{
			List<DirectoryInfo> directories = GetDirectories(mainPath);
			for (int i = 0; i < directories.Count; i++)
			{
				new LoadRequest(directories[i].FullName, businessLoader);
			}
		}
	}
}
