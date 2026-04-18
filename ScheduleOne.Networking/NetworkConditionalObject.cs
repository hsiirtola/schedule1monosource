using System;
using FishNet;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Networking;

public class NetworkConditionalObject : MonoBehaviour
{
	public enum ECondition
	{
		All,
		HostOnly
	}

	public ECondition condition;

	private void Awake()
	{
		Player.onLocalPlayerSpawned = (Action)Delegate.Remove(Player.onLocalPlayerSpawned, new Action(Check));
		Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, new Action(Check));
	}

	public void Check()
	{
		ECondition eCondition = condition;
		if (eCondition != ECondition.All && eCondition == ECondition.HostOnly && !InstanceFinder.IsHost)
		{
			((Component)this).gameObject.SetActive(false);
		}
	}
}
