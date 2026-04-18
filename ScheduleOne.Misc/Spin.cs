using UnityEngine;

namespace ScheduleOne.Misc;

public class Spin : MonoBehaviour
{
	public Vector3 Axis;

	public float Speed;

	private void Update()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.Rotate(Axis, Speed * Time.deltaTime, (Space)1);
	}
}
