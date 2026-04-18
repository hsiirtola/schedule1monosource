using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace ScheduleOne.Audio;

public class StartLoopStopAudio : MonoBehaviour
{
	[FormerlySerializedAs("FadeLoopIn")]
	[SerializeField]
	private bool _fadeLoopIn;

	[FormerlySerializedAs("FadeLoopOut")]
	[SerializeField]
	private bool _fadeLoopOut;

	[FormerlySerializedAs("StartSound")]
	[SerializeField]
	private AudioSourceController _startSound;

	[FormerlySerializedAs("LoopSound")]
	[SerializeField]
	private AudioSourceController _loopSound;

	[FormerlySerializedAs("StopSound")]
	[SerializeField]
	private AudioSourceController _stopSound;

	private Coroutine _audioRoutine;

	private bool _isRunning;

	private void Awake()
	{
	}

	public void StartAudio()
	{
		if (!_isRunning)
		{
			_isRunning = true;
			TryStartAudio();
		}
	}

	public void StopAudio()
	{
		if (_isRunning)
		{
			_isRunning = false;
			TryStopAudio();
		}
	}

	private IEnumerator StartAudioRoutine()
	{
		float timer = 0f;
		if (!_fadeLoopIn)
		{
			_loopSound.VolumeMultiplier = 1f;
			yield break;
		}
		float duration = _startSound.Clip.length;
		if (duration <= 0f)
		{
			_loopSound.VolumeMultiplier = 1f;
			yield break;
		}
		while (timer < duration)
		{
			timer += Time.deltaTime;
			_loopSound.VolumeMultiplier = Mathf.Lerp(0f, 1f, timer / duration);
			yield return null;
		}
		_loopSound.VolumeMultiplier = 1f;
		_audioRoutine = null;
	}

	private IEnumerator StopAudioRoutine()
	{
		float timer = 0f;
		if (!_fadeLoopOut)
		{
			_loopSound.VolumeMultiplier = 0f;
			if (_loopSound.IsPlaying)
			{
				_loopSound.Stop();
			}
			yield break;
		}
		float duration = _stopSound.Clip.length;
		if (duration <= 0f)
		{
			_loopSound.VolumeMultiplier = 0f;
			if (_loopSound.IsPlaying)
			{
				_loopSound.Stop();
			}
			yield break;
		}
		while (timer < duration)
		{
			timer += Time.deltaTime;
			_loopSound.VolumeMultiplier = Mathf.Lerp(1f, 0f, timer / duration);
			yield return null;
		}
		_loopSound.VolumeMultiplier = 0f;
		if (_loopSound.IsPlaying)
		{
			_loopSound.Stop();
		}
		_audioRoutine = null;
	}

	private void TryStartAudio()
	{
		_loopSound.Play();
		_loopSound.SetLoop(loop: true);
		_startSound.Play();
		if (_audioRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(_audioRoutine);
			_audioRoutine = null;
		}
		_audioRoutine = ((MonoBehaviour)this).StartCoroutine(StartAudioRoutine());
	}

	private void TryStopAudio()
	{
		_startSound.Stop();
		_stopSound.Play();
		if (_audioRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(_audioRoutine);
			_audioRoutine = null;
		}
		_audioRoutine = ((MonoBehaviour)this).StartCoroutine(StopAudioRoutine());
	}
}
