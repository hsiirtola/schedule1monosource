using ScheduleOne.DevUtilities;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Persistence;

public class LoadEventTransmitter : MonoBehaviour
{
	public UnityEvent onLoadComplete;

	private void Start()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(OnLoadComplete));
	}

	private void OnLoadComplete()
	{
		if (onLoadComplete != null)
		{
			onLoadComplete.Invoke();
		}
	}
}
