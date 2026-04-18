using System;
using UnityEngine;

namespace ScheduleOne.Vision;

public class VisionEvent
{
	private const float NOTICE_DROP_THRESHOLD = 1f;

	private float timeSinceSighted;

	private float currentNoticeTime;

	public bool playTremolo = true;

	public ISightable Target { get; protected set; }

	public EntityVisualState State { get; protected set; }

	public VisionCone Owner { get; protected set; }

	public float FullNoticeTime { get; protected set; }

	public float NormalizedNoticeLevel => currentNoticeTime / FullNoticeTime;

	public VisionEvent(VisionCone _owner, ISightable _target, EntityVisualState _state, float _noticeTime, bool _playTremolo)
	{
		Owner = _owner;
		Target = _target;
		State = _state;
		FullNoticeTime = _noticeTime;
		playTremolo = _playTremolo;
		EntityVisualState state = State;
		state.stateDestroyed = (Action)Delegate.Combine(state.stateDestroyed, new Action(EndEvent));
	}

	public void UpdateEvent(float visionDeltaThisFrame, float tickTime)
	{
		float normalizedNoticeLevel = NormalizedNoticeLevel;
		if (visionDeltaThisFrame > 0f)
		{
			timeSinceSighted = 0f;
		}
		else
		{
			timeSinceSighted += tickTime;
		}
		if (visionDeltaThisFrame > 0f)
		{
			currentNoticeTime += visionDeltaThisFrame * (Owner.Attentiveness * VisionCone.UniversalAttentivenessScale) * tickTime;
		}
		else if (timeSinceSighted > 1f * (Owner.Memory * VisionCone.UniversalMemoryScale))
		{
			currentNoticeTime -= tickTime / (Owner.Memory * VisionCone.UniversalMemoryScale);
		}
		currentNoticeTime = Mathf.Clamp(currentNoticeTime, 0f, FullNoticeTime);
		if (Target.HighestProgressionEvent == null || NormalizedNoticeLevel > Target.HighestProgressionEvent.NormalizedNoticeLevel)
		{
			Target.HighestProgressionEvent = this;
		}
		if (NormalizedNoticeLevel <= 0f && normalizedNoticeLevel > 0f)
		{
			EndEvent();
		}
		if (NormalizedNoticeLevel >= 0.5f && normalizedNoticeLevel < 0.5f)
		{
			if (Target.HighestProgressionEvent == this)
			{
				Target.HighestProgressionEvent = null;
			}
			Owner.EventHalfNoticed(this);
		}
		if (NormalizedNoticeLevel >= 1f && normalizedNoticeLevel < 1f)
		{
			if (Target.HighestProgressionEvent == this)
			{
				Target.HighestProgressionEvent = null;
			}
			Owner.EventFullyNoticed(this);
		}
	}

	public void EndEvent()
	{
		if (Target.HighestProgressionEvent == this)
		{
			Target.HighestProgressionEvent = null;
		}
		Owner.EventReachedZero(this);
	}
}
