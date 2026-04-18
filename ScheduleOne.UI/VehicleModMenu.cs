using System.Collections;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Money;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI.Compass;
using ScheduleOne.Vehicles;
using ScheduleOne.Vehicles.Modification;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public class VehicleModMenu : Singleton<VehicleModMenu>
{
	public static float repaintCost = 100f;

	[Header("UI References")]
	[SerializeField]
	protected Canvas canvas;

	[SerializeField]
	protected RectTransform buttonContainer;

	[SerializeField]
	protected RectTransform tempIndicator;

	[SerializeField]
	protected RectTransform permIndicator;

	[SerializeField]
	protected Button confirmButton_Online;

	[SerializeField]
	protected TextMeshProUGUI confirmText_Online;

	[Header("References")]
	public Transform CameraPosition;

	public Transform VehiclePosition;

	[Header("Prefabs")]
	[SerializeField]
	protected GameObject buttonPrefab;

	public UnityEvent onPaintPurchased;

	protected LandVehicle currentVehicle;

	protected List<RectTransform> colorButtons = new List<RectTransform>();

	protected Dictionary<EVehicleColor, RectTransform> colorToButton = new Dictionary<EVehicleColor, RectTransform>();

	protected EVehicleColor selectedColor = EVehicleColor.White;

	private Coroutine openCloseRoutine;

	public bool IsOpen { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		GameInput.RegisterExitListener(Exit, 1);
	}

	protected override void Start()
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Expected O, but got Unknown
		base.Start();
		((TMP_Text)confirmText_Online).text = "Confirm (" + MoneyManager.ApplyOnlineBalanceColor(MoneyManager.FormatAmount(repaintCost)) + ")";
		for (int i = 0; i < Singleton<VehicleColors>.Instance.colorLibrary.Count; i++)
		{
			RectTransform component = Object.Instantiate<GameObject>(buttonPrefab, (Transform)(object)buttonContainer).GetComponent<RectTransform>();
			component.anchoredPosition = new Vector2((0.5f + (float)colorButtons.Count) * component.sizeDelta.x, component.anchoredPosition.y);
			((Graphic)((Component)((Transform)component).Find("Image")).GetComponent<Image>()).color = Color32.op_Implicit(Singleton<VehicleColors>.Instance.colorLibrary[i].UIColor);
			EVehicleColor c = Singleton<VehicleColors>.Instance.colorLibrary[i].color;
			colorButtons.Add(component);
			colorToButton.Add(c, component);
			((UnityEvent)((Component)component).GetComponent<Button>().onClick).AddListener((UnityAction)delegate
			{
				ColorClicked(c);
			});
		}
	}

	private void Exit(ExitAction action)
	{
		if (!action.Used && IsOpen && openCloseRoutine == null && action.exitType == ExitType.Escape)
		{
			action.Used = true;
			Close();
		}
	}

	protected virtual void Update()
	{
		if (IsOpen)
		{
			UpdateConfirmButton();
		}
	}

	public void Open(LandVehicle vehicle)
	{
		currentVehicle = vehicle;
		selectedColor = vehicle.OwnedColor;
		PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(((Object)this).name);
		Singleton<CompassManager>.Instance.SetVisible(visible: false);
		openCloseRoutine = ((MonoBehaviour)this).StartCoroutine(Close());
		IEnumerator Close()
		{
			Singleton<BlackOverlay>.Instance.Open();
			yield return (object)new WaitForSeconds(0.6f);
			IsOpen = true;
			((Behaviour)canvas).enabled = true;
			currentVehicle.AlignTo(VehiclePosition, EParkingAlignment.RearToKerb, network: true);
			RefreshSelectionIndicator();
			UpdateConfirmButton();
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
			PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
			PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(CameraPosition.position, CameraPosition.rotation, 0f);
			PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(60f, 0f);
			Singleton<InputPromptsCanvas>.Instance.LoadModule("exitonly");
			Singleton<BlackOverlay>.Instance.Close();
			openCloseRoutine = null;
		}
	}

	public void Close()
	{
		if ((Object)(object)currentVehicle != (Object)null)
		{
			currentVehicle.ApplyOwnedColor();
		}
		openCloseRoutine = ((MonoBehaviour)this).StartCoroutine(Close());
		IEnumerator Close()
		{
			Singleton<BlackOverlay>.Instance.Open();
			yield return (object)new WaitForSeconds(0.6f);
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
			currentVehicle = null;
			IsOpen = false;
			((Behaviour)canvas).enabled = false;
			Singleton<CompassManager>.Instance.SetVisible(visible: true);
			PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: true);
			PlayerSingleton<PlayerCamera>.Instance.LockMouse();
			PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0f);
			PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(0f);
			PlayerSingleton<PlayerMovement>.Instance.CanMove = true;
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
			Singleton<InputPromptsCanvas>.Instance.UnloadModule();
			Singleton<BlackOverlay>.Instance.Close();
			openCloseRoutine = null;
		}
	}

	public void ColorClicked(EVehicleColor col)
	{
		selectedColor = col;
		currentVehicle.ApplyColor(col);
		RefreshSelectionIndicator();
		UpdateConfirmButton();
	}

	private void UpdateConfirmButton()
	{
		bool flag = NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance >= repaintCost;
		((Selectable)confirmButton_Online).interactable = flag && selectedColor != currentVehicle.OwnedColor;
	}

	private void RefreshSelectionIndicator()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		((Transform)tempIndicator).position = ((Transform)colorToButton[selectedColor]).position;
		((Transform)permIndicator).position = ((Transform)colorToButton[currentVehicle.OwnedColor]).position;
	}

	public void ConfirmButtonClicked()
	{
		NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction("Vehicle repaint", 0f - repaintCost, 1f, string.Empty);
		NetworkSingleton<MoneyManager>.Instance.PlayCashSound();
		currentVehicle.SendOwnedColor(selectedColor);
		RefreshSelectionIndicator();
		if (onPaintPurchased != null)
		{
			onPaintPurchased.Invoke();
		}
		Close();
	}
}
