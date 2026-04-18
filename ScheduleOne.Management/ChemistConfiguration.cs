using System;
using System.Collections.Generic;
using ScheduleOne.Employees;
using ScheduleOne.EntityFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence.Datas;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Management;

public class ChemistConfiguration : EntityConfiguration
{
	public ObjectField Home;

	public ObjectListField Stations;

	public List<ChemistryStation> ChemStations = new List<ChemistryStation>();

	public List<LabOven> LabOvens = new List<LabOven>();

	public List<Cauldron> Cauldrons = new List<Cauldron>();

	public List<MixingStation> MixStations = new List<MixingStation>();

	public int TotalStations => ChemStations.Count + LabOvens.Count + Cauldrons.Count + MixStations.Count;

	public Chemist chemist { get; protected set; }

	public EmployeeHome assignedHome { get; private set; }

	public override bool AllowRename()
	{
		return false;
	}

	public ChemistConfiguration(ConfigurationReplicator replicator, IConfigurable configurable, Chemist _chemist)
		: base(replicator, configurable, ConfigurableType.GetTypeName(_chemist.ConfigurableType))
	{
		chemist = _chemist;
		Home = new ObjectField(this);
		Home.onObjectChanged.AddListener((UnityAction<BuildableItem>)HomeChanged);
		Home.objectFilter = EmployeeHome.IsBuildableEntityAValidEmployeeHome;
		Stations = new ObjectListField(this);
		Stations.MaxItems = 4;
		Stations.TypeRequirements = new List<Type>
		{
			typeof(ChemistryStation),
			typeof(LabOven),
			typeof(Cauldron),
			typeof(MixingStation),
			typeof(MixingStationMk2)
		};
		Stations.onListChanged.AddListener((UnityAction<List<BuildableItem>>)delegate
		{
			InvokeChanged();
		});
		Stations.onListChanged.AddListener((UnityAction<List<BuildableItem>>)AssignedStationsChanged);
		Stations.objectFilter = IsStationValid;
	}

	public override void Reset()
	{
		Home.SetObject(null, network: false);
		foreach (ChemistryStation chemStation in ChemStations)
		{
			(chemStation.Configuration as ChemistryStationConfiguration).AssignedChemist.SetNPC(null, network: false);
		}
		foreach (LabOven labOven in LabOvens)
		{
			(labOven.Configuration as LabOvenConfiguration).AssignedChemist.SetNPC(null, network: false);
		}
		foreach (Cauldron cauldron in Cauldrons)
		{
			(cauldron.Configuration as CauldronConfiguration).AssignedChemist.SetNPC(null, network: false);
		}
		foreach (MixingStation mixStation in MixStations)
		{
			(mixStation.Configuration as MixingStationConfiguration).AssignedChemist.SetNPC(null, network: false);
		}
		base.Reset();
	}

	private bool IsStationValid(BuildableItem obj, out string reason)
	{
		reason = string.Empty;
		if (obj is ChemistryStation)
		{
			ChemistryStationConfiguration chemistryStationConfiguration = (obj as ChemistryStation).Configuration as ChemistryStationConfiguration;
			if ((Object)(object)chemistryStationConfiguration.AssignedChemist.SelectedNPC != (Object)null && (Object)(object)chemistryStationConfiguration.AssignedChemist.SelectedNPC != (Object)(object)chemist)
			{
				reason = "Already assigned to " + chemistryStationConfiguration.AssignedChemist.SelectedNPC.fullName;
				return false;
			}
			return true;
		}
		if (obj is LabOven)
		{
			LabOvenConfiguration labOvenConfiguration = (obj as LabOven).Configuration as LabOvenConfiguration;
			if ((Object)(object)labOvenConfiguration.AssignedChemist.SelectedNPC != (Object)null && (Object)(object)labOvenConfiguration.AssignedChemist.SelectedNPC != (Object)(object)chemist)
			{
				reason = "Already assigned to " + labOvenConfiguration.AssignedChemist.SelectedNPC.fullName;
				return false;
			}
			return true;
		}
		if (obj is Cauldron)
		{
			CauldronConfiguration cauldronConfiguration = (obj as Cauldron).Configuration as CauldronConfiguration;
			if ((Object)(object)cauldronConfiguration.AssignedChemist.SelectedNPC != (Object)null && (Object)(object)cauldronConfiguration.AssignedChemist.SelectedNPC != (Object)(object)chemist)
			{
				reason = "Already assigned to " + cauldronConfiguration.AssignedChemist.SelectedNPC.fullName;
				return false;
			}
			return true;
		}
		if (obj is MixingStation)
		{
			MixingStationConfiguration mixingStationConfiguration = (obj as MixingStation).Configuration as MixingStationConfiguration;
			if ((Object)(object)mixingStationConfiguration.AssignedChemist.SelectedNPC != (Object)null && (Object)(object)mixingStationConfiguration.AssignedChemist.SelectedNPC != (Object)(object)chemist)
			{
				reason = "Already assigned to " + mixingStationConfiguration.AssignedChemist.SelectedNPC.fullName;
				return false;
			}
			return true;
		}
		return false;
	}

	public void AssignedStationsChanged(List<BuildableItem> objects)
	{
		for (int i = 0; i < ChemStations.Count; i++)
		{
			if (!objects.Contains(ChemStations[i]))
			{
				ChemistryStation chemistryStation = ChemStations[i];
				ChemStations.RemoveAt(i);
				i--;
				if ((Object)(object)(chemistryStation.Configuration as ChemistryStationConfiguration).AssignedChemist.SelectedNPC == (Object)(object)chemist)
				{
					(chemistryStation.Configuration as ChemistryStationConfiguration).AssignedChemist.SetNPC(null, network: false);
				}
			}
		}
		for (int j = 0; j < LabOvens.Count; j++)
		{
			if (!objects.Contains(LabOvens[j]))
			{
				LabOven labOven = LabOvens[j];
				LabOvens.RemoveAt(j);
				j--;
				if ((Object)(object)(labOven.Configuration as LabOvenConfiguration).AssignedChemist.SelectedNPC == (Object)(object)chemist)
				{
					(labOven.Configuration as LabOvenConfiguration).AssignedChemist.SetNPC(null, network: false);
				}
			}
		}
		for (int k = 0; k < Cauldrons.Count; k++)
		{
			if (!objects.Contains(Cauldrons[k]))
			{
				Cauldron cauldron = Cauldrons[k];
				Cauldrons.RemoveAt(k);
				k--;
				if ((Object)(object)(cauldron.Configuration as CauldronConfiguration).AssignedChemist.SelectedNPC == (Object)(object)chemist)
				{
					(cauldron.Configuration as CauldronConfiguration).AssignedChemist.SetNPC(null, network: false);
				}
			}
		}
		for (int l = 0; l < MixStations.Count; l++)
		{
			if (!objects.Contains(MixStations[l]))
			{
				MixingStation mixingStation = MixStations[l];
				MixStations.RemoveAt(l);
				l--;
				if ((Object)(object)(mixingStation.Configuration as MixingStationConfiguration).AssignedChemist.SelectedNPC == (Object)(object)chemist)
				{
					(mixingStation.Configuration as MixingStationConfiguration).AssignedChemist.SetNPC(null, network: false);
				}
			}
		}
		for (int m = 0; m < objects.Count; m++)
		{
			if (objects[m] is ChemistryStation && !ChemStations.Contains(objects[m] as ChemistryStation))
			{
				ChemistryStation chemistryStation2 = objects[m] as ChemistryStation;
				ChemStations.Add(chemistryStation2);
				if ((Object)(object)(chemistryStation2.Configuration as ChemistryStationConfiguration).AssignedChemist.SelectedNPC != (Object)(object)chemist)
				{
					(chemistryStation2.Configuration as ChemistryStationConfiguration).AssignedChemist.SetNPC(chemist, network: false);
				}
			}
			if (objects[m] is LabOven && !LabOvens.Contains(objects[m] as LabOven))
			{
				LabOven labOven2 = objects[m] as LabOven;
				LabOvens.Add(labOven2);
				if ((Object)(object)(labOven2.Configuration as LabOvenConfiguration).AssignedChemist.SelectedNPC != (Object)(object)chemist)
				{
					(labOven2.Configuration as LabOvenConfiguration).AssignedChemist.SetNPC(chemist, network: false);
				}
			}
			if (objects[m] is Cauldron && !Cauldrons.Contains(objects[m] as Cauldron))
			{
				Cauldron cauldron2 = objects[m] as Cauldron;
				Cauldrons.Add(cauldron2);
				if ((Object)(object)(cauldron2.Configuration as CauldronConfiguration).AssignedChemist.SelectedNPC != (Object)(object)chemist)
				{
					(cauldron2.Configuration as CauldronConfiguration).AssignedChemist.SetNPC(chemist, network: false);
				}
			}
			if (objects[m] is MixingStation && !MixStations.Contains(objects[m] as MixingStation))
			{
				MixingStation mixingStation2 = objects[m] as MixingStation;
				MixStations.Add(mixingStation2);
				if ((Object)(object)(mixingStation2.Configuration as MixingStationConfiguration).AssignedChemist.SelectedNPC != (Object)(object)chemist)
				{
					(mixingStation2.Configuration as MixingStationConfiguration).AssignedChemist.SetNPC(chemist, network: false);
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
		if (ChemStations.Count > 0)
		{
			return true;
		}
		if (LabOvens.Count > 0)
		{
			return true;
		}
		if (Cauldrons.Count > 0)
		{
			return true;
		}
		return base.ShouldSave();
	}

	public override string GetSaveString()
	{
		return new ChemistConfigurationData(Home.GetData(), Stations.GetData()).GetJson();
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
			assignedHome.SetAssignedEmployee(chemist);
		}
		InvokeChanged();
	}
}
