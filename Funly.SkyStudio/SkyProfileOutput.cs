using UnityEngine;

namespace Funly.SkyStudio;

public class SkyProfileOutput
{
	public Color ambientSkyColor;

	public Color ambientEquatorColor;

	public Color ambientGroundColor;

	public Color fogColor;

	public float fogEndDistance;

	public Color sunLightColor;

	public float sunLightIntensity;

	public SkyProfileOutput(SkyProfile skyProfile, float timeOfDay)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		ambientSkyColor = skyProfile.GetColorPropertyValue("AmbientLightSkyColorKey", timeOfDay);
		ambientEquatorColor = skyProfile.GetColorPropertyValue("AmbientLightEquatorColorKey", timeOfDay);
		ambientGroundColor = skyProfile.GetColorPropertyValue("AmbientLightGroundColorKey", timeOfDay);
		fogColor = skyProfile.GetColorPropertyValue("FogColorKey", timeOfDay);
		fogEndDistance = skyProfile.GetNumberPropertyValue("FogEndDistanceKey", timeOfDay);
		sunLightColor = skyProfile.GetColorPropertyValue("SunLightColorKey", timeOfDay);
		sunLightIntensity = skyProfile.GetNumberPropertyValue("SunLightIntensityKey", timeOfDay);
	}
}
