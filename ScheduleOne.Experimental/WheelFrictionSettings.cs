using System;
using UnityEngine;

namespace ScheduleOne.Experimental;

[Serializable]
public class WheelFrictionSettings
{
	public float ExtremumSlip;

	public float ExtremumValue;

	public float AsymptoteSlip;

	public float AsymptoteValue;

	public float Stiffness;

	public WheelFrictionSettings Blend(WheelFrictionSettings other, float t)
	{
		return new WheelFrictionSettings
		{
			ExtremumSlip = Mathf.Lerp(ExtremumSlip, other.ExtremumSlip, t),
			ExtremumValue = Mathf.Lerp(ExtremumValue, other.ExtremumValue, t),
			AsymptoteSlip = Mathf.Lerp(AsymptoteSlip, other.AsymptoteSlip, t),
			AsymptoteValue = Mathf.Lerp(AsymptoteValue, other.AsymptoteValue, t),
			Stiffness = Mathf.Lerp(Stiffness, other.Stiffness, t)
		};
	}
}
