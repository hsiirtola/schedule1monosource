using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using ScheduleOne.Combat;
using ScheduleOne.Dialogue;
using ScheduleOne.Law;
using ScheduleOne.Noise;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Vision;
using ScheduleOne.VoiceOver;
using UnityEngine;

namespace ScheduleOne.NPCs.Responses;

public class NPCResponses_Civilian : NPCResponses
{
	public enum EAttackResponse
	{
		None,
		Panic,
		Flee,
		CallPolice,
		Fight
	}

	public enum EThreatType
	{
		None,
		AimedAt,
		GunshotHeard,
		ExplosionHeard
	}

	[Header("Response Settings")]
	public bool CanCallPolice = true;

	public bool OverrideThreatResponses;

	public EAttackResponse ThreatResponseOverride = EAttackResponse.Panic;

	private EAttackResponse currentThreatResponse;

	private float lastThreatTime;

	protected override void Awake()
	{
		base.Awake();
		lastThreatTime = Time.time;
	}

	public override void GunshotHeard(NoiseEvent gunshotSound)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		base.GunshotHeard(gunshotSound);
		if (InstanceFinder.IsServer && currentThreatResponse == EAttackResponse.None)
		{
			Player player = (((Object)(object)gunshotSound.source != (Object)null) ? gunshotSound.source.GetComponent<Player>() : null);
			lastThreatTime = Time.time;
			currentThreatResponse = GetThreatResponse(EThreatType.GunshotHeard, player);
			ExecuteThreatResponse(currentThreatResponse, player, gunshotSound.origin, new DischargeFirearm());
		}
	}

	public override void ExplosionHeard(NoiseEvent explosionSound)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		base.ExplosionHeard(explosionSound);
		if (InstanceFinder.IsServer)
		{
			Console.Log("Explosion heard by " + base.npc.fullName);
			if (currentThreatResponse == EAttackResponse.None)
			{
				Player threatSource = (((Object)(object)explosionSound.source != (Object)null) ? explosionSound.source.GetComponent<Player>() : null);
				lastThreatTime = Time.time;
				currentThreatResponse = GetThreatResponse(EThreatType.ExplosionHeard, threatSource);
				ExecuteThreatResponse(currentThreatResponse, null, explosionSound.origin);
			}
		}
	}

	public override void PlayerFailedPickpocket(Player player)
	{
		base.PlayerFailedPickpocket(player);
		string line = base.npc.DialogueHandler.Database.GetLine(EDialogueModule.Reactions, "noticed_pickpocket");
		base.npc.DialogueHandler.ShowWorldspaceDialogue(line, 3f);
		base.npc.Avatar.EmotionManager.AddEmotionOverride("Angry", "noticed_pickpocket", 20f, 3);
		if (base.npc.Aggression > 0.5f && Random.value < base.npc.Aggression)
		{
			base.npc.Behaviour.CombatBehaviour.SetTargetAndEnable_Server(((NetworkBehaviour)player).NetworkObject);
			base.npc.VoiceOverEmitter.Play(EVOLineType.Angry);
			return;
		}
		float value = Random.value;
		if (value > 0.3f && CanCallPolice)
		{
			base.actions.SetCallPoliceBehaviourCrime(new Theft());
			base.actions.CallPolice_Networked(((NetworkBehaviour)player).NetworkObject);
			base.npc.PlayVO(EVOLineType.Alerted);
		}
		else if (value > 0.1f)
		{
			base.npc.PlayVO(EVOLineType.Alerted);
			base.npc.Behaviour.FaceTargetBehaviour.SetTarget(((NetworkBehaviour)player).NetworkObject);
			base.npc.Behaviour.FaceTargetBehaviour.Enable_Server();
		}
		else
		{
			base.npc.PlayVO(EVOLineType.Alerted);
			base.npc.Behaviour.FleeBehaviour.SetEntityToFlee(((NetworkBehaviour)player).NetworkObject);
			base.npc.Behaviour.FleeBehaviour.Enable_Networked();
		}
	}

	protected override void RespondToFirstNonLethalAttack(Player perpetrator, Impact impact)
	{
		base.RespondToFirstNonLethalAttack(perpetrator, impact);
		if (base.npc.Aggression > 0.5f && Random.value < base.npc.Aggression)
		{
			base.npc.Behaviour.CombatBehaviour.SetTargetAndEnable_Server(((NetworkBehaviour)perpetrator).NetworkObject);
			base.npc.VoiceOverEmitter.Play(EVOLineType.Angry);
			return;
		}
		base.npc.DialogueHandler.PlayReaction("hurt", 2.5f, network: false);
		base.npc.Avatar.EmotionManager.AddEmotionOverride("Annoyed", "hurt", 20f, 3);
		if (InstanceFinder.IsServer)
		{
			base.npc.Behaviour.FaceTargetBehaviour.SetTarget(((NetworkBehaviour)perpetrator).NetworkObject);
			base.npc.Behaviour.FaceTargetBehaviour.Enable_Server();
		}
	}

	protected override void RespondToAnnoyingImpact(Player perpetrator, Impact impact)
	{
		base.RespondToAnnoyingImpact(perpetrator, impact);
		if (base.npc.Aggression > 0.6f && Random.value * 1.5f < base.npc.Aggression)
		{
			base.npc.Behaviour.CombatBehaviour.SetTargetAndEnable_Server(((NetworkBehaviour)perpetrator).NetworkObject);
			base.npc.VoiceOverEmitter.Play(EVOLineType.Angry);
			return;
		}
		base.npc.VoiceOverEmitter.Play(EVOLineType.Annoyed);
		base.npc.DialogueHandler.PlayReaction("annoyed", 2.5f, network: false);
		base.npc.Avatar.EmotionManager.AddEmotionOverride("Annoyed", "annoyed", 20f, 3);
		if (InstanceFinder.IsServer)
		{
			base.npc.Behaviour.FaceTargetBehaviour.SetTarget(((NetworkBehaviour)perpetrator).NetworkObject);
			base.npc.Behaviour.FaceTargetBehaviour.Enable_Server();
		}
	}

	protected override void RespondToLethalAttack(Player perpetrator, Impact impact)
	{
		base.RespondToLethalAttack(perpetrator, impact);
		RespondToLethalOrRepeatedAttack(perpetrator, impact);
	}

	protected override void RespondToRepeatedNonLethalAttack(Player perpetrator, Impact impact)
	{
		base.RespondToRepeatedNonLethalAttack(perpetrator, impact);
		RespondToLethalOrRepeatedAttack(perpetrator, impact);
	}

	private void RespondToLethalOrRepeatedAttack(Player perpetrator, Impact impact)
	{
		float value = Random.value;
		float aggression = base.npc.Aggression;
		if (aggression > 0.5f && value < aggression)
		{
			base.npc.Behaviour.CombatBehaviour.SetTargetAndEnable_Server(((NetworkBehaviour)perpetrator).NetworkObject);
			base.npc.VoiceOverEmitter.Play(EVOLineType.Angry);
			return;
		}
		if (value > 0.5f && CanCallPolice && perpetrator.CrimeData.CurrentPursuitLevel == PlayerCrimeData.EPursuitLevel.None)
		{
			if (Impact.IsLethal(impact.ImpactType))
			{
				base.actions.SetCallPoliceBehaviourCrime(new DeadlyAssault());
			}
			else
			{
				base.actions.SetCallPoliceBehaviourCrime(new Assault());
			}
			base.actions.CallPolice_Networked(((NetworkBehaviour)perpetrator).NetworkObject);
			return;
		}
		base.npc.SetPanicked_Server();
		base.npc.DialogueHandler.PlayReaction("panic_start", 3f, network: false);
		if (value > 0.2f)
		{
			base.npc.Behaviour.FleeBehaviour.SetEntityToFlee(((NetworkBehaviour)perpetrator).NetworkObject);
			base.npc.Behaviour.FleeBehaviour.Enable_Networked();
		}
	}

	public override void RespondToAimedAt(Player player)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		base.RespondToAimedAt(player);
		player.VisualState.ApplyState("aiming_at_npc", EVisualState.Brandishing, 2.5f);
		if (currentThreatResponse == EAttackResponse.None)
		{
			lastThreatTime = Time.time;
			currentThreatResponse = GetThreatResponse(EThreatType.AimedAt, player);
			ExecuteThreatResponse(currentThreatResponse, player, ((Component)player).transform.position, new BrandishingWeapon());
		}
	}

	private void ExecuteThreatResponse(EAttackResponse response, Player target, Vector3 threatOrigin, Crime crime = null)
	{
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		if (response == EAttackResponse.None)
		{
			return;
		}
		Console.Log(base.npc.fullName + " executing threat response: " + response.ToString() + " on target " + (object)target);
		switch (response)
		{
		case EAttackResponse.Panic:
			base.npc.SetPanicked_Server();
			base.npc.DialogueHandler.PlayReaction("panic_start", 3f, network: false);
			((MonoBehaviour)this).StartCoroutine(WaitForThreatResponseEnd(() => !base.npc.IsPanicked));
			break;
		case EAttackResponse.Flee:
			base.npc.SetPanicked_Server();
			base.npc.DialogueHandler.PlayReaction("panic_start", 3f, network: false);
			if ((Object)(object)target != (Object)null)
			{
				base.npc.Behaviour.FleeBehaviour.SetEntityToFlee(((NetworkBehaviour)target).NetworkObject);
			}
			else
			{
				base.npc.Behaviour.FleeBehaviour.SetPointToFlee(threatOrigin);
			}
			base.npc.Behaviour.FleeBehaviour.Enable_Networked();
			((MonoBehaviour)this).StartCoroutine(WaitForThreatResponseEnd(() => !base.npc.IsPanicked && !base.npc.Behaviour.FleeBehaviour.Enabled));
			break;
		case EAttackResponse.CallPolice:
			if ((Object)(object)target != (Object)null)
			{
				base.actions.SetCallPoliceBehaviourCrime(crime);
				base.actions.CallPolice_Networked(((NetworkBehaviour)target).NetworkObject);
			}
			((MonoBehaviour)this).StartCoroutine(WaitForThreatResponseEnd(() => !base.npc.Behaviour.CallPoliceBehaviour.Enabled));
			break;
		case EAttackResponse.Fight:
			if ((Object)(object)target != (Object)null)
			{
				base.npc.Behaviour.CombatBehaviour.SetTargetAndEnable_Server(((NetworkBehaviour)target).NetworkObject);
			}
			((MonoBehaviour)this).StartCoroutine(WaitForThreatResponseEnd(() => !base.npc.Behaviour.CombatBehaviour.Enabled));
			break;
		}
		IEnumerator WaitForThreatResponseEnd(Func<bool> condition)
		{
			yield return (object)new WaitUntil(condition);
			Debug.Log((object)(base.npc.fullName + " panic ended, resetting threat response"));
			currentThreatResponse = EAttackResponse.None;
		}
	}

	private EAttackResponse GetThreatResponse(EThreatType type, Player threatSource)
	{
		if (OverrideThreatResponses)
		{
			return ThreatResponseOverride;
		}
		if ((Object)(object)base.npc.CurrentVehicle != (Object)null)
		{
			return EAttackResponse.Panic;
		}
		switch (type)
		{
		case EThreatType.AimedAt:
			if (Random.Range(0f, 1f) < base.npc.Aggression)
			{
				return EAttackResponse.Fight;
			}
			if ((Object)(object)threatSource != (Object)null && threatSource.CrimeData.CurrentPursuitLevel == PlayerCrimeData.EPursuitLevel.None)
			{
				return Random.Range(0, 2) switch
				{
					0 => EAttackResponse.Panic, 
					1 => EAttackResponse.Flee, 
					_ => EAttackResponse.CallPolice, 
				};
			}
			if (Random.value < 0.5f)
			{
				return EAttackResponse.Panic;
			}
			return EAttackResponse.Flee;
		case EThreatType.GunshotHeard:
			if ((Object)(object)threatSource != (Object)null && threatSource.CrimeData.CurrentPursuitLevel == PlayerCrimeData.EPursuitLevel.None)
			{
				List<EAttackResponse> list = new List<EAttackResponse>
				{
					EAttackResponse.Panic,
					EAttackResponse.Flee
				};
				if (CanCallPolice)
				{
					list.Add(EAttackResponse.CallPolice);
				}
				return Random.Range(0, list.Count) switch
				{
					0 => EAttackResponse.Panic, 
					1 => EAttackResponse.Flee, 
					_ => EAttackResponse.CallPolice, 
				};
			}
			if (Random.value < 0.5f)
			{
				return EAttackResponse.Panic;
			}
			return EAttackResponse.Flee;
		case EThreatType.ExplosionHeard:
			if (Random.value < 0.5f)
			{
				return EAttackResponse.Panic;
			}
			return EAttackResponse.Flee;
		default:
			Console.LogError("Unhandled threat type: " + type);
			break;
		case EThreatType.None:
			break;
		}
		return EAttackResponse.None;
	}
}
