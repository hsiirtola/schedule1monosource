using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.StationFramework;
using ScheduleOne.UI;
using ScheduleOne.Variables;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.PlayerTasks;

public class InocculateGrainBagTask : Task
{
	public enum EStage
	{
		RemoveCap,
		InsertSyringe,
		PushPlunger
	}

	private MushroomSpawnStation _station;

	private MushroomSpawnStationItem _spawn;

	private SporeSyringeStationItem _syringe;

	private EStage _currentStage;

	private ItemInstance _grainBagInstance;

	private ItemInstance _syringeInstance;

	private ShroomSpawnDefinition _spawnDefinition;

	public override string TaskName { get; protected set; } = "Inocculate grain bag";

	public InocculateGrainBagTask(MushroomSpawnStation station)
	{
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Expected O, but got Unknown
		_station = station;
		SporeSyringeDefinition sporeSyringeDefinition = station.SyringeSlot.ItemInstance.Definition as SporeSyringeDefinition;
		_spawnDefinition = sporeSyringeDefinition.SpawnDefinition;
		_spawn = ((Component)Object.Instantiate<StationItem>(_spawnDefinition.StationItem, station.TaskContainer)).GetComponent<MushroomSpawnStationItem>();
		((Component)_spawn).transform.SetWorldTransformData(station.GrainBagStartTransform.GetWorldTransformData());
		_syringe = ((Component)Object.Instantiate<StationItem>(sporeSyringeDefinition.StationItem, station.TaskContainer)).GetComponent<SporeSyringeStationItem>();
		((Component)_syringe).transform.SetWorldTransformData(station.SyringeStartTransform.GetWorldTransformData());
		_syringe.onCapRemoved.AddListener(new UnityAction(OnSyringeCapRemoved));
		_syringe.onInserted.AddListener(new UnityAction(OnSyringeInserted));
		_syringe.onPlungerMoved.AddListener((UnityAction<float>)OnPlungerPushed);
		_syringe.SetCapInteractable(interactable: true);
		_syringe.SetInjectionPortCollider(_spawn.InjectionPortCollider);
		_grainBagInstance = station.GrainBagSlot.ItemInstance.GetCopy(1);
		station.GrainBagSlot.ChangeQuantity(-1);
		_syringeInstance = station.SyringeSlot.ItemInstance.GetCopy(1);
		station.SyringeSlot.ChangeQuantity(-1);
		Singleton<InputPromptsCanvas>.Instance.LoadModule("generictask");
	}

	public override void Success()
	{
		_station.OutputSlot.InsertItem(_spawnDefinition.GetDefaultInstance());
		if (NetworkSingleton<VariableDatabase>.InstanceExists)
		{
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("InocculatedGrainBagCount", (NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("InocculatedGrainBagCount") + 1f).ToString());
		}
		base.Success();
	}

	public override void StopTask()
	{
		Object.Destroy((Object)(object)((Component)_syringe).gameObject);
		Object.Destroy((Object)(object)((Component)_spawn).gameObject);
		if (Outcome == EOutcome.Cancelled)
		{
			_station.GrainBagSlot.InsertItem(_grainBagInstance);
			_station.SyringeSlot.InsertItem(_syringeInstance);
		}
		base.StopTask();
	}

	public override void Update()
	{
		base.Update();
		base.CurrentInstruction = GetInstructionForStage(_currentStage);
	}

	private string GetInstructionForStage(EStage stage)
	{
		return stage switch
		{
			EStage.RemoveCap => "Remove syringe cap", 
			EStage.InsertSyringe => "Insert syringe into grain bag", 
			EStage.PushPlunger => $"Push the plunger ({Mathf.FloorToInt(_syringe.PlungerPosition * 100f)}%)", 
			_ => "", 
		};
	}

	private void OnSyringeCapRemoved()
	{
		_currentStage = EStage.InsertSyringe;
		_syringe.SetSyringeDraggable(draggable: true);
		_spawn.SetInjectionPortHighlightActive(active: true);
	}

	private void OnSyringeInserted()
	{
		_currentStage = EStage.PushPlunger;
		_spawn.SetInjectionPortHighlightActive(active: false);
		_syringe.SetPlungerInteractable(interactable: true);
	}

	private void OnPlungerPushed(float amount)
	{
		_spawn.SetInocculationAmount(amount);
		if (amount >= 1f)
		{
			Success();
		}
	}
}
