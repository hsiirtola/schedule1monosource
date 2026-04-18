using FishNet;
using FishNet.Object;
using ScheduleOne.Cartel;
using ScheduleOne.Combat;
using ScheduleOne.Noise;
using ScheduleOne.NPCs;
using ScheduleOne.NPCs.Responses;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Vehicles;
using ScheduleOne.VoiceOver;
using UnityEngine;

namespace ScheduleOne.Police;

public class NPCResponses_CartelGoon : NPCResponses
{
	[Header("References")]
	public CartelGoon Goon;

	protected override void Awake()
	{
		base.Awake();
	}

	public override void ExplosionHeard(NoiseEvent explosionSound)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		base.ExplosionHeard(explosionSound);
		Goon.Behaviour.FaceTargetBehaviour.SetTarget(explosionSound.origin);
		Goon.Behaviour.FaceTargetBehaviour.Enable_Networked();
	}

	public override void GunshotHeard(NoiseEvent gunshotSound)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		base.GunshotHeard(gunshotSound);
		Goon.Behaviour.FaceTargetBehaviour.SetTarget(gunshotSound.origin);
		Goon.Behaviour.FaceTargetBehaviour.Enable_Networked();
	}

	public override void NoticePlayerDischargingWeapon(Player player)
	{
		base.NoticePlayerDischargingWeapon(player);
		Goon.Behaviour.FaceTargetBehaviour.SetTarget(((NetworkBehaviour)player).NetworkObject);
		Goon.Behaviour.FaceTargetBehaviour.Enable_Networked();
	}

	public override void PlayerFailedPickpocket(Player player)
	{
		base.PlayerFailedPickpocket(player);
		Goon.AttackEntity(player);
	}

	public override void HitByCar(LandVehicle vehicle)
	{
		Goon.AttackEntity(vehicle.DriverPlayer);
	}

	public override void ImpactReceived(Impact impact)
	{
		base.ImpactReceived(impact);
		if (!((Object)(object)impact.ImpactSource != (Object)null))
		{
			return;
		}
		Player component = ((Component)impact.ImpactSource).GetComponent<Player>();
		NPC component2 = ((Component)impact.ImpactSource).GetComponent<NPC>();
		ICombatTargetable target = null;
		if ((Object)(object)component != (Object)null)
		{
			target = component;
		}
		else if ((Object)(object)component2 != (Object)null)
		{
			if (component2 is CartelGoon)
			{
				return;
			}
			target = component2;
		}
		Goon.AttackEntity(target);
	}

	public override void RespondToAimedAt(Player player)
	{
		base.RespondToAimedAt(player);
		Goon.AttackEntity(player);
	}

	protected override void RespondToAnnoyingImpact(Player perpetrator, Impact impact)
	{
		base.RespondToAnnoyingImpact(perpetrator, impact);
		base.npc.VoiceOverEmitter.Play(EVOLineType.Annoyed);
		base.npc.DialogueHandler.PlayReaction("annoyed", 2.5f, network: false);
		base.npc.Avatar.EmotionManager.AddEmotionOverride("Annoyed", "annoyed", 20f, 3);
		if (InstanceFinder.IsServer)
		{
			base.npc.Behaviour.FaceTargetBehaviour.SetTarget(((NetworkBehaviour)perpetrator).NetworkObject);
			base.npc.Behaviour.FaceTargetBehaviour.Enable_Networked();
		}
	}
}
