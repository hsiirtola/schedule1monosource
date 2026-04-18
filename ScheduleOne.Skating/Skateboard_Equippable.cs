using FishNet.Object;
using ScheduleOne.DevUtilities;
using ScheduleOne.Equipping;
using ScheduleOne.ItemFramework;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ScheduleOne.Skating;

public class Skateboard_Equippable : Equippable_Viewmodel
{
	public const float ModelLerpSpeed = 8f;

	public const float SurfaceSampleDistance = 0.4f;

	public const float SurfaceSampleRayLength = 0.7f;

	public const float BoardSpawnUpwardsShift = 0.05f;

	public const float BoardSpawnAngleLimit = 30f;

	public const float MountTime = 0.33f;

	public const float BoardMomentumTransfer = 1.2f;

	public const float DismountAngle = 80f;

	public Skateboard SkateboardPrefab;

	public bool blockDismount;

	[Header("References")]
	public Transform ModelContainer;

	public Transform ModelPosition_Raised;

	public Transform ModelPosition_Lowered;

	private float mountTime;

	public bool IsRiding { get; private set; }

	public Skateboard ActiveSkateboard { get; private set; }

	public override void Equip(ItemInstance item)
	{
		base.Equip(item);
		GameInput.RegisterExitListener(Exit);
		Singleton<InputPromptsCanvas>.Instance.LoadModule("heldskateboard");
	}

	private void Exit(ExitAction action)
	{
		if (!action.Used && action.exitType == ExitType.Escape && IsRiding)
		{
			action.Used = true;
			Dismount();
		}
	}

	protected override void Update()
	{
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if (!blockDismount && !Singleton<PauseMenu>.Instance.IsPaused)
		{
			if (IsRiding)
			{
				if (GameInput.GetButtonDown(GameInput.ButtonCode.SkateboardDismount))
				{
					Dismount();
				}
			}
			else if (GameInput.GetButton(GameInput.ButtonCode.SkateboardMount))
			{
				if (CanMountHere() && !PlayerSingleton<PlayerMovement>.Instance.IsCrouched && (GameInput.GetButtonDown(GameInput.ButtonCode.SkateboardMount) || mountTime > 0f))
				{
					mountTime += Time.deltaTime;
					Singleton<HUD>.Instance.ShowRadialIndicator(mountTime / 0.33f);
					if (mountTime >= 0.33f)
					{
						Mount();
					}
				}
				else
				{
					mountTime = 0f;
				}
			}
			else
			{
				mountTime = 0f;
			}
		}
		else
		{
			mountTime = 0f;
		}
		if (IsRiding && Vector3.Angle(((Component)ActiveSkateboard).transform.up, Vector3.up) > 80f)
		{
			Dismount();
		}
		UpdateModel();
	}

	private void UpdateModel()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = (IsRiding ? ModelPosition_Lowered.localPosition : ModelPosition_Raised.localPosition);
		ModelContainer.localPosition = Vector3.Lerp(ModelContainer.localPosition, val, Time.deltaTime * 8f);
	}

	public override void Unequip()
	{
		base.Unequip();
		GameInput.DeregisterExitListener(Exit);
		Singleton<InputPromptsCanvas>.Instance.UnloadModule();
		if (IsRiding)
		{
			Dismount();
		}
	}

	public void Mount()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		IsRiding = true;
		mountTime = 0f;
		ActiveSkateboard = Object.Instantiate<GameObject>(((Component)SkateboardPrefab).gameObject, (Transform)null).GetComponent<Skateboard>();
		ActiveSkateboard.Equippable = this;
		Pose skateboardSpawnPose = GetSkateboardSpawnPose();
		((Component)ActiveSkateboard).transform.position = skateboardSpawnPose.position;
		((Component)ActiveSkateboard).transform.rotation = skateboardSpawnPose.rotation;
		((NetworkBehaviour)Player.Local).Spawn(((NetworkBehaviour)ActiveSkateboard).NetworkObject, Player.Local.Connection, default(Scene));
		Vector3 velocity = Player.Local.VelocityCalculator.Velocity;
		ActiveSkateboard.SetVelocity(velocity * 1.2f);
		Player.Local.MountSkateboard(ActiveSkateboard);
		Player.Local.Avatar.SetEquippable(string.Empty);
	}

	public void Dismount()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		IsRiding = false;
		mountTime = 0f;
		Vector3 velocity = ActiveSkateboard.Rb.velocity;
		float num = 50f;
		float time = 0.7f * Mathf.Clamp01(((Vector3)(ref velocity)).magnitude / 9f);
		Vector3 val = Vector3.ProjectOnPlane(velocity, Vector3.up);
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		PlayerSingleton<PlayerMovement>.Instance.SetResidualVelocity(normalized, ((Vector3)(ref velocity)).magnitude * num, time);
		Player.Local.DismountSkateboard();
		((NetworkBehaviour)Player.Local).Despawn(((NetworkBehaviour)ActiveSkateboard).NetworkObject, (DespawnType?)null);
		Object.Destroy((Object)(object)((Component)ActiveSkateboard).gameObject);
		Singleton<InputPromptsCanvas>.Instance.LoadModule("heldskateboard");
		ActiveSkateboard = null;
	}

	private bool CanMountHere()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (Vector3.Angle(GetSkateboardSpawnPose().rotation * Vector3.up, Vector3.up) > 30f)
		{
			return false;
		}
		return true;
	}

	private Pose GetSkateboardSpawnPose()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Player.Local.PlayerBasePosition + ((Component)Player.Local).transform.forward * 0.4f + Vector3.up * 0.4f;
		Vector3 val2 = Player.Local.PlayerBasePosition - ((Component)Player.Local).transform.forward * 0.4f + Vector3.up * 0.4f;
		Debug.DrawRay(val, Vector3.down * 0.7f, Color.cyan, 10f);
		Debug.DrawRay(val2, Vector3.down * 0.7f, Color.cyan, 10f);
		RaycastHit val3 = default(RaycastHit);
		if (!Physics.Raycast(val, Vector3.down, ref val3, 0.7f, LayerMask.op_Implicit(SkateboardPrefab.GroundDetectionMask), (QueryTriggerInteraction)1))
		{
			((RaycastHit)(ref val3)).point = val + Vector3.down * 0.7f;
		}
		RaycastHit val4 = default(RaycastHit);
		if (!Physics.Raycast(val2, Vector3.down, ref val4, 0.7f, LayerMask.op_Implicit(SkateboardPrefab.GroundDetectionMask), (QueryTriggerInteraction)1))
		{
			((RaycastHit)(ref val4)).point = val2 + Vector3.down * 0.7f;
		}
		Vector3 position = (((RaycastHit)(ref val3)).point + ((RaycastHit)(ref val4)).point) / 2f + Vector3.up * (0.05f + SkateboardPrefab.DefaultSettings.HoverHeight);
		Vector3 val5 = ((RaycastHit)(ref val3)).point - ((RaycastHit)(ref val4)).point;
		Vector3 normalized = ((Vector3)(ref val5)).normalized;
		val5 = Vector3.Cross(Vector3.up, normalized);
		Vector3 normalized2 = ((Vector3)(ref val5)).normalized;
		val5 = Vector3.Cross(normalized, normalized2);
		Vector3 normalized3 = ((Vector3)(ref val5)).normalized;
		Quaternion rotation = Quaternion.LookRotation(normalized, normalized3);
		return new Pose
		{
			position = position,
			rotation = rotation
		};
	}
}
