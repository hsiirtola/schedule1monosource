using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using UnityEngine;

namespace ScheduleOne.Property;

public class PropertyManager : Singleton<PropertyManager>, IBaseSaveable, ISaveable
{
	private PropertiesLoader loader = new PropertiesLoader();

	public string SaveFolderName => "Properties";

	public string SaveFileName => "Properties";

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
		for (int i = 0; i < Property.OwnedProperties.Count; i++)
		{
			try
			{
				if (Property.OwnedProperties[i].ShouldSave() && !(Property.OwnedProperties[i] is Business))
				{
					new SaveRequest(Property.OwnedProperties[i], containerFolder);
					list.Add(Property.OwnedProperties[i].SaveFolderName);
				}
			}
			catch (Exception ex)
			{
				Console.LogError("Error saving property: " + Property.OwnedProperties[i].PropertyCode + " - " + ex.Message);
				SaveManager.ReportSaveError();
			}
		}
		for (int j = 0; j < Property.UnownedProperties.Count; j++)
		{
			try
			{
				if (Property.UnownedProperties[j].ShouldSave() && !(Property.UnownedProperties[j] is Business))
				{
					new SaveRequest(Property.UnownedProperties[j], containerFolder);
					list.Add(Property.UnownedProperties[j].SaveFolderName);
				}
			}
			catch (Exception ex2)
			{
				Console.LogError("Error saving property: " + Property.OwnedProperties[j].PropertyCode + " - " + ex2.Message);
				SaveManager.ReportSaveError();
			}
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

	public void LoadProperty(PropertyData propertyData, string dataString)
	{
		if (propertyData == null)
		{
			Console.LogWarning("Property data is null!");
			return;
		}
		Property property = Property.Properties.FirstOrDefault((Property p) => (Object)(object)p != (Object)null && p.PropertyCode == propertyData.PropertyCode);
		if ((Object)(object)property == (Object)null)
		{
			property = Business.Businesses.FirstOrDefault((Business p) => (Object)(object)p != (Object)null && p.PropertyCode == propertyData.PropertyCode);
		}
		if ((Object)(object)property == (Object)null)
		{
			Console.LogWarning("Property not found for data: " + propertyData.PropertyCode);
		}
		else
		{
			property.Load(propertyData, dataString);
		}
	}

	public Property GetProperty(string code)
	{
		Property property = Property.UnownedProperties.FirstOrDefault((Property p) => p.PropertyCode == code);
		if ((Object)(object)property == (Object)null)
		{
			property = Property.OwnedProperties.FirstOrDefault((Property p) => p.PropertyCode == code);
		}
		return property;
	}

	public Property GetNearestProperty(Vector3 point, bool includeOwned = true, bool includeUnowned = true, bool includeBusinesses = true)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		List<Property> list = new List<Property>();
		if (includeOwned)
		{
			list.AddRange(Property.OwnedProperties);
		}
		if (includeUnowned)
		{
			list.AddRange(Property.UnownedProperties);
		}
		if (includeBusinesses)
		{
			if (includeOwned)
			{
				list.AddRange(Business.OwnedBusinesses);
			}
			if (includeUnowned)
			{
				list.AddRange(Business.UnownedBusinesses);
			}
		}
		return list.OrderBy((Property p) => Vector3.SqrMagnitude(((Component)p).transform.position - point)).FirstOrDefault();
	}
}
