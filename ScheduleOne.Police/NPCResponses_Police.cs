using FishNet;
using FishNet.Object;
using ScheduleOne.Combat;
using ScheduleOne.Law;
using ScheduleOne.Noise;
using ScheduleOne.NPCs.Responses;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Vehicles;
using ScheduleOne.VoiceOver;
using UnityEngine;

namespace ScheduleOne.Police;

public class NPCResponses_Police : NPCResponses
{
	private PoliceOfficer officer;

	protected override void Awake()
	{
		base.Awake();
		officer = base.npc as PoliceOfficer;
	}

	public override void HitByCar(LandVehicle vehicle)
	{
		if ((Object)(object)base.npc == (Object)null || (Object)(object)vehicle == (Object)null)
		{
			return;
		}
		base.npc.PlayVO(EVOLineType.Angry);
		if ((Object)(object)vehicle.DriverPlayer != (Object)null && ((NetworkBehaviour)vehicle.DriverPlayer).IsOwner && !officer.IgnorePlayers && base.npc.Movement.TimeSinceHitByCar > 2f)
		{
			vehicle.DriverPlayer.CrimeData.AddCrime(new VehicularAssault());
			if (vehicle.DriverPlayer.CrimeData.CurrentPursuitLevel == PlayerCrimeData.EPursuitLevel.None)
			{
				vehicle.DriverPlayer.CrimeData.SetPursuitLevel(PlayerCrimeData.EPursuitLevel.NonLethal);
			}
			else
			{
				vehicle.DriverPlayer.CrimeData.Escalate();
			}
		}
	}

	public override void NoticedDrugDeal(Player player)
	{
		base.NoticedDrugDeal(player);
		if (!((Object)(object)base.npc == (Object)null))
		{
			base.npc.PlayVO(EVOLineType.Command);
			if (((NetworkBehaviour)player).IsOwner && !officer.IgnorePlayers)
			{
				player.CrimeData.AddCrime(new DrugTrafficking());
				player.CrimeData.SetPursuitLevel(PlayerCrimeData.EPursuitLevel.Arresting);
				(base.npc as PoliceOfficer).BeginFootPursuit_Networked(player.PlayerCode);
			}
		}
	}

	public override void NoticedPettyCrime(Player player)
	{
		base.NoticedPettyCrime(player);
		if (!((Object)(object)base.npc == (Object)null))
		{
			base.npc.PlayVO(EVOLineType.Command);
			if (((NetworkBehaviour)player).IsOwner && !officer.IgnorePlayers)
			{
				player.CrimeData.SetPursuitLevel(PlayerCrimeData.EPursuitLevel.Arresting);
				(base.npc as PoliceOfficer).BeginFootPursuit_Networked(player.PlayerCode);
			}
		}
	}

	public override void NoticedVandalism(Player player)
	{
		base.NoticedVandalism(player);
		if (!((Object)(object)base.npc == (Object)null))
		{
			base.npc.PlayVO(EVOLineType.Command);
			if (((NetworkBehaviour)player).IsOwner && !officer.IgnorePlayers)
			{
				player.CrimeData.AddCrime(new Vandalism());
				player.CrimeData.SetPursuitLevel(PlayerCrimeData.EPursuitLevel.Arresting);
				(base.npc as PoliceOfficer).BeginFootPursuit_Networked(player.PlayerCode);
			}
		}
	}

	public override void SawPickpocketing(Player player)
	{
		base.SawPickpocketing(player);
		if (!((Object)(object)base.npc == (Object)null))
		{
			base.npc.PlayVO(EVOLineType.Command);
			if (((NetworkBehaviour)player).IsOwner && !officer.IgnorePlayers)
			{
				player.CrimeData.AddCrime(new Theft());
				player.CrimeData.SetPursuitLevel(PlayerCrimeData.EPursuitLevel.Arresting);
				(base.npc as PoliceOfficer).BeginFootPursuit_Networked(player.PlayerCode);
			}
		}
	}

	public override void PlayerFailedPickpocket(Player player)
	{
		base.PlayerFailedPickpocket(player);
		if (!((Object)(object)base.npc == (Object)null))
		{
			base.npc.PlayVO(EVOLineType.Angry);
			if (((NetworkBehaviour)player).IsOwner && !officer.IgnorePlayers)
			{
				player.CrimeData.AddCrime(new Theft());
				player.CrimeData.SetPursuitLevel(PlayerCrimeData.EPursuitLevel.Arresting);
				(base.npc as PoliceOfficer).BeginFootPursuit_Networked(player.PlayerCode);
			}
		}
	}

	public override void NoticePlayerBrandishingWeapon(Player player)
	{
		base.NoticePlayerBrandishingWeapon(player);
		if (!((Object)(object)base.npc == (Object)null))
		{
			base.npc.PlayVO(EVOLineType.Command);
			if (((NetworkBehaviour)player).IsOwner && !officer.IgnorePlayers)
			{
				player.CrimeData.AddCrime(new BrandishingWeapon());
				player.CrimeData.SetPursuitLevel(PlayerCrimeData.EPursuitLevel.NonLethal);
				(base.npc as PoliceOfficer).BeginFootPursuit_Networked(player.PlayerCode);
			}
		}
	}

	public override void NoticePlayerDischargingWeapon(Player player)
	{
		base.NoticePlayerDischargingWeapon(player);
		if (!((Object)(object)base.npc == (Object)null))
		{
			base.npc.PlayVO(EVOLineType.Command);
			if (((NetworkBehaviour)player).IsOwner && !officer.IgnorePlayers)
			{
				player.CrimeData.AddCrime(new DischargeFirearm());
				player.CrimeData.SetPursuitLevel(PlayerCrimeData.EPursuitLevel.NonLethal);
				(base.npc as PoliceOfficer).BeginFootPursuit_Networked(player.PlayerCode);
			}
		}
	}

	public override void NoticedWantedPlayer(Player player)
	{
		base.NoticedWantedPlayer(player);
		if ((Object)(object)base.npc == (Object)null)
		{
			return;
		}
		base.npc.PlayVO(EVOLineType.Command);
		if (((NetworkBehaviour)player).IsOwner && !officer.IgnorePlayers)
		{
			player.CrimeData.RecordLastKnownPosition(resetTimeSinceSighted: true);
			if ((Object)(object)base.npc.CurrentVehicle != (Object)null)
			{
				(base.npc as PoliceOfficer).BeginFootPursuit_Networked(player.PlayerCode, includeColleagues: false);
				(base.npc as PoliceOfficer).BeginVehiclePursuit_Networked(player.PlayerCode, ((NetworkBehaviour)base.npc.CurrentVehicle).NetworkObject, beginAsSighted: true);
			}
			else
			{
				(base.npc as PoliceOfficer).BeginFootPursuit_Networked(player.PlayerCode);
			}
		}
	}

	public override void NoticedSuspiciousPlayer(Player player)
	{
		base.NoticedSuspiciousPlayer(player);
		if (!((Object)(object)base.npc == (Object)null) && ((NetworkBehaviour)player).IsOwner && !officer.IgnorePlayers)
		{
			(base.npc as PoliceOfficer).BeginBodySearch_Networked(player.PlayerCode);
		}
	}

	public override void NoticedViolatingCurfew(Player player)
	{
		base.NoticedViolatingCurfew(player);
		if ((Object)(object)base.npc == (Object)null)
		{
			return;
		}
		base.npc.PlayVO(EVOLineType.Command);
		if (((NetworkBehaviour)player).IsOwner && !officer.IgnorePlayers)
		{
			player.CrimeData.AddCrime(new ViolatingCurfew());
			player.CrimeData.SetPursuitLevel(PlayerCrimeData.EPursuitLevel.Arresting);
			if ((Object)(object)base.npc.CurrentVehicle != (Object)null)
			{
				(base.npc as PoliceOfficer).BeginFootPursuit_Networked(player.PlayerCode, includeColleagues: false);
				(base.npc as PoliceOfficer).BeginVehiclePursuit_Networked(player.PlayerCode, ((NetworkBehaviour)base.npc.CurrentVehicle).NetworkObject, beginAsSighted: true);
			}
			else
			{
				(base.npc as PoliceOfficer).BeginFootPursuit_Networked(player.PlayerCode);
			}
		}
	}

	protected override void RespondToFirstNonLethalAttack(Player perpetrator, Impact impact)
	{
		base.RespondToFirstNonLethalAttack(perpetrator, impact);
		if (!((Object)(object)base.npc == (Object)null) && !officer.IgnorePlayers)
		{
			perpetrator.CrimeData.AddCrime(new Assault());
			if (perpetrator.CrimeData.CurrentPursuitLevel == PlayerCrimeData.EPursuitLevel.None)
			{
				perpetrator.CrimeData.SetPursuitLevel(PlayerCrimeData.EPursuitLevel.Arresting);
				officer.BeginFootPursuit_Networked(perpetrator.PlayerCode);
			}
			else
			{
				perpetrator.CrimeData.Escalate();
				officer.BeginFootPursuit_Networked(perpetrator.PlayerCode);
			}
		}
	}

	protected override void RespondToLethalAttack(Player perpetrator, Impact impact)
	{
		base.RespondToLethalAttack(perpetrator, impact);
		if (!((Object)(object)base.npc == (Object)null) && !officer.IgnorePlayers)
		{
			perpetrator.CrimeData.AddCrime(new DeadlyAssault());
			if (perpetrator.CrimeData.CurrentPursuitLevel < PlayerCrimeData.EPursuitLevel.Lethal)
			{
				perpetrator.CrimeData.SetPursuitLevel(PlayerCrimeData.EPursuitLevel.Lethal);
				officer.BeginFootPursuit_Networked(perpetrator.PlayerCode);
			}
		}
	}

	protected override void RespondToRepeatedNonLethalAttack(Player perpetrator, Impact impact)
	{
		base.RespondToRepeatedNonLethalAttack(perpetrator, impact);
		if (!officer.IgnorePlayers)
		{
			if (!perpetrator.CrimeData.IsCrimeOnRecord(typeof(Assault)))
			{
				perpetrator.CrimeData.AddCrime(new Assault());
			}
			if (perpetrator.CrimeData.CurrentPursuitLevel == PlayerCrimeData.EPursuitLevel.None)
			{
				perpetrator.CrimeData.SetPursuitLevel(PlayerCrimeData.EPursuitLevel.Arresting);
				officer.BeginFootPursuit_Networked(perpetrator.PlayerCode);
			}
			else
			{
				perpetrator.CrimeData.Escalate();
				officer.BeginFootPursuit_Networked(perpetrator.PlayerCode);
			}
		}
	}

	protected override void RespondToAnnoyingImpact(Player perpetrator, Impact impact)
	{
		base.RespondToAnnoyingImpact(perpetrator, impact);
		if (!officer.IgnorePlayers)
		{
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

	public override void RespondToAimedAt(Player player)
	{
		base.RespondToAimedAt(player);
		if (!officer.IgnorePlayers && player.CrimeData.CurrentPursuitLevel < PlayerCrimeData.EPursuitLevel.Lethal)
		{
			player.CrimeData.AddCrime(new Assault());
			player.CrimeData.SetPursuitLevel(PlayerCrimeData.EPursuitLevel.Lethal);
		}
	}

	public override void ImpactReceived(Impact impact)
	{
		base.ImpactReceived(impact);
		if (officer.PursuitBehaviour.Active)
		{
			officer.PursuitBehaviour.ResetArrestProgress();
		}
	}

	public override void GunshotHeard(NoiseEvent gunshotSound)
	{
		base.GunshotHeard(gunshotSound);
		if ((Object)(object)gunshotSound.source != (Object)null && (Object)(object)gunshotSound.source.GetComponent<Player>() != (Object)null && !officer.IgnorePlayers)
		{
			officer.Behaviour.FaceTargetBehaviour.SetTarget(((NetworkBehaviour)gunshotSound.source.GetComponent<Player>()).NetworkObject);
			officer.Behaviour.FaceTargetBehaviour.Enable_Server();
		}
	}
}
