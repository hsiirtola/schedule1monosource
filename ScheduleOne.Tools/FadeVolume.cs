using ScheduleOne.DevUtilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace ScheduleOne.Tools;

public class FadeVolume : MonoBehaviour
{
	[SerializeField]
	[FormerlySerializedAs("StartPoint")]
	private Transform _startPoint;

	[SerializeField]
	[FormerlySerializedAs("EndPoint")]
	private Transform _endPoint;

	[SerializeField]
	[FormerlySerializedAs("BoxCollider")]
	private BoxCollider _boxCollider;

	private void Awake()
	{
	}

	private void OnDrawGizmos()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Debug.DrawLine(_startPoint.position, _endPoint.position, Color.cyan);
	}

	public float GetPositionScalar(Vector3 point)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if (!_boxCollider.IsPointWithinCollider(point))
		{
			return 0f;
		}
		Vector3 val = _endPoint.position - _startPoint.position;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		float num = Vector3.Dot(point - _startPoint.position, normalized);
		float num2 = Vector3.Distance(_startPoint.position, _endPoint.position);
		return Mathf.Clamp01(num / num2);
	}
}
