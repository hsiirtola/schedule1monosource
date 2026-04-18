using System.Collections.Generic;
using System.IO;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using UnityEngine;

namespace ScheduleOne.Property;

public class BusinessManager : Singleton<BusinessManager>, IBaseSaveable, ISaveable
{
	private BusinessesLoader loader = new BusinessesLoader();

	public string SaveFolderName => "Businesses";

	public string SaveFileName => "Businesses";

	public Loader Loader => loader;

	public bool ShouldSaveUnderFolder => true;

	public List<string> LocalExtraFiles { get; set; } = new List<string>();

	public List<string> LocalExtraFolders { get; set; } = new List<string>();

	public bool HasChanged { get; set; } = true;

	public int LoadOrder { get; }

	protected override void Awake()
	{
		base.Awake();
		InitializeSaveable();
	}

	public virtual void InitializeSaveable()
	{
		Singleton<SaveManager>.Instance.RegisterSaveable(this);
	}

	public virtual string GetSaveString()
	{
		return string.Empty;
	}

	public virtual List<string> WriteData(string parentFolderPath)
	{
		List<string> list = new List<string>();
		string containerFolder = ((ISaveable)this).GetContainerFolder(parentFolderPath);
		for (int i = 0; i < Business.UnownedBusinesses.Count; i++)
		{
			new SaveRequest(Business.UnownedBusinesses[i], containerFolder);
			list.Add(Business.UnownedBusinesses[i].SaveFolderName);
		}
		for (int j = 0; j < Business.OwnedBusinesses.Count; j++)
		{
			new SaveRequest(Business.OwnedBusinesses[j], containerFolder);
			list.Add(Business.OwnedBusinesses[j].SaveFolderName);
		}
		return list;
	}

	public virtual void DeleteUnapprovedFiles(string parentFolderPath)
	{
		string[] directories = Directory.GetDirectories(((ISaveable)this).GetContainerFolder(parentFolderPath));
		for (int i = 0; i < directories.Length; i++)
		{
			new DirectoryInfo(directories[i]);
			Directory.Delete(directories[i], recursive: true);
		}
	}

	public void LoadBusiness(BusinessData businessData, string dataString)
	{
		if (businessData == null)
		{
			Debug.LogWarning((object)"Business data is null!");
			return;
		}
		Business business = Business.Businesses.FirstOrDefault((Business p) => (Object)(object)p != (Object)null && p.PropertyCode == businessData.PropertyCode);
		if ((Object)(object)business == (Object)null)
		{
			Debug.LogWarning((object)("Business not found: " + businessData.PropertyCode));
		}
		else
		{
			business.Load(businessData, dataString);
		}
	}
}
