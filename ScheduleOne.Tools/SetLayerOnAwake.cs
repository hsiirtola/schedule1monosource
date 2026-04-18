using UnityEngine;

namespace ScheduleOne.Tools;

public class SetLayerOnAwake : MonoBehaviour
{
	public LayerMask Layer;

	private void Awake()
	{
		((Component)this).gameObject.layer = ((LayerMask)(ref Layer)).value;
	}
}
