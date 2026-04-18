using System;
using UnityEngine;

namespace ScheduleOne.Tools;

[Serializable]
public class TransformLerp
{
	[SerializeField]
	private Transform _transform;

	[SerializeField]
	private Transform _min;

	[SerializeField]
	private Transform _max;

	[Header("Settings")]
	[SerializeField]
	private bool _lerpPosition = true;

	[SerializeField]
	private bool _lerpRotation = true;

	[SerializeField]
	private bool _lerpScale = true;

	[SerializeField]
	private bool _disableOnZero;

	private float _currentLerpValue;

	public void SetLerpValue(float lerpValue)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		_currentLerpValue = Mathf.Clamp01(lerpValue);
		if (_disableOnZero)
		{
			((Component)_transform).gameObject.SetActive(_currentLerpValue > 0f);
		}
		if (_lerpPosition)
		{
			_transform.localPosition = Vector3.Lerp(_min.localPosition, _max.localPosition, _currentLerpValue);
		}
		if (_lerpRotation)
		{
			_transform.localRotation = Quaternion.Lerp(_min.localRotation, _max.localRotation, _currentLerpValue);
		}
		if (_lerpScale)
		{
			_transform.localScale = Vector3.Lerp(_min.localScale, _max.localScale, _currentLerpValue);
		}
	}
}
