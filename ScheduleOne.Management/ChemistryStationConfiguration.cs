using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.NPCs;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.StationFramework;
using ScheduleOne.UI.Stations;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Management;

public class ChemistryStationConfiguration : EntityConfiguration
{
	public NPCField AssignedChemist;

	public StationRecipeField Recipe;

	public ObjectField Destination;

	public ChemistryStation Station { get; protected set; }

	public TransitRoute DestinationRoute { get; protected set; }

	public ChemistryStationConfiguration(ConfigurationReplicator replicator, IConfigurable configurable, ChemistryStation station)
		: base(replicator, configurable, station.GetDefaultManagementName())
	{
		Station = station;
		AssignedChemist = new NPCField(this);
		AssignedChemist.onNPCChanged.AddListener((UnityAction<NPC>)delegate
		{
			InvokeChanged();
		});
		Recipe = new StationRecipeField(this);
		Recipe.Options = Singleton<ChemistryStationCanvas>.Instance.Recipes;
		Recipe.onRecipeChanged.AddListener((UnityAction<StationRecipe>)delegate
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
			DestinationRoute = new TransitRoute(Station, Destination.SelectedObject as ITransitEntity);
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
		if (obj is ITransitEntity && (obj as ITransitEntity).Selectable && (Object)(object)obj != (Object)(object)Station)
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
		return new ChemistryStationConfigurationData(base.Name.GetData(), Recipe.GetData(), Destination.GetData()).GetJson();
	}
}
