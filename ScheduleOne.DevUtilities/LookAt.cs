using UnityEngine;

namespace ScheduleOne.DevUtilities;

public class LookAt : MonoBehaviour
{
	public Transform Target;

	private void LateUpdate()
	{
		if ((Object)(object)Target != (Object)null)
		{
			((Component)this).transform.LookAt(Target);
		}
	}
}
