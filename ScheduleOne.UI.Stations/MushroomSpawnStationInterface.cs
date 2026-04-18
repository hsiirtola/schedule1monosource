using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks;
using ScheduleOne.StationFramework;
using ScheduleOne.UI.Items;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Stations;

public class MushroomSpawnStationInterface : Singleton<MushroomSpawnStationInterface>
{
	private const float CameraLerpTime = 0.2f;

	[Header("References")]
	[SerializeField]
	private Canvas _canvas;

	[SerializeField]
	private RectTransform _container;

	[SerializeField]
	private ItemSlotUI _grainBagSlotUI;

	[SerializeField]
	private ItemSlotUI _syringeSlotUI;

	[SerializeField]
	private ItemSlotUI _outputSlotUI;

	[SerializeField]
	private Button _beginButton;

	[SerializeField]
	private TextMeshProUGUI _instructionLabel;

	public Action OnExitStation;

	public bool IsOpen { get; private set; }

	public MushroomSpawnStation Station { get; private set; }

	protected override void Awake()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		base.Awake();
		GameInput.RegisterExitListener(Exit, 4);
		((UnityEvent)_beginButton.onClick).AddListener(new UnityAction(BeginTask));
		((Behaviour)_canvas).enabled = false;
		((Component)_container).gameObject.SetActive(true);
		IsOpen = false;
	}

	protected override void Start()
	{
		base.Start();
		Singleton<ItemUIManager>.Instance.AddRaycaster(((Component)_canvas).GetComponent<GraphicRaycaster>());
	}

	private void Exit(ExitAction action)
	{
		if (!action.Used && IsOpen && action.exitType == ExitType.Escape)
		{
			action.Used = true;
			Close(exitStation: true);
		}
	}

	private void Update()
	{
		if (IsOpen && ((Selectable)_beginButton).interactable && GameInput.GetButtonDown(GameInput.ButtonCode.Submit))
		{
			BeginTask();
		}
	}

	public void Open(MushroomSpawnStation station)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		IsOpen = true;
		Station = station;
		((Behaviour)_canvas).enabled = true;
		((Component)_container).gameObject.SetActive(true);
		PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(((Object)this).name);
		PlayerSingleton<PlayerCamera>.Instance.OpenInterface(keepInventoryVisible: true);
		PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(Station.CameraTransform.position, Station.CameraTransform.rotation, 0.2f);
		PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(70f, 0.2f);
		_grainBagSlotUI.AssignSlot(station.GrainBagSlot);
		_syringeSlotUI.AssignSlot(station.SyringeSlot);
		_outputSlotUI.AssignSlot(station.OutputSlot);
		for (int i = 0; i < station.ItemSlots.Count; i++)
		{
			ItemSlot itemSlot = station.ItemSlots[i];
			itemSlot.onItemDataChanged = (Action)Delegate.Combine(itemSlot.onItemDataChanged, new Action(StationContentsChanged));
		}
		Singleton<InputPromptsCanvas>.Instance.LoadModule("exitonly");
		Singleton<ItemUIManager>.Instance.EnableQuickMove(PlayerSingleton<PlayerInventory>.Instance.GetAllInventorySlots(), Station.ItemSlots);
		UpdateInstruction();
		UpdateBeginButton();
	}

	public void Close(bool exitStation)
	{
		((Behaviour)_canvas).enabled = false;
		((Component)_container).gameObject.SetActive(false);
		PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
		_grainBagSlotUI.ClearSlot();
		_syringeSlotUI.ClearSlot();
		_outputSlotUI.ClearSlot();
		for (int i = 0; i < Station.ItemSlots.Count; i++)
		{
			ItemSlot itemSlot = Station.ItemSlots[i];
			itemSlot.onItemDataChanged = (Action)Delegate.Remove(itemSlot.onItemDataChanged, new Action(StationContentsChanged));
		}
		Singleton<InputPromptsCanvas>.Instance.UnloadModule();
		Singleton<ItemUIManager>.Instance.SetDraggingEnabled(enabled: false);
		if (exitStation)
		{
			PlayerSingleton<PlayerCamera>.Instance.CloseInterface();
			if (OnExitStation != null)
			{
				OnExitStation();
			}
			Station = null;
		}
		IsOpen = false;
	}

	private void StationContentsChanged()
	{
		UpdateBeginButton();
		UpdateInstruction();
	}

	private void UpdateInstruction()
	{
		if (CanBeginTask(out var instruction))
		{
			((Behaviour)_instructionLabel).enabled = false;
			return;
		}
		((TMP_Text)_instructionLabel).text = instruction;
		((Behaviour)_instructionLabel).enabled = true;
	}

	private bool CanBeginTask(out string instruction)
	{
		instruction = string.Empty;
		if (!Station.DoesStationContainRequiredItems())
		{
			instruction = "Insert grain bag + spore syringe into slots";
			return false;
		}
		if (!Station.DoesStationHaveOutputSpace())
		{
			instruction = "Output slot is full!";
			return false;
		}
		return true;
	}

	private void UpdateBeginButton()
	{
		((Selectable)_beginButton).interactable = CanBeginTask(out var _);
	}

	private void BeginTask()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		Close(exitStation: false);
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
		PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(Station.TaskCameraTransform.position, Station.TaskCameraTransform.rotation, 0.2f);
		PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(60f, 0.2f);
		InocculateGrainBagTask inocculateGrainBagTask = new InocculateGrainBagTask(Station);
		inocculateGrainBagTask.onTaskStop = (Action)Delegate.Combine(inocculateGrainBagTask.onTaskStop, new Action(TaskStopped));
		void TaskStopped()
		{
			Open(Station);
		}
	}
}
