using UnityEngine;

namespace Funly.SkyStudio;

public class SkyProfileOverride : MonoBehaviour
{
	public SkyProfile SkyProfile;

	[Range(0f, 1f)]
	public float Strength;

	[Header("Masks")]
	public bool AffectAmbientLight = true;

	public bool AffectFog = true;

	public bool AffectSunLight = true;

	public void Apply(SkyProfileOutput output, float timeOfDay)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		if (!(Strength <= 0.0001f) && !((Object)(object)SkyProfile == (Object)null))
		{
			if (AffectAmbientLight)
			{
				output.ambientSkyColor = Color.Lerp(output.ambientSkyColor, SkyProfile.GetColorPropertyValue("AmbientLightSkyColorKey", timeOfDay), Strength);
				output.ambientEquatorColor = Color.Lerp(output.ambientEquatorColor, SkyProfile.GetColorPropertyValue("AmbientLightEquatorColorKey", timeOfDay), Strength);
				output.ambientGroundColor = Color.Lerp(output.ambientGroundColor, SkyProfile.GetColorPropertyValue("AmbientLightGroundColorKey", timeOfDay), Strength);
			}
			if (AffectFog)
			{
				output.fogColor = Color.Lerp(output.fogColor, SkyProfile.GetColorPropertyValue("FogColorKey", timeOfDay), Strength);
				output.fogEndDistance = Mathf.Lerp(output.fogEndDistance, SkyProfile.GetNumberPropertyValue("FogEndDistanceKey", timeOfDay), Strength);
			}
			if (AffectSunLight)
			{
				output.sunLightColor = Color.Lerp(output.sunLightColor, SkyProfile.GetColorPropertyValue("SunLightColorKey", timeOfDay), Strength);
				output.sunLightIntensity = Mathf.Lerp(output.sunLightIntensity, SkyProfile.GetNumberPropertyValue("SunLightIntensityKey", timeOfDay), Strength);
			}
		}
	}
}
