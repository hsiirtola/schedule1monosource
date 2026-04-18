using System.Collections.Generic;
using System.Linq;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Employees;
using ScheduleOne.EntityFramework;
using ScheduleOne.Growing;
using ScheduleOne.ItemFramework;
using ScheduleOne.NPCs;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence.Datas;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Management;

public class PotConfiguration : EntityConfiguration
{
	public ItemField Seed;

	public ItemField Additive1;

	public ItemField Additive2;

	public ItemField Additive3;

	public NPCField AssignedBotanist;

	public ObjectField Destination;

	public Pot Pot { get; protected set; }

	public TransitRoute DestinationRoute { get; protected set; }

	public PotConfiguration(ConfigurationReplicator replicator, IConfigurable configurable, Pot pot)
		: base(replicator, configurable, pot.GetDefaultManagementName())
	{
		Pot = pot;
		Seed = new ItemField(this);
		Seed.CanSelectNone = true;
		List<ItemDefinition> options = Singleton<ManagementUtilities>.Instance.Seeds.Cast<ItemDefinition>().ToList();
		Seed.Options = options;
		Seed.onItemChanged.AddListener((UnityAction<ItemDefinition>)delegate
		{
			InvokeChanged();
		});
		List<ItemDefinition> options2 = Pot.AllowedAdditives.Cast<ItemDefinition>().ToList();
		Additive1 = new ItemField(this);
		Additive1.CanSelectNone = true;
		Additive1.Options = options2;
		Additive1.onItemChanged.AddListener((UnityAction<ItemDefinition>)delegate
		{
			InvokeChanged();
		});
		Additive2 = new ItemField(this);
		Additive2.CanSelectNone = true;
		Additive2.Options = options2;
		Additive2.onItemChanged.AddListener((UnityAction<ItemDefinition>)delegate
		{
			InvokeChanged();
		});
		Additive3 = new ItemField(this);
		Additive3.CanSelectNone = true;
		Additive3.Options = options2;
		Additive3.onItemChanged.AddListener((UnityAction<ItemDefinition>)delegate
		{
			InvokeChanged();
		});
		AssignedBotanist = new NPCField(this);
		AssignedBotanist.TypeRequirement = typeof(Botanist);
		AssignedBotanist.onNPCChanged.AddListener((UnityAction<NPC>)delegate
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

	public bool IsAdditiveSelected(ItemDefinition additive)
	{
		if ((Object)(object)Additive1.SelectedItem == (Object)(object)additive)
		{
			return true;
		}
		if ((Object)(object)Additive2.SelectedItem == (Object)(object)additive)
		{
			return true;
		}
		if ((Object)(object)Additive3.SelectedItem == (Object)(object)additive)
		{
			return true;
		}
		return false;
	}

	public string[] GetSelectedSeedIDs()
	{
		if ((Object)(object)Seed.SelectedItem != (Object)null)
		{
			return new string[1] { ((BaseItemDefinition)Seed.SelectedItem).ID };
		}
		return Singleton<ManagementUtilities>.Instance.Seeds.Select((SeedDefinition s) => ((BaseItemDefinition)s).ID).ToArray();
	}

	public override void Reset()
	{
		if ((Object)(object)AssignedBotanist.SelectedNPC != (Object)null)
		{
			((AssignedBotanist.SelectedNPC as Botanist).Configuration as BotanistConfiguration).Assigns.RemoveItem(Pot);
		}
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
			DestinationRoute = new TransitRoute(Pot, Destination.SelectedObject as ITransitEntity);
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
		if (obj is ITransitEntity && (obj as ITransitEntity).Selectable && (Object)(object)obj != (Object)(object)Pot)
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
		if ((Object)(object)Seed.SelectedItem != (Object)null)
		{
			return true;
		}
		if ((Object)(object)Additive1.SelectedItem != (Object)null)
		{
			return true;
		}
		if ((Object)(object)Additive2.SelectedItem != (Object)null)
		{
			return true;
		}
		if ((Object)(object)Additive3.SelectedItem != (Object)null)
		{
			return true;
		}
		if ((Object)(object)AssignedBotanist.SelectedNPC != (Object)null)
		{
			return true;
		}
		if ((Object)(object)Destination.SelectedObject != (Object)null)
		{
			return true;
		}
		return base.ShouldSave();
	}

	public override string GetSaveString()
	{
		return new PotConfigurationData(base.Name.GetData(), Seed.GetData(), Additive1.GetData(), Additive2.GetData(), Additive3.GetData(), Destination.GetData()).GetJson();
	}
}
