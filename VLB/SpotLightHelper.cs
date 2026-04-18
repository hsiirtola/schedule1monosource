using UnityEngine;

namespace VLB;

public static class SpotLightHelper
{
	public static float GetIntensity(Light light)
	{
		if (!((Object)(object)light != (Object)null))
		{
			return 0f;
		}
		return light.intensity;
	}

	public static float GetSpotAngle(Light light)
	{
		if (!((Object)(object)light != (Object)null))
		{
			return 0f;
		}
		return light.spotAngle;
	}

	public static float GetFallOffEnd(Light light)
	{
		if (!((Object)(object)light != (Object)null))
		{
			return 0f;
		}
		return light.range;
	}
}
