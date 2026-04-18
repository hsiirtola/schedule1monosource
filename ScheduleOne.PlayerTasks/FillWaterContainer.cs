using FishNet.Object;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Property;
using ScheduleOne.Tools;
using UnityEngine;

namespace ScheduleOne.PlayerTasks;

public class FillWaterContainer : Task
{
	private Tap _tap;

	private WaterContainerInstance _waterContainerItem;

	private FillableWaterContainer _fillable;

	public new string TaskName { get; protected set; } = "Fill watering can";

	public FillWaterContainer(Tap tap, WaterContainerInstance waterContainerItem)
	{
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		_tap = tap;
		_waterContainerItem = waterContainerItem;
		ClickDetectionEnabled = true;
		_fillable = Object.Instantiate<FillableWaterContainer>(_waterContainerItem.WaterContainerDefinition.FillablePrefab, _tap.FillableModelContainer);
		if ((Object)(object)_fillable.Visuals != (Object)null)
		{
			_fillable.Visuals.AssignWaterContainer(_waterContainerItem);
		}
		_tap.SetPlayerUser(((NetworkBehaviour)Player.Local).NetworkObject);
		_tap.SetHandleEnabled(enabled: true);
		_tap.SetMaxTapOpen(_fillable.MaxTapOpenValue);
		PlayerSingleton<PlayerCamera>.Instance.OpenInterface();
		PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(_tap.CameraPos.position, _tap.CameraPos.rotation, 0.25f);
		PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(70f, 0.25f);
		UpdateInstruction();
	}

	public override void StopTask()
	{
		_tap.SetHandleEnabled(enabled: false);
		_tap.SetPlayerUser(null);
		PlayerSingleton<PlayerCamera>.Instance.CloseInterface();
		if ((Object)(object)_fillable.Visuals != (Object)null)
		{
			_fillable.Visuals.UnassignWaterContainer();
		}
		Object.Destroy((Object)(object)((Component)_fillable).gameObject);
		base.StopTask();
	}

	public override void Update()
	{
		base.Update();
		if (_tap.ActualFlowRate > 0f)
		{
			_waterContainerItem.ChangeFillAmount(_tap.ActualFlowRate * Time.deltaTime);
		}
		if (_waterContainerItem.NormalizedFillAmount >= 1f)
		{
			Success();
			return;
		}
		UpdateFillSound();
		UpdateInstruction();
	}

	private void UpdateInstruction()
	{
		base.CurrentInstruction = $"Click and hold tap to fill ({Mathf.FloorToInt(_waterContainerItem.NormalizedFillAmount * 100f)}%)";
	}

	private void UpdateFillSound()
	{
		if ((Object)(object)_fillable.FillSound == (Object)null)
		{
			return;
		}
		if (_tap.ActualFlowRate > 0f)
		{
			if (!_fillable.FillSound.IsPlaying)
			{
				_fillable.FillSound.Play();
			}
			_fillable.FillSound.VolumeMultiplier = Mathf.MoveTowards(_fillable.FillSound.VolumeMultiplier, 1f, Time.deltaTime * 4f);
		}
		else if (_fillable.FillSound.IsPlaying)
		{
			_fillable.FillSound.VolumeMultiplier = Mathf.MoveTowards(_fillable.FillSound.VolumeMultiplier, 0f, Time.deltaTime * 4f);
			if (_fillable.FillSound.VolumeMultiplier <= 0f)
			{
				_fillable.FillSound.Stop();
			}
		}
	}
}
