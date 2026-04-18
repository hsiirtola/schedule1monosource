using System.Linq;
using UnityEngine;

namespace ScheduleOne.Cartel;

public class CartelAmbushLocation : MonoBehaviour
{
	public const int REQUIRED_AMBUSH_POINTS = 4;

	[Header("Settings")]
	[Range(2f, 20f)]
	public float DetectionRadius = 10f;

	public Transform[] AmbushPoints;

	private void Awake()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (AmbushPoints == null || AmbushPoints.Count((Transform x) => (Object)(object)x != (Object)null) < 4)
		{
			Debug.LogError((object)$"CartelAmbushLocation at {((Component)this).transform.position} requires at least {4} ambush points, but found only {AmbushPoints.Count((Transform x) => (Object)(object)x != (Object)null)}.");
		}
	}

	private void OnDrawGizmos()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(((Component)this).transform.position, DetectionRadius);
		if (AmbushPoints == null)
		{
			return;
		}
		Transform[] ambushPoints = AmbushPoints;
		foreach (Transform val in ambushPoints)
		{
			if ((Object)(object)val != (Object)null)
			{
				Gizmos.color = Color.green;
				Gizmos.DrawSphere(val.position, 0.5f);
			}
		}
	}
}
