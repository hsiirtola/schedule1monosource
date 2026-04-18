using UnityEngine;

namespace ScheduleOne.UI;

public class RectTransformLerpList : RectTransformLerp
{
	[SerializeField]
	private RectTransform[] _targetPositions;

	[SerializeField]
	private bool _scaleDurationWithDistance = true;

	[SerializeField]
	private bool _lerpLocalPosition = true;

	[SerializeField]
	private bool _lerpScale = true;

	private float _longestDistance;

	protected override void Awake()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		base.Awake();
		if (!_scaleDurationWithDistance)
		{
			return;
		}
		for (int i = 0; i < _targetPositions.Length; i++)
		{
			for (int j = 0; j < _targetPositions.Length; j++)
			{
				_longestDistance = Mathf.Max(Vector3.Distance(((Transform)_targetPositions[i]).localPosition, ((Transform)_targetPositions[j]).localPosition), _longestDistance);
			}
		}
	}

	public void LerpTo(int index, float duration)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		if (index < 0 || index >= _targetPositions.Length)
		{
			Debug.LogError((object)$"Index {index} is out of bounds for target positions.");
			return;
		}
		float duration2 = duration * GetDurationMultiplier(Vector2.op_Implicit(((Transform)_rectTransform).localPosition), Vector2.op_Implicit(((Transform)_targetPositions[index]).localPosition));
		if (_lerpLocalPosition)
		{
			LerpLocalPosition(((Transform)_targetPositions[index]).localPosition, duration2);
		}
		if (_lerpScale)
		{
			LerpLocalScale(((Transform)_targetPositions[index]).localScale, duration2);
		}
	}

	public void LerpTo(int index)
	{
		LerpTo(index, _defaultLerpDuration);
	}

	private float GetDurationMultiplier(Vector2 start, Vector2 end)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (!_scaleDurationWithDistance)
		{
			return 1f;
		}
		if (_longestDistance == 0f)
		{
			return 1f;
		}
		return Mathf.Clamp01(Vector3.Distance(Vector2.op_Implicit(start), Vector2.op_Implicit(end)) / _longestDistance);
	}
}
