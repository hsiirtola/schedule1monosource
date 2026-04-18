using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.Weather;

public class BasicEnclosure : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField]
	private Vector3 _center;

	[SerializeField]
	private Vector3 _size;

	[Header("Blend Zone Settings")]
	[SerializeField]
	private bool _isBlendZone;

	[SerializeField]
	private float _backRadius = 1f;

	[SerializeField]
	private float _frontRadius = 1f;

	[SerializeField]
	private AnimationCurve _blendCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[Header("Debug")]
	[SerializeField]
	private bool _debugMode;

	[SerializeField]
	private bool _debugShowFrontAndBackSeparately;

	[SerializeField]
	private GameObject _debugObject;

	private Vector3 _debugClosestPoint;

	private Vector3 _debugOppositePoint;

	private float _debugBlendValue;

	private float _debugActiveRadius;

	public Vector3 StartPoint => ((Component)this).transform.TransformPoint(_center) - ((Component)this).transform.forward * (_size.z / 2f);

	public Vector3 EndPoint => ((Component)this).transform.TransformPoint(_center) + ((Component)this).transform.forward * (_size.z / 2f);

	public bool IsBlendZone => _isBlendZone;

	private void Awake()
	{
	}

	private void Update()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_debugObject == (Object)null))
		{
			bool flag = WithinEnclosure(_debugObject.transform.position);
			if (flag)
			{
				GetEnclosureBlend(_debugObject.transform.position);
			}
			Debug.Log((object)$"Within Enclosure: {flag}");
		}
	}

	public bool WithinEnclosure(Vector3 targetPosition)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)this).transform.InverseTransformPoint(targetPosition) - GetCenter();
		Vector3 size = GetSize();
		if (Mathf.Abs(val.x) <= size.x / 2f && Mathf.Abs(val.y) <= size.y / 2f)
		{
			return Mathf.Abs(val.z) <= size.z / 2f;
		}
		return false;
	}

	public float GetEnclosureBlend(Vector3 targetPosition)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		if (!_isBlendZone)
		{
			return 1f;
		}
		bool flag = ((Component)this).transform.InverseTransformPoint(targetPosition).z < _center.z;
		Vector3 closestPointOnZFaces = GetClosestPointOnZFaces(targetPosition);
		Vector3 oppositeFacePoint = GetOppositeFacePoint(closestPointOnZFaces);
		float normalizedPositionAlongSegment = MathUtility.GetNormalizedPositionAlongSegment(closestPointOnZFaces, oppositeFacePoint, targetPosition);
		float num = (flag ? normalizedPositionAlongSegment : (1f - normalizedPositionAlongSegment));
		float num2 = _frontRadius + _size.z + _backRadius;
		float num3 = 0f;
		if (num >= 1f || num <= 0f)
		{
			float num4 = Vector3.Distance(closestPointOnZFaces, targetPosition);
			num3 = ((!flag) ? ((_frontRadius + _size.z + num4) / num2) : (Mathf.Max(_frontRadius - num4, 0f) / num2));
		}
		else
		{
			num3 = (_frontRadius + num * _size.z) / num2;
		}
		return _blendCurve.Evaluate(Mathf.Clamp01(num3));
	}

	public Vector3 GetClosestPointOnZFaces(Vector3 targetPosition)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)this).transform.InverseTransformPoint(targetPosition);
		Vector3 val2 = _size * 0.5f;
		float num = Mathf.Clamp(val.x - _center.x, 0f - val2.x, val2.x);
		float num2 = Mathf.Clamp(val.y - _center.y, 0f - val2.y, val2.y);
		float num3 = ((val.z - _center.z >= 0f) ? val2.z : (0f - val2.z));
		return ((Component)this).transform.TransformPoint(new Vector3(num + _center.x, num2 + _center.y, num3 + _center.z));
	}

	public Vector3 GetOppositeFacePoint(Vector3 surfacePoint)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)this).transform.InverseTransformPoint(surfacePoint) - _center;
		_ = _size;
		float num = _size.y * 0.5f;
		float num2 = _size.z * 0.5f;
		if (Mathf.Abs(Mathf.Abs(val.z) - num2) < 0.0001f)
		{
			val.z = 0f - val.z;
		}
		else if (Mathf.Abs(Mathf.Abs(val.y) - num) < 0.0001f)
		{
			val.y = 0f - val.y;
		}
		else
		{
			val.x = 0f - val.x;
		}
		return ((Component)this).transform.TransformPoint(val + _center);
	}

	protected Vector3 GetSize()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Max(_backRadius, _frontRadius);
		if (!_isBlendZone)
		{
			return _size;
		}
		return new Vector3(_size.x + num * 2f, _size.y + num * 2f, _size.z + _backRadius + _frontRadius);
	}

	protected Vector3 GetCenter()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (!_isBlendZone)
		{
			return _center;
		}
		return _center + Vector3.forward * ((_backRadius - _frontRadius) / 2f);
	}
}
