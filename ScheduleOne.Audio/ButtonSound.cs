using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ScheduleOne.Audio;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(EventTrigger))]
[RequireComponent(typeof(AudioSourceController))]
public class ButtonSound : MonoBehaviour
{
	[SerializeField]
	[FormerlySerializedAs("PlaySoundOnClickStart")]
	private bool _playSoundOnClickStart;

	[SerializeField]
	[FormerlySerializedAs("HoverClip")]
	private AudioClip _hoverClip;

	[SerializeField]
	[FormerlySerializedAs("HoverSoundVolume")]
	private float _hoverVolume = 1f;

	[SerializeField]
	[FormerlySerializedAs("ClickClip")]
	private AudioClip _clickClip;

	[SerializeField]
	[FormerlySerializedAs("ClickSoundVolume")]
	private float _clickVolume = 1f;

	private AudioSourceController _audioSource;

	private Button _button;

	private EventTrigger _eventTrigger;

	public void Awake()
	{
		_eventTrigger = ((Component)this).GetComponent<EventTrigger>();
		_button = ((Component)this).GetComponent<Button>();
		_audioSource = ((Component)this).GetComponent<AudioSourceController>();
		AddEventTrigger(_eventTrigger, (EventTriggerType)0, Hovered);
		AddEventTrigger(_eventTrigger, (EventTriggerType)9, Hovered);
		if (_playSoundOnClickStart)
		{
			AddEventTrigger(_eventTrigger, (EventTriggerType)2, Clicked);
		}
		else
		{
			AddEventTrigger(_eventTrigger, (EventTriggerType)4, Clicked);
		}
	}

	public void AddEventTrigger(EventTrigger eventTrigger, EventTriggerType eventTriggerType, Action action)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		Entry val = new Entry();
		val.eventID = eventTriggerType;
		((UnityEvent<BaseEventData>)(object)val.callback).AddListener((UnityAction<BaseEventData>)delegate
		{
			action();
		});
		eventTrigger.triggers.Add(val);
	}

	protected virtual void Hovered()
	{
		if (((Selectable)_button).interactable)
		{
			if (_audioSource.IsPlaying)
			{
				_audioSource.Stop();
			}
			_audioSource.VolumeMultiplier = _hoverVolume;
			_audioSource.SetClip(_hoverClip);
			_audioSource.PitchMultiplier = 0.9f;
			_audioSource.Play();
		}
	}

	protected virtual void Clicked()
	{
		if (((Selectable)_button).interactable)
		{
			_audioSource.VolumeMultiplier = _clickVolume;
			_audioSource.SetClip(_clickClip);
			_audioSource.Play();
		}
	}
}
