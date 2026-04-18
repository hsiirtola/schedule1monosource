using ScheduleOne.Employees;
using ScheduleOne.EntityFramework;
using ScheduleOne.ItemFramework;
using ScheduleOne.NPCs;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence.Datas;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Management;

public class DryingRackConfiguration : EntityConfiguration
{
	public NPCField AssignedBotanist;

	public QualityField TargetQuality;

	public NumberField StartThreshold;

	public ObjectField Destination;

	public DryingRack Rack { get; protected set; }

	public TransitRoute DestinationRoute { get; protected set; }

	public DryingRackConfiguration(ConfigurationReplicator replicator, IConfigurable configurable, DryingRack rack)
		: base(replicator, configurable, rack.GetDefaultManagementName())
	{
		Rack = rack;
		AssignedBotanist = new NPCField(this);
		AssignedBotanist.TypeRequirement = typeof(Botanist);
		AssignedBotanist.onNPCChanged.AddListener((UnityAction<NPC>)delegate
		{
			InvokeChanged();
		});
		TargetQuality = new QualityField(this);
		TargetQuality.onValueChanged.AddListener((UnityAction<EQuality>)delegate
		{
			InvokeChanged();
		});
		TargetQuality.SetValue(EQuality.Premium, network: false);
		Destination = new ObjectField(this);
		Destination.objectFilter = DestinationFilter;
		Destination.onObjectChanged.AddListener((UnityAction<BuildableItem>)delegate
		{
			InvokeChanged();
		});
		Destination.onObjectChanged.AddListener((UnityAction<BuildableItem>)DestinationChanged);
		Destination.DrawTransitLine = true;
		StartThreshold = new NumberField(this);
		StartThreshold.Configure(1f, Rack.ItemCapacity, wholeNumbers: true);
		StartThreshold.SetValue(1f, network: false);
		StartThreshold.onItemChanged.AddListener((UnityAction<float>)delegate
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
			DestinationRoute = new TransitRoute(Rack, Destination.SelectedObject as ITransitEntity);
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
		if (obj is ITransitEntity && (obj as ITransitEntity).Selectable && (Object)(object)obj != (Object)(object)Rack)
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
		return new DryingRackConfigurationData(base.Name.GetData(), TargetQuality.GetData(), Destination.GetData(), StartThreshold.GetData()).GetJson();
	}
}
