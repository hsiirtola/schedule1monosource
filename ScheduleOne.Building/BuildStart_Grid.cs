using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.ItemFramework;
using ScheduleOne.Tiles;
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.Building;

public class BuildStart_Grid : BuildStart_Base
{
	protected GridItem ghostModelClass;

	public override void StartBuilding(ItemInstance itemInstance)
	{
		ghostModelClass = CreateGhostModel(itemInstance.Definition as BuildableItemDefinition);
		Singleton<HUD>.Instance.SetCrosshairVisible(vis: false);
		for (int i = 0; i < ghostModelClass.CoordinateFootprintTilePairs.Count; i++)
		{
			ghostModelClass.CoordinateFootprintTilePairs[i].footprintTile.tileDetector.tileDetectionMode = ETileDetectionMode.Tile;
		}
		Singleton<InputPromptsCanvas>.Instance.LoadModule(GetInputPromptsModuleName());
		((Component)this).gameObject.GetComponent<BuildUpdate_Grid>().Initialize(ghostModelClass, itemInstance, ((Component)ghostModelClass).gameObject);
	}

	protected virtual string GetInputPromptsModuleName()
	{
		if (((Component)this).GetComponent<BuildUpdate_Grid>().AllowToggleShowTemperatures)
		{
			return "building_temperaturetoggle";
		}
		return "building";
	}

	protected virtual GridItem CreateGhostModel(BuildableItemDefinition itemDefinition)
	{
		itemDefinition.BuiltItem.isGhost = true;
		GameObject val = Object.Instantiate<GameObject>(((Component)itemDefinition.BuiltItem).gameObject, ((Component)this).transform);
		itemDefinition.BuiltItem.isGhost = false;
		GridItem component = val.GetComponent<GridItem>();
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
		NetworkSingleton<BuildManager>.Instance.DisableCanvases(val);
		NetworkSingleton<BuildManager>.Instance.DisableLights(val);
		ActivateDuringBuild[] componentsInChildren = val.GetComponentsInChildren<ActivateDuringBuild>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			((Component)componentsInChildren[i]).gameObject.SetActive(true);
		}
		component.SetFootprintTileVisiblity(visible: false);
		component.onGhostModel.Invoke();
		return component;
	}
}
