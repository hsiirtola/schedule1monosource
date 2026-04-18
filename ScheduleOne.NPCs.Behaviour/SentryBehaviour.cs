using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Law;
using ScheduleOne.Police;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour;

public class SentryBehaviour : Behaviour
{
	private const float BodySearchChance = 0.75f;

	private const int FlashlightMinTime = 1930;

	private int FlashlightMaxTime = 500;

	private const string FlashlightAssetPath = "Tools/Flashlight/Flashlight_AvatarEquippable";

	private const float AngularSpeedMultiplier = 0.2f;

	private const float WalkSpeed = 0.035f;

	public bool UseFlashlight = true;

	private bool flashlightEquipped;

	private PoliceOfficer officer;

	private int _currentRoutePointIndex;

	private int _minutesAtCurrentPoint;

	private bool _movementModifiersApplied;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ESentryBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ESentryBehaviourAssembly_002DCSharp_002Edll_Excuted;

	public SentryLocation AssignedLocation { get; private set; }

	private SentryLocation.SentryRoute _currentRoute
	{
		get
		{
			if (!((Object)(object)AssignedLocation != (Object)null))
			{
				return null;
			}
			return AssignedLocation.Routes[AssignedLocation.AssignedOfficers.IndexOf(officer)];
		}
	}

	private Transform _standPoint
	{
		get
		{
			if (_currentRoute == null)
			{
				return null;
			}
			return _currentRoute.RoutePoints[_currentRoutePointIndex];
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002ESentryBehaviour_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public override void Deactivate()
	{
		base.Deactivate();
		if (flashlightEquipped)
		{
			SetFlashlightEquipped(equipped: false);
		}
		RemoveMovementModifiers();
		officer.SetRandomAvoidancePriority();
	}

	public override void Pause()
	{
		base.Pause();
		RemoveMovementModifiers();
		if (flashlightEquipped)
		{
			SetFlashlightEquipped(equipped: false);
		}
	}

	public override void Disable()
	{
		base.Disable();
		Deactivate();
	}

	public void AssignLocation(SentryLocation loc)
	{
		if ((Object)(object)AssignedLocation != (Object)null)
		{
			UnassignLocation();
		}
		AssignedLocation = loc;
		AssignedLocation.AssignedOfficers.Add(officer);
		_currentRoutePointIndex = 0;
	}

	public void UnassignLocation()
	{
		if ((Object)(object)AssignedLocation != (Object)null)
		{
			AssignedLocation.AssignedOfficers.Remove(officer);
			AssignedLocation = null;
		}
	}

	public override void OnActiveTick()
	{
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		base.OnActiveTick();
		if (!InstanceFinder.IsServer)
		{
			return;
		}
		officer.BodySearchChance = 0.1f;
		if (base.Npc.Movement.IsMoving)
		{
			return;
		}
		if (IsAtStandPoint())
		{
			if (!_movementModifiersApplied)
			{
				ApplyMovementModifiers();
			}
			officer.BodySearchChance = 0.75f;
			officer.SetAvoidancePriority(100);
			if (!base.Npc.Movement.FaceDirectionInProgress)
			{
				base.Npc.Movement.FaceDirection(_standPoint.forward, 1f);
			}
		}
		else if (base.Npc.Movement.CanMove())
		{
			base.Npc.Movement.SetDestination(_standPoint.position);
		}
	}

	public override void OnActiveUncappedMinutePass()
	{
		base.OnActiveUncappedMinutePass();
		if (!InstanceFinder.IsServer)
		{
			return;
		}
		if (NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(1930, FlashlightMaxTime))
		{
			if (UseFlashlight && !flashlightEquipped)
			{
				SetFlashlightEquipped(equipped: true);
			}
		}
		else if (flashlightEquipped)
		{
			SetFlashlightEquipped(equipped: false);
		}
		if (!base.Npc.Movement.IsMoving && IsAtStandPoint())
		{
			_minutesAtCurrentPoint++;
			if (_minutesAtCurrentPoint >= _currentRoute.MinutesPerPoint)
			{
				_minutesAtCurrentPoint = 0;
				_currentRoutePointIndex = (_currentRoutePointIndex + 1) % _currentRoute.RoutePoints.Length;
			}
		}
	}

	private bool IsAtStandPoint()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Distance(((Component)base.Npc).transform.position, _standPoint.position) < 2f;
	}

	private void SetFlashlightEquipped(bool equipped)
	{
		flashlightEquipped = equipped;
		if (equipped)
		{
			base.Npc.SetEquippable_Client(null, "Tools/Flashlight/Flashlight_AvatarEquippable");
		}
		else
		{
			base.Npc.SetEquippable_Client(null, string.Empty);
		}
	}

	private void ApplyMovementModifiers()
	{
		_movementModifiersApplied = true;
		officer.Movement.SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("sentry", 1, 0.035f));
		officer.Movement.SetAngularSpeedMultiplier(0.2f);
	}

	private void RemoveMovementModifiers()
	{
		_movementModifiersApplied = false;
		officer.Movement.SpeedController.RemoveSpeedControl("sentry");
		officer.Movement.SetAngularSpeedMultiplier(1f);
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ESentryBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ESentryBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ESentryBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ESentryBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	protected override void Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002ESentryBehaviour_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		officer = base.Npc as PoliceOfficer;
	}
}
