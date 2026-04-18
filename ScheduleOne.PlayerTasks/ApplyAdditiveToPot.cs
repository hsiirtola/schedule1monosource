using ScheduleOne.Core.Items.Framework;
using ScheduleOne.Growing;
using ScheduleOne.ItemFramework;
using ScheduleOne.PlayerTasks.Tasks;
using UnityEngine;

namespace ScheduleOne.PlayerTasks;

public class ApplyAdditiveToPot : GrowContainerPourTask
{
	private AdditiveDefinition def;

	protected override bool UseCoverage => true;

	protected override GrowContainerCameraHandler.ECameraPosition CameraPosition => GrowContainerCameraHandler.ECameraPosition.BirdsEye;

	public ApplyAdditiveToPot(GrowContainer _growContainer, ItemInstance _itemInstance, Pourable _pourablePrefab)
		: base(_growContainer, _itemInstance, _pourablePrefab)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		def = _itemInstance.Definition as AdditiveDefinition;
		base.CurrentInstruction = "Cover soil with " + ((BaseItemDefinition)def).Name + " (0%)";
		removeItemAfterInitialPour = false;
		growContainer.SurfaceCover.ConfigureAppearance((pourable as PourableAdditive).LiquidColor, 0.28f);
	}

	public override void Update()
	{
		base.Update();
		int num = Mathf.FloorToInt(growContainer.SurfaceCover.GetNormalizedProgress() * 100f);
		base.CurrentInstruction = "Cover soil with " + ((BaseItemDefinition)def).Name + " (" + num + "%)";
	}

	protected override void FullyCovered()
	{
		base.FullyCovered();
		growContainer.ApplyAdditive_Server(((BaseItemDefinition)def).ID);
		RemoveItem();
		Success();
	}
}
