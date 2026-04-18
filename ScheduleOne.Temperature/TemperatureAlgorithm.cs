using UnityEngine;

namespace ScheduleOne.Temperature;

public static class TemperatureAlgorithm
{
	public const float NegligibleInfluenceThreshold = 0.1111f;

	public static float GetTemperatureAtPoint(float ambientTemperature, Vector3 originPoint, Vector3 point, TemperatureEmitterInfo[] emitters)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		float num2 = 0f;
		for (int i = 0; i < emitters.Length; i++)
		{
			TemperatureEmitterInfo temperatureEmitterInfo = emitters[i];
			Vector3 val = temperatureEmitterInfo.Position - point;
			float sqrMagnitude = ((Vector3)(ref val)).sqrMagnitude;
			float num3 = Mathf.Max(1f - sqrMagnitude / temperatureEmitterInfo.SqrRange, 0f);
			num += temperatureEmitterInfo.Temperature * num3;
			num2 += num3;
		}
		float num4 = ((num2 > 0f) ? (num / num2) : 0f);
		if (num2 <= 1f)
		{
			num4 = Mathf.Lerp(ambientTemperature, num4, num2);
		}
		return num4;
	}
}
