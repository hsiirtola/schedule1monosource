using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Police;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour;

public class FootPatrolBehaviour : Behaviour
{
	public const float MOVE_SPEED = 0.08f;

	public const int FLASHLIGHT_MIN_TIME = 1930;

	public int FLASHLIGHT_MAX_TIME = 500;

	public const string FLASHLIGHT_ASSET_PATH = "Tools/Flashlight/Flashlight_AvatarEquippable";

	public bool UseFlashlight = true;

	private bool flashlightEquipped;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EFootPatrolBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EFootPatrolBehaviourAssembly_002DCSharp_002Edll_Excuted;

	public PatrolGroup Group { get; protected set; }

	public override void Activate()
	{
		base.Activate();
		if (InstanceFinder.IsServer && Group == null)
		{
			Console.LogError("Foot patrol behaviour started without a group!");
		}
		base.Npc.Movement.SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("footpatrol", 1, 0.08f));
		(base.Npc as PoliceOfficer).BodySearchChance = 0.4f;
	}

	public override void Resume()
	{
		base.Resume();
		if (InstanceFinder.IsServer && Group == null)
		{
			Console.LogError("Foot patrol behaviour resumed without a group!");
		}
		base.Npc.Movement.SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("footpatrol", 1, 0.08f));
		(base.Npc as PoliceOfficer).BodySearchChance = 0.25f;
	}

	public override void Pause()
	{
		base.Pause();
		base.Npc.Movement.SpeedController.RemoveSpeedControl("footpatrol");
		(base.Npc as PoliceOfficer).BodySearchChance = 0.1f;
		if (flashlightEquipped)
		{
			SetFlashlightEquipped(equipped: false);
		}
	}

	public override void Deactivate()
	{
		base.Deactivate();
		if (Group != null)
		{
			Group.Members.Remove(base.Npc);
		}
		base.Npc.Movement.SpeedController.RemoveSpeedControl("footpatrol");
		if (flashlightEquipped)
		{
			SetFlashlightEquipped(equipped: false);
		}
		(base.Npc as PoliceOfficer).BodySearchChance = 0.1f;
	}

	public override void OnActiveTick()
	{
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		base.OnActiveTick();
		if (!InstanceFinder.IsServer)
		{
			return;
		}
		if (NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(1930, FLASHLIGHT_MAX_TIME))
		{
			if (UseFlashlight && !flashlightEquipped && Group.Members.Count > 0 && (Object)(object)Group.Members[0] == (Object)(object)base.Npc)
			{
				SetFlashlightEquipped(equipped: true);
			}
		}
		else if (flashlightEquipped)
		{
			SetFlashlightEquipped(equipped: false);
		}
		if (Group == null)
		{
			return;
		}
		if (!Group.Members.Contains(base.Npc))
		{
			Console.LogWarning("Foot patrol behaviour is not in group members list! Adding now");
			SetGroup(Group);
		}
		if (Group.IsPaused())
		{
			if (base.Npc.Movement.IsMoving)
			{
				base.Npc.Movement.Stop();
			}
		}
		else
		{
			if (base.Npc.Movement.IsMoving)
			{
				return;
			}
			if (IsReadyToAdvance())
			{
				if (Group.Members.Count > 0 && (Object)(object)Group.Members[0] == (Object)(object)base.Npc && Group.IsGroupReadyToAdvance())
				{
					Group.AdvanceGroup();
				}
			}
			else if (!IsAtDestination())
			{
				base.Npc.Movement.SetDestination(Group.GetDestination(base.Npc));
			}
		}
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

	public void SetGroup(PatrolGroup group)
	{
		Group = group;
		Group.Members.Add(base.Npc);
	}

	public bool IsReadyToAdvance()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		Vector3 destination = Group.GetDestination(base.Npc);
		if (IsAtDestination(4f))
		{
			if (base.beh.DEBUG_MODE)
			{
				Console.Log(((Object)base.Npc).name + " is ready to advance (at destination)");
			}
			return true;
		}
		if (base.Npc.Movement.IsAsCloseAsPossible(destination, 4f))
		{
			if (base.beh.DEBUG_MODE)
			{
				Console.Log(((Object)base.Npc).name + " is ready to advance (as close as possible)");
			}
			return true;
		}
		if (base.Npc.Movement.IsMoving)
		{
			if (base.beh.DEBUG_MODE)
			{
				Console.Log(((Object)base.Npc).name + " is not ready to advance (still walking)");
			}
			return false;
		}
		return false;
	}

	private bool IsAtDestination(float threshold = 2f)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (Group == null)
		{
			return false;
		}
		return Vector3.Distance(base.Npc.Movement.FootPosition, Group.GetDestination(base.Npc)) < threshold;
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EFootPatrolBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EFootPatrolBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EFootPatrolBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EFootPatrolBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
