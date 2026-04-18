using System.Collections.Generic;
using ScheduleOne.Employees;
using ScheduleOne.EntityFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence.Datas;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Management;

public class CleanerConfiguration : EntityConfiguration
{
	public ObjectField Home;

	public ObjectListField Bins;

	public Cleaner cleaner { get; protected set; }

	public List<TrashContainerItem> binItems { get; private set; } = new List<TrashContainerItem>();

	public EmployeeHome assignedHome { get; private set; }

	public override bool AllowRename()
	{
		return false;
	}

	public CleanerConfiguration(ConfigurationReplicator replicator, IConfigurable configurable, Cleaner _cleaner)
		: base(replicator, configurable, ConfigurableType.GetTypeName(_cleaner.ConfigurableType))
	{
		cleaner = _cleaner;
		Home = new ObjectField(this);
		Home.onObjectChanged.AddListener((UnityAction<BuildableItem>)HomeChanged);
		Home.objectFilter = EmployeeHome.IsBuildableEntityAValidEmployeeHome;
		Bins = new ObjectListField(this);
		Bins.MaxItems = 6;
		Bins.onListChanged.AddListener((UnityAction<List<BuildableItem>>)delegate
		{
			InvokeChanged();
		});
		Bins.onListChanged.AddListener((UnityAction<List<BuildableItem>>)AssignedBinsChanged);
		Bins.objectFilter = IsObjValid;
	}

	public override void Reset()
	{
		Home.SetObject(null, network: false);
		base.Reset();
	}

	private bool IsObjValid(BuildableItem obj, out string reason)
	{
		TrashContainerItem trashContainerItem = obj as TrashContainerItem;
		if ((Object)(object)trashContainerItem == (Object)null)
		{
			reason = string.Empty;
			return false;
		}
		if (!trashContainerItem.UsableByCleaners)
		{
			reason = "This trash can is not usable by cleaners.";
			return false;
		}
		reason = string.Empty;
		return true;
	}

	public void AssignedBinsChanged(List<BuildableItem> objects)
	{
		for (int i = 0; i < binItems.Count; i++)
		{
			if (!objects.Contains(binItems[i]))
			{
				binItems.RemoveAt(i);
				i--;
			}
		}
		for (int j = 0; j < objects.Count; j++)
		{
			if (!binItems.Contains(objects[j] as TrashContainerItem))
			{
				binItems.Add(objects[j] as TrashContainerItem);
			}
		}
	}

	public override bool ShouldSave()
	{
		if ((Object)(object)Home.SelectedObject != (Object)null)
		{
			return true;
		}
		if (Bins.SelectedObjects.Count > 0)
		{
			return true;
		}
		return base.ShouldSave();
	}

	public override string GetSaveString()
	{
		return new CleanerConfigurationData(Home.GetData(), Bins.GetData()).GetJson();
	}

	private void HomeChanged(BuildableItem newItem)
	{
		EmployeeHome employeeHome = assignedHome;
		if ((Object)(object)employeeHome != (Object)null)
		{
			employeeHome.SetAssignedEmployee(null);
		}
		assignedHome = (((Object)(object)newItem != (Object)null) ? ((Component)newItem).GetComponent<EmployeeHome>() : null);
		if ((Object)(object)assignedHome != (Object)null)
		{
			assignedHome.SetAssignedEmployee(cleaner);
		}
		InvokeChanged();
	}
}
