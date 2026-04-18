using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.ScriptableObjects;
using ScheduleOne.UI.Phone;
using UnityEngine;

namespace ScheduleOne.Calling;

public class CallManager : Singleton<CallManager>
{
	private PhoneCallData QueuedCallData { get; set; }

	public event Action<PhoneCallData> OnCallQueued;

	protected override void Start()
	{
		base.Start();
		if ((Object)(object)Singleton<CallInterface>.Instance == (Object)null)
		{
			Debug.LogError((object)"CallInterface instance is null. CallManager cannot function without it.");
			return;
		}
		CallInterface callInterface = Singleton<CallInterface>.Instance;
		callInterface.CallCompleted = (Action<PhoneCallData>)Delegate.Combine(callInterface.CallCompleted, new Action<PhoneCallData>(CallCompleted));
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((Object)(object)Singleton<CallInterface>.Instance != (Object)null)
		{
			CallInterface callInterface = Singleton<CallInterface>.Instance;
			callInterface.CallCompleted = (Action<PhoneCallData>)Delegate.Remove(callInterface.CallCompleted, new Action<PhoneCallData>(CallCompleted));
		}
	}

	public void QueueCall(PhoneCallData data)
	{
		QueuedCallData = data;
		this.OnCallQueued(data);
	}

	public void ClearQueuedCall()
	{
		QueuedCallData = null;
		this.OnCallQueued(null);
	}

	private void CallCompleted(PhoneCallData call)
	{
		if ((Object)(object)call == (Object)(object)QueuedCallData)
		{
			ClearQueuedCall();
		}
	}
}
