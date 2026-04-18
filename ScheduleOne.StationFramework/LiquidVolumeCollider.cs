using UnityEngine;

namespace ScheduleOne.StationFramework;

public class LiquidVolumeCollider : MonoBehaviour
{
	public LiquidContainer LiquidContainer;

	private void Awake()
	{
		if ((Object)(object)LiquidContainer == (Object)null)
		{
			LiquidContainer = ((Component)this).GetComponentInParent<LiquidContainer>();
		}
	}
}
