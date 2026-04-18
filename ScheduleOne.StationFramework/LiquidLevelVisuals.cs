using UnityEngine;

namespace ScheduleOne.StationFramework;

public class LiquidLevelVisuals : MonoBehaviour
{
	public LiquidContainer Container;

	public Transform LiquidSurface;

	public Transform LiquidSurface_Min;

	public Transform LiquidSurface_Max;

	private void Update()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)Container == (Object)null))
		{
			float num = Container.CurrentLiquidLevel / Container.MaxLevel;
			LiquidSurface.localPosition = Vector3.Lerp(LiquidSurface_Min.localPosition, LiquidSurface_Max.localPosition, num);
			LiquidSurface.localScale = new Vector3(LiquidSurface.localScale.x, num, LiquidSurface.localScale.z);
			((Component)LiquidSurface).gameObject.SetActive(Container.CurrentLiquidLevel > 0f);
		}
	}
}
