using System;
using UnityEngine;

namespace ScheduleOne;

public class UIInputDetectBehaviour : MonoBehaviour
{
	private float initialHoldThreshold = 0.5f;

	private float repeatInterval = 0.25f;

	private float timer;

	private bool wasPressedLastFrame;

	private Action<float> onAction;

	public void Initialize(Action<float> action, float holdThreshold, float repeat)
	{
		onAction = action;
		initialHoldThreshold = holdThreshold;
		repeatInterval = repeat;
	}

	public void ResetData()
	{
		timer = 0f;
		wasPressedLastFrame = false;
	}

	public void DoUpdate(float value)
	{
		bool flag = value != 0f;
		if (flag)
		{
			if (!wasPressedLastFrame)
			{
				onAction?.Invoke(value);
				timer = initialHoldThreshold;
			}
			else
			{
				timer -= Time.unscaledDeltaTime;
				if (timer <= 0f)
				{
					onAction?.Invoke(value);
					timer += repeatInterval;
				}
			}
		}
		else
		{
			timer = 0f;
		}
		wasPressedLastFrame = flag;
	}
}
