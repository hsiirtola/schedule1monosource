using System;
using FishNet.Object;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Growing;
using ScheduleOne.ItemFramework;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Trash;
using ScheduleOne.UI;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.PlayerTasks.Tasks;

public class GrowContainerPourTask : Task
{
	protected GrowContainer growContainer;

	protected ItemInstance item;

	protected Pourable pourable;

	protected bool removeItemAfterInitialPour;

	public override string TaskName { get; protected set; } = "Pour";

	protected virtual bool UseCoverage { get; }

	protected virtual bool FailOnEmpty { get; } = true;

	protected virtual GrowContainerCameraHandler.ECameraPosition CameraPosition { get; } = GrowContainerCameraHandler.ECameraPosition.Midshot;

	public GrowContainerPourTask(GrowContainer _growContainer, ItemInstance _itemInstance, Pourable _pourablePrefab)
	{
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Expected O, but got Unknown
		if ((Object)(object)_growContainer == (Object)null)
		{
			Console.LogWarning("PourIntoPotTask: pot null");
			StopTask();
			return;
		}
		if ((Object)(object)_pourablePrefab == (Object)null)
		{
			Console.LogWarning("PourIntoPotTask: pourablePrefab null");
			StopTask();
			return;
		}
		ClickDetectionEnabled = true;
		item = _itemInstance;
		growContainer = _growContainer;
		if (growContainer.HidePlantDuringPourTasks)
		{
			growContainer.SetGrowableVisible(visible: false);
		}
		growContainer.SetPlayerUser(((NetworkBehaviour)Player.Local).NetworkObject);
		Transform cameraPosition = growContainer.CameraHandler.GetCameraPosition(CameraPosition);
		PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(cameraPosition.position, cameraPosition.rotation, 0.25f);
		PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(70f, 0.25f);
		PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
		PlayerSingleton<PlayerMovement>.Instance.CanMove = false;
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
		Singleton<OnScreenMouse>.Instance.Activate();
		pourable = Object.Instantiate<GameObject>(((Component)_pourablePrefab).gameObject, NetworkSingleton<GameManager>.Instance.Temp).GetComponent<Pourable>();
		((Component)pourable).transform.position = growContainer.PourableStartPoint.position;
		pourable.Rb.position = growContainer.PourableStartPoint.position;
		pourable.Origin = growContainer.PourableStartPoint.position;
		pourable.MaxDistanceFromOrigin = growContainer.GetGrowSurfaceSideLength();
		pourable.LocationRestrictionEnabled = true;
		pourable.TargetGrowContainer = _growContainer;
		Pourable obj = pourable;
		obj.onInitialPour = (Action)Delegate.Combine(obj.onInitialPour, new Action(OnInitialPour));
		Vector3 val = ((Component)cameraPosition).transform.position - ((Component)pourable).transform.position;
		((Component)pourable).transform.rotation = Quaternion.LookRotation(new Vector3(val.x, 0f, val.z), Vector3.up);
		pourable.Rb.rotation = Quaternion.LookRotation(new Vector3(val.x, 0f, val.z), Vector3.up);
		Singleton<InputPromptsCanvas>.Instance.LoadModule("pourable");
		if (UseCoverage)
		{
			growContainer.SurfaceCover.Reset();
			((Component)growContainer.SurfaceCover).gameObject.SetActive(true);
			growContainer.SurfaceCover.onSufficientCoverage.AddListener(new UnityAction(FullyCovered));
		}
	}

	public override void Update()
	{
		base.Update();
		if (FailOnEmpty && pourable.CurrentQuantity <= 0f)
		{
			Fail();
		}
	}

	public override void StopTask()
	{
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Expected O, but got Unknown
		base.StopTask();
		PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0.15f);
		PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(0.15f);
		PlayerSingleton<PlayerCamera>.Instance.LockMouse();
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
		Singleton<OnScreenMouse>.Instance.Deactivate();
		PlayerSingleton<PlayerMovement>.Instance.CanMove = true;
		Object.Destroy((Object)(object)((Component)pourable).gameObject);
		Singleton<InputPromptsCanvas>.Instance.UnloadModule();
		if (UseCoverage)
		{
			growContainer.SurfaceCover.onSufficientCoverage.RemoveListener(new UnityAction(FullyCovered));
			((Component)growContainer.SurfaceCover).gameObject.SetActive(false);
		}
		growContainer.SetGrowableVisible(visible: true);
		growContainer.SetPlayerUser(null);
	}

	protected virtual void OnInitialPour()
	{
		if (removeItemAfterInitialPour)
		{
			RemoveItem();
		}
	}

	protected void RemoveItem()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		PlayerSingleton<PlayerInventory>.Instance.RemoveAmountOfItem(((BaseItemInstance)item).ID);
		if ((Object)(object)pourable.TrashItem != (Object)null)
		{
			NetworkSingleton<TrashManager>.Instance.CreateTrashItem(pourable.TrashItem.ID, ((Component)Player.Local.Avatar).transform.position + Vector3.up * 0.3f, Random.rotation);
		}
	}

	protected virtual void FullyCovered()
	{
	}
}
