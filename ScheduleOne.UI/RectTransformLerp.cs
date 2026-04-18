using System.Collections;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.UI;

[RequireComponent(typeof(RectTransform))]
public class RectTransformLerp : MonoBehaviour
{
	[SerializeField]
	protected float _defaultLerpDuration = 0.5f;

	[SerializeField]
	private bool _lerpPosition = true;

	protected RectTransform _rectTransform;

	private Coroutine _positionRoutine;

	private Coroutine _scaleRoutine;

	protected virtual void Awake()
	{
		_rectTransform = ((Component)this).GetComponent<RectTransform>();
	}

	public void LerpLocalPosition(Vector3 endLocalPosition, float duration = -1f)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (duration < 0f)
		{
			duration = _defaultLerpDuration;
		}
		if (Singleton<CoroutineService>.InstanceExists)
		{
			if (_positionRoutine != null)
			{
				((MonoBehaviour)Singleton<CoroutineService>.Instance).StopCoroutine(_positionRoutine);
			}
			_positionRoutine = ((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			Vector2 startLocalPosition = Vector2.op_Implicit(((Transform)_rectTransform).localPosition);
			for (float i = 0f; i < duration; i += Time.unscaledDeltaTime)
			{
				((Transform)_rectTransform).localPosition = Vector2.op_Implicit(Vector2.Lerp(startLocalPosition, Vector2.op_Implicit(endLocalPosition), i / duration));
				yield return null;
			}
			((Transform)_rectTransform).localPosition = endLocalPosition;
		}
	}

	public void LerpLocalScale(Vector3 endLocalscale, float duration = -1f)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (duration < 0f)
		{
			duration = _defaultLerpDuration;
		}
		if (_scaleRoutine != null)
		{
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StopCoroutine(_scaleRoutine);
		}
		_scaleRoutine = ((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			Vector2 startLocalscale = Vector2.op_Implicit(((Transform)_rectTransform).localScale);
			for (float i = 0f; i < duration; i += Time.unscaledDeltaTime)
			{
				((Transform)_rectTransform).localScale = Vector2.op_Implicit(Vector2.Lerp(startLocalscale, Vector2.op_Implicit(endLocalscale), i / duration));
				yield return null;
			}
			((Transform)_rectTransform).localScale = endLocalscale;
		}
	}
}
