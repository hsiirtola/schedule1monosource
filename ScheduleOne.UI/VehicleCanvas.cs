using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Vehicles;
using TMPro;
using UnityEngine;

namespace ScheduleOne.UI;

public class VehicleCanvas : Singleton<VehicleCanvas>
{
	[Header("References")]
	public Canvas Canvas;

	public TextMeshProUGUI SpeedText;

	public GameObject DriverPromptsContainer;

	private LandVehicle currentVehicle;

	protected override void Start()
	{
		base.Start();
		Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, new Action(Subscribe));
	}

	private void Subscribe()
	{
		Player local = Player.Local;
		local.onEnterVehicle = (Player.VehicleEvent)Delegate.Combine(local.onEnterVehicle, new Player.VehicleEvent(VehicleEntered));
		Player local2 = Player.Local;
		local2.onExitVehicle = (Player.VehicleTransformEvent)Delegate.Combine(local2.onExitVehicle, new Player.VehicleTransformEvent(VehicleExited));
	}

	private void Update()
	{
		if (!((Object)(object)Player.Local == (Object)null) && (Object)(object)Player.Local.CurrentVehicle != (Object)null)
		{
			((Behaviour)Canvas).enabled = !Singleton<GameplayMenu>.Instance.IsOpen;
		}
	}

	private void LateUpdate()
	{
		if ((Object)(object)currentVehicle != (Object)null)
		{
			UpdateSpeedText();
		}
	}

	private void VehicleEntered(LandVehicle veh)
	{
		currentVehicle = veh;
		UpdateSpeedText();
		((Behaviour)Canvas).enabled = true;
		DriverPromptsContainer.SetActive(currentVehicle.LocalPlayerIsDriver);
	}

	private void VehicleExited(LandVehicle veh, Transform exitPoint)
	{
		((Behaviour)Canvas).enabled = false;
		currentVehicle = null;
	}

	private void UpdateSpeedText()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)SpeedText == (Object)null))
		{
			TextMeshProUGUI speedText = SpeedText;
			Vector3 velocity = currentVehicle.VelocityCalculator.Velocity;
			((TMP_Text)speedText).text = UnitsUtility.FormatSpeed(Mathf.Abs(((Vector3)(ref velocity)).magnitude * 1.4f), UnitsUtility.ERoundingType.Nearest, 0);
		}
	}
}
