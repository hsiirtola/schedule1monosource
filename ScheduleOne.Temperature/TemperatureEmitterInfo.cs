using UnityEngine;

namespace ScheduleOne.Temperature;

public struct TemperatureEmitterInfo
{
	public float Temperature;

	public float SqrRange;

	public Vector3 Position;

	public static int SizeOf => 20;

	public TemperatureEmitterInfo(float temperature, float sqrRange, Vector3 position)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Temperature = temperature;
		SqrRange = sqrRange;
		Position = position;
	}
}
