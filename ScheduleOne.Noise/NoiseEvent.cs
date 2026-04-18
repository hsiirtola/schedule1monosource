using ScheduleOne.DevUtilities;
using ScheduleOne.Map;
using UnityEngine;

namespace ScheduleOne.Noise;

public class NoiseEvent
{
	public Vector3 origin;

	public float range;

	public ENoiseType type;

	public GameObject source;

	public bool OriginInSewer { get; private set; }

	public NoiseEvent(Vector3 _origin, float _range, ENoiseType _type, GameObject _source = null)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		origin = _origin;
		range = _range;
		type = _type;
		source = _source;
		OriginInSewer = Singleton<SewerCameraPresense>.InstanceExists && Singleton<SewerCameraPresense>.Instance.IsPointInSewerArea(origin);
	}
}
