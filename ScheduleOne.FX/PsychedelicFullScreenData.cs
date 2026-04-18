using UnityEngine;

namespace ScheduleOne.FX;

[CreateAssetMenu(fileName = "PsychedelicFullScreenData", menuName = "ScriptableObjects/FX/Psychedelic FullScreen Data", order = 1)]
public class PsychedelicFullScreenData : ScriptableObject
{
	[Header("Properties")]
	public float NoiseScale = 15f;

	public float Blend = 0.016f;

	public Vector2 PanSpeed = new Vector2(0.05f, 0.05f);

	public bool DoesBounce;

	public float Amplitude = 0.19f;

	public PsychedelicFullScreenFeature.MaterialProperties ConvertToMaterialProperties()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		return new PsychedelicFullScreenFeature.MaterialProperties
		{
			NoiseScale = NoiseScale,
			Blend = Blend,
			PanSpeed = PanSpeed,
			DoesBounce = DoesBounce,
			Amplitude = Amplitude
		};
	}
}
