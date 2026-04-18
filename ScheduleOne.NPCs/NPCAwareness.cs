using System;
using ScheduleOne.Noise;
using ScheduleOne.NPCs.Responses;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Vehicles;
using ScheduleOne.Vision;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.NPCs;

public class NPCAwareness : MonoBehaviour
{
	public const float PLAYER_AIM_DETECTION_RANGE = 15f;

	public bool AwarenessActiveByDefault = true;

	[Header("References")]
	public VisionCone VisionCone;

	public Listener Listener;

	public NPCResponses Responses;

	public UnityEvent<Player> onNoticedGeneralCrime;

	public UnityEvent<Player> onNoticedPettyCrime;

	public UnityEvent<Player> onNoticedDrugDealing;

	public UnityEvent<Player> onNoticedPlayerViolatingCurfew;

	public UnityEvent<Player> onNoticedSuspiciousPlayer;

	public UnityEvent<NoiseEvent> onGunshotHeard;

	public UnityEvent<NoiseEvent> onExplosionHeard;

	public UnityEvent<LandVehicle> onHitByCar;

	private NPC npc;

	protected virtual void Awake()
	{
		npc = ((Component)this).GetComponentInParent<NPC>();
		if ((Object)(object)Responses == (Object)null)
		{
			Console.LogError("NPCAwareness doesn't have a reference to NPCResponses - responses won't be automatically connected.");
		}
		VisionCone visionCone = VisionCone;
		visionCone.onVisionEventFull = (VisionCone.EventStateChange)Delegate.Combine(visionCone.onVisionEventFull, new VisionCone.EventStateChange(VisionEvent));
		Listener listener = Listener;
		listener.onNoiseHeard = (Listener.HearingEvent)Delegate.Combine(listener.onNoiseHeard, new Listener.HearingEvent(NoiseEvent));
		SetAwarenessActive(AwarenessActiveByDefault);
	}

	public void SetAwarenessActive(bool active)
	{
		((Behaviour)Listener).enabled = active;
		((Behaviour)VisionCone).enabled = active;
		((Behaviour)this).enabled = active;
	}

	public void VisionEvent(VisionEventReceipt vEvent)
	{
		if (!((Behaviour)this).enabled)
		{
			return;
		}
		switch (vEvent.State)
		{
		case EVisualState.DisobeyingCurfew:
			if (onNoticedPlayerViolatingCurfew != null)
			{
				onNoticedPlayerViolatingCurfew.Invoke(((Component)vEvent.Target).GetComponent<Player>());
			}
			if ((Object)(object)Responses != (Object)null)
			{
				Responses.NoticedViolatingCurfew(((Component)vEvent.Target).GetComponent<Player>());
			}
			break;
		case EVisualState.PettyCrime:
			if (onNoticedPettyCrime != null)
			{
				onNoticedPettyCrime.Invoke(((Component)vEvent.Target).GetComponent<Player>());
			}
			if (onNoticedGeneralCrime != null)
			{
				onNoticedGeneralCrime.Invoke(((Component)vEvent.Target).GetComponent<Player>());
			}
			if ((Object)(object)Responses != (Object)null)
			{
				Responses.NoticedPettyCrime(((Component)vEvent.Target).GetComponent<Player>());
			}
			break;
		case EVisualState.Vandalizing:
			if ((Object)(object)Responses != (Object)null)
			{
				Responses.NoticedVandalism(((Component)vEvent.Target).GetComponent<Player>());
			}
			break;
		case EVisualState.Pickpocketing:
			if ((Object)(object)Responses != (Object)null)
			{
				Responses.SawPickpocketing(((Component)vEvent.Target).GetComponent<Player>());
			}
			break;
		case EVisualState.DrugDealing:
			if (onNoticedDrugDealing != null)
			{
				onNoticedDrugDealing.Invoke(((Component)vEvent.Target).GetComponent<Player>());
			}
			if (onNoticedGeneralCrime != null)
			{
				onNoticedGeneralCrime.Invoke(((Component)vEvent.Target).GetComponent<Player>());
			}
			if ((Object)(object)Responses != (Object)null)
			{
				Responses.NoticedDrugDeal(((Component)vEvent.Target).GetComponent<Player>());
			}
			break;
		case EVisualState.Wanted:
			if ((Object)(object)Responses != (Object)null)
			{
				Responses.NoticedWantedPlayer(((Component)vEvent.Target).GetComponent<Player>());
			}
			break;
		case EVisualState.Suspicious:
			if (onNoticedSuspiciousPlayer != null)
			{
				onNoticedSuspiciousPlayer.Invoke(((Component)vEvent.Target).GetComponent<Player>());
			}
			if ((Object)(object)Responses != (Object)null)
			{
				Responses.NoticedSuspiciousPlayer(((Component)vEvent.Target).GetComponent<Player>());
			}
			break;
		case EVisualState.Brandishing:
			if ((Object)(object)Responses != (Object)null)
			{
				Responses.NoticePlayerBrandishingWeapon(((Component)vEvent.Target).GetComponent<Player>());
			}
			break;
		case EVisualState.DischargingWeapon:
			if ((Object)(object)Responses != (Object)null)
			{
				Responses.NoticePlayerDischargingWeapon(((Component)vEvent.Target).GetComponent<Player>());
			}
			break;
		case EVisualState.Visible:
			break;
		}
	}

	public void NoiseEvent(NoiseEvent nEvent)
	{
		if (!((Behaviour)this).enabled)
		{
			return;
		}
		if (nEvent.type == ENoiseType.Gunshot)
		{
			if (onGunshotHeard != null)
			{
				onGunshotHeard.Invoke(nEvent);
			}
			if ((Object)(object)Responses != (Object)null)
			{
				Responses.GunshotHeard(nEvent);
			}
		}
		if (nEvent.type == ENoiseType.Explosion)
		{
			if (onExplosionHeard != null)
			{
				onExplosionHeard.Invoke(nEvent);
			}
			if ((Object)(object)Responses != (Object)null)
			{
				Responses.ExplosionHeard(nEvent);
			}
		}
	}

	public void HitByCar(LandVehicle vehicle)
	{
		if (onHitByCar != null)
		{
			onHitByCar.Invoke(vehicle);
		}
		if ((Object)(object)Responses != (Object)null)
		{
			Responses.HitByCar(vehicle);
		}
	}
}
