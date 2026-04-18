using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.Employees;
using ScheduleOne.EntityFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Packaging;
using ScheduleOne.Persistence.Datas;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Management;

public class PackagerConfiguration : EntityConfiguration
{
	public ObjectField Home;

	public ObjectListField Stations;

	public RouteListField Routes;

	public List<PackagingStation> AssignedStations = new List<PackagingStation>();

	public List<BrickPress> AssignedBrickPresses = new List<BrickPress>();

	public int AssignedStationCount => AssignedStations.Count + AssignedBrickPresses.Count;

	public Packager packager { get; protected set; }

	public EmployeeHome assignedHome { get; private set; }

	public override bool AllowRename()
	{
		return false;
	}

	public PackagerConfiguration(ConfigurationReplicator replicator, IConfigurable configurable, Packager _packager)
		: base(replicator, configurable, ConfigurableType.GetTypeName(_packager.ConfigurableType))
	{
		packager = _packager;
		Home = new ObjectField(this);
		Home.onObjectChanged.AddListener((UnityAction<BuildableItem>)HomeChanged);
		Home.objectFilter = EmployeeHome.IsBuildableEntityAValidEmployeeHome;
		Stations = new ObjectListField(this);
		Stations.MaxItems = packager.MaxAssignedStations;
		Stations.TypeRequirements = new List<Type>
		{
			typeof(PackagingStation),
			typeof(PackagingStationMk2),
			typeof(BrickPress)
		};
		Stations.onListChanged.AddListener((UnityAction<List<BuildableItem>>)delegate
		{
			InvokeChanged();
		});
		Stations.onListChanged.AddListener((UnityAction<List<BuildableItem>>)AssignedStationsChanged);
		Stations.objectFilter = IsStationValid;
		Routes = new RouteListField(this);
		Routes.MaxRoutes = 5;
		Routes.onListChanged.AddListener((UnityAction<List<AdvancedTransitRoute>>)delegate
		{
			InvokeChanged();
		});
	}

	public override void Reset()
	{
		Home.SetObject(null, network: false);
		foreach (PackagingStation assignedStation in AssignedStations)
		{
			(assignedStation.Configuration as PackagingStationConfiguration).AssignedPackager.SetNPC(null, network: false);
		}
		foreach (BrickPress assignedBrickPress in AssignedBrickPresses)
		{
			(assignedBrickPress.Configuration as BrickPressConfiguration).AssignedPackager.SetNPC(null, network: false);
		}
		base.Reset();
	}

	private bool IsStationValid(BuildableItem obj, out string reason)
	{
		reason = string.Empty;
		if (obj is PackagingStation)
		{
			PackagingStationConfiguration packagingStationConfiguration = (obj as PackagingStation).Configuration as PackagingStationConfiguration;
			if ((Object)(object)packagingStationConfiguration.AssignedPackager.SelectedNPC != (Object)null && (Object)(object)packagingStationConfiguration.AssignedPackager.SelectedNPC != (Object)(object)packager)
			{
				reason = "Already assigned to " + packagingStationConfiguration.AssignedPackager.SelectedNPC.fullName;
				return false;
			}
			return true;
		}
		if (obj is BrickPress)
		{
			BrickPressConfiguration brickPressConfiguration = (obj as BrickPress).Configuration as BrickPressConfiguration;
			if ((Object)(object)brickPressConfiguration.AssignedPackager.SelectedNPC != (Object)null && (Object)(object)brickPressConfiguration.AssignedPackager.SelectedNPC != (Object)(object)packager)
			{
				reason = "Already assigned to " + brickPressConfiguration.AssignedPackager.SelectedNPC.fullName;
				return false;
			}
			return true;
		}
		return false;
	}

	public void AssignedStationsChanged(List<BuildableItem> objects)
	{
		for (int i = 0; i < AssignedStations.Count; i++)
		{
			if (!objects.Contains(AssignedStations[i]))
			{
				PackagingStation packagingStation = AssignedStations[i];
				AssignedStations.RemoveAt(i);
				i--;
				if ((Object)(object)(packagingStation.Configuration as PackagingStationConfiguration).AssignedPackager.SelectedNPC == (Object)(object)packager)
				{
					(packagingStation.Configuration as PackagingStationConfiguration).AssignedPackager.SetNPC(null, network: false);
				}
			}
		}
		for (int j = 0; j < AssignedBrickPresses.Count; j++)
		{
			if (!objects.Contains(AssignedBrickPresses[j]))
			{
				BrickPress brickPress = AssignedBrickPresses[j];
				AssignedBrickPresses.RemoveAt(j);
				j--;
				if ((Object)(object)(brickPress.Configuration as BrickPressConfiguration).AssignedPackager.SelectedNPC == (Object)(object)packager)
				{
					(brickPress.Configuration as BrickPressConfiguration).AssignedPackager.SetNPC(null, network: false);
				}
			}
		}
		for (int k = 0; k < objects.Count; k++)
		{
			if (objects[k] is PackagingStation)
			{
				if (!AssignedStations.Contains(objects[k]))
				{
					PackagingStation packagingStation2 = objects[k] as PackagingStation;
					AssignedStations.Add(packagingStation2);
					if ((Object)(object)(packagingStation2.Configuration as PackagingStationConfiguration).AssignedPackager.SelectedNPC != (Object)(object)packager)
					{
						(packagingStation2.Configuration as PackagingStationConfiguration).AssignedPackager.SetNPC(packager, network: false);
					}
				}
			}
			else if (objects[k] is BrickPress && !AssignedBrickPresses.Contains(objects[k]))
			{
				BrickPress brickPress2 = objects[k] as BrickPress;
				AssignedBrickPresses.Add(brickPress2);
				if ((Object)(object)(brickPress2.Configuration as BrickPressConfiguration).AssignedPackager.SelectedNPC != (Object)(object)packager)
				{
					(brickPress2.Configuration as BrickPressConfiguration).AssignedPackager.SetNPC(packager, network: false);
				}
			}
		}
	}

	public override bool ShouldSave()
	{
		if ((Object)(object)Home.SelectedObject != (Object)null)
		{
			return true;
		}
		if (AssignedStations.Count > 0)
		{
			return true;
		}
		return base.ShouldSave();
	}

	public override string GetSaveString()
	{
		return new PackagerConfigurationData(Home.GetData(), Stations.GetData(), Routes.GetData()).GetJson();
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
			assignedHome.SetAssignedEmployee(packager);
		}
		InvokeChanged();
	}
}
