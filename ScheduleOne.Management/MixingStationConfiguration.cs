using ScheduleOne.Employees;
using ScheduleOne.EntityFramework;
using ScheduleOne.NPCs;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence.Datas;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Management;

public class MixingStationConfiguration : EntityConfiguration
{
	public NPCField AssignedChemist;

	public ObjectField Destination;

	public NumberField StartThrehold;

	public MixingStation station { get; protected set; }

	public TransitRoute DestinationRoute { get; protected set; }

	public MixingStationConfiguration(ConfigurationReplicator replicator, IConfigurable configurable, MixingStation station)
		: base(replicator, configurable, station.GetDefaultManagementName())
	{
		this.station = station;
		AssignedChemist = new NPCField(this);
		AssignedChemist.TypeRequirement = typeof(Chemist);
		AssignedChemist.onNPCChanged.AddListener((UnityAction<NPC>)delegate
		{
			InvokeChanged();
		});
		Destination = new ObjectField(this);
		Destination.objectFilter = DestinationFilter;
		Destination.onObjectChanged.AddListener((UnityAction<BuildableItem>)delegate
		{
			InvokeChanged();
		});
		Destination.onObjectChanged.AddListener((UnityAction<BuildableItem>)DestinationChanged);
		Destination.DrawTransitLine = true;
		StartThrehold = new NumberField(this);
		StartThrehold.Configure(1f, station.MaxMixQuantity, wholeNumbers: true);
		StartThrehold.SetValue(1f, network: false);
		StartThrehold.onItemChanged.AddListener((UnityAction<float>)delegate
		{
			InvokeChanged();
		});
	}

	public override void Reset()
	{
		if (DestinationRoute != null)
		{
			DestinationRoute.Destroy();
			DestinationRoute = null;
		}
		base.Reset();
	}

	private void DestinationChanged(BuildableItem item)
	{
		if (DestinationRoute != null)
		{
			DestinationRoute.Destroy();
			DestinationRoute = null;
		}
		if ((Object)(object)Destination.SelectedObject != (Object)null)
		{
			DestinationRoute = new TransitRoute(station, Destination.SelectedObject as ITransitEntity);
			if (base.IsSelected)
			{
				DestinationRoute.SetVisualsActive(active: true);
			}
		}
		else
		{
			DestinationRoute = null;
		}
	}

	public bool DestinationFilter(BuildableItem obj, out string reason)
	{
		reason = "";
		if (obj is ITransitEntity && (obj as ITransitEntity).Selectable && (Object)(object)obj != (Object)(object)station)
		{
			return true;
		}
		return false;
	}

	public override void Selected()
	{
		base.Selected();
		if (DestinationRoute != null)
		{
			DestinationRoute.SetVisualsActive(active: true);
		}
	}

	public override void Deselected()
	{
		base.Deselected();
		if (DestinationRoute != null)
		{
			DestinationRoute.SetVisualsActive(active: false);
		}
	}

	public override bool ShouldSave()
	{
		if ((Object)(object)Destination.SelectedObject != (Object)null)
		{
			return true;
		}
		return base.ShouldSave();
	}

	public override string GetSaveString()
	{
		return new MixingStationConfigurationData(base.Name.GetData(), Destination.GetData(), StartThrehold.GetData()).GetJson();
	}
}
