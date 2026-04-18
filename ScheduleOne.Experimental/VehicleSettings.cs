using System;

namespace ScheduleOne.Experimental;

[Serializable]
public class VehicleSettings
{
	public WheelFrictionSettings ForwardFriction = new WheelFrictionSettings
	{
		ExtremumSlip = 2f,
		ExtremumValue = 1f,
		AsymptoteSlip = 0.8f,
		AsymptoteValue = 0.5f,
		Stiffness = 2.5f
	};

	public WheelFrictionSettings SidewaysFriction = new WheelFrictionSettings
	{
		ExtremumSlip = 0.4f,
		ExtremumValue = 1f,
		AsymptoteSlip = 0.5f,
		AsymptoteValue = 0.75f,
		Stiffness = 2f
	};

	public VehicleSettings Clone()
	{
		return new VehicleSettings
		{
			ForwardFriction = new WheelFrictionSettings
			{
				ExtremumSlip = ForwardFriction.ExtremumSlip,
				ExtremumValue = ForwardFriction.ExtremumValue,
				AsymptoteSlip = ForwardFriction.AsymptoteSlip,
				AsymptoteValue = ForwardFriction.AsymptoteValue,
				Stiffness = ForwardFriction.Stiffness
			},
			SidewaysFriction = new WheelFrictionSettings
			{
				ExtremumSlip = SidewaysFriction.ExtremumSlip,
				ExtremumValue = SidewaysFriction.ExtremumValue,
				AsymptoteSlip = SidewaysFriction.AsymptoteSlip,
				AsymptoteValue = SidewaysFriction.AsymptoteValue,
				Stiffness = SidewaysFriction.Stiffness
			}
		};
	}

	public VehicleSettings Blend(VehicleSettings other, float t)
	{
		return new VehicleSettings
		{
			ForwardFriction = ForwardFriction.Blend(other.ForwardFriction, t),
			SidewaysFriction = SidewaysFriction.Blend(other.SidewaysFriction, t)
		};
	}
}
