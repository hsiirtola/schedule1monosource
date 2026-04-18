using FishNet.Object;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.Growing;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.PlayerTasks;

public class HarvestPlant : Task
{
	protected Pot pot;

	private int HarvestCount;

	private int HarvestTotal;

	private float rotation;

	private static bool hintShown;

	private static bool CanDrag;

	private AudioSourceController SoundLoop;

	public override string TaskName { get; protected set; } = "Harvest plant";

	public HarvestPlant(Pot _pot, bool canDrag, AudioSourceController soundLoopPrefab)
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_pot == (Object)null)
		{
			Console.LogWarning("HarvestPlant: pot null");
			StopTask();
			return;
		}
		if ((Object)(object)_pot.Plant == (Object)null)
		{
			Console.LogWarning("HarvestPlant: pot has no plant in it");
		}
		ClickDetectionEnabled = true;
		CanDrag = canDrag;
		ClickDetectionRadius = 0.02f;
		pot = _pot;
		pot.SetPlayerUser(((NetworkBehaviour)Player.Local).NetworkObject);
		Transform cameraPosition = pot.CameraHandler.GetCameraPosition(GrowContainerCameraHandler.ECameraPosition.Fullshot);
		PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(cameraPosition.position, cameraPosition.rotation, 0.25f);
		PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(70f, 0.25f);
		PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
		PlayerSingleton<PlayerMovement>.Instance.CanMove = false;
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
		pot.Plant.Collider.enabled = false;
		((Component)pot.LeafDropPoint).transform.rotation = Quaternion.LookRotation(((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position - pot.LeafDropPoint.position, Vector3.up);
		HarvestTotal = pot.Plant.ActiveHarvestables.Count;
		UpdateInstructionText();
		if ((Object)(object)soundLoopPrefab != (Object)null)
		{
			SoundLoop = Object.Instantiate<AudioSourceController>(soundLoopPrefab, NetworkSingleton<GameManager>.Instance.Temp);
			SoundLoop.VolumeMultiplier = 0f;
			((Component)SoundLoop).transform.position = ((Component)pot).transform.position + Vector3.up * 1f;
			SoundLoop.Play();
		}
		Singleton<InputPromptsCanvas>.Instance.LoadModule("harvestplant");
		Singleton<OnScreenMouse>.Instance.Activate();
	}

	private void UpdateInstructionText()
	{
		if (!((Object)(object)pot == (Object)null) && !((Object)(object)pot.Plant == (Object)null))
		{
			if (CanDrag)
			{
				base.CurrentInstruction = "Click and hold over " + pot.Plant.HarvestTarget + " to harvest (" + HarvestCount + "/" + HarvestTotal + ")";
			}
			else
			{
				base.CurrentInstruction = "Click " + pot.Plant.HarvestTarget + " to harvest (" + HarvestCount + "/" + HarvestTotal + ")";
			}
		}
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
		if ((Object)(object)pot.Plant != (Object)null)
		{
			pot.Plant.Collider.enabled = true;
		}
		if ((Object)(object)SoundLoop != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)SoundLoop).gameObject);
		}
		pot.SetPlayerUser(null);
		Singleton<CursorManager>.Instance.SetCursorAppearance(CursorManager.ECursorType.Default);
		Singleton<OnScreenMouse>.Instance.Deactivate();
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

	public override void Update()
	{
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if ((Object)(object)pot == (Object)null || (Object)(object)pot.Plant == (Object)null)
		{
			StopTask();
			return;
		}
		PlantHarvestable hoveredHarvestable = GetHoveredHarvestable();
		if ((Object)(object)SoundLoop != (Object)null)
		{
			if (GameInput.GetButton(GameInput.ButtonCode.PrimaryClick))
			{
				SoundLoop.VolumeMultiplier = Mathf.MoveTowards(SoundLoop.VolumeMultiplier, 1f, Time.deltaTime * 4f);
			}
			else
			{
				SoundLoop.VolumeMultiplier = Mathf.MoveTowards(SoundLoop.VolumeMultiplier, 0f, Time.deltaTime * 4f);
			}
		}
		if ((Object)(object)hoveredHarvestable != (Object)null)
		{
			if (!PlayerSingleton<PlayerInventory>.Instance.CanItemFitInInventory(pot.Plant.GetHarvestedProduct(), hoveredHarvestable.ProductQuantity))
			{
				Singleton<MouseTooltip>.Instance.ShowIcon(Singleton<MouseTooltip>.Instance.Sprite_Cross, Singleton<MouseTooltip>.Instance.Color_Invalid);
				Singleton<MouseTooltip>.Instance.ShowTooltip("Inventory full", Singleton<MouseTooltip>.Instance.Color_Invalid);
			}
			else if (GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick) || (GameInput.GetButton(GameInput.ButtonCode.PrimaryClick) && CanDrag))
			{
				GameObject obj = Object.Instantiate<GameObject>(((Component)pot.Plant.SnipSound).gameObject);
				obj.transform.position = ((Component)hoveredHarvestable).transform.position;
				obj.GetComponent<AudioSourceController>().PlayOneShot();
				Object.Destroy((Object)(object)obj, 1f);
				hoveredHarvestable.Harvest();
				HarvestCount++;
				UpdateInstructionText();
				if ((Object)(object)pot.Plant == (Object)null)
				{
					Success();
				}
			}
		}
		if (GameInput.GetButton(GameInput.ButtonCode.Left) || Input.GetKey((KeyCode)276))
		{
			rotation -= Time.deltaTime * 100f;
		}
		if (GameInput.GetButton(GameInput.ButtonCode.Right) || Input.GetKey((KeyCode)275))
		{
			rotation += Time.deltaTime * 100f;
		}
		pot.OverrideRotation(rotation);
	}

	private PlantHarvestable GetHoveredHarvestable()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (PlayerSingleton<PlayerCamera>.Instance.MouseRaycast(3f, out var hit, clickablesLayerMask))
		{
			PlantHarvestable componentInParent = ((Component)((RaycastHit)(ref hit)).collider).gameObject.GetComponentInParent<PlantHarvestable>();
			if ((Object)(object)componentInParent != (Object)null && ((Component)componentInParent).transform.IsChildOf(((Component)pot).transform))
			{
				return componentInParent;
			}
		}
		return null;
	}
}
