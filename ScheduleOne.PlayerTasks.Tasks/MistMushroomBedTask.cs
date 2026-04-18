using FishNet.Object;
using ScheduleOne.DevUtilities;
using ScheduleOne.Growing;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.PlayerTasks.Tasks;

public class MistMushroomBedTask : Task
{
	private const float TARGET_SPRAY_RADIUS = 0.15f;

	private const float TARGET_SPRAY_DISTANCE = 0.35f;

	private MushroomBed _mushroomBed;

	private Sprayable _sprayable;

	private GameObject _sprayableObj;

	private WaterContainerInstance _waterContainerInstance;

	public override string TaskName { get; protected set; } = "Mist";

	public MistMushroomBedTask(MushroomBed mushroomBed, ItemInstance item, GameObject sprayablePrefab)
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Expected O, but got Unknown
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)mushroomBed == (Object)null)
		{
			Console.LogWarning("[MistMushroomBedTask] mushroomBed null");
			StopTask();
			return;
		}
		if ((Object)(object)sprayablePrefab == (Object)null)
		{
			Console.LogWarning("[MistMushroomBedTask] sprayablePrefab null");
			StopTask();
			return;
		}
		base.CurrentInstruction = "Mist the target area";
		ClickDetectionEnabled = true;
		_mushroomBed = mushroomBed;
		_waterContainerInstance = item as WaterContainerInstance;
		Singleton<InputPromptsCanvas>.Instance.LoadModule("mistingtask");
		mushroomBed.SetPlayerUser(((NetworkBehaviour)Player.Local).NetworkObject);
		Transform cameraPosition = mushroomBed.CameraHandler.GetCameraPosition(GrowContainerCameraHandler.ECameraPosition.BirdsEye);
		PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(cameraPosition.position, cameraPosition.rotation, 0.25f);
		PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(70f, 0.25f);
		PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
		PlayerSingleton<PlayerMovement>.Instance.CanMove = false;
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
		Vector3 forward = ((Component)cameraPosition).transform.forward;
		forward.y = 0f;
		_sprayableObj = Object.Instantiate<GameObject>(sprayablePrefab, NetworkSingleton<GameManager>.Instance.Temp);
		_sprayable = _sprayableObj.GetComponentInChildren<Sprayable>();
		_sprayable.Initialise(0.15f, 0.35f);
		_sprayableObj.transform.position = mushroomBed.PourableStartPoint.position;
		((Component)_sprayable).transform.position = mushroomBed.PourableStartPoint.position;
		_sprayable.Rb.position = mushroomBed.PourableStartPoint.position;
		_sprayable.Origin = mushroomBed.PourableStartPoint.position;
		_sprayable.MaxDistanceFromOrigin = mushroomBed.GetGrowSurfaceSideLength();
		_sprayable.LocationRestrictionEnabled = true;
		Vector3 val = ((Component)cameraPosition).transform.position - _sprayableObj.transform.position;
		_sprayableObj.transform.rotation = Quaternion.LookRotation(new Vector3(val.x, 0f, val.z), Vector3.up);
		mushroomBed.RandomizePourTargetPosition();
		mushroomBed.SetPourTargetActive(active: true);
		_sprayable.SetCurrentTarget(mushroomBed.GetCurrentTargetPosition());
		_sprayable.SubscribeToSuccessfulSpray(OnSuccessfulSpray);
		_sprayable.onSpray.AddListener(new UnityAction(OnSpray));
		mushroomBed.SurfaceCover.Reset();
		((Component)mushroomBed.SurfaceCover).gameObject.SetActive(true);
		mushroomBed.SurfaceCover.PourApplicationStrength = 3f;
		mushroomBed.SurfaceCover.ConfigureAppearance(Color.black, 0.6f);
		mushroomBed.SurfaceCover.UseApplyOverTime = true;
	}

	private void OnSuccessfulSpray()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		_mushroomBed.SurfaceCover.QueuePour(_mushroomBed.GetCurrentTargetPosition());
		_mushroomBed.ChangeMoistureAmount(1f);
		_mushroomBed.RandomizePourTargetPosition();
		_sprayable.SetCurrentTarget(_mushroomBed.GetCurrentTargetPosition());
		_mushroomBed.SyncMoistureData();
		Singleton<TaskManager>.Instance.PlayTaskCompleteSound();
		if (_mushroomBed.NormalizedMoistureAmount >= 0.95f)
		{
			Success();
		}
		else if (_waterContainerInstance.CurrentFillAmount <= 0f)
		{
			Fail();
		}
	}

	private void OnSpray()
	{
		_waterContainerInstance.ChangeFillAmountByPercentage(-0.02f);
	}

	public override void StopTask()
	{
		base.StopTask();
		PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0.15f);
		PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(0.15f);
		PlayerSingleton<PlayerCamera>.Instance.LockMouse();
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
		PlayerSingleton<PlayerMovement>.Instance.CanMove = true;
		_sprayable.UnsubscribeFromSuccessfulSpray(OnSuccessfulSpray);
		Object.Destroy((Object)(object)_sprayableObj.gameObject);
		Singleton<InputPromptsCanvas>.Instance.UnloadModule();
		((Component)_mushroomBed.SurfaceCover).gameObject.SetActive(false);
		_mushroomBed.SetPourTargetActive(active: false);
		_mushroomBed.SetPlayerUser(null);
	}
}
