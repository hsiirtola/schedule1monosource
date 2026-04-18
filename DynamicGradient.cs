using System;
using UnityEngine;

[Serializable]
public class DynamicGradient
{
	public Gradient Gradient;

	[Range(0f, 2f)]
	[SerializeField]
	private float _saturationMultiplier = 1f;

	[Range(0f, 2f)]
	[SerializeField]
	private float _brightnessMultiplier = 1f;

	public Color Evaluate(float value)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		float num = default(float);
		float num2 = default(float);
		float num3 = default(float);
		Color.RGBToHSV(Gradient.Evaluate(value), ref num, ref num2, ref num3);
		num2 *= _saturationMultiplier;
		num3 *= _brightnessMultiplier;
		return Color.HSVToRGB(num, Mathf.Clamp01(num2), Mathf.Clamp01(num3));
	}
}
