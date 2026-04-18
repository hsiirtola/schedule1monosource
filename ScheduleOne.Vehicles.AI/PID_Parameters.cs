using System;

namespace ScheduleOne.Vehicles.AI;

[Serializable]
public struct PID_Parameters(float P, float I, float D)
{
	public float P = P;

	public float I = I;

	public float D = D;
}
