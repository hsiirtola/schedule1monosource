using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks;
using ScheduleOne.Product.Packaging;
using ScheduleOne.UI.Compass;
using ScheduleOne.UI.Items;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Stations;

public class PackagingStationCanvas : Singleton<PackagingStationCanvas>
{
	public bool ShowHintOnOpen;

	public bool ShowShiftClickHint;

	public PackagingStation.EMode CurrentMode;

	public Color InstructionWarningColor;

	[Header("References")]
	public Canvas Canvas;

	public GameObject Container;

	public UIScreen UIScreen;

	public ItemSlotUI PackagingSlotUI;

	public ItemSlotUI ProductSlotUI;

	public ItemSlotUI OutputSlotUI;

	public TextMeshProUGUI InstructionLabel;

	public Image InstructionShadow;

	public Button BeginButton;

	public Animation ModeAnimation;

	public TextMeshProUGUI ButtonLabel;

	public bool isOpen { get; protected set; }

	public PackagingStation PackagingStation { get; protected set; }

	protected override void Awake()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		base.Awake();
		((UnityEvent)BeginButton.onClick).AddListener(new UnityAction(BeginButtonPressed));
	}

	protected override void Start()
	{
		base.Start();
		Singleton<ItemUIManager>.Instance.AddRaycaster(((Component)Canvas).GetComponent<GraphicRaycaster>());
		SetIsOpen(null, open: false);
	}

	protected virtual void Update()
	{
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		if (!isOpen)
		{
			return;
		}
		if (CurrentMode == PackagingStation.EMode.Package)
		{
			((TMP_Text)ButtonLabel).text = "PACK";
		}
		else
		{
			((TMP_Text)ButtonLabel).text = "UNPACK";
		}
		if (GameInput.GetCurrentInputDeviceIsKeyboardMouse() && ((Selectable)BeginButton).interactable && GameInput.GetButtonDown(GameInput.ButtonCode.Submit))
		{
			BeginButtonPressed();
			return;
		}
		PackagingStation.EState state = PackagingStation.GetState(CurrentMode);
		if (state == PackagingStation.EState.CanBegin)
		{
			((Behaviour)InstructionLabel).enabled = false;
			((Behaviour)InstructionShadow).enabled = false;
			((Selectable)BeginButton).interactable = true;
			return;
		}
		if (CurrentMode == PackagingStation.EMode.Package)
		{
			switch (state)
			{
			case PackagingStation.EState.MissingItems:
				((TMP_Text)InstructionLabel).text = "Insert product + packaging into slots";
				((Graphic)InstructionLabel).color = Color.white;
				break;
			case PackagingStation.EState.InsufficentProduct:
				((TMP_Text)InstructionLabel).text = "This packaging type requires <color=#FFC73D>" + (PackagingStation.PackagingSlot.ItemInstance.Definition as PackagingDefinition).Quantity + "x</color> product";
				((Graphic)InstructionLabel).color = Color.white;
				break;
			case PackagingStation.EState.OutputSlotFull:
				((TMP_Text)InstructionLabel).text = "Output slot is full!";
				((Graphic)InstructionLabel).color = InstructionWarningColor;
				break;
			case PackagingStation.EState.Mismatch:
				((TMP_Text)InstructionLabel).text = "Output slot is full!";
				((Graphic)InstructionLabel).color = InstructionWarningColor;
				break;
			}
		}
		else
		{
			switch (state)
			{
			case PackagingStation.EState.MissingItems:
				((TMP_Text)InstructionLabel).text = "Insert packaged product into output";
				((Graphic)InstructionLabel).color = Color.white;
				break;
			case PackagingStation.EState.PackageSlotFull:
				((TMP_Text)InstructionLabel).text = "Unpackaged items won't fit!";
				((Graphic)InstructionLabel).color = InstructionWarningColor;
				break;
			case PackagingStation.EState.ProductSlotFull:
				((TMP_Text)InstructionLabel).text = "Unpackaged items won't fit!";
				((Graphic)InstructionLabel).color = InstructionWarningColor;
				break;
			}
		}
		((Behaviour)InstructionLabel).enabled = true;
		((Behaviour)InstructionShadow).enabled = true;
		((Selectable)BeginButton).interactable = false;
	}

	public void SetIsOpen(PackagingStation station, bool open, bool removeUI = true)
	{
		isOpen = open;
		((Behaviour)Canvas).enabled = open;
		Container.gameObject.SetActive(open);
		PackagingStation = station;
		if (PlayerSingleton<PlayerCamera>.InstanceExists)
		{
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
			if (open)
			{
				PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(((Object)this).name);
			}
		}
		if ((Object)(object)station != (Object)null)
		{
			PackagingSlotUI.AssignSlot(station.PackagingSlot);
			ProductSlotUI.AssignSlot(station.ProductSlot);
			OutputSlotUI.AssignSlot(station.OutputSlot);
		}
		else
		{
			PackagingSlotUI.ClearSlot();
			ProductSlotUI.ClearSlot();
			OutputSlotUI.ClearSlot();
		}
		if (open)
		{
			Singleton<InputPromptsCanvas>.Instance.LoadModule("exitonly");
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
			PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: false);
			Singleton<UIScreenManager>.Instance.AddScreen(UIScreen);
			if (PlayerSingleton<PlayerInventory>.InstanceExists)
			{
				PlayerSingleton<PlayerInventory>.Instance.AttachToScreen(UIScreen);
			}
			if (ShowShiftClickHint && station.OutputSlot.Quantity > 0)
			{
				Singleton<HintDisplay>.Instance.ShowHint_20s("<Input_QuickMove><h1> + click</h> an item to quickly move it");
			}
		}
		else if (removeUI)
		{
			Singleton<InputPromptsCanvas>.Instance.UnloadModule();
			if (PlayerSingleton<PlayerInventory>.InstanceExists)
			{
				PlayerSingleton<PlayerInventory>.Instance.DetachFromScreen();
			}
			Singleton<UIScreenManager>.Instance.RemoveScreen(UIScreen);
		}
		Singleton<ItemUIManager>.Instance.SetDraggingEnabled(open);
		if (open)
		{
			if (CurrentMode == PackagingStation.EMode.Package)
			{
				Singleton<ItemUIManager>.Instance.EnableQuickMove(PlayerSingleton<PlayerInventory>.Instance.GetAllInventorySlots(), new List<ItemSlot> { station.ProductSlot, station.PackagingSlot, station.OutputSlot });
			}
			else
			{
				Singleton<ItemUIManager>.Instance.EnableQuickMove(PlayerSingleton<PlayerInventory>.Instance.GetAllInventorySlots(), new List<ItemSlot> { station.OutputSlot, station.PackagingSlot, station.ProductSlot });
			}
		}
		if (isOpen)
		{
			Singleton<CompassManager>.Instance.SetVisible(visible: false);
		}
	}

	public void BeginButtonPressed()
	{
		if (!((Behaviour)BeginButton).isActiveAndEnabled || !((Selectable)BeginButton).interactable || (Object)(object)PackagingStation == (Object)null || PackagingStation.GetState(CurrentMode) != PackagingStation.EState.CanBegin)
		{
			return;
		}
		if (CurrentMode == PackagingStation.EMode.Unpackage)
		{
			PackagingStation.Unpack();
			Singleton<TaskManager>.Instance.PlayTaskCompleteSound();
			return;
		}
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
		PackagingStation.StartTask();
		if (ShowHintOnOpen)
		{
			Singleton<HintDisplay>.Instance.ShowHint_20s("When performing tasks at stations, click and drag items to move them.");
		}
		SetIsOpen(null, open: false, removeUI: false);
	}

	private void UpdateSlotPositions()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)PackagingStation != (Object)null)
		{
			((Transform)PackagingSlotUI.Rect).position = PlayerSingleton<PlayerCamera>.Instance.Camera.WorldToScreenPoint(PackagingStation.PackagingSlotPosition.position);
			((Transform)ProductSlotUI.Rect).position = PlayerSingleton<PlayerCamera>.Instance.Camera.WorldToScreenPoint(PackagingStation.ProductSlotPosition.position);
			((Transform)OutputSlotUI.Rect).position = PlayerSingleton<PlayerCamera>.Instance.Camera.WorldToScreenPoint(PackagingStation.OutputSlotPosition.position);
		}
	}

	public void ToggleMode()
	{
		SetMode((CurrentMode == PackagingStation.EMode.Package) ? PackagingStation.EMode.Unpackage : PackagingStation.EMode.Package);
	}

	public void SetMode(PackagingStation.EMode mode)
	{
		CurrentMode = mode;
		if (mode == PackagingStation.EMode.Package)
		{
			ModeAnimation.Play("Packaging station switch to package");
		}
		else
		{
			ModeAnimation.Play("Packaging station switch to unpackage");
		}
		if (CurrentMode == PackagingStation.EMode.Package)
		{
			Singleton<ItemUIManager>.Instance.EnableQuickMove(PlayerSingleton<PlayerInventory>.Instance.GetAllInventorySlots(), new List<ItemSlot> { PackagingStation.ProductSlot, PackagingStation.PackagingSlot, PackagingStation.OutputSlot });
		}
		else
		{
			Singleton<ItemUIManager>.Instance.EnableQuickMove(PlayerSingleton<PlayerInventory>.Instance.GetAllInventorySlots(), new List<ItemSlot> { PackagingStation.OutputSlot, PackagingStation.PackagingSlot, PackagingStation.ProductSlot });
		}
	}
}
