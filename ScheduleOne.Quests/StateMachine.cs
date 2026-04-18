using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Quests;

public class StateMachine : MonoBehaviour
{
	public static Action OnStateChange;

	private static bool stateChanged;

	private void Start()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		Singleton<LoadManager>.Instance.onPreSceneChange.AddListener(new UnityAction(Clean));
	}

	private void Update()
	{
		if (stateChanged)
		{
			OnStateChange?.Invoke();
			stateChanged = false;
		}
	}

	private void Clean()
	{
		Debug.Log((object)"Clearing state change...");
		OnStateChange = null;
	}

	public static void ChangeState()
	{
		stateChanged = true;
	}
}
