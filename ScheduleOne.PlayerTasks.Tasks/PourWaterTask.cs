using ScheduleOne.DevUtilities;
using ScheduleOne.Growing;
using ScheduleOne.ItemFramework;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using ScheduleOne.Variables;
using UnityEngine;

namespace ScheduleOne.PlayerTasks.Tasks;

public class PourWaterTask : PourOntoTargetTask
{
	public const float NORMALIZED_FILL_PER_TARGET = 0.2f;

	public static bool hintShown;

	protected override bool UseCoverage => true;

	protected override bool FailOnEmpty => false;

	protected override GrowContainerCameraHandler.ECameraPosition CameraPosition => GrowContainerCameraHandler.ECameraPosition.BirdsEye;

	public PourWaterTask(GrowContainer _growContainer, ItemInstance _itemInstance, Pourable _pourablePrefab)
		: base(_growContainer, _itemInstance, _pourablePrefab)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		base.CurrentInstruction = "Pour water over target";
		removeItemAfterInitialPour = false;
		((Component)pourable).GetComponent<WaterContainerPourable>().SetupWaterContainerPourable(_itemInstance as WaterContainerInstance);
		((Component)pourable).transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
		growContainer.SurfaceCover.ConfigureAppearance(Color.black, 0.6f);
		if (NetworkSingleton<GameManager>.Instance.IsTutorial && !hintShown)
		{
			hintShown = true;
			Singleton<HintDisplay>.Instance.ShowHint_20s("While dragging an item, press <Input_Left> or <Input_Right> to rotate it.");
		}
		Singleton<OnScreenMouse>.Instance.Activate();
	}

	public override void StopTask()
	{
		growContainer.SyncMoistureData();
		Singleton<OnScreenMouse>.Instance.Deactivate();
		base.StopTask();
	}

	public override void TargetReached()
	{
		growContainer.ChangeMoistureAmount(0.2f * growContainer.MoistureCapacity);
		growContainer.SyncMoistureData();
		if (growContainer.NormalizedMoistureAmount >= 0.975f)
		{
			Success();
			float value = NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("WateredPotsCount");
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("WateredPotsCount", (value + 1f).ToString());
		}
		base.TargetReached();
	}
}
