using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using ScheduleOne.AvatarFramework.Equipping;
using ScheduleOne.Core;
using ScheduleOne.DevUtilities;
using ScheduleOne.Dialogue;
using ScheduleOne.Economy;
using ScheduleOne.Levelling;
using ScheduleOne.Map;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Police;
using ScheduleOne.Product;
using ScheduleOne.VoiceOver;
using UnityEngine;

namespace ScheduleOne.Cartel;

public class Ambush : CartelActivity
{
	public const float MIN_DISTANCE_TO_POLICE_OFFICER = 15f;

	public const int CANCEL_AMBUSH_AFTER_MINS = 360;

	public const float AMBUSH_DEFEATED_INFLUENCE_CHANGE = -0.1f;

	public static FullRank MIN_RANK_FOR_RANGED_WEAPONS = new FullRank(ERank.Hustler, 1);

	private CartelRegionActivities _regionActivities;

	[Header("Settings")]
	public AvatarWeapon[] RangedWeapons;

	public AvatarWeapon[] MeleeWeapons;

	[Header("Debugging & Development")]
	public EMapRegion region;

	public override void Activate(EMapRegion region)
	{
		base.Activate(region);
		if (InstanceFinder.IsServer)
		{
			ProductManager instance = NetworkSingleton<ProductManager>.Instance;
			instance.onContractReceiptRecorded = (Action<ContractReceipt>)Delegate.Combine(instance.onContractReceiptRecorded, new Action<ContractReceipt>(ContractReceiptRecorded));
			_regionActivities = NetworkSingleton<Cartel>.Instance.Activities.GetRegionalActivities(region);
		}
	}

	protected override void Deactivate()
	{
		base.Deactivate();
		if (InstanceFinder.IsServer)
		{
			ProductManager instance = NetworkSingleton<ProductManager>.Instance;
			instance.onContractReceiptRecorded = (Action<ContractReceipt>)Delegate.Remove(instance.onContractReceiptRecorded, new Action<ContractReceipt>(ContractReceiptRecorded));
		}
	}

	protected override void MinPassed()
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		base.MinPassed();
		if (!InstanceFinder.IsServer || !base.IsActive)
		{
			return;
		}
		if (base.MinsSinceActivation >= 360)
		{
			Console.LogWarning("Ambush cancelled due to timeout");
			Deactivate();
			return;
		}
		CartelAmbushLocation[] ambushLocations = _regionActivities.AmbushLocations;
		foreach (CartelAmbushLocation cartelAmbushLocation in ambushLocations)
		{
			for (int j = 0; j < Player.PlayerList.Count; j++)
			{
				if (CanPlayerBeAmbushed(Player.PlayerList[j]) && Vector3.Distance(Player.PlayerList[j].Avatar.CenterPoint, ((Component)cartelAmbushLocation).transform.position) <= cartelAmbushLocation.DetectionRadius)
				{
					Console.Log("Player " + Player.PlayerList[j].PlayerName + " is within ambush detection radius of " + ((Object)cartelAmbushLocation).name);
					if ((Object)(object)PoliceOfficer.GetNearestOfficer(Player.PlayerList[j].Avatar.CenterPoint, out var distanceToTarget) != (Object)null && distanceToTarget < 15f)
					{
						break;
					}
					Vector3[] array = (Vector3[])(object)new Vector3[cartelAmbushLocation.AmbushPoints.Length];
					for (int k = 0; k < cartelAmbushLocation.AmbushPoints.Length; k++)
					{
						array[k] = cartelAmbushLocation.AmbushPoints[k].position;
					}
					SpawnAmbush(Player.PlayerList[j], array);
					return;
				}
			}
		}
	}

	private bool CanPlayerBeAmbushed(Player player)
	{
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		if (!player.Health.IsAlive)
		{
			return false;
		}
		if (player.CrimeData.BodySearchPending || player.CrimeData.CurrentPursuitLevel != PlayerCrimeData.EPursuitLevel.None)
		{
			return false;
		}
		return true;
	}

	private void ContractReceiptRecorded(ContractReceipt receipt)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		if (!InstanceFinder.IsServer || receipt.CompletedBy != EContractParty.PlayerDealer)
		{
			return;
		}
		NPC nPC = NPCManager.GetNPC(receipt.CustomerId);
		if ((Object)(object)nPC == (Object)null || nPC.Region != base.Region)
		{
			return;
		}
		float distance;
		Player closestPlayer = Player.GetClosestPlayer(((Component)nPC).transform.position, out distance);
		if (distance > 10f)
		{
			return;
		}
		if ((Object)(object)PoliceOfficer.GetNearestOfficer(closestPlayer.Avatar.CenterPoint, out var distanceToTarget) != (Object)null && distanceToTarget < 15f)
		{
			Console.LogWarning("Post-deal ambush cancelled by nearby officer");
			return;
		}
		Vector3 val = ((Component)closestPlayer).transform.position - ((Component)closestPlayer).transform.forward * 8f;
		if (!NavMeshUtility.SamplePosition(val, out var _, 5f, -1))
		{
			Vector3[] array = (Vector3[])(object)new Vector3[4];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = val + (((i % 2 == 0) ? Vector3.left : Vector3.right) + ((i % 2 == 1) ? Vector3.forward : Vector3.back)) * 2f;
			}
			SpawnAmbush(closestPlayer, array);
		}
	}

	private void SpawnAmbush(Player target, Vector3[] potentialSpawnPoints)
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		Deactivate();
		int num = Random.Range(2, 5);
		int num2 = 0;
		if (NetworkSingleton<LevelManager>.Instance.GetFullRank() > MIN_RANK_FOR_RANGED_WEAPONS)
		{
			num2 = Random.Range(0, 2);
		}
		int num3 = Random.Range(1, 3);
		Console.Log($"Spawning ambush with {num} ambushers: {num2} ranged, {num3} melee.");
		List<CartelGoon> spawnedAmbushers = NetworkSingleton<Cartel>.Instance.GoonPool.SpawnMultipleGoons(potentialSpawnPoints[0], num);
		if (spawnedAmbushers.Count == 0)
		{
			return;
		}
		for (int i = 0; i < spawnedAmbushers.Count; i++)
		{
			spawnedAmbushers[i].Movement.Warp(potentialSpawnPoints[i]);
			Console.Log($"Ambusher {i + 1}/{spawnedAmbushers.Count} spawned at {potentialSpawnPoints[i]}.");
			if (num2 > 0)
			{
				num2--;
				spawnedAmbushers[i].Behaviour.CombatBehaviour.DefaultWeapon = RangedWeapons[Random.Range(0, RangedWeapons.Length)];
			}
			else if (num3 > 0)
			{
				num3--;
				spawnedAmbushers[i].Behaviour.CombatBehaviour.DefaultWeapon = MeleeWeapons[Random.Range(0, MeleeWeapons.Length)];
			}
			else
			{
				spawnedAmbushers[i].Behaviour.CombatBehaviour.DefaultWeapon = null;
			}
		}
		spawnedAmbushers[0].ShowWorldSpaceDialogue(spawnedAmbushers[0].DialogueHandler.Database.GetLine(EDialogueModule.CartelGoon, "ambush_start"), 5f);
		spawnedAmbushers[0].PlayVO(EVOLineType.Angry, network: true);
		spawnedAmbushers[0].AttackEntity(target);
		((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(MonitorAmbush());
		IEnumerator MonitorAmbush()
		{
			while (true)
			{
				yield return (object)new WaitForSeconds(1f);
				if ((Object)(object)target == (Object)null || !target.Health.IsAlive || target.IsArrested)
				{
					break;
				}
				bool flag = true;
				for (int j = 0; j < spawnedAmbushers.Count; j++)
				{
					if (!((Object)(object)spawnedAmbushers[j] == (Object)null) && spawnedAmbushers[j].IsConscious)
					{
						flag = false;
					}
				}
				if (flag)
				{
					Console.Log("All ambushers defeated");
					NetworkSingleton<Cartel>.Instance.Influence.ChangeInfluence(base.Region, -0.1f);
					break;
				}
			}
		}
	}

	[Button]
	public void TriggerAmbushForPlayer()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		_regionActivities = NetworkSingleton<Cartel>.Instance.Activities.GetRegionalActivities(region);
		CartelAmbushLocation[] ambushLocations = _regionActivities.AmbushLocations;
		foreach (CartelAmbushLocation cartelAmbushLocation in ambushLocations)
		{
			for (int j = 0; j < Player.PlayerList.Count; j++)
			{
				if (CanPlayerBeAmbushed(Player.PlayerList[j]) && Vector3.Distance(Player.PlayerList[j].Avatar.CenterPoint, ((Component)cartelAmbushLocation).transform.position) <= cartelAmbushLocation.DetectionRadius)
				{
					Console.Log("Player " + Player.PlayerList[j].PlayerName + " is within ambush detection radius of " + ((Object)cartelAmbushLocation).name);
					Vector3[] array = (Vector3[])(object)new Vector3[cartelAmbushLocation.AmbushPoints.Length];
					for (int k = 0; k < cartelAmbushLocation.AmbushPoints.Length; k++)
					{
						array[k] = cartelAmbushLocation.AmbushPoints[k].position;
					}
					SpawnAmbush(Player.PlayerList[j], array);
					return;
				}
			}
		}
	}
}
