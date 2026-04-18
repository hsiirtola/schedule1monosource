using Funly.SkyStudio;
using UnityEngine;

public class ModifySkyAtRuntime : MonoBehaviour
{
	[Range(0f, 1f)]
	public float speed = 0.15f;

	private void Update()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		SkyProfile skyProfile = TimeOfDayController.instance.skyProfile;
		ColorKeyframe colorKeyframe = skyProfile.GetGroup<ColorKeyframeGroup>("SkyMiddleColorKey").keyframes[0];
		float num = Time.timeSinceLevelLoad * speed % 1f;
		colorKeyframe.color = Color.HSVToRGB(num, 0.8f, 0.8f);
		skyProfile.GetGroup<ColorKeyframeGroup>("SkyUpperColorKey").keyframes[0].color = colorKeyframe.color;
		TimeOfDayController.instance.UpdateSkyForCurrentTime();
	}
}
