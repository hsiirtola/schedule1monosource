using System;
using System.Collections;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.AvatarFramework.Equipping;
using ScheduleOne.DevUtilities;
using ScheduleOne.NPCs;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.Tools;
using ScheduleOne.Vision;
using ScheduleOne.VoiceOver;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace ScheduleOne.Combat;

public class CombatBehaviour : Behaviour
{
	public const float RECENT_VISIBILITY_THRESHOLD = 3.5f;

	public const float REPOSITION_TIME = 4f;

	public const float SEARCH_RADIUS_MIN = 25f;

	public const float SEARCH_RADIUS_MAX = 60f;

	public const float SEARCH_SPEED = 0.4f;

	public const float CONSECUTIVE_MISS_ACCURACY_BOOST = 0.1f;

	public const float REACHED_DESTINATION_DISTANCE = 2f;

	public const float DelayBeforeFirstAttack = 0.25f;

	public bool DEBUG;

	[Header("General Setttings")]
	public float GiveUpRange = 20f;

	public int GiveUpAfterSuccessfulHits;

	public bool PlayAngryVO = true;

	[Header("Movement settings")]
	[Range(0f, 1f)]
	public float DefaultMovementSpeed = 0.6f;

	[Header("Weapon settings")]
	public AvatarWeapon DefaultWeapon;

	public AvatarMeleeWeapon VirtualPunchWeapon;

	[Header("Search settings")]
	public float DefaultSearchTime = 30f;

	[Header("References")]
	public SmoothedVelocityCalculator TargetVelocityTracker;

	[Header("Debug settings")]
	public bool CombatOnStart;

	public NetworkObject DebugTarget;

	protected float timeSinceLastSighting = 10000f;

	protected Vector3 lastKnownTargetPosition = Vector3.zero;

	private float timeSinceLastReposition;

	private float timeWithinAttackRange;

	private bool visionEventReceived;

	private float _timeOnCombatStart;

	protected AvatarWeapon currentWeapon;

	protected int successfulHits;

	protected int consecutiveMissedShots;

	protected Coroutine rangedWeaponRoutine;

	protected Coroutine searchRoutine;

	protected Vector3 currentSearchDestination = Vector3.zero;

	protected bool hasSearchDestination;

	private float nextAngryVO;

	public Action onSuccessfulHit;

	private bool NetworkInitialize___EarlyScheduleOne_002ECombat_002ECombatBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ECombat_002ECombatBehaviourAssembly_002DCSharp_002Edll_Excuted;

	public ICombatTargetable Target { get; protected set; }

	public bool IsSearching { get; protected set; }

	public float TimeSinceTargetReacquired { get; protected set; }

	public bool IsTargetRecentlyVisible { get; private set; }

	public bool IsTargetImmediatelyVisible { get; private set; }

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ECombat_002ECombatBehaviour_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	private void Start()
	{
		VirtualPunchWeapon.Equip(base.Npc.Avatar);
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		((NetworkBehaviour)this).OnSpawnServer(connection);
		if (base.Active && Target != null)
		{
			SetTarget_Client(connection, Target.NetworkObject);
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SetTargetAndEnable_Server(NetworkObject target)
	{
		RpcWriter___Server_SetTargetAndEnable_Server_3323014238(target);
		RpcLogic___SetTargetAndEnable_Server_3323014238(target);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	protected void SetTarget_Client(NetworkConnection conn, NetworkObject target)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetTarget_Client_1824087381(conn, target);
			RpcLogic___SetTarget_Client_1824087381(conn, target);
		}
		else
		{
			RpcWriter___Target_SetTarget_Client_1824087381(conn, target);
		}
	}

	protected virtual void SetTarget(NetworkObject target)
	{
		if (!((Object)(object)target == (Object)null))
		{
			Target = ((Component)target).GetComponent<ICombatTargetable>();
			timeSinceLastSighting = 0f;
			visionEventReceived = true;
			TimeSinceTargetReacquired = 0f;
			TargetVelocityTracker.SetTarget(Target.gameObject.transform);
			TargetVelocityTracker.SampleLength = 2f;
		}
	}

	public override void Activate()
	{
		base.Activate();
		base.Npc.Avatar.EmotionManager.AddEmotionOverride("Annoyed", "post combat", 120f, 1);
		StartCombat();
	}

	public override void Resume()
	{
		base.Resume();
		StartCombat();
	}

	public override void Pause()
	{
		base.Pause();
		EndCombat();
	}

	public override void Deactivate()
	{
		base.Deactivate();
		EndCombat();
	}

	public override void Disable()
	{
		base.Disable();
		Target = null;
	}

	protected virtual void StartCombat()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (Target == null)
		{
			Console.LogWarning("StartCombat called with null target. Cannot start combat without a valid target.");
			return;
		}
		base.Npc.Awareness.VisionCone.AddSightableOfInterest(Target);
		CheckTargetVisibility();
		lastKnownTargetPosition = Target.CenterPoint;
		timeSinceLastReposition = 100f;
		SetMovementSpeed(DefaultMovementSpeed);
		base.Npc.Movement.SetStance(NPCMovement.EStance.Stanced);
		base.Npc.Movement.SetAgentType(NPCMovement.EAgentType.IgnoreCosts);
		base.Npc.Avatar.LookController.BodyRotationSpeedMultiplier = 1.7f;
		base.Npc.Movement.Agent.avoidancePriority = Random.Range(0, 10);
		base.Npc.Avatar.EmotionManager.AddEmotionOverride("Angry", "combat", 0f, 3);
		if (InstanceFinder.IsServer && (Object)(object)DefaultWeapon != (Object)null)
		{
			SetWeapon(DefaultWeapon.AssetPath);
		}
		nextAngryVO = Time.time + Random.Range(5f, 15f);
		_timeOnCombatStart = Time.time;
		successfulHits = 0;
	}

	protected virtual void EndCombat()
	{
		base.Npc.Awareness.VisionCone.RemoveSightableOfInterest(Target);
		StopSearching();
		if (InstanceFinder.IsServer && (Object)(object)currentWeapon != (Object)null)
		{
			ClearWeapon();
		}
		base.Npc.Movement.SpeedController.RemoveSpeedControl("combat");
		base.Npc.Movement.SetAgentType(NPCMovement.EAgentType.Humanoid);
		base.Npc.Movement.SetStance(NPCMovement.EStance.None);
		base.Npc.Movement.ObstacleAvoidanceEnabled = true;
		base.Npc.Avatar.LookController.BodyRotationSpeedMultiplier = 1f;
		base.Npc.Avatar.EmotionManager.RemoveEmotionOverride("combat");
		if (Target != null)
		{
			base.Npc.Awareness.VisionCone.SetSightableStateEnabled(Target, EVisualState.Visible, enabled: false);
		}
		timeSinceLastSighting = 10000f;
	}

	public override void BehaviourUpdate()
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0318: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		base.BehaviourUpdate();
		CheckTargetVisibility();
		UpdateTimeout();
		UpdateLookAt();
		if (InstanceFinder.IsServer && !IsTargetValid())
		{
			Disable_Networked(null);
			return;
		}
		if (Time.time > nextAngryVO && PlayAngryVO)
		{
			base.Npc.PlayVO(EVOLineType.Angry);
			nextAngryVO = Time.time + Random.Range(5f, 15f);
		}
		if (IsTargetRecentlyVisible)
		{
			lastKnownTargetPosition = Target.CenterPoint;
		}
		if (!InstanceFinder.IsServer)
		{
			return;
		}
		timeSinceLastReposition += Time.deltaTime;
		TimeSinceTargetReacquired += Time.deltaTime;
		if (DEBUG)
		{
			Console.Log("Target immediately visible: " + IsTargetImmediatelyVisible + ", recently visible: " + IsTargetRecentlyVisible);
			Console.Log("Is searching: " + IsSearching);
			Console.Log("Vision event received: " + visionEventReceived);
		}
		if (IsSearching)
		{
			if (!IsTargetImmediatelyVisible)
			{
				return;
			}
			StopSearching();
		}
		bool flag = false;
		if (IsTargetRecentlyVisible)
		{
			if (DEBUG)
			{
				Console.Log("Target recently visible");
			}
			if (IsCurrentWeaponMelee())
			{
				if (IsTargetInRange(((Component)base.Npc).transform.position + Vector3.up * 1f))
				{
					if (IsTargetImmediatelyVisible)
					{
						flag = true;
						if (DEBUG)
						{
							Console.Log("In melee range. Ready to attack: " + ReadyToAttack(checkTarget: false));
						}
						if (ReadyToAttack(checkTarget: false))
						{
							Attack();
						}
						if (timeWithinAttackRange > 4f && timeSinceLastReposition > 4f)
						{
							RepositionToTargetMeleeRange(GetPredictedFutureTargetPosition());
						}
					}
				}
				else if (!base.Npc.Movement.IsMoving || !IsTargetInRange(base.Npc.Movement.CurrentDestination + Vector3.up * 1f))
				{
					RepositionToTargetMeleeRange(GetPredictedFutureTargetPosition());
				}
			}
			else
			{
				EnsureRangedWeaponRoutineIsRunning();
			}
		}
		else
		{
			if (DEBUG)
			{
				Console.Log("Target NOT recently visible");
			}
			if (base.Npc.Movement.IsMoving)
			{
				if (Vector3.Distance(base.Npc.Movement.CurrentDestination, lastKnownTargetPosition) > 2f)
				{
					SetDestination(lastKnownTargetPosition, teleportIfFail: false);
				}
			}
			else if (Vector3.Distance(((Component)this).transform.position, lastKnownTargetPosition) < 2f)
			{
				StartSearching();
			}
			else if (base.Npc.Movement.CanGetTo(lastKnownTargetPosition, 2f))
			{
				SetDestination(lastKnownTargetPosition, teleportIfFail: false);
			}
		}
		if (flag)
		{
			timeWithinAttackRange += Time.deltaTime;
		}
		else
		{
			timeWithinAttackRange = 0f;
		}
	}

	protected void UpdateTimeout()
	{
		if (InstanceFinder.IsServer && timeSinceLastSighting > GetSearchTime())
		{
			Disable_Networked(null);
		}
	}

	protected virtual void UpdateLookAt()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (IsTargetRecentlyVisible && Target != null)
		{
			base.Npc.Avatar.LookController.OverrideLookTarget(Target.LookAtPoint, 10, rotateBody: true);
		}
	}

	protected void SetMovementSpeed(float speed, string label = "combat", int priority = 5)
	{
		base.Npc.Movement.SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl(label, priority, speed));
	}

	private void EnsureRangedWeaponRoutineIsRunning()
	{
		if (rangedWeaponRoutine == null)
		{
			rangedWeaponRoutine = ((MonoBehaviour)this).StartCoroutine(RangedWeaponRoutine());
		}
	}

	protected Vector3 GetPredictedFutureTargetPosition(float lead_Min = 0f, float lead_Max = 2f)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		return Target.CenterPoint + TargetVelocityTracker.Velocity * Random.Range(lead_Min, lead_Max);
	}

	protected unsafe override void SetDestination(Vector3 position, bool teleportIfFail = true, float successThreshold = 1f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		base.SetDestination(position, teleportIfFail, successThreshold);
		if (DEBUG)
		{
			Vector3 val = position;
			Console.Log("Combat set destination to " + ((object)(*(Vector3*)(&val))/*cast due to .constrained prefix*/).ToString());
		}
	}

	[ObserversRpc(RunLocally = true)]
	protected virtual void SetWeapon(string weaponPath)
	{
		RpcWriter___Observers_SetWeapon_3615296227(weaponPath);
		RpcLogic___SetWeapon_3615296227(weaponPath);
	}

	protected virtual void OnCurrentWeaponChanged(AvatarWeapon weapon)
	{
	}

	[ObserversRpc(RunLocally = true)]
	protected void ClearWeapon()
	{
		RpcWriter___Observers_ClearWeapon_2166136261();
		RpcLogic___ClearWeapon_2166136261();
	}

	protected virtual bool ReadyToAttack(bool checkTarget = true)
	{
		if (TimeSinceTargetReacquired < 0.5f && checkTarget)
		{
			Console.Log("Not ready to attack: target just reacquired");
			return false;
		}
		if ((Object)(object)currentWeapon != (Object)null)
		{
			return currentWeapon.IsReadyToAttack();
		}
		if (base.Npc.Movement.IsOnLadder)
		{
			return false;
		}
		if (Time.time - _timeOnCombatStart < 0.25f)
		{
			return false;
		}
		return VirtualPunchWeapon.IsReadyToAttack();
	}

	private bool IsCurrentWeaponMelee()
	{
		if (!((Object)(object)currentWeapon == (Object)null))
		{
			return currentWeapon is AvatarMeleeWeapon;
		}
		return true;
	}

	[ObserversRpc(RunLocally = true)]
	protected virtual void Attack()
	{
		RpcWriter___Observers_Attack_2166136261();
		RpcLogic___Attack_2166136261();
	}

	protected void SucessfulHit()
	{
		successfulHits++;
		if (onSuccessfulHit != null)
		{
			onSuccessfulHit();
		}
		if (GiveUpAfterSuccessfulHits > 0 && successfulHits >= GiveUpAfterSuccessfulHits)
		{
			Disable_Networked(null);
		}
	}

	private IEnumerator RangedWeaponRoutine()
	{
		bool forceReposition = false;
		AvatarRangedWeapon rangedWeapon = currentWeapon as AvatarRangedWeapon;
		ERangedWeaponAction action;
		int shots;
		while (true)
		{
			if ((forceReposition || !(base.Npc.Movement.IsMoving ? IsTargetInRange(base.Npc.Movement.CurrentDestination) : IsTargetInRange(((Component)base.Npc).transform.position))) && IsTargetRecentlyVisible && !((Object)(object)rangedWeapon == (Object)null) && !((Object)(object)currentWeapon == (Object)null) && IsTargetValid())
			{
				forceReposition = false;
				SetWeaponRaised(raised: false);
				yield return ((MonoBehaviour)this).StartCoroutine(RepositionToRangedWeaponRange());
				continue;
			}
			if (!IsTargetRecentlyVisible || (Object)(object)rangedWeapon == (Object)null || (Object)(object)currentWeapon == (Object)null || !IsTargetValid())
			{
				break;
			}
			action = ERangedWeaponAction.Shoot;
			if (rangedWeapon.CanShootWhileMoving)
			{
				float idealUseRange = rangedWeapon.GetIdealUseRange();
				if (Mathf.Clamp01(Mathf.Abs(Vector3.Distance(base.Npc.CenterPoint, Target.CenterPoint) - idealUseRange) / idealUseRange) > Random.Range(0f, 1f))
				{
					action = ERangedWeaponAction.RepositionAndShoot;
				}
				else
				{
					action = ERangedWeaponAction.Shoot;
				}
			}
			if (Target.IsPlayer && Target.AsPlayer.Health.TimeSinceLastDamage < 1f)
			{
				action = ERangedWeaponAction.Reposition;
			}
			if (action == ERangedWeaponAction.RepositionAndShoot)
			{
				((MonoBehaviour)this).StartCoroutine(RepositionToRangedWeaponRange());
				base.Npc.Movement.SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("combat_reposition", 10, 0.3f));
			}
			if (!rangedWeapon.IsRaised)
			{
				SetWeaponRaised(raised: true);
				yield return (object)new WaitForSeconds(Random.Range(rangedWeapon.AimTime_Min, rangedWeapon.AimTime_Max));
			}
			shots = 0;
			while (CanShoot())
			{
				yield return (object)new WaitForSeconds(0.1f);
				yield return (object)new WaitUntil((Func<bool>)(() => ReadyToAttack(checkTarget: false)));
				if (!CanShoot())
				{
					break;
				}
				bool num = Shoot();
				shots++;
				if (num && rangedWeapon.RepositionAfterHit)
				{
					forceReposition = true;
					break;
				}
			}
			base.Npc.Movement.SpeedController.RemoveSpeedControl("combat_reposition");
			yield return (object)new WaitForEndOfFrame();
		}
		rangedWeaponRoutine = null;
		bool CanShoot()
		{
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			if (!IsTargetValid())
			{
				return false;
			}
			if ((Object)(object)rangedWeapon == (Object)null)
			{
				return false;
			}
			if (!IsTargetInRange(base.Npc.CenterPoint))
			{
				return false;
			}
			if (action == ERangedWeaponAction.Shoot && shots >= rangedWeapon.MaxStationaryShotsBeforeReposition)
			{
				return false;
			}
			if (action == ERangedWeaponAction.RepositionAndShoot && shots >= rangedWeapon.MaxMovingShotsBeforeReposition)
			{
				return false;
			}
			if (base.Npc.Movement.IsOnLadder)
			{
				return false;
			}
			if (!IsTargetImmediatelyVisible)
			{
				return false;
			}
			return true;
		}
	}

	private IEnumerator RepositionToRangedWeaponRange()
	{
		Vector3 val = base.Npc.CenterPoint - Target.CenterPoint;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		float num = Vector3.Distance(base.Npc.CenterPoint, Target.CenterPoint);
		float idealRangedWeaponDistance = GetIdealRangedWeaponDistance();
		float num2 = Mathf.Lerp(num, idealRangedWeaponDistance, Random.Range(0.5f, 1f));
		Vector3 sourcePosition = GetPredictedFutureTargetPosition() + normalized * num2;
		float num3 = 1f;
		NavMeshHit hit;
		while (!NavMeshUtility.SamplePosition(sourcePosition, out hit, num3, -1))
		{
			num3 += 5f;
			if (num3 >= 100f)
			{
				Console.LogError("Failed to find reposition location");
				yield break;
			}
		}
		SetDestination(((NavMeshHit)(ref hit)).position, teleportIfFail: false);
		yield return (object)new WaitForEndOfFrame();
		while (base.Npc.Movement.IsMoving && IsTargetInRange(base.Npc.Movement.CurrentDestination))
		{
			yield return (object)new WaitForSeconds(0.5f);
		}
	}

	protected virtual float GetIdealRangedWeaponDistance()
	{
		return Mathf.Lerp(currentWeapon.MinUseRange, currentWeapon.MaxUseRange, 0.3f);
	}

	private bool Shoot()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)currentWeapon == (Object)null)
		{
			return false;
		}
		if (Target == null)
		{
			return false;
		}
		AvatarRangedWeapon avatarRangedWeapon = currentWeapon as AvatarRangedWeapon;
		if ((Object)(object)avatarRangedWeapon == (Object)null)
		{
			return false;
		}
		bool flag = false;
		float num = Mathf.Lerp(avatarRangedWeapon.HitChance_MinRange, avatarRangedWeapon.HitChance_MaxRange, Mathf.Clamp01(Vector3.Distance(((Component)this).transform.position, Target.CenterPoint) / avatarRangedWeapon.MaxUseRange));
		num *= 1f + 0.1f * (float)consecutiveMissedShots;
		if (Target.IsPlayer)
		{
			num *= Mathf.Lerp(0f, 1f, Mathf.Clamp01(Target.AsPlayer.Health.TimeSinceLastDamage / 4f));
		}
		float num2 = num;
		Vector3 val = Target.Velocity;
		num = num2 * Mathf.Lerp(1f, 0.3f, Mathf.Clamp01(((Vector3)(ref val)).magnitude / 10f));
		if (Random.Range(0f, 1f) < num)
		{
			flag = true;
		}
		Vector3 val2 = Target.CenterPoint;
		IDamageable damageable = null;
		RaycastHit val3 = default(RaycastHit);
		bool flag2 = false;
		bool flag3 = false;
		if (flag && avatarRangedWeapon.IsTargetInLoS(Target))
		{
			flag3 = true;
			damageable = Target;
		}
		else
		{
			float num3 = Mathf.Lerp(20f, 5f, num);
			val = val2 - avatarRangedWeapon.MuzzlePoint.position;
			Vector3 normalized = ((Vector3)(ref val)).normalized;
			Vector3 val4 = Quaternion.Euler(Random.Range(0f - num3, num3), Random.Range(0f - num3, num3), 0f) * normalized;
			RaycastHit[] array = Physics.RaycastAll(avatarRangedWeapon.MuzzlePoint.position, val4, avatarRangedWeapon.MaxUseRange, LayerMask.op_Implicit(NetworkSingleton<CombatManager>.Instance.RangedWeaponLayerMask));
			bool flag4 = false;
			RaycastHit[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				RaycastHit val5 = array2[i];
				IDamageable componentInParent = ((Component)((RaycastHit)(ref val5)).collider).GetComponentInParent<IDamageable>();
				if (componentInParent == null || !((Object)(object)componentInParent.gameObject == (Object)(object)((Component)base.Npc).gameObject))
				{
					flag4 = true;
					val2 = ((RaycastHit)(ref val5)).point;
					flag2 = true;
					damageable = ((Component)((RaycastHit)(ref val5)).collider).GetComponentInParent<IDamageable>();
					break;
				}
			}
			if (!flag4)
			{
				val2 = avatarRangedWeapon.MuzzlePoint.position + val4 * avatarRangedWeapon.MaxUseRange;
			}
		}
		if (flag3)
		{
			consecutiveMissedShots = 0;
		}
		else
		{
			consecutiveMissedShots++;
		}
		if (damageable != null)
		{
			avatarRangedWeapon.ApplyHitToDamageable(damageable, flag2 ? ((RaycastHit)(ref val3)).point : Target.CenterPoint);
			flag3 = damageable == Target;
		}
		base.Npc.SendEquippableMessage_Networked_Vector(null, "Shoot", val2);
		return flag3;
	}

	private void SetWeaponRaised(bool raised)
	{
		if (!((Object)(object)currentWeapon == (Object)null) && currentWeapon is AvatarRangedWeapon && (currentWeapon as AvatarRangedWeapon).IsRaised != raised)
		{
			if (raised)
			{
				base.Npc.SendEquippableMessage_Networked(null, "Raise");
			}
			else
			{
				base.Npc.SendEquippableMessage_Networked(null, "Lower");
			}
		}
	}

	protected void CheckTargetVisibility()
	{
		if (Target == null)
		{
			return;
		}
		base.Npc.Awareness.VisionCone.SetSightableStateEnabled(Target, EVisualState.Visible, !IsTargetRecentlyVisible);
		if (IsTargetVisibleThisFrame() && visionEventReceived)
		{
			IsTargetImmediatelyVisible = true;
			IsTargetRecentlyVisible = true;
		}
		else
		{
			timeSinceLastSighting += Time.fixedDeltaTime;
			IsTargetImmediatelyVisible = false;
			if (timeSinceLastSighting < 3.5f || (base.Npc.Movement.IsOnLadder && timeSinceLastSighting < 7f))
			{
				Target.RecordLastKnownPosition(resetTimeSinceLastSeen: false);
				IsTargetRecentlyVisible = true;
			}
			else
			{
				visionEventReceived = false;
				IsTargetRecentlyVisible = false;
			}
		}
		if (IsTargetRecentlyVisible)
		{
			MarkPlayerVisible();
		}
	}

	public void MarkPlayerVisible()
	{
		if (IsTargetVisibleThisFrame())
		{
			Target.RecordLastKnownPosition(resetTimeSinceLastSeen: true);
			timeSinceLastSighting = 0f;
		}
		else
		{
			Target.RecordLastKnownPosition(resetTimeSinceLastSeen: false);
		}
	}

	protected bool IsTargetVisibleThisFrame()
	{
		return base.Npc.Awareness.VisionCone.IsTargetVisible(Target);
	}

	protected void ProcessVisionEvent(VisionEventReceipt visionEventReceipt)
	{
		if (base.Active && (Object)(object)visionEventReceipt.Target == (Object)(object)Target.NetworkObject)
		{
			TargetSpotted();
		}
	}

	protected virtual void TargetSpotted()
	{
		if (!IsTargetRecentlyVisible)
		{
			TimeSinceTargetReacquired = 0f;
		}
		visionEventReceived = true;
		IsTargetRecentlyVisible = true;
		IsTargetImmediatelyVisible = true;
		timeSinceLastSighting = 0f;
		NotifyServerTargetSeen();
		if (PlayAngryVO)
		{
			base.Npc.PlayVO(EVOLineType.Angry);
			nextAngryVO = Time.time + Random.Range(5f, 15f);
		}
	}

	[ServerRpc(RequireOwnership = false)]
	public void NotifyServerTargetSeen()
	{
		RpcWriter___Server_NotifyServerTargetSeen_2166136261();
	}

	protected virtual float GetSearchTime()
	{
		return DefaultSearchTime;
	}

	private void StartSearching()
	{
		if (InstanceFinder.IsServer)
		{
			IsSearching = true;
			base.Npc.Movement.SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("searching", 6, 0.4f));
			searchRoutine = ((MonoBehaviour)this).StartCoroutine(SearchRoutine());
		}
	}

	private void StopSearching()
	{
		if (InstanceFinder.IsServer)
		{
			IsSearching = false;
			base.Npc.Movement.SpeedController.RemoveSpeedControl("searching");
			hasSearchDestination = false;
			if (searchRoutine != null)
			{
				((MonoBehaviour)this).StopCoroutine(searchRoutine);
			}
		}
	}

	private IEnumerator SearchRoutine()
	{
		while (IsSearching)
		{
			if (!hasSearchDestination)
			{
				currentSearchDestination = GetNextSearchLocation();
				base.Npc.Movement.SetDestination(currentSearchDestination);
				hasSearchDestination = true;
			}
			while (true)
			{
				if (!base.Npc.Movement.IsMoving && base.Npc.Movement.CanMove())
				{
					base.Npc.Movement.SetDestination(currentSearchDestination);
				}
				if (Vector3.Distance(((Component)this).transform.position, currentSearchDestination) < 2f)
				{
					break;
				}
				yield return (object)new WaitForSeconds(1f);
			}
			hasSearchDestination = false;
			yield return (object)new WaitForSeconds(Random.Range(1f, 6f));
		}
		searchRoutine = null;
		StopSearching();
	}

	private Vector3 GetNextSearchLocation()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Lerp(25f, 60f, Mathf.Clamp(timeSinceLastSighting / Target.GetSearchTime(), 0f, 1f));
		num = Mathf.Min(num, Vector3.Distance(((Component)this).transform.position, Target.CenterPoint));
		GetRandomReachablePointNear(Target.CenterPoint, num, out var randomPoint);
		return randomPoint;
	}

	protected virtual bool IsTargetValid()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		if (Target == null || Target.IsNull())
		{
			if (DEBUG)
			{
				Console.LogWarning("Target is null");
			}
			return false;
		}
		if (!Target.IsCurrentlyTargetable)
		{
			if (DEBUG)
			{
				Console.LogWarning("Target is not targetable");
			}
			return false;
		}
		if (Vector3.Distance(((Component)this).transform.position, Target.CenterPoint) > GiveUpRange)
		{
			if (DEBUG)
			{
				Console.LogWarning("Target is too far away");
			}
			return false;
		}
		return true;
	}

	private void RepositionToTargetMeleeRange(Vector3 origin)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (Target != null)
		{
			Vector3 val = origin - Vector3.up * 1f;
			timeSinceLastReposition = 0f;
			if (GetRandomReachablePointNear(val, GetMaxTargetDistance(), out var randomPoint, GetMinTargetDistance()))
			{
				SetDestination(randomPoint, teleportIfFail: false);
			}
			else
			{
				SetDestination(val, teleportIfFail: false, 10f);
			}
		}
	}

	private unsafe bool GetRandomReachablePointNear(Vector3 originPoint, float randomRadius, out Vector3 randomPoint, float minDistance = 0f)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		randomPoint = originPoint;
		int num = 0;
		while (!flag)
		{
			Vector2 insideUnitCircle = Random.insideUnitCircle;
			Vector3 val = new Vector3(insideUnitCircle.x, 0f, insideUnitCircle.y);
			Vector3 normalized = ((Vector3)(ref val)).normalized;
			NavMeshUtility.SamplePosition(originPoint + normalized * Random.Range(minDistance, randomRadius), out var hit, 5f, base.Npc.Movement.Agent.areaMask);
			if (base.Npc.Movement.CanGetTo(((NavMeshHit)(ref hit)).position, 2f) && Vector3.Distance(originPoint, ((NavMeshHit)(ref hit)).position) > minDistance)
			{
				flag = true;
				_ = ((NavMeshHit)(ref hit)).position;
				break;
			}
			num++;
			if (num > 5)
			{
				string[] obj = new string[5] { "Failed to find reachable point near: ", null, null, null, null };
				val = originPoint;
				obj[1] = ((object)(*(Vector3*)(&val))/*cast due to .constrained prefix*/).ToString();
				obj[2] = " within ";
				obj[3] = randomRadius.ToString();
				obj[4] = "m";
				Console.LogWarning(string.Concat(obj));
				break;
			}
		}
		return flag;
	}

	protected float GetMinTargetDistance()
	{
		if ((Object)(object)currentWeapon != (Object)null)
		{
			return currentWeapon.MinUseRange;
		}
		return VirtualPunchWeapon.MinUseRange;
	}

	protected float GetMaxTargetDistance()
	{
		if ((Object)(object)currentWeapon != (Object)null)
		{
			return currentWeapon.MaxUseRange;
		}
		return VirtualPunchWeapon.MaxUseRange;
	}

	protected bool IsTargetInRange(Vector3 origin = default(Vector3))
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		if (origin == default(Vector3))
		{
			origin = ((Component)this).transform.position;
		}
		if (Target == null)
		{
			return false;
		}
		float num = Vector3.Distance(origin, Target.CenterPoint);
		int num2;
		if (num > GetMinTargetDistance())
		{
			num2 = ((num < GetMaxTargetDistance()) ? 1 : 0);
			if (num2 != 0)
			{
				Debug.DrawLine(origin, Target.CenterPoint, Color.green, 0.1f);
				return (byte)num2 != 0;
			}
		}
		else
		{
			num2 = 0;
		}
		Debug.DrawLine(origin, Target.CenterPoint, Color.red, 0.1f);
		return (byte)num2 != 0;
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ECombat_002ECombatBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ECombat_002ECombatBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_SetTargetAndEnable_Server_3323014238));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_SetTarget_Client_1824087381));
			((NetworkBehaviour)this).RegisterTargetRpc(2u, new ClientRpcDelegate(RpcReader___Target_SetTarget_Client_1824087381));
			((NetworkBehaviour)this).RegisterObserversRpc(3u, new ClientRpcDelegate(RpcReader___Observers_SetWeapon_3615296227));
			((NetworkBehaviour)this).RegisterObserversRpc(4u, new ClientRpcDelegate(RpcReader___Observers_ClearWeapon_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(5u, new ClientRpcDelegate(RpcReader___Observers_Attack_2166136261));
			((NetworkBehaviour)this).RegisterServerRpc(6u, new ServerRpcDelegate(RpcReader___Server_NotifyServerTargetSeen_2166136261));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ECombat_002ECombatBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ECombat_002ECombatBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_SetTargetAndEnable_Server_3323014238(NetworkObject target)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkBehaviour)this).IsClientInitialized)
		{
			NetworkManager networkManager = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager == null)
			{
				networkManager = InstanceFinder.NetworkManager;
			}
			if (networkManager != null)
			{
				networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((Writer)writer).WriteNetworkObject(target);
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetTargetAndEnable_Server_3323014238(NetworkObject target)
	{
		SetTarget_Client(null, target);
		Enable_Networked();
	}

	private void RpcReader___Server_SetTargetAndEnable_Server_3323014238(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkObject target = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetTargetAndEnable_Server_3323014238(target);
		}
	}

	private void RpcWriter___Observers_SetTarget_Client_1824087381(NetworkConnection conn, NetworkObject target)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkBehaviour)this).IsServerInitialized)
		{
			NetworkManager networkManager = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager == null)
			{
				networkManager = InstanceFinder.NetworkManager;
			}
			if (networkManager != null)
			{
				networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((Writer)writer).WriteNetworkObject(target);
			((NetworkBehaviour)this).SendObserversRpc(1u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	protected void RpcLogic___SetTarget_Client_1824087381(NetworkConnection conn, NetworkObject target)
	{
		SetTarget(target);
	}

	private void RpcReader___Observers_SetTarget_Client_1824087381(PooledReader PooledReader0, Channel channel)
	{
		NetworkObject target = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetTarget_Client_1824087381(null, target);
		}
	}

	private void RpcWriter___Target_SetTarget_Client_1824087381(NetworkConnection conn, NetworkObject target)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkBehaviour)this).IsServerInitialized)
		{
			NetworkManager networkManager = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager == null)
			{
				networkManager = InstanceFinder.NetworkManager;
			}
			if (networkManager != null)
			{
				networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((Writer)writer).WriteNetworkObject(target);
			((NetworkBehaviour)this).SendTargetRpc(2u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetTarget_Client_1824087381(PooledReader PooledReader0, Channel channel)
	{
		NetworkObject target = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetTarget_Client_1824087381(((NetworkBehaviour)this).LocalConnection, target);
		}
	}

	private void RpcWriter___Observers_SetWeapon_3615296227(string weaponPath)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkBehaviour)this).IsServerInitialized)
		{
			NetworkManager networkManager = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager == null)
			{
				networkManager = InstanceFinder.NetworkManager;
			}
			if (networkManager != null)
			{
				networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((Writer)writer).WriteString(weaponPath);
			((NetworkBehaviour)this).SendObserversRpc(3u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	protected virtual void RpcLogic___SetWeapon_3615296227(string weaponPath)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Expected O, but got Unknown
		if ((Object)(object)currentWeapon != (Object)null)
		{
			if (weaponPath == currentWeapon.AssetPath)
			{
				return;
			}
			ClearWeapon();
		}
		if (!(weaponPath == string.Empty))
		{
			VirtualPunchWeapon.onSuccessfulHit.RemoveListener(new UnityAction(SucessfulHit));
			currentWeapon = base.Npc.SetEquippable_Return(weaponPath) as AvatarWeapon;
			currentWeapon.onSuccessfulHit.AddListener(new UnityAction(SucessfulHit));
			if ((Object)(object)currentWeapon == (Object)null)
			{
				Console.LogError("Failed to equip weapon");
			}
			else
			{
				OnCurrentWeaponChanged(currentWeapon);
			}
		}
	}

	private void RpcReader___Observers_SetWeapon_3615296227(PooledReader PooledReader0, Channel channel)
	{
		string weaponPath = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetWeapon_3615296227(weaponPath);
		}
	}

	private void RpcWriter___Observers_ClearWeapon_2166136261()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkBehaviour)this).IsServerInitialized)
		{
			NetworkManager networkManager = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager == null)
			{
				networkManager = InstanceFinder.NetworkManager;
			}
			if (networkManager != null)
			{
				networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((NetworkBehaviour)this).SendObserversRpc(4u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	protected void RpcLogic___ClearWeapon_2166136261()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		if (!((Object)(object)currentWeapon == (Object)null))
		{
			currentWeapon.onSuccessfulHit.RemoveListener(new UnityAction(SucessfulHit));
			base.Npc.SetEquippable_Client(null, string.Empty);
			currentWeapon = null;
			VirtualPunchWeapon.onSuccessfulHit.AddListener(new UnityAction(SucessfulHit));
			OnCurrentWeaponChanged(currentWeapon);
		}
	}

	private void RpcReader___Observers_ClearWeapon_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___ClearWeapon_2166136261();
		}
	}

	private void RpcWriter___Observers_Attack_2166136261()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkBehaviour)this).IsServerInitialized)
		{
			NetworkManager networkManager = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager == null)
			{
				networkManager = InstanceFinder.NetworkManager;
			}
			if (networkManager != null)
			{
				networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((NetworkBehaviour)this).SendObserversRpc(5u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	protected virtual void RpcLogic___Attack_2166136261()
	{
		if (ReadyToAttack(checkTarget: false))
		{
			if ((Object)(object)currentWeapon != (Object)null)
			{
				currentWeapon.Attack();
			}
			else
			{
				VirtualPunchWeapon.Attack();
			}
		}
	}

	private void RpcReader___Observers_Attack_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___Attack_2166136261();
		}
	}

	private void RpcWriter___Server_NotifyServerTargetSeen_2166136261()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkBehaviour)this).IsClientInitialized)
		{
			NetworkManager networkManager = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager == null)
			{
				networkManager = InstanceFinder.NetworkManager;
			}
			if (networkManager != null)
			{
				networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((NetworkBehaviour)this).SendServerRpc(6u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___NotifyServerTargetSeen_2166136261()
	{
		visionEventReceived = true;
		IsTargetRecentlyVisible = true;
		IsTargetImmediatelyVisible = true;
	}

	private void RpcReader___Server_NotifyServerTargetSeen_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___NotifyServerTargetSeen_2166136261();
		}
	}

	protected override void Awake_UserLogic_ScheduleOne_002ECombat_002ECombatBehaviour_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		VisionCone visionCone = base.Npc.Awareness.VisionCone;
		visionCone.onVisionEventFull = (VisionCone.EventStateChange)Delegate.Combine(visionCone.onVisionEventFull, new VisionCone.EventStateChange(ProcessVisionEvent));
	}
}
