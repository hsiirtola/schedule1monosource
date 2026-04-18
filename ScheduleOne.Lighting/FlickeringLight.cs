using UnityEngine;

namespace ScheduleOne.Lighting;

[RequireComponent(typeof(Light))]
public class FlickeringLight : MonoBehaviour
{
	[Header("Intensity Settings")]
	[Tooltip("The minimum light intensity.")]
	public float minIntensity = 0.8f;

	[Tooltip("The maximum light intensity.")]
	public float maxIntensity = 1.2f;

	[Header("Color Settings")]
	[Tooltip("Enable slight color shifts to simulate a warm flame.")]
	public bool enableColorShift = true;

	public Color minColor = new Color(1f, 0.8f, 0.6f);

	public Color maxColor = new Color(1f, 0.9f, 0.7f);

	[Header("Flicker Speed")]
	[Tooltip("How quickly the light flickers (lower is faster).")]
	public float flickerSpeed = 0.1f;

	private Light lightSource;

	private float targetIntensity;

	private Color targetColor;

	private void Start()
	{
		lightSource = ((Component)this).GetComponent<Light>();
		UpdateTargetValues();
	}

	private void Update()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		lightSource.intensity = Mathf.Lerp(lightSource.intensity, targetIntensity, flickerSpeed * Time.deltaTime);
		if (enableColorShift)
		{
			lightSource.color = Color.Lerp(lightSource.color, targetColor, flickerSpeed * Time.deltaTime);
		}
		if (Mathf.Abs(lightSource.intensity - targetIntensity) < 0.05f)
		{
			UpdateTargetValues();
		}
	}

	private void UpdateTargetValues()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		targetIntensity = Random.Range(minIntensity, maxIntensity);
		if (enableColorShift)
		{
			targetColor = Color.Lerp(minColor, maxColor, Random.value);
		}
	}
}
