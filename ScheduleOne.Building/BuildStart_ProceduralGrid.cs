using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.ItemFramework;
using ScheduleOne.Tiles;
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.Building;

public class BuildStart_ProceduralGrid : BuildStart_Base
{
	public override void StartBuilding(ItemInstance itemInstance)
	{
		ProceduralGridItem proceduralGridItem = CreateGhostModel(itemInstance.Definition as BuildableItemDefinition);
		if (!((Object)(object)proceduralGridItem == (Object)null))
		{
			Singleton<HUD>.Instance.SetCrosshairVisible(vis: false);
			ProceduralGridItem component = ((Component)proceduralGridItem).GetComponent<ProceduralGridItem>();
			((Component)this).gameObject.GetComponent<BuildUpdate_ProceduralGrid>().GhostModel = ((Component)proceduralGridItem).gameObject;
			((Component)this).gameObject.GetComponent<BuildUpdate_ProceduralGrid>().ItemClass = component;
			((Component)this).gameObject.GetComponent<BuildUpdate_ProceduralGrid>().ItemInstance = itemInstance;
			Singleton<InputPromptsCanvas>.Instance.LoadModule("building");
			for (int i = 0; i < component.CoordinateFootprintTilePairs.Count; i++)
			{
				component.CoordinateFootprintTilePairs[i].footprintTile.tileDetector.tileDetectionMode = ETileDetectionMode.ProceduralTile;
			}
		}
	}

	protected virtual ProceduralGridItem CreateGhostModel(BuildableItemDefinition itemDefinition)
	{
		itemDefinition.BuiltItem.isGhost = true;
		GameObject val = Object.Instantiate<GameObject>(((Component)itemDefinition.BuiltItem).gameObject, ((Component)this).transform);
		itemDefinition.BuiltItem.isGhost = false;
		ProceduralGridItem component = val.GetComponent<ProceduralGridItem>();
		if ((Object)(object)component == (Object)null)
		{
			Console.LogWarning("CreateGhostModel: asset path is not a BuildableItem!");
			return null;
		}
		((Behaviour)component).enabled = false;
		component.isGhost = true;
		NetworkSingleton<BuildManager>.Instance.DisableColliders(val);
		NetworkSingleton<BuildManager>.Instance.DisableNavigation(val);
		NetworkSingleton<BuildManager>.Instance.DisableNetworking(val);
		component.SetFootprintTileVisiblity(visible: false);
		return component;
	}
}
