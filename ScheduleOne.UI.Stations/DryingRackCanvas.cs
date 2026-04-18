using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI.Items;
using ScheduleOne.UI.Stations.Drying_rack;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Stations;

public class DryingRackCanvas : Singleton<DryingRackCanvas>
{
	[Header("References")]
	public Canvas Canvas;

	public RectTransform Container;

	public UIScreen UIScreen;

	public UIPanel ProgressContainerPanel;

	public ItemSlotUI InputSlotUI;

	public ItemSlotUI OutputSlotUI;

	public TextMeshProUGUI InstructionLabel;

	public TextMeshProUGUI CapacityLabel;

	public Button InsertButton;

	public RectTransform IndicatorContainer;

	public RectTransform[] IndicatorAlignments;

	[Header("Prefabs")]
	public DryingOperationUI IndicatorPrefab;

	private List<DryingOperationUI> operationUIs = new List<DryingOperationUI>();

	public bool isOpen { get; protected set; }

	public DryingRack Rack { get; protected set; }

	protected override void Start()
	{
		base.Start();
		Singleton<ItemUIManager>.Instance.AddRaycaster(((Component)Canvas).GetComponent<GraphicRaycaster>());
		SetIsOpen(null, open: false);
		NetworkSingleton<TimeManager>.Instance.onMinutePass += new Action(MinPass);
	}

	private void MinPass()
	{
		if (isOpen)
		{
			UpdateDryingOperations();
		}
	}

	protected virtual void Update()
	{
		if (isOpen)
		{
			UpdateUI();
		}
	}

	private void UpdateUI()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		((Selectable)InsertButton).interactable = Rack.CanStartOperation();
		((TMP_Text)CapacityLabel).text = Rack.GetTotalDryingItems() + " / " + Rack.ItemCapacity;
		((Graphic)CapacityLabel).color = ((Rack.GetTotalDryingItems() >= Rack.ItemCapacity) ? Color32.op_Implicit(new Color32(byte.MaxValue, (byte)50, (byte)50, byte.MaxValue)) : Color.white);
	}

	private void UpdateDryingOperations()
	{
		foreach (DryingOperationUI operationUI in operationUIs)
		{
			RectTransform alignment = null;
			DryingOperation assignedOperation = operationUI.AssignedOperation;
			if (assignedOperation.StartQuality == EQuality.Trash)
			{
				alignment = IndicatorAlignments[0];
			}
			else if (assignedOperation.StartQuality == EQuality.Poor)
			{
				alignment = IndicatorAlignments[1];
			}
			else if (assignedOperation.StartQuality == EQuality.Standard)
			{
				alignment = IndicatorAlignments[2];
			}
			else if (assignedOperation.StartQuality == EQuality.Premium)
			{
				alignment = IndicatorAlignments[3];
			}
			else
			{
				Console.LogWarning("Alignment not found for quality: " + assignedOperation.StartQuality);
			}
			if (Object.op_Implicit((Object)(object)Rack))
			{
				operationUI.SetDryRate(Rack.GetDryMultiplier());
			}
			operationUI.SetAlignment(alignment);
		}
	}

	private void UpdateQuantities()
	{
		foreach (DryingOperationUI operationUI in operationUIs)
		{
			operationUI.RefreshQuantity();
		}
	}

	public void SetIsOpen(DryingRack rack, bool open)
	{
		isOpen = open;
		((Behaviour)Canvas).enabled = open;
		((Component)Container).gameObject.SetActive(open);
		if (PlayerSingleton<PlayerCamera>.InstanceExists)
		{
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
		}
		if (open)
		{
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(((Object)this).name);
			InputSlotUI.AssignSlot(rack.InputSlot);
			OutputSlotUI.AssignSlot(rack.OutputSlot);
			Singleton<InputPromptsCanvas>.Instance.LoadModule("exitonly");
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
			PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: false);
			Singleton<UIScreenManager>.Instance.AddScreen(UIScreen);
			if (PlayerSingleton<PlayerInventory>.InstanceExists)
			{
				PlayerSingleton<PlayerInventory>.Instance.AttachToScreen(UIScreen);
			}
			for (int i = 0; i < rack.DryingOperations.Count; i++)
			{
				CreateOperationUI(rack.DryingOperations[i]);
			}
			rack.onOperationStart = (Action<DryingOperation>)Delegate.Combine(rack.onOperationStart, new Action<DryingOperation>(CreateOperationUI));
			rack.onOperationComplete = (Action<DryingOperation>)Delegate.Combine(rack.onOperationComplete, new Action<DryingOperation>(DestroyOperationUI));
			rack.onOperationsChanged = (Action)Delegate.Combine(rack.onOperationsChanged, new Action(UpdateQuantities));
		}
		else
		{
			InputSlotUI.ClearSlot();
			OutputSlotUI.ClearSlot();
			Singleton<InputPromptsCanvas>.Instance.UnloadModule();
			if ((Object)(object)Rack != (Object)null)
			{
				DryingRack rack2 = Rack;
				rack2.onOperationStart = (Action<DryingOperation>)Delegate.Remove(rack2.onOperationStart, new Action<DryingOperation>(CreateOperationUI));
				DryingRack rack3 = Rack;
				rack3.onOperationComplete = (Action<DryingOperation>)Delegate.Remove(rack3.onOperationComplete, new Action<DryingOperation>(DestroyOperationUI));
				DryingRack rack4 = Rack;
				rack4.onOperationsChanged = (Action)Delegate.Remove(rack4.onOperationsChanged, new Action(UpdateQuantities));
			}
			foreach (DryingOperationUI operationUI in operationUIs)
			{
				Object.Destroy((Object)(object)((Component)operationUI).gameObject);
			}
			operationUIs.Clear();
			ProgressContainerPanel.ClearAllSelectables();
			if (PlayerSingleton<PlayerInventory>.InstanceExists)
			{
				PlayerSingleton<PlayerInventory>.Instance.DetachFromScreen();
			}
			Singleton<UIScreenManager>.Instance.RemoveScreen(UIScreen);
		}
		Singleton<ItemUIManager>.Instance.SetDraggingEnabled(open);
		if (open)
		{
			List<ItemSlot> list = new List<ItemSlot>();
			list.AddRange(rack.InputSlots);
			list.Add(rack.OutputSlot);
			Singleton<ItemUIManager>.Instance.EnableQuickMove(PlayerSingleton<PlayerInventory>.Instance.GetAllInventorySlots(), list);
		}
		Rack = rack;
		if (open)
		{
			UpdateUI();
			MinPass();
		}
	}

	private void CreateOperationUI(DryingOperation operation)
	{
		DryingOperationUI dryingOperationUI = Object.Instantiate<DryingOperationUI>(IndicatorPrefab, (Transform)(object)IndicatorContainer);
		UISelectable component = ((Component)dryingOperationUI).GetComponent<UISelectable>();
		if (Object.op_Implicit((Object)(object)component))
		{
			ProgressContainerPanel.AddSelectable(component);
		}
		dryingOperationUI.SetOperation(operation);
		operationUIs.Add(dryingOperationUI);
		UpdateDryingOperations();
	}

	private void DestroyOperationUI(DryingOperation operation)
	{
		DryingOperationUI dryingOperationUI = operationUIs.FirstOrDefault((DryingOperationUI x) => x.AssignedOperation == operation);
		if ((Object)(object)dryingOperationUI != (Object)null)
		{
			UISelectable component = ((Component)dryingOperationUI).GetComponent<UISelectable>();
			if (Object.op_Implicit((Object)(object)component))
			{
				ProgressContainerPanel.RemoveSelectable(component);
			}
			operationUIs.Remove(dryingOperationUI);
			Object.Destroy((Object)(object)((Component)dryingOperationUI).gameObject);
		}
	}

	public void Insert()
	{
		if (((Behaviour)InsertButton).isActiveAndEnabled && ((Selectable)InsertButton).interactable)
		{
			Rack.StartOperation();
		}
	}
}
