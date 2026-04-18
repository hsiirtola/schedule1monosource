using System;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Object;
using ScheduleOne.Core;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Law;
using ScheduleOne.Networking;
using ScheduleOne.NPCs.Other;
using ScheduleOne.NPCs.Schedules;
using ScheduleOne.Persistence;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;

namespace ScheduleOne.NPCs;

public class NPCScheduleManager : MonoBehaviour
{
	private static readonly NPCActionOrderByDescending orderByDescending = new NPCActionOrderByDescending();

	public bool DEBUG_MODE;

	[Header("References")]
	public GameObject[] EnabledDuringCurfew;

	public GameObject[] EnabledDuringNoCurfew;

	public List<NPCAction> ActionList = new List<NPCAction>();

	[Header("Discrete Actions")]
	[SerializeField]
	private List<NPCDiscreteAction> discreteActions = new List<NPCDiscreteAction>();

	protected int lastProcessedTime;

	public bool ScheduleEnabled { get; protected set; }

	public bool CurfewModeEnabled { get; protected set; }

	public NPCAction ActiveAction { get; set; }

	public List<NPCAction> PendingActions { get; set; } = new List<NPCAction>();

	public NPC Npc { get; protected set; }

	public List<NPCDiscreteAction> DiscreteActions => discreteActions;

	protected List<NPCAction> ActionsAwaitingStart { get; set; } = new List<NPCAction>();

	protected TimeManager Time => NetworkSingleton<TimeManager>.Instance;

	protected virtual void Awake()
	{
		Npc = ((Component)this).GetComponentInParent<NPC>();
		SetCurfewModeEnabled(enabled: false);
	}

	protected virtual void Start()
	{
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Expected O, but got Unknown
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Expected O, but got Unknown
		InitializeActions();
		TimeManager time = Time;
		time.onTimeSet = (Action)Delegate.Remove(time.onTimeSet, new Action(EnforceState));
		TimeManager time2 = Time;
		time2.onTimeSet = (Action)Delegate.Combine(time2.onTimeSet, new Action(EnforceState));
		Time.onTick -= new Action(OnTick);
		Time.onTick += new Action(OnTick);
		Time.onMinutePass -= new Action(OnMinPass);
		Time.onMinutePass += new Action(OnMinPass);
		Player.onLocalPlayerSpawned = (Action)Delegate.Remove(Player.onLocalPlayerSpawned, new Action(LocalPlayerSpawned));
		Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, new Action(LocalPlayerSpawned));
		if (NetworkSingleton<CurfewManager>.InstanceExists)
		{
			NetworkSingleton<CurfewManager>.Instance.onCurfewEnabled.AddListener(new UnityAction(CurfewEnabled));
			NetworkSingleton<CurfewManager>.Instance.onCurfewDisabled.AddListener(new UnityAction(CurfewDisabled));
		}
		if (DEBUG_MODE)
		{
			int min = 1250;
			int max = 930;
			GetActionsTotallyOccurringWithinRange(min, max, checkShouldStart: true);
		}
	}

	private void LocalPlayerSpawned()
	{
		if (InstanceFinder.IsServer)
		{
			EnforceState(initial: true);
		}
	}

	private void OnValidate()
	{
		_ = Application.isPlaying;
	}

	protected virtual void Update()
	{
		if ((Object)(object)ActiveAction != (Object)null)
		{
			ActiveAction.ActiveUpdate();
		}
	}

	public void EnableSchedule()
	{
		ScheduleEnabled = true;
		OnTick();
		OnMinPass();
	}

	public void DisableSchedule()
	{
		ScheduleEnabled = false;
		OnTick();
		OnMinPass();
		if (Npc.Movement.IsMoving)
		{
			Npc.Movement.Stop();
		}
	}

	[Button]
	public void InitializeActions()
	{
		List<NPCAction> list = ((Component)this).gameObject.GetComponentsInChildren<NPCAction>(true).ToList();
		list.Sort(delegate(NPCAction a, NPCAction b)
		{
			float num = a.StartTime;
			float value = b.StartTime;
			int num2 = num.CompareTo(value);
			return (num2 == 0) ? ((!a.IsSignal) ? 1 : (-1)) : num2;
		});
		if (!Application.isPlaying)
		{
			foreach (NPCAction item in list)
			{
				((Object)((Component)item).transform).name = item.GetName() + " (" + item.GetTimeDescription() + ")";
				((Component)item).transform.SetAsLastSibling();
			}
		}
		ActionList = list;
	}

	protected virtual void OnMinPass()
	{
		if ((InstanceFinder.IsServer || NetworkSingleton<ReplicationQueue>.Instance.ReplicationDoneForLocalPlayer) && ((NetworkBehaviour)Npc).IsSpawned && (Object)(object)ActiveAction != (Object)null)
		{
			ActiveAction.OnActiveMinPass();
		}
	}

	private void UpdateActions()
	{
		if (!ScheduleEnabled)
		{
			if ((Object)(object)ActiveAction != (Object)null)
			{
				ActiveAction.Interrupt();
			}
			return;
		}
		if ((Object)(object)ActiveAction != (Object)null)
		{
			ActiveAction.OnActiveTick();
		}
		if ((Object)(object)ActiveAction != (Object)null && !((Component)ActiveAction).gameObject.activeInHierarchy)
		{
			ActiveAction.End();
		}
		List<NPCAction> actionsOccurringAt = GetActionsOccurringAt(NetworkSingleton<TimeManager>.Instance.CurrentTime);
		if (DEBUG_MODE)
		{
			Debug.Log((object)("Actions occurring at: " + NetworkSingleton<TimeManager>.Instance.CurrentTime + ": " + actionsOccurringAt.Count));
			for (int i = 0; i < actionsOccurringAt.Count; i++)
			{
				Debug.Log((object)(i + ": " + actionsOccurringAt[i].GetName()));
			}
		}
		if (actionsOccurringAt.Count > 0)
		{
			NPCAction nPCAction = actionsOccurringAt[0];
			if ((Object)(object)ActiveAction != (Object)(object)nPCAction)
			{
				if ((Object)(object)ActiveAction != (Object)null && nPCAction.Priority > ActiveAction.Priority)
				{
					if (DEBUG_MODE)
					{
						Debug.Log((object)("New active action: " + nPCAction.GetName()));
					}
					ActiveAction.Interrupt();
				}
				if ((Object)(object)ActiveAction == (Object)null)
				{
					StartAction(nPCAction);
				}
			}
		}
		foreach (NPCAction item in actionsOccurringAt)
		{
			if (!item.HasStarted && !ActionsAwaitingStart.Contains(item))
			{
				ActionsAwaitingStart.Add(item);
			}
		}
		foreach (NPCAction item2 in ActionsAwaitingStart.ToList())
		{
			if (!NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(item2.StartTime, item2.GetEndTime()))
			{
				item2.Skipped();
				ActionsAwaitingStart.Remove(item2);
			}
		}
		lastProcessedTime = Time.CurrentTime;
		if (DEBUG_MODE)
		{
			Console.Log("Active action: " + (((Object)(object)ActiveAction != (Object)null) ? ActiveAction.GetName() : "None"));
		}
		CollectionPool<List<NPCAction>, NPCAction>.Release(actionsOccurringAt);
	}

	protected virtual void OnTick()
	{
		UpdateActions();
	}

	private List<NPCAction> GetActionsOccurringAt(int time)
	{
		List<NPCAction> list = CollectionPool<List<NPCAction>, NPCAction>.Get();
		foreach (NPCAction action in ActionList)
		{
			if (!((Object)(object)action == (Object)null) && action.ShouldStart() && TimeManager.IsGivenTimeWithinRange(time, action.StartTime, TimeManager.AddMinutesTo24HourTime(action.GetEndTime(), -1)))
			{
				list.Add(action);
			}
		}
		list.Sort(orderByDescending);
		return list;
	}

	private List<NPCAction> GetActionsTotallyOccurringWithinRange(int min, int max, bool checkShouldStart)
	{
		List<NPCAction> list = CollectionPool<List<NPCAction>, NPCAction>.Get();
		foreach (NPCAction action in ActionList)
		{
			if ((!checkShouldStart || action.ShouldStart()) && TimeManager.IsGivenTimeWithinRange(action.StartTime, min, max) && TimeManager.IsGivenTimeWithinRange(action.GetEndTime(), min, max))
			{
				list.Add(action);
			}
		}
		list.Sort(orderByDescending);
		_ = DEBUG_MODE;
		return list;
	}

	private void StartAction(NPCAction action)
	{
		if ((Object)(object)ActiveAction != (Object)null)
		{
			Console.LogWarning("JumpToAction called but there is already an active action! Existing action should first be ended or interrupted!");
		}
		if (ActionsAwaitingStart.Contains(action))
		{
			ActionsAwaitingStart.Remove(action);
		}
		if (NetworkSingleton<TimeManager>.Instance.CurrentTime == action.StartTime)
		{
			action.Started();
		}
		else if (action.HasStarted)
		{
			action.Resume();
		}
		else
		{
			action.LateStarted();
		}
	}

	private void EnforceState()
	{
		EnforceState(Singleton<LoadManager>.Instance.IsLoading);
	}

	public void EnforceState(bool initial = false)
	{
		ActionsAwaitingStart.Clear();
		int currentTime = NetworkSingleton<TimeManager>.Instance.CurrentTime;
		int minSumFrom24HourTime = TimeManager.GetMinSumFrom24HourTime(currentTime);
		if (DEBUG_MODE)
		{
			Debug.Log((object)("Enforcing state. Last processed time: " + lastProcessedTime + ", Current time: " + NetworkSingleton<TimeManager>.Instance.CurrentTime));
		}
		List<NPCAction> actionsTotallyOccurringWithinRange = GetActionsTotallyOccurringWithinRange(lastProcessedTime, NetworkSingleton<TimeManager>.Instance.CurrentTime, checkShouldStart: true);
		List<NPCAction> actionsOccurringThisFrame = GetActionsOccurringAt(NetworkSingleton<TimeManager>.Instance.CurrentTime);
		actionsTotallyOccurringWithinRange.RemoveAll((NPCAction x) => x.IsActive || actionsOccurringThisFrame.Contains(x));
		NPCAction nPCAction = null;
		if (actionsOccurringThisFrame.Count > 0 && ScheduleEnabled)
		{
			nPCAction = actionsOccurringThisFrame[0];
		}
		if ((Object)(object)ActiveAction != (Object)null && (Object)(object)ActiveAction != (Object)(object)nPCAction)
		{
			ActiveAction.Interrupt();
		}
		Dictionary<NPCAction, float> skippedActionOrder = CollectionPool<Dictionary<NPCAction, float>, KeyValuePair<NPCAction, float>>.Get();
		for (int num = 0; num < actionsTotallyOccurringWithinRange.Count; num++)
		{
			float num2 = 0f;
			num2 = ((actionsTotallyOccurringWithinRange[num].StartTime < currentTime) ? (1440f - (float)minSumFrom24HourTime + (float)TimeManager.GetMinSumFrom24HourTime(actionsTotallyOccurringWithinRange[num].StartTime)) : ((float)(TimeManager.GetMinSumFrom24HourTime(actionsTotallyOccurringWithinRange[num].StartTime) - minSumFrom24HourTime)));
			num2 -= 0.01f * (float)actionsTotallyOccurringWithinRange[num].Priority;
			skippedActionOrder.Add(actionsTotallyOccurringWithinRange[num], num2);
		}
		actionsTotallyOccurringWithinRange = actionsTotallyOccurringWithinRange.OrderBy((NPCAction x) => skippedActionOrder[x]).ToList();
		if (DEBUG_MODE)
		{
			Debug.Log((object)("Ordered skipped actions: " + actionsTotallyOccurringWithinRange.Count));
		}
		if (!initial)
		{
			for (int num3 = 0; num3 < actionsTotallyOccurringWithinRange.Count; num3++)
			{
				actionsTotallyOccurringWithinRange[num3].Skipped();
			}
		}
		if ((Object)(object)nPCAction != (Object)null)
		{
			nPCAction.JumpTo();
		}
		CollectionPool<List<NPCAction>, NPCAction>.Release(actionsTotallyOccurringWithinRange);
		CollectionPool<List<NPCAction>, NPCAction>.Release(actionsOccurringThisFrame);
		CollectionPool<Dictionary<NPCAction, float>, KeyValuePair<NPCAction, float>>.Release(skippedActionOrder);
	}

	protected virtual void CurfewEnabled()
	{
		SetCurfewModeEnabled(enabled: true);
	}

	protected virtual void CurfewDisabled()
	{
		SetCurfewModeEnabled(enabled: false);
	}

	public void SetCurfewModeEnabled(bool enabled)
	{
		for (int i = 0; i < EnabledDuringCurfew.Length; i++)
		{
			if (!((Object)(object)EnabledDuringCurfew[i] == (Object)null))
			{
				EnabledDuringCurfew[i].gameObject.SetActive(enabled);
			}
		}
		for (int j = 0; j < EnabledDuringNoCurfew.Length; j++)
		{
			if (!((Object)(object)EnabledDuringNoCurfew[j] == (Object)null))
			{
				EnabledDuringNoCurfew[j].gameObject.SetActive(!enabled);
			}
		}
	}
}
