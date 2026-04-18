using System.Collections.Generic;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Product;
using ScheduleOne.StationFramework;
using ScheduleOne.Trash;
using ScheduleOne.UI;
using ScheduleOne.UI.Stations;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.PlayerTasks.Tasks;

public class UseMixingStationTask : Task
{
	public enum EStep
	{
		CombineIngredients,
		StartMixing
	}

	private List<StationItem> items = new List<StationItem>();

	private List<StationItem> mixerItems = new List<StationItem>();

	private List<IngredientPiece> ingredientPieces = new List<IngredientPiece>();

	private ItemInstance[] removedIngredients;

	private Beaker Jug;

	public MixingStation Station { get; private set; }

	public EStep CurrentStep { get; private set; }

	public static string GetStepDescription(EStep step)
	{
		return step switch
		{
			EStep.CombineIngredients => "Combine ingredients in bowl", 
			EStep.StartMixing => "Start mixing machine", 
			_ => "Unknown step", 
		};
	}

	public UseMixingStationTask(MixingStation station)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		MixingStation station2 = station;
		base._002Ector();
		UseMixingStationTask useMixingStationTask = this;
		Station = station2;
		Station.onStartButtonClicked.AddListener(new UnityAction(StartButtonPressed));
		ClickDetectionRadius = 0.012f;
		PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(Station.CameraPosition_CombineIngredients.position, Station.CameraPosition_CombineIngredients.rotation, 0.2f);
		PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(TaskName);
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
		removedIngredients = new ItemInstance[2];
		int mixQuantity = station2.GetMixQuantity();
		removedIngredients[0] = station2.ProductSlot.ItemInstance.GetCopy(mixQuantity);
		removedIngredients[1] = station2.MixerSlot.ItemInstance.GetCopy(mixQuantity);
		station2.ProductSlot.ChangeQuantity(-mixQuantity);
		station2.MixerSlot.ChangeQuantity(-mixQuantity);
		EnableMultiDragging(station2.ItemContainer, 0.12f);
		int num = 0;
		Singleton<InputPromptsCanvas>.Instance.LoadModule("packaging");
		for (int i = 0; i < mixQuantity; i++)
		{
			SetupIngredient(removedIngredients[1].Definition as StorableItemDefinition, num, mixer: true);
			num++;
		}
		for (int j = 0; j < mixQuantity; j++)
		{
			SetupIngredient(removedIngredients[0].Definition as StorableItemDefinition, num, mixer: false);
			num++;
		}
		if ((Object)(object)Jug != (Object)null)
		{
			Jug.Pourable.LiquidCapacity_L = Jug.Fillable.LiquidCapacity_L;
			Jug.Pourable.DefaultLiquid_L = Jug.Fillable.GetTotalLiquidVolume();
			Jug.Pourable.SetLiquidLevel(Jug.Pourable.DefaultLiquid_L);
			Jug.Pourable.PourParticlesColor = Jug.Fillable.LiquidContainer.LiquidColor;
			Jug.Pourable.LiquidColor = Jug.Fillable.LiquidContainer.LiquidColor;
			TriggerModule trigger = Jug.Pourable.PourParticles[0].trigger;
			((TriggerModule)(ref trigger)).AddCollider((Component)(object)Station.BowlFillable.LiquidContainer.Collider);
			Jug.Fillable.FillableEnabled = false;
		}
		Singleton<OnScreenMouse>.Instance.Activate();
		void SetupIngredient(StorableItemDefinition def, int index, bool mixer)
		{
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0120: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)def.StationItem == (Object)null)
			{
				Console.LogError("Ingredient '" + ((BaseItemDefinition)def).Name + "' does not have a station item");
			}
			else
			{
				if (mixer)
				{
					mixerItems.Add(def.StationItem);
				}
				if (def.StationItem.HasModule<PourableModule>())
				{
					if ((Object)(object)Jug == (Object)null)
					{
						Jug = CreateJug();
					}
					PourableModule module = def.StationItem.GetModule<PourableModule>();
					Jug.Fillable.AddLiquid(module.LiquidType, module.LiquidCapacity_L, module.LiquidColor);
				}
				else
				{
					StationItem stationItem = Object.Instantiate<StationItem>(def.StationItem, station2.ItemContainer);
					((Component)stationItem).transform.rotation = station2.IngredientTransforms[items.Count].rotation;
					Vector3 eulerAngles = ((Component)stationItem).transform.eulerAngles;
					eulerAngles.y = Random.Range(0f, 360f);
					((Component)stationItem).transform.eulerAngles = eulerAngles;
					((Component)stationItem).transform.position = station2.IngredientTransforms[items.Count].position;
					stationItem.Initialize(def);
					if (stationItem.HasModule<IngredientModule>())
					{
						stationItem.ActivateModule<IngredientModule>();
						IngredientPiece[] pieces = stationItem.GetModule<IngredientModule>().Pieces;
						foreach (IngredientPiece ingredientPiece in pieces)
						{
							ingredientPieces.Add(ingredientPiece);
							ingredientPiece.DisableInteractionInLiquid = false;
						}
					}
					else
					{
						Console.LogError("Ingredient '" + ((BaseItemDefinition)def).Name + "' does not have an ingredient or pourable module");
					}
					Draggable[] componentsInChildren = ((Component)stationItem).GetComponentsInChildren<Draggable>();
					foreach (Draggable obj in componentsInChildren)
					{
						obj.DragProjectionMode = Draggable.EDragProjectionMode.FlatCameraForward;
						DraggableConstraint component = ((Component)obj).gameObject.GetComponent<DraggableConstraint>();
						if ((Object)(object)component != (Object)null)
						{
							component.ProportionalZClamp = true;
						}
					}
					items.Add(stationItem);
				}
			}
		}
	}

	private Beaker CreateJug()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		Beaker component = Object.Instantiate<GameObject>(Station.JugPrefab, Station.ItemContainer).GetComponent<Beaker>();
		((Component)component).transform.position = Station.JugAlignment.position;
		((Component)component).transform.rotation = Station.JugAlignment.rotation;
		((Component)component).GetComponent<DraggableConstraint>().Container = Station.ItemContainer;
		component.ActivateModule<PourableModule>();
		return component;
	}

	public override void Update()
	{
		base.Update();
		CheckProgress();
		UpdateInstruction();
	}

	private void UpdateInstruction()
	{
		base.CurrentInstruction = GetStepDescription(CurrentStep);
		if (CurrentStep == EStep.CombineIngredients)
		{
			int num = items.Count;
			if ((Object)(object)Jug != (Object)null)
			{
				num++;
			}
			int combinedIngredients = GetCombinedIngredients();
			base.CurrentInstruction = base.CurrentInstruction + " (" + combinedIngredients + "/" + num + ")";
		}
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
		if (GetCombinedIngredients() >= items.Count + (((Object)(object)Jug != (Object)null) ? 1 : 0))
		{
			ProgressStep();
		}
	}

	private int GetCombinedIngredients()
	{
		int num = 0;
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].HasModule<IngredientModule>())
			{
				IngredientModule module = items[i].GetModule<IngredientModule>();
				bool flag = true;
				IngredientPiece[] pieces = module.Pieces;
				for (int j = 0; j < pieces.Length; j++)
				{
					if ((Object)(object)pieces[j].CurrentLiquidContainer != (Object)(object)Station.BowlFillable.LiquidContainer)
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					num++;
				}
			}
			else if (items[i].HasModule<PourableModule>() && items[i].GetModule<PourableModule>().NormalizedLiquidLevel <= 0.02f)
			{
				num++;
			}
		}
		if ((Object)(object)Jug != (Object)null && Jug.Pourable.NormalizedLiquidLevel <= 0.02f)
		{
			num++;
		}
		return num;
	}

	private void ProgressStep()
	{
		CurrentStep++;
		if (CurrentStep == EStep.StartMixing)
		{
			Station.SetStartButtonClickable(clickable: true);
		}
	}

	private void StartButtonPressed()
	{
		if (CurrentStep == EStep.StartMixing)
		{
			Success();
		}
	}

	public override void Success()
	{
		ProductItemInstance productItemInstance = removedIngredients[0] as ProductItemInstance;
		string iD = ((BaseItemDefinition)removedIngredients[1].Definition).ID;
		CreateTrash();
		Singleton<MixingStationCanvas>.Instance.StartMixOperation(new MixOperation(((BaseItemInstance)productItemInstance).ID, productItemInstance.Quality, iD, ((BaseItemInstance)productItemInstance).Quantity));
		base.Success();
	}

	private void CreateTrash()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		BoxCollider trashSpawnVolume = Station.TrashSpawnVolume;
		for (int i = 0; i < Mathf.CeilToInt((float)mixerItems.Count / 2f); i++)
		{
			if (!((Object)(object)mixerItems[0].TrashPrefab == (Object)null))
			{
				Vector3 posiiton = ((Component)trashSpawnVolume).transform.TransformPoint(new Vector3(Random.Range((0f - trashSpawnVolume.size.x) / 2f, trashSpawnVolume.size.x / 2f), 0f, Random.Range((0f - trashSpawnVolume.size.z) / 2f, trashSpawnVolume.size.z / 2f)));
				Vector3 forward = ((Component)trashSpawnVolume).transform.forward;
				forward = Quaternion.Euler(0f, Random.Range(-45f, 45f), 0f) * forward;
				float num = Random.Range(0.25f, 0.4f);
				NetworkSingleton<TrashManager>.Instance.CreateTrashItem(mixerItems[0].TrashPrefab.ID, posiiton, Random.rotation, forward * num);
			}
		}
	}

	public override void StopTask()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		Station.onStartButtonClicked.RemoveListener(new UnityAction(StartButtonPressed));
		Station.BowlFillable.ResetContents();
		if (Outcome != EOutcome.Success)
		{
			Station.ProductSlot.AddItem(removedIngredients[0]);
			Station.MixerSlot.AddItem(removedIngredients[1]);
		}
		Singleton<InputPromptsCanvas>.Instance.UnloadModule();
		foreach (StationItem item in items)
		{
			item.Destroy();
		}
		items.Clear();
		PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(TaskName);
		Station.Open();
		if ((Object)(object)Jug != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)Jug).gameObject);
		}
		base.StopTask();
		Singleton<OnScreenMouse>.Instance.Deactivate();
	}
}
