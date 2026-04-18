using System;
using System.Collections;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.Interaction;
using ScheduleOne.Lighting;
using ScheduleOne.PlayerScripts;
using ScheduleOne.ScriptableObjects;
using ScheduleOne.UI.Phone;
using UnityEngine;

namespace ScheduleOne.Calling;

public class PayPhone : MonoBehaviour
{
	public const float RING_INTERVAL = 4f;

	public const float RING_RANGE = 9f;

	private const float ringRangeSquared = 81f;

	public PhoneCallData QueuedCall;

	public PhoneCallData ActiveCall;

	public BlinkingLight Light;

	public AudioSourceController RingSound;

	public AudioSourceController AnswerSound;

	public InteractableObject IntObj;

	public Transform CameraPosition;

	private float lastRingTime;

	private Coroutine periodicRingHandle;

	private void Start()
	{
		Singleton<CallManager>.Instance.OnCallQueued -= OnCallQueued;
		CallInterface instance = Singleton<CallInterface>.Instance;
		instance.CallStarted = (Action<PhoneCallData>)Delegate.Remove(instance.CallStarted, new Action<PhoneCallData>(OnCallStarted));
		CallInterface instance2 = Singleton<CallInterface>.Instance;
		instance2.CallCompleted = (Action<PhoneCallData>)Delegate.Remove(instance2.CallCompleted, new Action<PhoneCallData>(OnCallCompleted));
		Singleton<CallManager>.Instance.OnCallQueued += OnCallQueued;
		CallInterface instance3 = Singleton<CallInterface>.Instance;
		instance3.CallStarted = (Action<PhoneCallData>)Delegate.Combine(instance3.CallStarted, new Action<PhoneCallData>(OnCallStarted));
		CallInterface instance4 = Singleton<CallInterface>.Instance;
		instance4.CallCompleted = (Action<PhoneCallData>)Delegate.Combine(instance4.CallCompleted, new Action<PhoneCallData>(OnCallCompleted));
		lastRingTime = Time.timeSinceLevelLoad;
	}

	private void OnDestroy()
	{
		if (Singleton<CallManager>.InstanceExists)
		{
			Singleton<CallManager>.Instance.OnCallQueued -= OnCallQueued;
		}
		if (Singleton<CallInterface>.InstanceExists)
		{
			CallInterface instance = Singleton<CallInterface>.Instance;
			instance.CallStarted = (Action<PhoneCallData>)Delegate.Remove(instance.CallStarted, new Action<PhoneCallData>(OnCallStarted));
			CallInterface instance2 = Singleton<CallInterface>.Instance;
			instance2.CallCompleted = (Action<PhoneCallData>)Delegate.Remove(instance2.CallCompleted, new Action<PhoneCallData>(OnCallCompleted));
		}
	}

	private void OnCallStarted(PhoneCallData data)
	{
		ActiveCall = data;
		UpdateCallState();
	}

	private void OnCallCompleted(PhoneCallData data)
	{
		ActiveCall = null;
		UpdateCallState();
	}

	private void OnCallQueued(PhoneCallData data)
	{
		QueuedCall = data;
		UpdateCallState();
	}

	private void UpdateCallState()
	{
		bool flag = (Object)(object)QueuedCall != (Object)null && (Object)(object)ActiveCall == (Object)null;
		Light.IsOn = flag;
		if (!flag && periodicRingHandle != null)
		{
			((MonoBehaviour)this).StopCoroutine(periodicRingHandle);
			periodicRingHandle = null;
		}
		else if (flag && periodicRingHandle == null)
		{
			periodicRingHandle = ((MonoBehaviour)this).StartCoroutine(PeriodicRing());
		}
	}

	private IEnumerator PeriodicRing()
	{
		while (true)
		{
			yield return (object)new WaitForEndOfFrame();
			if (!(Time.timeSinceLevelLoad - lastRingTime < 4f) && !(Vector3.SqrMagnitude(((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position - ((Component)this).transform.position) >= 81f))
			{
				RingSound.Play();
				lastRingTime = Time.timeSinceLevelLoad;
			}
		}
	}

	public void Hovered()
	{
		if (CanInteract())
		{
			IntObj.SetMessage("Answer phone");
			IntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
		}
		else
		{
			IntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
		}
	}

	public void Interacted()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (CanInteract())
		{
			Singleton<CallInterface>.Instance.StartCall(QueuedCall, QueuedCall.CallerID);
			RingSound.Stop();
			AnswerSound.Play();
			PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(CameraPosition.position, CameraPosition.rotation, 0.2f);
			PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(60f, 0.2f);
		}
	}

	private bool CanInteract()
	{
		if ((Object)(object)QueuedCall == (Object)null)
		{
			return false;
		}
		if ((Object)(object)ActiveCall != (Object)null)
		{
			return false;
		}
		if (Singleton<CallInterface>.Instance.IsOpen)
		{
			return false;
		}
		return true;
	}
}
