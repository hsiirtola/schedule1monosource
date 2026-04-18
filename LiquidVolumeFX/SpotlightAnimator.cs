using UnityEngine;

namespace LiquidVolumeFX;

public class SpotlightAnimator : MonoBehaviour
{
	public float lightOnDelay = 2f;

	public float targetIntensity = 3.5f;

	public float initialIntensity;

	public float duration = 3f;

	public float nextColorInterval = 2f;

	public float colorChangeDuration = 2f;

	private Light spotLight;

	private float lastColorChange;

	private float colorChangeStarted;

	private Color currentColor;

	private Color nextColor;

	private bool changingColor;

	private void Awake()
	{
		spotLight = ((Component)this).GetComponent<Light>();
		spotLight.intensity = 0f;
	}

	private void Update()
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		if (Time.time < lightOnDelay)
		{
			return;
		}
		float num = (Time.time - lightOnDelay) / duration;
		spotLight.intensity = Mathf.Lerp(initialIntensity, targetIntensity, num);
		if (!(Time.time - lastColorChange > nextColorInterval))
		{
			return;
		}
		if (changingColor)
		{
			num = (Time.time - colorChangeStarted) / colorChangeDuration;
			if (num >= 1f)
			{
				changingColor = false;
				lastColorChange = Time.time;
			}
			spotLight.color = Color.Lerp(currentColor, nextColor, num);
		}
		else
		{
			currentColor = spotLight.color;
			nextColor = new Color(Mathf.Clamp01(Random.value + 0.25f), Mathf.Clamp01(Random.value + 0.25f), Mathf.Clamp01(Random.value + 0.25f), 1f);
			changingColor = true;
			colorChangeStarted = Time.time;
		}
	}
}
