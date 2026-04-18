using UnityEngine;

namespace ScheduleOne.Skating;

[RequireComponent(typeof(Skateboard))]
public class SkateboardEffects : MonoBehaviour
{
	private Skateboard skateboard;

	[Header("References")]
	public TrailRenderer[] Trails;

	private float trailsOpacity;

	private void Awake()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		skateboard = ((Component)this).GetComponent<Skateboard>();
		trailsOpacity = Trails[0].startColor.a;
	}

	private void FixedUpdate()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		TrailRenderer[] trails = Trails;
		foreach (TrailRenderer obj in trails)
		{
			Color startColor = obj.startColor;
			startColor.a = trailsOpacity * Mathf.Clamp01(skateboard.CurrentSpeed_Kmh / skateboard.CurentSettings.TopSpeed_Kmh);
			obj.startColor = startColor;
		}
	}
}
