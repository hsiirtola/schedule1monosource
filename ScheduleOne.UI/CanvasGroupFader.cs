using System.Collections;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.UI;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasGroupFader : MonoBehaviour
{
	[SerializeField]
	private float _defaultFadeDuration = 0.5f;

	[SerializeField]
	private bool _scaleDurationWithFadeAmount = true;

	private CanvasGroup _canvasGroup;

	private Coroutine _fadeRoutine;

	private void Awake()
	{
		_canvasGroup = ((Component)this).GetComponent<CanvasGroup>();
	}

	public void FadeTo(float targetAlpha)
	{
		FadeTo(targetAlpha, _defaultFadeDuration);
	}

	public void FadeTo(float targetAlpha, float duration)
	{
		if (_fadeRoutine != null)
		{
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StopCoroutine(_fadeRoutine);
		}
		duration *= Mathf.Abs(targetAlpha - _canvasGroup.alpha);
		_fadeRoutine = ((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			float startAlpha = _canvasGroup.alpha;
			for (float i = 0f; i < duration; i += Time.unscaledDeltaTime)
			{
				_canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, i / duration);
				yield return null;
			}
			_canvasGroup.alpha = targetAlpha;
		}
	}
}
