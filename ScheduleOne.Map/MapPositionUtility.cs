using ScheduleOne.Core;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.Map;

public class MapPositionUtility : Singleton<MapPositionUtility>
{
	public Transform OriginPoint;

	public Transform EdgePoint;

	public float MapDimensions = 2048f;

	private float conversionFactor { get; set; }

	protected override void Awake()
	{
		base.Awake();
		Recalculate();
	}

	public Vector2 GetMapPosition(Vector3 worldPosition)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(worldPosition.x - OriginPoint.position.x, worldPosition.z - OriginPoint.position.z) * conversionFactor;
	}

	[Button]
	public void Recalculate()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		conversionFactor = MapDimensions * 0.5f / Vector3.Distance(OriginPoint.position, EdgePoint.position);
	}
}
