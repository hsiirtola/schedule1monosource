using System;
using System.Collections;
using RootMotion.FinalIK;
using ScheduleOne.DevUtilities;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Skating;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.AvatarFramework.Animation;

public class AvatarAnimation : MonoBehaviour
{
	public enum EFlinchType
	{
		Light,
		Heavy
	}

	public enum EFlinchDirection
	{
		Forward,
		Backward,
		Left,
		Right
	}

	public const bool ImpostorsEnabled = true;

	public const float AnimationRangeSqr = 2500f;

	public const float FrustrumCullMinDist = 225f;

	public const float RunningAnimationSpeed = 8f;

	public const float MaxBoneOffset = 0.01f;

	public const float MaxBoneOffsetSqr = 0.0001f;

	public static Vector3 SITTING_OFFSET = new Vector3(0f, -0.825f, 0f);

	public const float SEAT_TIME = 0.5f;

	private const string StandUpFromBackClipName = "Stand up from back";

	private const string StandUpFromFrontClipName = "Stand up from front";

	public bool DEBUG_MODE;

	[Header("References")]
	public Animator animator;

	public Transform HipBone;

	public Transform[] Bones;

	protected Avatar avatar;

	public Transform LeftHandContainer;

	public Transform RightHandContainer;

	public Transform RightHandAlignmentPoint;

	public Transform LeftHandAlignmentPoint;

	public AvatarIKController IKController;

	public AvatarFootstepDetector FootstepDetector;

	[Header("Settings")]
	public LayerMask GroundingMask;

	public bool AllowCulling = true;

	public UnityEvent onStandupStart;

	public UnityEvent onStandupDone;

	public UnityEvent onHeavyFlinch;

	private BoneTransform[] standUpFromBackBoneTransforms;

	private BoneTransform[] standUpFromFrontBoneTransforms;

	private BoneTransform[] ragdollBoneTransforms;

	private Coroutine standUpRoutine;

	private Coroutine seatRoutine;

	private Skateboard activeSkateboard;

	private bool animationEnabled = true;

	private BoneTransform[] _lastFrameBoneTransforms;

	public bool IsCrouched { get; protected set; }

	public bool IsSeated => (Object)(object)CurrentSeat != (Object)null;

	public float TimeSinceSitEnd { get; protected set; } = 1000f;

	public AvatarSeat CurrentSeat { get; protected set; }

	public bool StandUpAnimationPlaying { get; protected set; }

	public bool IsAvatarCulled { get; private set; }

	protected virtual void Awake()
	{
		avatar = ((Component)this).GetComponent<Avatar>();
		avatar.onRagdollChange.AddListener((UnityAction<bool, bool, bool>)RagdollChange);
		ragdollBoneTransforms = new BoneTransform[Bones.Length];
		for (int i = 0; i < Bones.Length; i++)
		{
			ragdollBoneTransforms[i] = new BoneTransform();
		}
		Player componentInParent = ((Component)this).GetComponentInParent<Player>();
		if ((Object)(object)componentInParent != (Object)null)
		{
			componentInParent.onSkateboardMounted = (Action<Skateboard>)Delegate.Combine(componentInParent.onSkateboardMounted, new Action<Skateboard>(SkateboardMounted));
			componentInParent.onSkateboardDismounted = (Action)Delegate.Combine(componentInParent.onSkateboardDismounted, new Action(SkateboardDismounted));
		}
		_lastFrameBoneTransforms = new BoneTransform[Bones.Length];
		animator.keepAnimatorStateOnDisable = true;
		((MonoBehaviour)this).InvokeRepeating("UpdateAnimationActive", Random.Range(0f, 0.5f), 0.1f);
	}

	private void Start()
	{
		if (standUpFromBackBoneTransforms == null)
		{
			standUpFromBackBoneTransforms = new BoneTransform[Bones.Length];
			for (int i = 0; i < Bones.Length; i++)
			{
				standUpFromBackBoneTransforms[i] = new BoneTransform();
			}
			PopulateAnimationStartBoneTransforms("Stand up from back", standUpFromBackBoneTransforms);
		}
		if (standUpFromFrontBoneTransforms == null)
		{
			standUpFromFrontBoneTransforms = new BoneTransform[Bones.Length];
			for (int j = 0; j < Bones.Length; j++)
			{
				standUpFromFrontBoneTransforms[j] = new BoneTransform();
			}
			PopulateAnimationStartBoneTransforms("Stand up from front", standUpFromFrontBoneTransforms);
		}
	}

	private void Update()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		if (IsSeated)
		{
			TimeSinceSitEnd = 0f;
		}
		else
		{
			TimeSinceSitEnd += Time.deltaTime;
		}
		if (seatRoutine == null && (Object)(object)CurrentSeat != (Object)null)
		{
			((Component)this).transform.position = CurrentSeat.SittingPoint.position + SITTING_OFFSET * ((Component)this).transform.localScale.y;
			((Component)this).transform.rotation = CurrentSeat.SittingPoint.rotation;
		}
	}

	private void LateUpdate()
	{
		PopulateBoneTransforms(_lastFrameBoneTransforms);
	}

	private void UpdateAnimationActive()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		if (!PlayerSingleton<PlayerCamera>.InstanceExists)
		{
			return;
		}
		float num = Vector3.SqrMagnitude(((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position - avatar.CenterPoint);
		bool flag = num < 2500f * QualitySettings.lodBias;
		if (flag && num > 225f)
		{
			flag = Vector3.Dot(((Component)PlayerSingleton<PlayerCamera>.Instance).transform.forward, avatar.CenterPoint - ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position) > 0f;
		}
		if (DEBUG_MODE)
		{
			Debug.Log((object)("Avatar Animation In Range: " + flag + " | DistSqr: " + num), (Object)(object)this);
		}
		if (Time.timeSinceLevelLoad < 3f)
		{
			flag = true;
		}
		if (!AllowCulling)
		{
			flag = true;
		}
		bool isAvatarCulled = IsAvatarCulled;
		IsAvatarCulled = !flag;
		if (IsAvatarCulled != isAvatarCulled)
		{
			((Component)avatar.BodyContainer).gameObject.SetActive(!IsAvatarCulled);
			if (IsAvatarCulled)
			{
				avatar.Impostor.EnableImpostor();
			}
			else
			{
				avatar.Impostor.DisableImpostor();
			}
		}
		((Behaviour)animator).enabled = animationEnabled && !IsAvatarCulled;
	}

	public void SetDirection(float dir)
	{
		animator.SetFloat(AAId.DIRECTION, dir);
	}

	public void SetStrafe(float strafe)
	{
		animator.SetFloat(AAId.STRAFE, strafe);
	}

	public void SetTimeAirborne(float airbone)
	{
		animator.SetFloat(AAId.TIME_AIRBORNE, airbone);
	}

	public void SetCrouched(bool crouched)
	{
		IsCrouched = crouched;
		animator.SetBool(AAId.IS_CROUCHED, crouched);
	}

	public void SetGrounded(bool grounded)
	{
		animator.SetBool(AAId.IS_GROUNDED, grounded);
	}

	public void Jump()
	{
		animator.SetTrigger(AAId.JUMP);
	}

	public void SetAnimationEnabled(bool enabled)
	{
		if (DEBUG_MODE)
		{
			Console.Log("Setting animation enabled: " + enabled, (Object)(object)this);
		}
		animationEnabled = enabled;
		UpdateAnimationActive();
	}

	public void ResetAnimatorState()
	{
		bool keepAnimatorStateOnDisable = animator.keepAnimatorStateOnDisable;
		animator.keepAnimatorStateOnDisable = false;
		if (((Component)this).gameObject.activeInHierarchy)
		{
			((Component)animator).gameObject.SetActive(false);
			((Component)animator).gameObject.SetActive(true);
		}
		animator.keepAnimatorStateOnDisable = keepAnimatorStateOnDisable;
	}

	public void Flinch(Vector3 forceDirection, EFlinchType flinchType)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)this).transform.InverseTransformDirection(forceDirection);
		EFlinchDirection eFlinchDirection = EFlinchDirection.Forward;
		eFlinchDirection = ((Mathf.Abs(val.z) > Mathf.Abs(val.x)) ? ((!(val.z > 0f)) ? EFlinchDirection.Backward : EFlinchDirection.Forward) : ((!(val.x > 0f)) ? EFlinchDirection.Left : EFlinchDirection.Right));
		if (flinchType == EFlinchType.Light)
		{
			switch (eFlinchDirection)
			{
			case EFlinchDirection.Forward:
				animator.SetTrigger(AAId.FLINCH_FORWARD);
				break;
			case EFlinchDirection.Backward:
				animator.SetTrigger(AAId.FLINCH_BACKWARD);
				break;
			case EFlinchDirection.Left:
				animator.SetTrigger(AAId.FLINCH_LEFT);
				break;
			case EFlinchDirection.Right:
				animator.SetTrigger(AAId.FLINCH_RIGHT);
				break;
			}
			return;
		}
		switch (eFlinchDirection)
		{
		case EFlinchDirection.Forward:
			animator.SetTrigger(AAId.FLINCH_HEAVY_FORWARD);
			break;
		case EFlinchDirection.Backward:
			animator.SetTrigger(AAId.FLINCH_HEAVY_BACKWARD);
			break;
		case EFlinchDirection.Left:
			animator.SetTrigger(AAId.FLINCH_HEAVY_LEFT);
			break;
		case EFlinchDirection.Right:
			animator.SetTrigger(AAId.FLINCH_HEAVY_RIGHT);
			break;
		}
		if (onHeavyFlinch != null)
		{
			onHeavyFlinch.Invoke();
		}
	}

	public void PlayStandUpAnimation()
	{
		StandUpAnimationPlaying = true;
		if (onStandupStart != null)
		{
			onStandupStart.Invoke();
		}
		PopulateBoneTransforms(ragdollBoneTransforms);
		bool standUpFromBack = ShouldGetUpFromBack();
		PopulateAnimationStartBoneTransforms("Stand up from front", standUpFromFrontBoneTransforms);
		PopulateAnimationStartBoneTransforms("Stand up from back", standUpFromBackBoneTransforms);
		BoneTransform[] finalBoneTransforms = (standUpFromBack ? standUpFromBackBoneTransforms : standUpFromFrontBoneTransforms);
		if (standUpRoutine != null)
		{
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StopCoroutine(standUpRoutine);
		}
		standUpRoutine = ((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(StandUpRoutine());
		IEnumerator StandUpRoutine()
		{
			float time = 0.3f;
			for (int i = 0; i < Bones.Length; i++)
			{
				Rigidbody val = null;
				if (((Component)Bones[i]).TryGetComponent<Rigidbody>(ref val))
				{
					val.interpolation = (RigidbodyInterpolation)0;
				}
			}
			for (float i2 = 0f; i2 < time; i2 += Time.deltaTime)
			{
				for (int j = 0; j < Bones.Length; j++)
				{
					Bones[j].localPosition = Vector3.Lerp(ragdollBoneTransforms[j].Position, finalBoneTransforms[j].Position, i2 / time);
					Bones[j].localRotation = Quaternion.Lerp(ragdollBoneTransforms[j].Rotation, finalBoneTransforms[j].Rotation, i2 / time);
				}
				yield return (object)new WaitForEndOfFrame();
			}
			for (int k = 0; k < Bones.Length; k++)
			{
				Bones[k].localPosition = finalBoneTransforms[k].Position;
				Bones[k].localRotation = finalBoneTransforms[k].Rotation;
			}
			SetAnimationEnabled(enabled: true);
			if (((Behaviour)animator).enabled)
			{
				int trigger = (standUpFromBack ? AAId.STANDUP_BACK : AAId.STANDUP_FRONT);
				animator.SetTrigger(trigger);
			}
			for (int l = 0; l < Bones.Length; l++)
			{
				Rigidbody val2 = null;
				if (((Component)Bones[l]).TryGetComponent<Rigidbody>(ref val2))
				{
					val2.interpolation = (RigidbodyInterpolation)1;
				}
			}
			yield return (object)new WaitForSecondsRealtime(1.5f);
			if (onStandupDone != null)
			{
				onStandupDone.Invoke();
			}
			standUpRoutine = null;
			StandUpAnimationPlaying = false;
		}
	}

	protected void RagdollChange(bool wasRagdolled, bool ragdoll, bool playStandUpAnim)
	{
		if (wasRagdolled == ragdoll)
		{
			return;
		}
		if (ragdoll && IsSeated)
		{
			if ((Object)(object)CurrentSeat != (Object)null)
			{
				CurrentSeat.SetOccupant(null);
				CurrentSeat = null;
			}
			animator.SetBool(AAId.SITTING, false);
			((Component)this).GetComponentInParent<NPCMovement>().SpeedController.RemoveSpeedControl("seated");
		}
		if (ragdoll)
		{
			if (standUpRoutine != null)
			{
				((MonoBehaviour)this).StopCoroutine(standUpRoutine);
			}
			SetAnimationEnabled(enabled: false);
			ApplyBoneTransforms(_lastFrameBoneTransforms);
		}
		else
		{
			ResetAnimatorState();
			AlignPositionToHips();
			if (playStandUpAnim)
			{
				PlayStandUpAnimation();
			}
			else
			{
				SetAnimationEnabled(enabled: true);
			}
		}
	}

	private void AlignPositionToHips()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = HipBone.position;
		Quaternion rotation = HipBone.rotation;
		((Component)this).transform.position = HipBone.position;
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(((Component)this).transform.position, Vector3.down, ref val, 10f, LayerMask.op_Implicit(GroundingMask)))
		{
			((Component)this).transform.position = new Vector3(((Component)this).transform.position.x, ((RaycastHit)(ref val)).point.y, ((Component)this).transform.position.z);
		}
		((Component)this).transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(ShouldGetUpFromBack() ? (-HipBone.up) : HipBone.up, Vector3.up), Vector3.up);
		HipBone.position = position;
		HipBone.rotation = rotation;
	}

	private bool ShouldGetUpFromBack()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Angle(HipBone.forward, Vector3.up) < 90f;
	}

	private void PopulateBoneTransforms(BoneTransform[] boneTransforms)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Bones.Length; i++)
		{
			boneTransforms[i] = new BoneTransform();
			boneTransforms[i].Position = Bones[i].localPosition;
			boneTransforms[i].Rotation = Bones[i].localRotation;
		}
	}

	private void PopulateAnimationStartBoneTransforms(string clipName, BoneTransform[] boneTransforms)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)animator).transform.position;
		Quaternion rotation = ((Component)animator).transform.rotation;
		if ((Object)(object)animator.runtimeAnimatorController == (Object)null)
		{
			return;
		}
		AnimationClip[] animationClips = animator.runtimeAnimatorController.animationClips;
		foreach (AnimationClip val in animationClips)
		{
			if (((Object)val).name == clipName)
			{
				val.SampleAnimation(((Component)animator).gameObject, 0f);
				PopulateBoneTransforms(boneTransforms);
				break;
			}
		}
		((Component)animator).transform.position = position;
		((Component)animator).transform.rotation = rotation;
	}

	private void ApplyBoneTransforms(BoneTransform[] boneTransforms)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (boneTransforms == null)
		{
			return;
		}
		for (int i = 0; i < Bones.Length; i++)
		{
			if (boneTransforms[i] != null)
			{
				Bones[i].localPosition = boneTransforms[i].Position;
				Bones[i].localRotation = boneTransforms[i].Rotation;
			}
		}
	}

	public void SetTrigger(string trigger)
	{
		if (!string.IsNullOrEmpty(trigger))
		{
			if ((Object)(object)animator != (Object)null)
			{
				int trigger2 = AAId.Get(trigger);
				animator.SetTrigger(trigger2);
			}
			UpdateAnimationActive();
		}
	}

	public void ResetTrigger(string trigger)
	{
		int num = AAId.Get(trigger);
		animator.ResetTrigger(num);
	}

	public void SetBool(string id, bool value)
	{
		if ((Object)(object)animator != (Object)null)
		{
			int num = AAId.Get(id);
			animator.SetBool(num, value);
		}
		UpdateAnimationActive();
	}

	public void SetSeat(AvatarSeat seat)
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		Vector3 startPos;
		Quaternion startRot;
		Vector3 endPos;
		Quaternion endRot;
		if (!((Object)(object)seat == (Object)(object)CurrentSeat))
		{
			if ((Object)(object)CurrentSeat != (Object)null)
			{
				CurrentSeat.SetOccupant(null);
			}
			CurrentSeat = seat;
			if ((Object)(object)CurrentSeat != (Object)null)
			{
				CurrentSeat.SetOccupant(((Component)this).GetComponentInParent<NPC>());
			}
			animator.SetBool(AAId.SITTING, IsSeated);
			startPos = ((Component)this).transform.position;
			startRot = ((Component)this).transform.rotation;
			if (seatRoutine != null)
			{
				((MonoBehaviour)Singleton<CoroutineService>.Instance).StopCoroutine(seatRoutine);
			}
			if ((Object)(object)CurrentSeat != (Object)null)
			{
				endPos = CurrentSeat.SittingPoint.position + SITTING_OFFSET * ((Component)this).transform.localScale.y;
				endRot = CurrentSeat.SittingPoint.rotation;
				seatRoutine = ((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Lerp(resetLocalCoordinates: false));
				((Component)this).GetComponentInParent<NPCMovement>().SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("seated", 100, -1f));
			}
			else
			{
				endPos = ((Component)this).transform.parent.position;
				endRot = ((Component)this).transform.parent.rotation;
				seatRoutine = ((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Lerp(resetLocalCoordinates: true));
			}
		}
		IEnumerator Lerp(bool resetLocalCoordinates)
		{
			for (float i = 0f; i < 0.5f; i += Time.deltaTime)
			{
				((Component)this).transform.position = Vector3.Lerp(startPos, endPos, i / 0.5f);
				((Component)this).transform.rotation = Quaternion.Lerp(startRot, endRot, i / 0.5f);
				yield return (object)new WaitForEndOfFrame();
			}
			((Component)this).transform.position = endPos;
			((Component)this).transform.rotation = endRot;
			if (resetLocalCoordinates)
			{
				NPCMovement componentInParent = ((Component)this).GetComponentInParent<NPCMovement>();
				if ((Object)(object)componentInParent != (Object)null)
				{
					((Component)componentInParent).transform.position = endPos;
					((Component)componentInParent).transform.rotation = endRot;
					componentInParent.SpeedController.RemoveSpeedControl("seated");
				}
				((Component)this).transform.localPosition = Vector3.zero;
				((Component)this).transform.localRotation = Quaternion.identity;
			}
			seatRoutine = null;
		}
	}

	public void SkateboardMounted(Skateboard board)
	{
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Expected O, but got Unknown
		IKController.BodyIK.solvers.pelvis.target = board.Animation.PelvisAlignment.Transform;
		((IKSolverHeuristic)IKController.BodyIK.solvers.spine).target = board.Animation.SpineAlignment.Transform;
		((IKSolverTrigonometric)IKController.BodyIK.solvers.leftFoot).target = board.Animation.LeftFootAlignment.Transform;
		((IKSolverTrigonometric)IKController.BodyIK.solvers.rightFoot).target = board.Animation.RightFootAlignment.Transform;
		((IKSolverTrigonometric)IKController.BodyIK.solvers.leftHand).target = board.Animation.LeftHandAlignment.Transform;
		((IKSolverTrigonometric)IKController.BodyIK.solvers.rightHand).target = board.Animation.RightHandAlignment.Transform;
		((IKSolverTrigonometric)IKController.BodyIK.solvers.rightFoot).SetBendPlaneToCurrent();
		((IKSolverTrigonometric)IKController.BodyIK.solvers.leftFoot).SetBendPlaneToCurrent();
		IKController.OverrideLegBendTargets(board.Animation.LeftLegBendTarget.Transform, board.Animation.RightLegBendTarget.Transform);
		IKController.SetIKActive(active: true);
		avatar.SetEquippable(string.Empty);
		avatar.LookController.ForceLookTarget = board.Animation.AvatarFaceTarget;
		avatar.LookController.ForceLookRotateBody = true;
		SetBool("SkateIdle", value: true);
		activeSkateboard = board;
		activeSkateboard.OnPushStart.AddListener(new UnityAction(SkateboardPush));
	}

	public void SkateboardDismounted()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		IKController.ResetLegBendTargets();
		IKController.SetIKActive(active: false);
		avatar.LookController.ForceLookTarget = null;
		avatar.LookController.ForceLookRotateBody = false;
		SetBool("SkateIdle", value: false);
		activeSkateboard.OnPushStart.RemoveListener(new UnityAction(SkateboardPush));
		activeSkateboard = null;
	}

	private void SkateboardPush()
	{
		SetTrigger("SkatePush");
	}
}
