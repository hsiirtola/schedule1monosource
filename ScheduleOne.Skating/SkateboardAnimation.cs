using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Skating;

public class SkateboardAnimation : MonoBehaviour
{
	[Serializable]
	public class AlignmentSet
	{
		public Transform Transform;

		public Transform Default;

		public Transform Animated;
	}

	[Header("Settings")]
	public float JumpCrouchAmount = 0.4f;

	public float CrouchSpeed = 4f;

	public float ArmLiftRate = 5f;

	public float PelvisMaxRotation = 10f;

	public float HandsMaxRotation = 10f;

	public float PelvisOffsetBlend;

	public float VerticalMomentumMultiplier = 0.5f;

	public float VerticalMomentumOffsetClamp = 0.3f;

	public float MomentumMoveSpeed = 5f;

	public float IKBlendChangeRate = 3f;

	public float PushAnimationDuration = 1.1f;

	public float PushAnimationSpeed = 1.3f;

	public AnimationClip PushAnim;

	[Header("References")]
	public AlignmentSet PelvisContainerAlignment;

	public AlignmentSet PelvisAlignment;

	public AlignmentSet SpineContainerAlignment;

	public AlignmentSet SpineAlignment;

	public Transform SpineAlignment_Hunched;

	public AlignmentSet LeftFootAlignment;

	public AlignmentSet RightFootAlignment;

	public AlignmentSet LeftLegBendTarget;

	public AlignmentSet RightLegBendTarget;

	public AlignmentSet LeftHandAlignment;

	public AlignmentSet RightHandAlignment;

	public Transform AvatarFaceTarget;

	public Transform HandContainer;

	public Animation IKAnimation;

	[Header("Arm Lift")]
	public AlignmentSet LeftHandLoweredAlignment;

	public AlignmentSet LeftHandRaisedAlignment;

	public AlignmentSet RightHandLoweredAlignment;

	public AlignmentSet RightHandRaisedAlignment;

	private Skateboard board;

	private float currentCrouchShift;

	private float targetArmLift;

	private float currentArmLift;

	private Quaternion pelvisDefaultRotation;

	private Vector3 pelvisDefaultPosition;

	private Vector3 spineDefaultPosition;

	private float currentMomentumOffset;

	private float ikBlend;

	private List<AlignmentSet> alignmentSets = new List<AlignmentSet>();

	public float CurrentCrouchShift => currentCrouchShift;

	private void Awake()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		board = ((Component)this).GetComponent<Skateboard>();
		board.OnPushStart.AddListener(new UnityAction(OnPushStart));
		pelvisDefaultPosition = PelvisAlignment.Transform.localPosition;
		pelvisDefaultRotation = PelvisAlignment.Transform.localRotation;
		spineDefaultPosition = SpineAlignment.Transform.localPosition;
		alignmentSets.Add(PelvisContainerAlignment);
		alignmentSets.Add(PelvisAlignment);
		alignmentSets.Add(SpineContainerAlignment);
		alignmentSets.Add(SpineAlignment);
		alignmentSets.Add(LeftFootAlignment);
		alignmentSets.Add(RightFootAlignment);
		alignmentSets.Add(LeftLegBendTarget);
		alignmentSets.Add(RightLegBendTarget);
		alignmentSets.Add(LeftHandAlignment);
		alignmentSets.Add(RightHandAlignment);
		alignmentSets.Add(LeftHandLoweredAlignment);
		alignmentSets.Add(LeftHandRaisedAlignment);
		alignmentSets.Add(RightHandLoweredAlignment);
		alignmentSets.Add(RightHandRaisedAlignment);
	}

	private void Update()
	{
		UpdateIKBlend();
	}

	private void LateUpdate()
	{
		UpdateBodyAlignment();
		UpdateArmLift();
		UpdatePelvisRotation();
	}

	private void FixedUpdate()
	{
	}

	private void UpdateIKBlend()
	{
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		if (board.IsPushing || (board.TimeSincePushStart < PushAnimationDuration && board.JumpBuildAmount < 0.1f))
		{
			ikBlend = Mathf.Lerp(ikBlend, 1f, Time.deltaTime * IKBlendChangeRate);
		}
		else
		{
			ikBlend = Mathf.Lerp(ikBlend, 0f, Time.deltaTime * IKBlendChangeRate);
		}
		foreach (AlignmentSet alignmentSet in alignmentSets)
		{
			alignmentSet.Transform.localPosition = Vector3.Lerp(alignmentSet.Default.localPosition, alignmentSet.Animated.localPosition, ikBlend);
			alignmentSet.Transform.localRotation = Quaternion.Lerp(alignmentSet.Default.localRotation, alignmentSet.Animated.localRotation, ikBlend);
		}
	}

	private void UpdateBodyAlignment()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = PelvisAlignment.Transform.parent.TransformPoint(new Vector3(pelvisDefaultPosition.x, -0.01f, pelvisDefaultPosition.z));
		Vector3 val2 = default(Vector3);
		((Vector3)(ref val2))._002Ector(0f, pelvisDefaultPosition.y, 0f);
		Vector3 val3 = ((Component)this).transform.up * pelvisDefaultPosition.y;
		val += Vector3.Lerp(val2, val3, PelvisOffsetBlend);
		float jumpBuildAmount = board.JumpBuildAmount;
		float num = Mathf.Clamp01(board.CurrentSpeed_Kmh / board.CurentSettings.TopSpeed_Kmh) * 0.1f;
		float num2 = Mathf.Max(jumpBuildAmount, num);
		currentCrouchShift = Mathf.Lerp(currentCrouchShift, num2, Time.deltaTime * CrouchSpeed);
		val.y -= currentCrouchShift * JumpCrouchAmount;
		float num3 = Mathf.Clamp((0f - board.Accelerometer.Acceleration.y) * VerticalMomentumMultiplier, 0f - VerticalMomentumOffsetClamp, 0f);
		float num4 = 1f;
		if (num3 < currentMomentumOffset)
		{
			num4 = 0.3f;
		}
		currentMomentumOffset = Mathf.Lerp(currentMomentumOffset, num3, Time.deltaTime * MomentumMoveSpeed * num4);
		val.y += currentMomentumOffset;
		PelvisAlignment.Transform.position = val;
		SpineAlignment.Transform.localPosition = Vector3.Lerp(spineDefaultPosition, SpineAlignment_Hunched.localPosition, currentCrouchShift);
	}

	private void UpdateArmLift()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		float jumpBuildAmount = board.JumpBuildAmount;
		float num = Mathf.Clamp01(board.CurrentSpeed_Kmh / board.CurentSettings.TopSpeed_Kmh) * 0f;
		float num2 = Mathf.Abs(board.CurrentSteerInput) * 0.2f;
		SetArmLift(Mathf.Max(new float[3] { jumpBuildAmount, num, num2 }));
		currentArmLift = Mathf.Lerp(currentArmLift, targetArmLift, Time.deltaTime * ArmLiftRate);
		RightHandAlignment.Transform.localPosition = Vector3.Lerp(RightHandLoweredAlignment.Transform.localPosition, RightHandRaisedAlignment.Transform.localPosition, currentArmLift);
		RightHandAlignment.Transform.localRotation = Quaternion.Lerp(RightHandLoweredAlignment.Transform.localRotation, RightHandRaisedAlignment.Transform.localRotation, currentArmLift);
		LeftHandAlignment.Transform.localPosition = Vector3.Lerp(LeftHandLoweredAlignment.Transform.localPosition, LeftHandRaisedAlignment.Transform.localPosition, currentArmLift);
		LeftHandAlignment.Transform.localRotation = Quaternion.Lerp(LeftHandLoweredAlignment.Transform.localRotation, LeftHandRaisedAlignment.Transform.localRotation, currentArmLift);
	}

	private void UpdatePelvisRotation()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		float num = board.CurrentSteerInput * PelvisMaxRotation;
		Quaternion val = pelvisDefaultRotation * Quaternion.AngleAxis(num, Vector3.up);
		PelvisAlignment.Transform.localRotation = Quaternion.Lerp(PelvisAlignment.Transform.localRotation, val, Time.deltaTime * 5f);
		HandContainer.localRotation = Quaternion.Lerp(HandContainer.localRotation, Quaternion.Euler(num, 0f, 0f), Time.deltaTime * 5f);
	}

	public void SetArmLift(float lift)
	{
		targetArmLift = lift;
	}

	private void OnPushStart()
	{
		IKAnimation.Stop();
		IKAnimation[((Object)PushAnim).name].speed = PushAnimationSpeed;
		IKAnimation.Play(((Object)PushAnim).name);
	}
}
