using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Tools;

public class DemoBoundary : MonoBehaviour
{
	public Collider Collider;

	private void OnValidate()
	{
		if ((Object)(object)Collider == (Object)null)
		{
			Collider = ((Component)this).GetComponent<Collider>();
		}
	}

	private void Start()
	{
		((MonoBehaviour)this).InvokeRepeating("UpdateBoundary", 0f, 0.25f);
	}

	private void UpdateBoundary()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)Player.Local == (Object)null))
		{
			Vector3 val = ((Component)Collider).transform.InverseTransformPoint(((Component)Player.Local).transform.position);
			Collider.enabled = val.x > 0f;
		}
	}
}
