using System;
using UnityEngine;

namespace ScheduleOne.Experimental;

[Serializable]
public class SkateboardSettings
{
	public float TurnForce = 1f;

	public float TurnChangeRate = 2f;

	public float TurnReturnToRestRate = 1f;

	public float TurnSpeedBoost = 1f;

	public AnimationCurve TurnForceMap;

	public float Gravity = 10f;

	public float BrakeForce = 1f;

	public float ReverseTopSpeed_Kmh = 5f;

	public float RotationClampForce = 1f;

	public bool FrictionEnabled = true;

	public AnimationCurve LongitudinalFrictionCurve;

	public float LongitudinalFrictionMultiplier = 1f;

	public float LateralFrictionForceMultiplier = 1f;

	public float JumpForce = 1f;

	public float JumpDuration_Min = 0.2f;

	public float JumpDuration_Max = 0.5f;

	public AnimationCurve FrontAxleJumpCurve;

	public AnimationCurve RearAxleJumpCurve;

	public AnimationCurve JumpForwardForceCurve;

	public float JumpForwardBoost = 1f;

	public float HoverForce = 1f;

	public float HoverRayLength = 0.1f;

	public float HoverHeight = 0.05f;

	public float Hover_P = 1f;

	public float Hover_I = 1f;

	public float Hover_D = 1f;

	[Tooltip("Top speed in m/s")]
	public float TopSpeed_Kmh = 10f;

	public float PushForceMultiplier = 1f;

	public AnimationCurve PushForceMultiplierMap;

	public float PushForceDuration = 0.4f;

	public float PushDelay = 0.35f;

	public AnimationCurve PushForceCurve;

	public bool AirMovementEnabled = true;

	public float AirMovementForce = 1f;

	public float AirMovementJumpReductionDuration = 0.25f;

	public AnimationCurve AirMovementJumpReductionCurve;

	public float TopSpeed_Ms => TopSpeed_Kmh / 3.6f;

	public SkateboardSettings Clone()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Expected O, but got Unknown
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Expected O, but got Unknown
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Expected O, but got Unknown
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Expected O, but got Unknown
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Expected O, but got Unknown
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Expected O, but got Unknown
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Expected O, but got Unknown
		return new SkateboardSettings
		{
			TurnForce = TurnForce,
			TurnChangeRate = TurnChangeRate,
			TurnReturnToRestRate = TurnReturnToRestRate,
			TurnSpeedBoost = TurnSpeedBoost,
			TurnForceMap = new AnimationCurve(TurnForceMap.keys),
			Gravity = Gravity,
			BrakeForce = BrakeForce,
			ReverseTopSpeed_Kmh = ReverseTopSpeed_Kmh,
			RotationClampForce = RotationClampForce,
			FrictionEnabled = FrictionEnabled,
			LongitudinalFrictionCurve = new AnimationCurve(LongitudinalFrictionCurve.keys),
			LongitudinalFrictionMultiplier = LongitudinalFrictionMultiplier,
			LateralFrictionForceMultiplier = LateralFrictionForceMultiplier,
			JumpForce = JumpForce,
			JumpDuration_Min = JumpDuration_Min,
			JumpDuration_Max = JumpDuration_Max,
			FrontAxleJumpCurve = new AnimationCurve(FrontAxleJumpCurve.keys),
			RearAxleJumpCurve = new AnimationCurve(RearAxleJumpCurve.keys),
			JumpForwardForceCurve = new AnimationCurve(JumpForwardForceCurve.keys),
			JumpForwardBoost = JumpForwardBoost,
			HoverForce = HoverForce,
			HoverRayLength = HoverRayLength,
			HoverHeight = HoverHeight,
			Hover_P = Hover_P,
			Hover_I = Hover_I,
			Hover_D = Hover_D,
			TopSpeed_Kmh = TopSpeed_Kmh,
			PushForceMultiplier = PushForceMultiplier,
			PushForceMultiplierMap = new AnimationCurve(PushForceMultiplierMap.keys),
			PushForceDuration = PushForceDuration,
			PushDelay = PushDelay,
			PushForceCurve = new AnimationCurve(PushForceCurve.keys),
			AirMovementEnabled = AirMovementEnabled,
			AirMovementForce = AirMovementForce,
			AirMovementJumpReductionDuration = AirMovementJumpReductionDuration,
			AirMovementJumpReductionCurve = new AnimationCurve(AirMovementJumpReductionCurve.keys)
		};
	}

	public SkateboardSettings Blend(SkateboardSettings other, float blendFactor)
	{
		SkateboardSettings skateboardSettings = Clone();
		skateboardSettings.TurnForce *= ((other.TurnForce != -1f) ? Mathf.Lerp(1f, other.TurnForce, blendFactor) : 1f);
		skateboardSettings.TurnChangeRate *= ((other.TurnChangeRate != -1f) ? Mathf.Lerp(1f, other.TurnChangeRate, blendFactor) : 1f);
		skateboardSettings.TurnReturnToRestRate *= ((other.TurnReturnToRestRate != -1f) ? Mathf.Lerp(1f, other.TurnReturnToRestRate, blendFactor) : 1f);
		skateboardSettings.TurnSpeedBoost *= ((other.TurnSpeedBoost != -1f) ? Mathf.Lerp(1f, other.TurnSpeedBoost, blendFactor) : 1f);
		skateboardSettings.TurnForceMap = TurnForceMap;
		skateboardSettings.Gravity *= ((other.Gravity != -1f) ? Mathf.Lerp(1f, other.Gravity, blendFactor) : 1f);
		skateboardSettings.BrakeForce *= ((other.BrakeForce != -1f) ? Mathf.Lerp(1f, other.BrakeForce, blendFactor) : 1f);
		skateboardSettings.ReverseTopSpeed_Kmh *= ((other.ReverseTopSpeed_Kmh != -1f) ? Mathf.Lerp(1f, other.ReverseTopSpeed_Kmh, blendFactor) : 1f);
		skateboardSettings.RotationClampForce *= ((other.RotationClampForce != -1f) ? Mathf.Lerp(1f, other.RotationClampForce, blendFactor) : 1f);
		skateboardSettings.FrictionEnabled = FrictionEnabled;
		skateboardSettings.LongitudinalFrictionCurve = LongitudinalFrictionCurve;
		skateboardSettings.LongitudinalFrictionMultiplier *= ((other.LongitudinalFrictionMultiplier != -1f) ? Mathf.Lerp(1f, other.LongitudinalFrictionMultiplier, blendFactor) : 1f);
		skateboardSettings.LateralFrictionForceMultiplier *= ((other.LateralFrictionForceMultiplier != -1f) ? Mathf.Lerp(1f, other.LateralFrictionForceMultiplier, blendFactor) : 1f);
		skateboardSettings.JumpForce *= ((other.JumpForce != -1f) ? Mathf.Lerp(1f, other.JumpForce, blendFactor) : 1f);
		skateboardSettings.JumpDuration_Min *= ((other.JumpDuration_Min != -1f) ? Mathf.Lerp(1f, other.JumpDuration_Min, blendFactor) : 1f);
		skateboardSettings.JumpDuration_Max *= ((other.JumpDuration_Max != -1f) ? Mathf.Lerp(1f, other.JumpDuration_Max, blendFactor) : 1f);
		skateboardSettings.JumpForwardBoost *= ((other.JumpForwardBoost != -1f) ? Mathf.Lerp(1f, other.JumpForwardBoost, blendFactor) : 1f);
		skateboardSettings.HoverForce *= ((other.HoverForce != -1f) ? Mathf.Lerp(1f, other.HoverForce, blendFactor) : 1f);
		skateboardSettings.HoverRayLength *= ((other.HoverRayLength != -1f) ? Mathf.Lerp(1f, other.HoverRayLength, blendFactor) : 1f);
		skateboardSettings.HoverHeight *= ((other.HoverHeight != -1f) ? Mathf.Lerp(1f, other.HoverHeight, blendFactor) : 1f);
		skateboardSettings.Hover_P *= ((other.Hover_P != -1f) ? Mathf.Lerp(1f, other.Hover_P, blendFactor) : 1f);
		skateboardSettings.Hover_I *= ((other.Hover_I != -1f) ? Mathf.Lerp(1f, other.Hover_I, blendFactor) : 1f);
		skateboardSettings.Hover_D *= ((other.Hover_D != -1f) ? Mathf.Lerp(1f, other.Hover_D, blendFactor) : 1f);
		skateboardSettings.TopSpeed_Kmh *= ((other.TopSpeed_Kmh != -1f) ? Mathf.Lerp(1f, other.TopSpeed_Kmh, blendFactor) : 1f);
		skateboardSettings.PushForceMultiplier *= ((other.PushForceMultiplier != -1f) ? Mathf.Lerp(1f, other.PushForceMultiplier, blendFactor) : 1f);
		skateboardSettings.PushForceDuration *= ((other.PushForceDuration != -1f) ? Mathf.Lerp(1f, other.PushForceDuration, blendFactor) : 1f);
		skateboardSettings.PushDelay *= ((other.PushDelay != -1f) ? Mathf.Lerp(1f, other.PushDelay, blendFactor) : 1f);
		skateboardSettings.AirMovementForce *= ((other.AirMovementForce != -1f) ? Mathf.Lerp(1f, other.AirMovementForce, blendFactor) : 1f);
		skateboardSettings.AirMovementJumpReductionDuration *= ((other.AirMovementJumpReductionDuration != -1f) ? Mathf.Lerp(1f, other.AirMovementJumpReductionDuration, blendFactor) : 1f);
		skateboardSettings.AirMovementJumpReductionCurve = AirMovementJumpReductionCurve;
		return skateboardSettings;
	}
}
