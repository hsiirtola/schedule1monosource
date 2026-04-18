using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.Employees;
using ScheduleOne.EntityFramework;
using ScheduleOne.NPCs;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.StationFramework;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Management;

public class BotanistConfiguration : EntityConfiguration
{
	public static readonly Type[] AssignableTypes = new Type[4]
	{
		typeof(Pot),
		typeof(DryingRack),
		typeof(MushroomBed),
		typeof(MushroomSpawnStation)
	};

	public ObjectField Home;

	public ObjectField Supplies;

	public ObjectListField Assigns;

	private List<BuildableItem> _thisBotanistAssignedOn = new List<BuildableItem>();

	private Botanist _botanist;

	public List<Pot> AssignedPots { get; private set; } = new List<Pot>();

	public List<DryingRack> AssignedRacks { get; private set; } = new List<DryingRack>();

	public List<MushroomBed> AssignedBeds { get; private set; } = new List<MushroomBed>();

	public List<MushroomSpawnStation> AssignedSpawnStations { get; private set; } = new List<MushroomSpawnStation>();

	public EmployeeHome AssignedHome { get; private set; }

	public override bool AllowRename()
	{
		return false;
	}

	public BotanistConfiguration(ConfigurationReplicator replicator, IConfigurable configurable, Botanist _botanist)
		: base(replicator, configurable, ConfigurableType.GetTypeName(_botanist.ConfigurableType))
	{
		this._botanist = _botanist;
		Home = new ObjectField(this);
		Home.onObjectChanged.AddListener((UnityAction<BuildableItem>)HomeChanged);
		Home.objectFilter = EmployeeHome.IsBuildableEntityAValidEmployeeHome;
		Supplies = new ObjectField(this);
		Supplies.TypeRequirements = new List<Type> { typeof(PlaceableStorageEntity) };
		Supplies.onObjectChanged.AddListener((UnityAction<BuildableItem>)delegate
		{
			InvokeChanged();
		});
		Assigns = new ObjectListField(this);
		Assigns.MaxItems = this._botanist.MaxAssignedPots;
		Assigns.TypeRequirements = AssignableTypes.ToList();
		Assigns.onListChanged.AddListener((UnityAction<List<BuildableItem>>)delegate
		{
			InvokeChanged();
		});
		Assigns.onListChanged.AddListener((UnityAction<List<BuildableItem>>)AssignsChanged);
		Assigns.objectFilter = IsStationValid;
	}

	public override void Reset()
	{
		Home.SetObject(null, network: false);
		Supplies.SetObject(null, network: false);
		Assigns.SetList(new List<BuildableItem>(), network: false);
		base.Reset();
	}

	private bool IsStationValid(BuildableItem obj, out string reason)
	{
		reason = string.Empty;
		if (!AssignableTypes.Contains(((object)obj).GetType()))
		{
			reason = "Can't be assigned to botanist";
			return false;
		}
		NPC nPC = null;
		Pot pot = obj as Pot;
		if ((Object)(object)pot != (Object)null)
		{
			nPC = (pot.Configuration as PotConfiguration).AssignedBotanist.SelectedNPC;
		}
		DryingRack dryingRack = obj as DryingRack;
		if ((Object)(object)dryingRack != (Object)null)
		{
			nPC = (dryingRack.Configuration as DryingRackConfiguration).AssignedBotanist.SelectedNPC;
		}
		MushroomBed mushroomBed = obj as MushroomBed;
		if ((Object)(object)mushroomBed != (Object)null)
		{
			nPC = (mushroomBed.Configuration as MushroomBedConfiguration).AssignedBotanist.SelectedNPC;
		}
		MushroomSpawnStation mushroomSpawnStation = obj as MushroomSpawnStation;
		if ((Object)(object)mushroomSpawnStation != (Object)null)
		{
			nPC = (mushroomSpawnStation.Configuration as SpawnStationConfiguration).AssignedBotanist.SelectedNPC;
		}
		if ((Object)(object)nPC != (Object)null && (Object)(object)nPC != (Object)(object)_botanist)
		{
			reason = "Already assigned to " + nPC.fullName;
			return false;
		}
		return true;
	}

	public void AssignsChanged(List<BuildableItem> objects)
	{
		AssignedPots.Clear();
		AssignedRacks.Clear();
		AssignedBeds.Clear();
		AssignedSpawnStations.Clear();
		for (int i = 0; i < _thisBotanistAssignedOn.Count; i++)
		{
			if (!((Object)(object)_thisBotanistAssignedOn[i] == (Object)null) && !objects.Contains(_thisBotanistAssignedOn[i]))
			{
				NPCField nPCField = GetNPCField(_thisBotanistAssignedOn[i] as IConfigurable);
				if (nPCField != null && (Object)(object)nPCField.SelectedNPC == (Object)(object)_botanist)
				{
					nPCField.SetNPC(null, network: false);
				}
				_thisBotanistAssignedOn.RemoveAt(i);
				i--;
			}
		}
		foreach (BuildableItem @object in objects)
		{
			if (!((Object)(object)@object == (Object)null) && !_thisBotanistAssignedOn.Contains(@object))
			{
				_thisBotanistAssignedOn.Add(@object);
				GetNPCField(@object as IConfigurable)?.SetNPC(_botanist, network: false);
			}
		}
		AssignedPots = objects.OfType<Pot>().ToList();
		AssignedRacks = objects.OfType<DryingRack>().ToList();
		AssignedBeds = objects.OfType<MushroomBed>().ToList();
		AssignedSpawnStations = objects.OfType<MushroomSpawnStation>().ToList();
	}

	private NPCField GetNPCField(IConfigurable configurable)
	{
		if (configurable == null)
		{
			return null;
		}
		if (configurable.Configuration == null)
		{
			return null;
		}
		return configurable.Configuration.GetField<NPCField>();
	}

	public override bool ShouldSave()
	{
		if (Assigns.SelectedObjects.Count > 0)
		{
			return true;
		}
		if ((Object)(object)Supplies.SelectedObject != (Object)null)
		{
			return true;
		}
		if ((Object)(object)Home.SelectedObject != (Object)null)
		{
			return true;
		}
		return base.ShouldSave();
	}

	public override string GetSaveString()
	{
		return new BotanistConfigurationData(Home.GetData(), Supplies.GetData(), Assigns.GetData()).GetJson();
	}

	private void HomeChanged(BuildableItem newItem)
	{
		EmployeeHome assignedHome = AssignedHome;
		if ((Object)(object)assignedHome != (Object)null)
		{
			assignedHome.SetAssignedEmployee(null);
		}
		AssignedHome = (((Object)(object)newItem != (Object)null) ? ((Component)newItem).GetComponent<EmployeeHome>() : null);
		if ((Object)(object)AssignedHome != (Object)null)
		{
			AssignedHome.SetAssignedEmployee(_botanist);
		}
		InvokeChanged();
	}
}
