using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.StationFramework;
using ScheduleOne.UI;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.PlayerTasks;

public class CauldronTask : Task
{
	public enum EStep
	{
		CombineIngredients,
		StartMixing
	}

	private StationItem[] CocaLeaves;

	private StationItem Gasoline;

	private Draggable Tub;

	public Cauldron Cauldron { get; private set; }

	public EStep CurrentStep { get; private set; }

	public static string GetStepDescription(EStep step)
	{
		return step switch
		{
			EStep.CombineIngredients => "Combine leaves and gasoline in cauldron", 
			EStep.StartMixing => "Start cauldron", 
			_ => "Unknown step", 
		};
	}

	public CauldronTask(Cauldron caudron)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		Cauldron = caudron;
		Cauldron.onStartButtonClicked.AddListener(new UnityAction(StartButtonPressed));
		((Behaviour)Cauldron.OverheadLight).enabled = true;
		ClickDetectionRadius = 0.012f;
		PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(Cauldron.CameraPosition_CombineIngredients.position, Cauldron.CameraPosition_CombineIngredients.rotation, 0.2f);
		PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(TaskName);
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
		EnableMultiDragging(Cauldron.ItemContainer, 0.15f);
		Singleton<InputPromptsCanvas>.Instance.LoadModule("packaging");
		Gasoline = Object.Instantiate<StationItem>(Cauldron.GasolinePrefab, caudron.ItemContainer);
		((Component)Gasoline).transform.rotation = caudron.GasolineSpawnPoint.rotation;
		((Component)Gasoline).transform.position = caudron.GasolineSpawnPoint.position;
		((Component)Gasoline).transform.localScale = Vector3.one * 1.5f;
		Gasoline.ActivateModule<PourableModule>();
		((Component)Gasoline).GetComponentInChildren<Rigidbody>().rotation = caudron.GasolineSpawnPoint.rotation;
		CocaLeaves = new StationItem[20];
		for (int i = 0; i < CocaLeaves.Length; i++)
		{
			CocaLeaves[i] = Object.Instantiate<StationItem>(Cauldron.CocaLeafPrefab, caudron.ItemContainer);
			((Component)CocaLeaves[i]).transform.rotation = caudron.LeafSpawns[i].rotation;
			((Component)CocaLeaves[i]).transform.position = caudron.LeafSpawns[i].position;
			CocaLeaves[i].ActivateModule<IngredientModule>();
			((Component)CocaLeaves[i]).transform.localScale = Vector3.one * 0.85f;
			IngredientPiece obj = CocaLeaves[i].GetModule<IngredientModule>().Pieces[0];
			((Component)obj).GetComponent<Draggable>().DragProjectionMode = Draggable.EDragProjectionMode.CustomPlane;
			((Component)obj).GetComponent<Draggable>().CustomDragPlane = caudron.LeafDragProjectionPlane;
			((Component)obj).transform.SetParent(caudron.ItemContainer);
		}
		Singleton<OnScreenMouse>.Instance.Activate();
	}

	public override void Success()
	{
		EQuality quality = Cauldron.RemoveIngredients();
		Cauldron.SendCookOperation(Cauldron.CookTime, quality);
		Cauldron.CreateTrash(new List<StationItem> { Gasoline });
		base.Success();
	}

	public override void StopTask()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		((Behaviour)Cauldron.OverheadLight).enabled = false;
		Cauldron.onStartButtonClicked.RemoveListener(new UnityAction(StartButtonPressed));
		Cauldron.StartButtonClickable.ClickableEnabled = false;
		Singleton<InputPromptsCanvas>.Instance.UnloadModule();
		PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(TaskName);
		Cauldron.Open();
		StationItem[] cocaLeaves = CocaLeaves;
		foreach (StationItem obj in cocaLeaves)
		{
			Object.Destroy((Object)(object)((Component)obj.GetModule<IngredientModule>().Pieces[0]).gameObject);
			obj.Destroy();
		}
		Gasoline.Destroy();
		if (Outcome != EOutcome.Success)
		{
			Cauldron.CauldronFillable.ResetContents();
		}
		Singleton<OnScreenMouse>.Instance.Deactivate();
		base.StopTask();
	}

	public override void Update()
	{
		base.Update();
		CheckProgress();
		UpdateInstruction();
	}

	private void CheckProgress()
	{
		if (CurrentStep == EStep.CombineIngredients)
		{
			CheckStep_CombineIngredients();
		}
	}

	private void CheckStep_CombineIngredients()
	{
		if (Gasoline.GetModule<PourableModule>().LiquidLevel > 0.01f)
		{
			return;
		}
		StationItem[] cocaLeaves = CocaLeaves;
		for (int i = 0; i < cocaLeaves.Length; i++)
		{
			if ((Object)(object)cocaLeaves[i].GetModule<IngredientModule>().Pieces[0].CurrentLiquidContainer == (Object)null)
			{
				return;
			}
		}
		StartMixing();
	}

	private void StartMixing()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		CurrentStep = EStep.StartMixing;
		bool isHeld = Gasoline.GetModule<PourableModule>().Draggable.IsHeld;
		Gasoline.GetModule<PourableModule>().Draggable.ClickableEnabled = false;
		if (isHeld)
		{
			Gasoline.GetModule<PourableModule>().Draggable.Rb.AddForce(((Component)Cauldron).transform.right * 10f, (ForceMode)2);
		}
		StationItem[] cocaLeaves = CocaLeaves;
		for (int i = 0; i < cocaLeaves.Length; i++)
		{
			((Component)cocaLeaves[i].GetModule<IngredientModule>().Pieces[0]).GetComponent<Draggable>().ClickableEnabled = false;
		}
		Cauldron.StartButtonClickable.ClickableEnabled = true;
		PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(Cauldron.CameraPosition_StartMachine.position, Cauldron.CameraPosition_StartMachine.rotation, 0.2f);
	}

	private void UpdateInstruction()
	{
		base.CurrentInstruction = GetStepDescription(CurrentStep);
	}

	private void StartButtonPressed()
	{
		if (CurrentStep == EStep.StartMixing)
		{
			Success();
		}
	}
}
