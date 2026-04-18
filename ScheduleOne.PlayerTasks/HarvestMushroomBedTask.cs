using FishNet.Object;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.Growing;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.PlayerTasks;

public class HarvestMushroomBedTask : Task
{
	private AudioSourceController _soundLoop;

	private MushroomBed _mushroomBed;

	protected bool _canDrag;

	private int _harvestCount;

	private int _harvestTotal;

	public HarvestMushroomBedTask(MushroomBed mushroomBed, bool canDrag, AudioSourceController soundLoopPrefab)
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)mushroomBed == (Object)null)
		{
			Console.LogWarning("HarvestMushroomBedTask: mushroomBed null");
			StopTask();
			return;
		}
		if ((Object)(object)mushroomBed.CurrentColony == (Object)null)
		{
			Console.LogWarning("HarvestMushroomBedTask: mushroomBed has no colony");
		}
		ClickDetectionEnabled = true;
		_canDrag = canDrag;
		ClickDetectionRadius = 0.02f;
		_mushroomBed = mushroomBed;
		_mushroomBed.SetPlayerUser(((NetworkBehaviour)Player.Local).NetworkObject);
		Transform cameraPosition = mushroomBed.CameraHandler.GetCameraPosition(GrowContainerCameraHandler.ECameraPosition.Midshot);
		PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(cameraPosition.position, cameraPosition.rotation, 0.25f);
		PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(70f, 0.25f);
		PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
		PlayerSingleton<PlayerMovement>.Instance.CanMove = false;
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
		_harvestTotal = mushroomBed.CurrentColony.GrownMushroomCount;
		UpdateInstructionText();
		if ((Object)(object)soundLoopPrefab != (Object)null)
		{
			_soundLoop = Object.Instantiate<AudioSourceController>(soundLoopPrefab, NetworkSingleton<GameManager>.Instance.Temp);
			_soundLoop.VolumeMultiplier = 0f;
			((Component)_soundLoop).transform.position = ((Component)mushroomBed).transform.position + Vector3.up * 1f;
			_soundLoop.Play();
		}
		Singleton<InputPromptsCanvas>.Instance.LoadModule("harvestshrooms");
	}

	public override void StopTask()
	{
		base.StopTask();
		PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0.25f);
		PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(0.25f);
		PlayerSingleton<PlayerCamera>.Instance.LockMouse();
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
		PlayerSingleton<PlayerMovement>.Instance.CanMove = true;
		Singleton<InputPromptsCanvas>.Instance.UnloadModule();
		_mushroomBed.SetPlayerUser(null);
		if ((Object)(object)_soundLoop != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)_soundLoop).gameObject);
		}
		Singleton<CursorManager>.Instance.SetCursorAppearance(CursorManager.ECursorType.Default);
	}

	public override void Update()
	{
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if ((Object)(object)_mushroomBed == (Object)null || (Object)(object)_mushroomBed.CurrentColony == (Object)null)
		{
			StopTask();
			return;
		}
		GrowingMushroom hoveredHarvestable = GetHoveredHarvestable();
		if ((Object)(object)_soundLoop != (Object)null)
		{
			if (GameInput.GetButton(GameInput.ButtonCode.PrimaryClick))
			{
				_soundLoop.VolumeMultiplier = Mathf.MoveTowards(_soundLoop.VolumeMultiplier, 1f, Time.deltaTime * 4f);
			}
			else
			{
				_soundLoop.VolumeMultiplier = Mathf.MoveTowards(_soundLoop.VolumeMultiplier, 0f, Time.deltaTime * 4f);
			}
		}
		if (!((Object)(object)hoveredHarvestable != (Object)null))
		{
			return;
		}
		if (!PlayerSingleton<PlayerInventory>.Instance.CanItemFitInInventory(_mushroomBed.CurrentColony.GetHarvestedShroom()))
		{
			Singleton<MouseTooltip>.Instance.ShowIcon(Singleton<MouseTooltip>.Instance.Sprite_Cross, Singleton<MouseTooltip>.Instance.Color_Invalid);
			Singleton<MouseTooltip>.Instance.ShowTooltip("Inventory full", Singleton<MouseTooltip>.Instance.Color_Invalid);
		}
		else if (GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick) || (GameInput.GetButton(GameInput.ButtonCode.PrimaryClick) && _canDrag))
		{
			GameObject obj = Object.Instantiate<GameObject>(((Component)_mushroomBed.CurrentColony.SnipSound).gameObject);
			obj.transform.position = ((Component)hoveredHarvestable).transform.position;
			obj.GetComponent<AudioSourceController>().PlayOneShot();
			Object.Destroy((Object)(object)obj, 1f);
			hoveredHarvestable.Harvest();
			_harvestCount++;
			UpdateInstructionText();
			if ((Object)(object)_mushroomBed.CurrentColony == (Object)null)
			{
				Success();
			}
		}
	}

	private void UpdateInstructionText()
	{
		if (!((Object)(object)_mushroomBed == (Object)null) && !((Object)(object)_mushroomBed.CurrentColony == (Object)null))
		{
			if (_canDrag)
			{
				base.CurrentInstruction = "Click and hold over mushrooms to harvest (" + _harvestCount + "/" + _harvestTotal + ")";
			}
			else
			{
				base.CurrentInstruction = "Click mushrooms to harvest (" + _harvestCount + "/" + _harvestTotal + ")";
			}
		}
	}

	protected override void UpdateCursor()
	{
		if ((Object)(object)GetHoveredHarvestable() != (Object)null)
		{
			Singleton<CursorManager>.Instance.SetCursorAppearance(CursorManager.ECursorType.Scissors);
		}
		else
		{
			Singleton<CursorManager>.Instance.SetCursorAppearance(CursorManager.ECursorType.Default);
		}
	}

	private GrowingMushroom GetHoveredHarvestable()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (PlayerSingleton<PlayerCamera>.Instance.MouseRaycast(3f, out var hit, clickablesLayerMask))
		{
			GrowingMushroom componentInParent = ((Component)((RaycastHit)(ref hit)).collider).gameObject.GetComponentInParent<GrowingMushroom>();
			if ((Object)(object)componentInParent != (Object)null && ((Component)componentInParent).transform.IsChildOf(((Component)_mushroomBed).transform))
			{
				return componentInParent;
			}
		}
		return null;
	}
}
