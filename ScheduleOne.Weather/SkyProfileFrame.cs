using Funly.SkyStudio;
using UnityEngine;

namespace ScheduleOne.Weather;

public class SkyProfileFrame
{
	public Color AmbientLightSkyColor;

	public Color AmbientLightEquatorColor;

	public Color AmbientLightGroundColor;

	public Color SkyUpperColor;

	public Color SkyMiddleColor;

	public Color SkyLowerColor;

	public float SkyMiddleColorPosition;

	public float HorizonTrasitionStart;

	public float HorizonTransitionLength;

	public float StarTransitionStart;

	public float StarTransitionLength;

	public float HorizonStarScale;

	public SkyProfileFrame(SkyProfile skyProfile, float timeOfDay)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		AmbientLightSkyColor = skyProfile.GetColorPropertyValue("AmbientLightSkyColorKey", timeOfDay);
		AmbientLightEquatorColor = skyProfile.GetColorPropertyValue("AmbientLightEquatorColorKey", timeOfDay);
		AmbientLightGroundColor = skyProfile.GetColorPropertyValue("AmbientLightGroundColorKey", timeOfDay);
		SkyUpperColor = skyProfile.GetColorPropertyValue("SkyUpperColorKey", timeOfDay);
		SkyMiddleColor = skyProfile.GetColorPropertyValue("SkyMiddleColorKey", timeOfDay);
		SkyLowerColor = skyProfile.GetColorPropertyValue("SkyLowerColorKey", timeOfDay);
		SkyMiddleColorPosition = skyProfile.GetNumberPropertyValue("SkyMiddleColorPosition", timeOfDay);
		HorizonTrasitionStart = skyProfile.GetNumberPropertyValue("HorizonTransitionStartKey", timeOfDay);
		HorizonTransitionLength = skyProfile.GetNumberPropertyValue("HorizonTransitionLengthKey", timeOfDay);
		StarTransitionStart = skyProfile.GetNumberPropertyValue("StarTransitionStartKey", timeOfDay);
	}
}
