using System.Collections;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.StationFramework;
using ScheduleOne.UI.Stations;
using UnityEngine;

namespace ScheduleOne.PlayerTasks;

public class StartLabOvenTask : Task
{
	public enum EStep
	{
		OpenDoor,
		Pour,
		CloseDoor,
		PressButton
	}

	private ItemInstance ingredient;

	private Coroutine pourRoutine;

	private StationItem stationItem;

	private PourableModule pourableModule;

	private bool pourAnimDone;

	public LabOven Oven { get; private set; }

	public EStep CurrentStep { get; protected set; }

	public StartLabOvenTask(LabOven oven)
	{
		Oven = oven;
		oven.ResetPourableContainer();
		stationItem = oven.CreateStationItems()[0];
		stationItem.ActivateModule<PourableModule>();
		pourableModule = stationItem.GetModule<PourableModule>();
		ConfigurableJoint componentInChildren = ((Component)stationItem).GetComponentInChildren<ConfigurableJoint>();
		if ((Object)(object)componentInChildren != (Object)null)
		{
			Object.Destroy((Object)(object)componentInChildren);
		}
		Rigidbody componentInChildren2 = ((Component)stationItem).GetComponentInChildren<Rigidbody>();
		if ((Object)(object)componentInChildren2 != (Object)null)
		{
			Object.Destroy((Object)(object)componentInChildren2);
		}
		Draggable componentInChildren3 = ((Component)stationItem).GetComponentInChildren<Draggable>();
		if ((Object)(object)componentInChildren3 != (Object)null)
		{
			componentInChildren3.ClickableEnabled = false;
		}
		ingredient = Oven.IngredientSlot.ItemInstance.GetCopy(1);
		((BaseItemInstance)Oven.IngredientSlot.ItemInstance).ChangeQuantity(-1);
		PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(65f, 0.2f);
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
		oven.Door.SetInteractable(interactable: true);
		Singleton<OnScreenMouse>.Instance.Activate();
	}

	public override void Update()
	{
		base.Update();
		CheckProgress();
		base.CurrentInstruction = GetStepInstruction(CurrentStep);
	}

	public override void Success()
	{
		string iD = ((BaseItemDefinition)(ingredient.Definition as StorableItemDefinition).StationItem.GetModule<CookableModule>().Product).ID;
		EQuality ingredientQuality = EQuality.Standard;
		if (ingredient is QualityItemInstance)
		{
			ingredientQuality = (ingredient as QualityItemInstance).Quality;
		}
		Oven.SendCookOperation(new OvenCookOperation(((BaseItemInstance)ingredient).ID, ingredientQuality, 1, iD));
		base.Success();
	}

	public override void StopTask()
	{
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		base.StopTask();
		if (Outcome != EOutcome.Success)
		{
			Oven.IngredientSlot.AddItem(ingredient);
			((Component)Oven.LiquidMesh).gameObject.SetActive(false);
		}
		stationItem.Destroy();
		if (pourRoutine != null)
		{
			Oven.PourAnimation.Stop();
			((MonoBehaviour)Oven).StopCoroutine(pourRoutine);
		}
		Oven.ClearDecals();
		Oven.Door.SetPosition(0f);
		Oven.Door.SetInteractable(interactable: false);
		Oven.WireTray.SetPosition(0f);
		Oven.Button.SetInteractable(interactable: false);
		Singleton<LabOvenCanvas>.Instance.SetIsOpen(Oven, open: true);
		PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(70f, 0.2f);
		PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(Oven.CameraPosition_Default.position, Oven.CameraPosition_Default.rotation, 0.2f);
		Singleton<OnScreenMouse>.Instance.Deactivate();
	}

	private void CheckProgress()
	{
		switch (CurrentStep)
		{
		case EStep.OpenDoor:
			CheckStep_OpenDoor();
			break;
		case EStep.Pour:
			CheckStep_Pour();
			break;
		case EStep.CloseDoor:
			CheckStep_CloseDoor();
			break;
		case EStep.PressButton:
			CheckStep_PressButton();
			break;
		}
	}

	private void ProgressStep()
	{
		if (CurrentStep == EStep.PressButton)
		{
			Success();
			return;
		}
		CurrentStep++;
		if (CurrentStep == EStep.Pour)
		{
			Oven.WireTray.SetPosition(1f);
		}
		if (CurrentStep == EStep.CloseDoor)
		{
			Oven.Door.SetInteractable(interactable: true);
		}
		if (CurrentStep == EStep.Pour)
		{
			pourRoutine = ((MonoBehaviour)Oven).StartCoroutine(PlayPourAnimation());
		}
		if (CurrentStep == EStep.PressButton)
		{
			Oven.Button.SetInteractable(interactable: true);
		}
	}

	private void CheckStep_OpenDoor()
	{
		if (Oven.Door.TargetPosition > 0.9f)
		{
			ProgressStep();
			Oven.Door.SetInteractable(interactable: false);
			Oven.Door.SetPosition(1f);
		}
	}

	private void CheckStep_Pour()
	{
		if (pourAnimDone)
		{
			ProgressStep();
		}
	}

	private void CheckStep_CloseDoor()
	{
		if (Oven.Door.TargetPosition < 0.05f)
		{
			ProgressStep();
			Oven.Door.SetInteractable(interactable: false);
			Oven.Door.SetPosition(0f);
		}
	}

	private void CheckStep_PressButton()
	{
		if (Oven.Button.Pressed)
		{
			ProgressStep();
		}
	}

	private IEnumerator PlayPourAnimation()
	{
		Oven.SetLiquidColor(stationItem.GetModule<CookableModule>().LiquidColor);
		Oven.PourAnimation.Play();
		yield return (object)new WaitForSeconds(0.6f);
		float pourTime = 1f;
		for (float i = 0f; i < pourTime; i += Time.deltaTime)
		{
			pourableModule.LiquidContainer.SetLiquidLevel(1f - i / pourTime);
			yield return null;
		}
		pourableModule.LiquidContainer.SetLiquidLevel(0f);
		pourAnimDone = true;
	}

	public static string GetStepInstruction(EStep step)
	{
		return step switch
		{
			EStep.OpenDoor => "Open oven door", 
			EStep.Pour => "Pour liquid into tray", 
			EStep.CloseDoor => "Close oven door", 
			EStep.PressButton => "Start oven", 
			_ => string.Empty, 
		};
	}
}
