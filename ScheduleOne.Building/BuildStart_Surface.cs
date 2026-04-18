using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.ItemFramework;
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.Building;

public class BuildStart_Surface : BuildStart_Base
{
	public override void StartBuilding(ItemInstance itemInstance)
	{
		SurfaceItem surfaceItem = CreateGhostModel(itemInstance.Definition as BuildableItemDefinition);
		if (!((Object)(object)surfaceItem == (Object)null))
		{
			Singleton<HUD>.Instance.SetCrosshairVisible(vis: false);
			Singleton<InputPromptsCanvas>.Instance.LoadModule("building");
			((Component)this).gameObject.GetComponent<BuildUpdate_Surface>().GhostModel = ((Component)surfaceItem).gameObject;
			((Component)this).gameObject.GetComponent<BuildUpdate_Surface>().BuildableItemClass = surfaceItem;
			((Component)this).gameObject.GetComponent<BuildUpdate_Surface>().ItemInstance = itemInstance;
		}
	}

	protected virtual SurfaceItem CreateGhostModel(BuildableItemDefinition itemDefinition)
	{
		itemDefinition.BuiltItem.isGhost = true;
		GameObject val = Object.Instantiate<GameObject>(((Component)itemDefinition.BuiltItem).gameObject, ((Component)this).transform);
		itemDefinition.BuiltItem.isGhost = false;
		SurfaceItem component = val.GetComponent<SurfaceItem>();
		if ((Object)(object)component == (Object)null)
		{
			Console.LogWarning("CreateGhostModel: asset path is not a SurfaceItem!");
			return null;
		}
		((Behaviour)component).enabled = false;
		component.isGhost = true;
		NetworkSingleton<BuildManager>.Instance.DisableColliders(val);
		NetworkSingleton<BuildManager>.Instance.DisableNavigation(val);
		NetworkSingleton<BuildManager>.Instance.DisableNetworking(val);
		NetworkSingleton<BuildManager>.Instance.DisableCanvases(val);
		ActivateDuringBuild[] componentsInChildren = val.GetComponentsInChildren<ActivateDuringBuild>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			((Component)componentsInChildren[i]).gameObject.SetActive(true);
		}
		return component;
	}
}
