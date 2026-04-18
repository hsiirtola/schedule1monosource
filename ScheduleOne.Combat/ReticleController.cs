using System.Collections;
using ScheduleOne.DevUtilities;
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.Combat;

public class ReticleController : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	private ReticleUI _reticleUI;

	[Header("Settings")]
	[SerializeField]
	private float _fadeDuration = 0.25f;

	private bool _isActive;

	private Coroutine _fadeCo;

	public bool IsActive => _isActive;

	private void Awake()
	{
		_reticleUI.Alpha = 0f;
		_isActive = false;
	}

	public void ShowReticle(float duration = -1f)
	{
		if (_fadeCo != null)
		{
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StopCoroutine(_fadeCo);
		}
		duration = ((duration != -1f) ? duration : _fadeDuration);
		_fadeCo = ((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(DoRecticleFade(1f, duration));
		_isActive = true;
	}

	public void HideReticle(float duration = -1f)
	{
		if (_fadeCo != null)
		{
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StopCoroutine(_fadeCo);
		}
		duration = ((duration != -1f) ? duration : _fadeDuration);
		_fadeCo = ((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(DoRecticleFade(0f, duration));
		_isActive = false;
	}

	public void SetReticle(float spreadAngle)
	{
		if (_isActive)
		{
			_reticleUI.Set(spreadAngle);
		}
	}

	private IEnumerator DoRecticleFade(float endAlpha, float duration)
	{
		float startAlpha = _reticleUI.Alpha;
		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float num = Mathf.Clamp01(elapsed / duration);
			_reticleUI.Alpha = Mathf.Lerp(startAlpha, endAlpha, num);
			yield return null;
		}
		_reticleUI.Alpha = endAlpha;
		_fadeCo = null;
	}
}
