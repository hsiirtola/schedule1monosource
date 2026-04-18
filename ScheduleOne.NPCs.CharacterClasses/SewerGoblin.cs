using System;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Object;
using ScheduleOne.Audio;
using ScheduleOne.Combat;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Doors;
using ScheduleOne.GameTime;
using ScheduleOne.ItemFramework;
using ScheduleOne.Map;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.NPCs.Schedules;
using ScheduleOne.PlayerScripts;
using ScheduleOne.VoiceOver;
using UnityEngine;

namespace ScheduleOne.NPCs.CharacterClasses;

public class SewerGoblin : NPC
{
	public enum ESewerGoblinState
	{
		Inactive,
		Attacking,
		Retrieving,
		Retreating
	}

	public const int COOLDOWN_HOURS_BETWEEN_DEPLOYS = 12;

	public const float HOURLY_DEPLOY_CHANCE = 0.1f;

	public const float NORMALIZED_HEALTH_THRESHOLD_TO_RETREAT = 0.5f;

	public const float RETREAT_CHANCE_AFTER_HIT = 0.3f;

	public const int MAX_CANCELLED_RETRIEVE_ATTEMPTS = 3;

	[Header("References")]
	public NPCEnterableBuilding SewerHidingBuilding;

	public NPCEvent_StayInBuilding StayInBuildingEvent;

	public ItemDefinition PacifyItem;

	public SewerGoblinRetrieveBehaviour RetrieveBehaviour;

	public AudioSourceController ExitSound;

	[HideInInspector]
	public int cancelledRetrieveAttempts;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002ESewerGoblinAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002ESewerGoblinAssembly_002DCSharp_002Edll_Excuted;

	public Player TargetPlayer { get; private set; }

	public ESewerGoblinState CurrentState { get; private set; }

	public int HoursSinceLastDeploy { get; set; }

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ENPCs_002ECharacterClasses_002ESewerGoblin_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	protected override void Start()
	{
		base.Start();
		NetworkSingleton<TimeManager>.Instance.onMinutePass -= new Action(OnMinPass);
		NetworkSingleton<TimeManager>.Instance.onMinutePass += new Action(OnMinPass);
		TimeManager instance = NetworkSingleton<TimeManager>.Instance;
		instance.onHourPass = (Action)Delegate.Remove(instance.onHourPass, new Action(OnHourPass));
		TimeManager instance2 = NetworkSingleton<TimeManager>.Instance;
		instance2.onHourPass = (Action)Delegate.Combine(instance2.onHourPass, new Action(OnHourPass));
	}

	private void Update()
	{
		if (CurrentState == ESewerGoblinState.Attacking)
		{
			if (Health.NormalizedHealth < 0.5f)
			{
				Retreat();
			}
			if (CanBeginRetieve() && IsPlayerHoldingPacifyItem(TargetPlayer))
			{
				BeginRetrieve();
			}
		}
	}

	private void OnMinPass()
	{
		if (InstanceFinder.IsServer && CurrentState == ESewerGoblinState.Inactive && HoursSinceLastDeploy > 12 && Random.Range(0f, 1f) < 0.0016666667f)
		{
			List<Player> playersInSewer = NetworkSingleton<SewerManager>.Instance.GetPlayersInSewer();
			playersInSewer = playersInSewer.Where((Player player) => IsPlayerValidTarget(player)).ToList();
			if (playersInSewer.Count > 0)
			{
				DeployToPlayer(playersInSewer[Random.Range(0, playersInSewer.Count)]);
			}
		}
	}

	private void OnHourPass()
	{
		if (InstanceFinder.IsServer && CurrentState == ESewerGoblinState.Inactive)
		{
			HoursSinceLastDeploy++;
		}
	}

	public void DeployToPlayer(Player player)
	{
		TargetPlayer = player;
		HoursSinceLastDeploy = 0;
		cancelledRetrieveAttempts = 0;
		List<StaticDoor> source = new List<StaticDoor>(SewerHidingBuilding.Doors);
		source = source.OrderBy((StaticDoor door) => Vector3.Distance(((Component)door).transform.position, player.CenterPointTransform.position)).ToList();
		StaticDoor lastEnteredDoor = source[Mathf.Min(2, source.Count - 1)];
		base.LastEnteredDoor = lastEnteredDoor;
		AttackTarget();
		Console.Log("Sewer Goblin deploying to player: " + player.PlayerName);
	}

	private void AttackTarget()
	{
		CurrentState = ESewerGoblinState.Attacking;
		Behaviour.CombatBehaviour.SetTargetAndEnable_Server(((NetworkBehaviour)TargetPlayer).NetworkObject);
		PlayVO(EVOLineType.Angry);
	}

	public void Retreat()
	{
		Console.Log("Sewer Goblin retreating!");
		CurrentState = ESewerGoblinState.Retreating;
		Behaviour.CombatBehaviour.Disable_Networked(null);
		Movement.SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("Retreat", 10, 1.3f));
		List<StaticDoor> source = new List<StaticDoor>(SewerHidingBuilding.Doors);
		source = source.OrderBy((StaticDoor door) => Vector3.Distance(((Component)door).transform.position, base.CenterPointTransform.position)).ToList();
		source.RemoveRange(0, 2);
		StayInBuildingEvent.Door = source[Random.Range(0, source.Count)];
	}

	protected override void EnterBuilding(string buildingGUID, int doorIndex)
	{
		base.EnterBuilding(buildingGUID, doorIndex);
		CurrentState = ESewerGoblinState.Inactive;
		TargetPlayer = null;
		Health.RestoreHealth();
	}

	protected override void ExitBuilding(NPCEnterableBuilding building)
	{
		base.ExitBuilding(building);
		ExitSound.Play();
	}

	public void DeployToLocalPlayer()
	{
		DeployToPlayer(Player.Local);
	}

	private void OnSuccesfulCombatHit()
	{
		if (InstanceFinder.IsServer && CurrentState == ESewerGoblinState.Attacking && Random.Range(0f, 1f) < 0.3f)
		{
			Retreat();
		}
	}

	private bool CanBeginRetieve()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (Vector3.Distance(((Component)this).transform.position, ((Component)TargetPlayer).transform.position) > 10f)
		{
			return false;
		}
		if (Health.NormalizedHealth < 0.99f)
		{
			return false;
		}
		if (cancelledRetrieveAttempts >= 3)
		{
			return false;
		}
		if (!Awareness.VisionCone.IsPlayerVisible(TargetPlayer))
		{
			return false;
		}
		return true;
	}

	private void BeginRetrieve()
	{
		Console.Log("Sewer Goblin beginning retrieve!");
		CurrentState = ESewerGoblinState.Retrieving;
		RetrieveBehaviour.Enable_Networked();
	}

	private void OnRetrieveCancel()
	{
		cancelledRetrieveAttempts++;
		if (IsPlayerValidTarget(TargetPlayer))
		{
			AttackTarget();
		}
		else
		{
			Retreat();
		}
	}

	private void OnRetrieveSuccess()
	{
		Retreat();
	}

	public bool IsPlayerValidTarget(Player player)
	{
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		if (player.IsArrested)
		{
			return false;
		}
		if (!player.Health.IsAlive)
		{
			return false;
		}
		if (player.IsSleeping)
		{
			return false;
		}
		if ((Object)(object)player.CurrentProperty != (Object)null)
		{
			return false;
		}
		return true;
	}

	public bool IsPlayerHoldingPacifyItem(Player player)
	{
		ItemInstance equippedItem = player.GetEquippedItem();
		if (equippedItem != null)
		{
			return ((BaseItemInstance)equippedItem).ID == ((BaseItemDefinition)PacifyItem).ID;
		}
		return false;
	}

	public override void ProcessImpactForce(Vector3 forcePoint, Vector3 forceDirection, float force)
	{
	}

	private void OnTakeDamage(float damageAmount)
	{
		if (CurrentState == ESewerGoblinState.Retrieving)
		{
			RetrieveBehaviour.CancelRetrieve();
		}
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002ESewerGoblinAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002ESewerGoblinAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002ESewerGoblinAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002ESewerGoblinAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	protected override void Awake_UserLogic_ScheduleOne_002ENPCs_002ECharacterClasses_002ESewerGoblin_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		CombatBehaviour combatBehaviour = Behaviour.CombatBehaviour;
		combatBehaviour.onSuccessfulHit = (Action)Delegate.Combine(combatBehaviour.onSuccessfulHit, new Action(OnSuccesfulCombatHit));
		SewerGoblinRetrieveBehaviour retrieveBehaviour = RetrieveBehaviour;
		retrieveBehaviour.onRetrieveComplete = (Action)Delegate.Combine(retrieveBehaviour.onRetrieveComplete, new Action(OnRetrieveSuccess));
		SewerGoblinRetrieveBehaviour retrieveBehaviour2 = RetrieveBehaviour;
		retrieveBehaviour2.onRetrieveCancelled = (Action)Delegate.Combine(retrieveBehaviour2.onRetrieveCancelled, new Action(OnRetrieveCancel));
		NPCHealth health = Health;
		health.onTakeDamage = (Action<float>)Delegate.Combine(health.onTakeDamage, new Action<float>(OnTakeDamage));
	}
}
