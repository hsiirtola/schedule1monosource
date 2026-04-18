using System;
using FishNet;
using FishNet.Object;
using ScheduleOne.AvatarFramework.Equipping;
using ScheduleOne.Combat;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Police;
using ScheduleOne.Vision;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour;

public class PursuitBehaviour : CombatBehaviour
{
	private enum EPursuitAction
	{
		None,
		Move,
		Shoot,
		MoveAndShoot
	}

	public const float ARREST_RANGE = 2.75f;

	public const float ARREST_TIME = 1.75f;

	public const float EXTRA_VISIBILITY_TIME = 2f;

	public const float MOVE_SPEED_INVESTIGATING = 0.35f;

	public const float MOVE_SPEED_ARRESTING = 0.7f;

	public const float MOVE_SPEED_CHASE = 0.9f;

	public const float CHASE_SPEED_DISTANCE_THRESHOLD = 6f;

	public const float ARREST_MAX_DISTANCE = 15f;

	public const int LEAVE_ARREST_CIRCLE_LIMIT = 3;

	[Header("Settings")]
	public float ArrestCircle_MaxVisibleDistance = 5f;

	public float ArrestCircle_MaxOpacity = 0.25f;

	[Header("Weapons")]
	public AvatarWeapon Weapon_Baton;

	public AvatarWeapon Weapon_Taser;

	public AvatarWeapon Weapon_Gun;

	protected bool arrestingEnabled = true;

	protected float currentPursuitLevelDuration;

	protected float timeWithinArrestRange;

	protected float distanceOnPursuitStart;

	private PoliceOfficer officer;

	private bool targetWasDrivingOnPursuitStart;

	private bool wasInArrestCircleLastFrame;

	private int leaveArrestCircleCount;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EPursuitBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EPursuitBehaviourAssembly_002DCSharp_002Edll_Excuted;

	public Player TargetPlayer { get; protected set; }

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EPursuitBehaviour_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	private void OnDestroy()
	{
		PoliceOfficer.OnPoliceVisionEvent = (Action<VisionEventReceipt>)Delegate.Remove(PoliceOfficer.OnPoliceVisionEvent, new Action<VisionEventReceipt>(OnThirdPartyVisionEvent));
	}

	protected override void SetTarget(NetworkObject target)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		base.SetTarget(target);
		if (!Object.op_Implicit((Object)(object)((Component)target).GetComponent<Player>()))
		{
			Debug.LogError((object)("PursuitBehaviour assigned target is not a Player! " + ((Object)target).name));
			return;
		}
		TargetPlayer = ((Component)target).GetComponent<Player>();
		timeWithinArrestRange = 0f;
		leaveArrestCircleCount = 0;
		wasInArrestCircleLastFrame = false;
		distanceOnPursuitStart = Vector3.Distance(((Component)this).transform.position, TargetPlayer.Avatar.CenterPoint);
		targetWasDrivingOnPursuitStart = (Object)(object)TargetPlayer.CurrentVehicle != (Object)null;
	}

	public override void Activate()
	{
		base.Activate();
		officer.ProxCircle.SetRadius(2.75f);
	}

	public override void Resume()
	{
		base.Resume();
		officer.ProxCircle.SetRadius(2.75f);
	}

	public override void Disable()
	{
		base.Disable();
		TargetPlayer = null;
	}

	public override void BehaviourUpdate()
	{
		base.BehaviourUpdate();
		if (base.Target == null || (Object)(object)TargetPlayer == (Object)null)
		{
			return;
		}
		if ((Object)(object)TargetPlayer != (Object)null && officer.IgnorePlayers)
		{
			if (InstanceFinder.IsServer)
			{
				Disable_Networked(null);
			}
			return;
		}
		switch (TargetPlayer.CrimeData.CurrentPursuitLevel)
		{
		case PlayerCrimeData.EPursuitLevel.Investigating:
			UpdateInvestigatingBehaviour();
			break;
		case PlayerCrimeData.EPursuitLevel.Arresting:
			UpdateArrestBehaviour();
			break;
		case PlayerCrimeData.EPursuitLevel.NonLethal:
			UpdateNonLethalBehaviour();
			break;
		case PlayerCrimeData.EPursuitLevel.Lethal:
			UpdateLethalBehaviour();
			break;
		}
		currentPursuitLevelDuration += Time.deltaTime;
		UpdateArrest(Time.deltaTime);
		UpdateArrestCircle();
	}

	public override void OnActiveTick()
	{
		base.OnActiveTick();
	}

	protected override bool IsTargetValid()
	{
		if ((Object)(object)TargetPlayer != (Object)null && TargetPlayer.CrimeData.CurrentPursuitLevel == PlayerCrimeData.EPursuitLevel.None)
		{
			if (DEBUG)
			{
				Console.LogWarning($"Target ({TargetPlayer.PlayerName}) is no longer wanted. (Pursuit level = {TargetPlayer.CrimeData.CurrentPursuitLevel})");
			}
			return false;
		}
		return base.IsTargetValid();
	}

	protected virtual void UpdateInvestigatingBehaviour()
	{
		SetMovementSpeed(0.35f);
		if (base.IsTargetImmediatelyVisible && (Object)(object)TargetPlayer != (Object)null && TargetPlayer.CrimeData.CurrentPursuitLevel == PlayerCrimeData.EPursuitLevel.Investigating)
		{
			TargetPlayer.CrimeData.Escalate();
		}
	}

	protected virtual void UpdateArrestBehaviour()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		SetMovementSpeed(0.7f);
		SetWeapon(((Object)(object)Weapon_Baton != (Object)null) ? Weapon_Baton.AssetPath : string.Empty);
		if (Vector3.Distance(((Component)this).transform.position, TargetPlayer.Avatar.CenterPoint) > Mathf.Max(15f, distanceOnPursuitStart + 5f) && timeSinceLastSighting < 1f)
		{
			Debug.Log((object)"Target too far! Escalating");
			TargetPlayer.CrimeData.Escalate();
		}
		if ((Object)(object)TargetPlayer.CurrentVehicle != (Object)null && !targetWasDrivingOnPursuitStart && timeSinceLastSighting < 1f)
		{
			Debug.Log((object)"Target got in vehicle! Escalating");
			TargetPlayer.CrimeData.Escalate();
		}
		if (leaveArrestCircleCount >= 3 && timeSinceLastSighting < 1f)
		{
			Debug.Log((object)"Left arrest circle too many times! Escalating");
			TargetPlayer.CrimeData.Escalate();
		}
	}

	protected virtual void UpdateNonLethalBehaviour()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector3.Distance(((Component)this).transform.position, TargetPlayer.Avatar.CenterPoint);
		SetMovementSpeed(Mathf.Lerp(0.7f, 0.9f, Mathf.Clamp01(num / 6f)));
		SetWeapon(((Object)(object)Weapon_Taser != (Object)null) ? Weapon_Taser.AssetPath : string.Empty);
	}

	protected virtual void UpdateLethalBehaviour()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector3.Distance(((Component)this).transform.position, TargetPlayer.Avatar.CenterPoint);
		SetMovementSpeed(Mathf.Lerp(0.7f, 0.9f, Mathf.Clamp01(num / 6f)));
		SetWeapon(((Object)(object)Weapon_Gun != (Object)null) ? Weapon_Gun.AssetPath : string.Empty);
	}

	protected override void OnCurrentWeaponChanged(AvatarWeapon weapon)
	{
		base.OnCurrentWeaponChanged(weapon);
		officer.belt.SetBatonVisible((Object)(object)weapon == (Object)null || !(weapon.AssetPath == Weapon_Baton.AssetPath));
		officer.belt.SetTaserVisible((Object)(object)weapon == (Object)null || !(weapon.AssetPath == Weapon_Taser.AssetPath));
		officer.belt.SetGunVisible((Object)(object)weapon == (Object)null || !(weapon.AssetPath == Weapon_Gun.AssetPath));
	}

	protected override float GetIdealRangedWeaponDistance()
	{
		return 0.5f;
	}

	private void UpdateArrest(float tick)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)TargetPlayer == (Object)null)
		{
			return;
		}
		if (Vector3.Distance(((Component)this).transform.position, TargetPlayer.Avatar.CenterPoint) < 2.75f && arrestingEnabled && base.IsTargetRecentlyVisible)
		{
			timeWithinArrestRange += tick;
			if (timeWithinArrestRange > 0.5f)
			{
				wasInArrestCircleLastFrame = true;
			}
		}
		else
		{
			if (wasInArrestCircleLastFrame)
			{
				leaveArrestCircleCount++;
				wasInArrestCircleLastFrame = false;
			}
			timeWithinArrestRange = Mathf.Clamp(timeWithinArrestRange - tick, 0f, float.MaxValue);
		}
		if (((NetworkBehaviour)TargetPlayer).IsOwner && timeWithinArrestRange / 1.75f > TargetPlayer.CrimeData.CurrentArrestProgress)
		{
			TargetPlayer.CrimeData.SetArrestProgress(timeWithinArrestRange / 1.75f);
		}
	}

	private void ClearSpeedControls()
	{
		if (base.Npc.Movement.SpeedController.DoesSpeedControlExist("investigating"))
		{
			base.Npc.Movement.SpeedController.RemoveSpeedControl("investigating");
		}
		if (base.Npc.Movement.SpeedController.DoesSpeedControlExist("arresting"))
		{
			base.Npc.Movement.SpeedController.RemoveSpeedControl("arresting");
		}
		if (base.Npc.Movement.SpeedController.DoesSpeedControlExist("chasing"))
		{
			base.Npc.Movement.SpeedController.RemoveSpeedControl("chasing");
		}
		if (base.Npc.Movement.SpeedController.DoesSpeedControlExist("shooting"))
		{
			base.Npc.Movement.SpeedController.RemoveSpeedControl("shooting");
		}
	}

	protected override void EndCombat()
	{
		base.EndCombat();
		ClearSpeedControls();
		SetArrestCircleAlpha(0f);
		officer.Avatar.EmotionManager.RemoveEmotionOverride("pursuit");
		timeSinceLastSighting = 10000f;
		currentPursuitLevelDuration = 0f;
		timeWithinArrestRange = 0f;
		if ((Object)(object)TargetPlayer != (Object)null)
		{
			base.Npc.Awareness.VisionCone.SetSightableStateEnabled(TargetPlayer, EVisualState.Visible, enabled: false);
		}
	}

	protected virtual void UpdateArrestCircle()
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)TargetPlayer == (Object)null || !arrestingEnabled || (Object)(object)TargetPlayer != (Object)(object)Player.Local)
		{
			SetArrestCircleAlpha(0f);
			return;
		}
		if ((Object)(object)TargetPlayer.CrimeData.NearestOfficer != (Object)(object)base.Npc)
		{
			SetArrestCircleAlpha(0f);
			return;
		}
		if (!base.IsTargetRecentlyVisible)
		{
			SetArrestCircleAlpha(0f);
			return;
		}
		float num = Vector3.Distance(TargetPlayer.Avatar.CenterPoint, ((Component)this).transform.position);
		if (num < 2.75f)
		{
			SetArrestCircleAlpha(ArrestCircle_MaxOpacity);
			SetArrestCircleColor(Color32.op_Implicit(new Color32(byte.MaxValue, (byte)50, (byte)50, byte.MaxValue)));
		}
		else if (num < ArrestCircle_MaxVisibleDistance)
		{
			float arrestCircleAlpha = Mathf.Lerp(ArrestCircle_MaxOpacity, 0f, (num - 2.75f) / (ArrestCircle_MaxVisibleDistance - 2.75f));
			SetArrestCircleAlpha(arrestCircleAlpha);
			SetArrestCircleColor(Color.white);
		}
		else
		{
			SetArrestCircleAlpha(0f);
		}
	}

	public void ResetArrestProgress()
	{
		timeWithinArrestRange = 0f;
	}

	private void SetArrestCircleAlpha(float alpha)
	{
		officer.ProxCircle.SetAlpha(alpha);
	}

	private void SetArrestCircleColor(Color col)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		officer.ProxCircle.SetColor(col);
	}

	private void OnThirdPartyVisionEvent(VisionEventReceipt receipt)
	{
		ProcessVisionEvent(receipt);
	}

	protected override void TargetSpotted()
	{
		base.TargetSpotted();
		if (TargetPlayer.CrimeData.CurrentPursuitLevel == PlayerCrimeData.EPursuitLevel.Investigating)
		{
			TargetPlayer.CrimeData.Escalate();
		}
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EPursuitBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EPursuitBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EPursuitBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EPursuitBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	protected override void Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EPursuitBehaviour_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		officer = base.Npc as PoliceOfficer;
		PoliceOfficer.OnPoliceVisionEvent = (Action<VisionEventReceipt>)Delegate.Combine(PoliceOfficer.OnPoliceVisionEvent, new Action<VisionEventReceipt>(OnThirdPartyVisionEvent));
	}
}
