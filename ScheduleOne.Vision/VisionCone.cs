using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.Audio;
using ScheduleOne.Core;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI.WorldspacePopup;
using ScheduleOne.Vehicles;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace ScheduleOne.Vision;

public class VisionCone : NetworkBehaviour
{
	public enum EEventLevel
	{
		Start,
		Half,
		Full,
		Zero
	}

	[Serializable]
	public class StateContainer
	{
		public EVisualState state;

		public bool Enabled;

		[Range(0.5f, 4f)]
		public float NoticeTimeMultiplier = 1f;

		public float RequiredNoticeTime => 0.2f * NoticeTimeMultiplier;

		public StateContainer GetCopy()
		{
			return new StateContainer
			{
				state = state,
				Enabled = Enabled,
				NoticeTimeMultiplier = NoticeTimeMultiplier
			};
		}
	}

	public class SightableData
	{
		public ISightable Sightable;

		public float VisionDelta;

		public float TimeVisible;
	}

	public delegate void EventStateChange(VisionEventReceipt _event);

	public const float VISION_UPDATE_INTERVAL = 0.1f;

	public const float MinVisionDelta = 0.075f;

	private const float ExclamationSoundCooldown = 1f;

	private static float TimeOnLastExclamationSound = 0f;

	public static float UniversalAttentivenessScale = 1f;

	public static float UniversalMemoryScale = 1f;

	public const float HorizontalFOV = 135f;

	public const float VerticalFOV = 100f;

	public const float Range = 25f;

	public const float MinorWidth = 3f;

	public const float MinorHeight = 1.5f;

	public bool DEBUG;

	public Transform VisionOrigin;

	[Header("Vision Settings")]
	public AnimationCurve VisionFalloff;

	public LayerMask VisibilityBlockingLayers;

	[Range(0f, 2f)]
	public float RangeMultiplier = 1f;

	[Header("Interest settings")]
	[FormerlySerializedAs("StatesOfInterest")]
	public List<StateContainer> DefaultStatesOfInterest = new List<StateContainer>();

	[Header("Notice Settings")]
	public float Attentiveness = 1f;

	public float Memory = 1f;

	[Header("Sound Settings")]
	public bool UseTremoloSound = true;

	[Header("Worldspace Icons")]
	public bool WorldspaceIconsEnabled = true;

	public WorldspacePopup QuestionMarkPopup;

	public WorldspacePopup ExclamationPointPopup;

	public AudioSourceController ExclamationSound;

	public EventStateChange onVisionEventStarted;

	public EventStateChange onVisionEventHalf;

	public EventStateChange onVisionEventFull;

	public EventStateChange onVisionEventExpired;

	protected List<ISightable> sightablesOfInterest = new List<ISightable>();

	protected Dictionary<ISightable, SightableData> sightableDatas = new Dictionary<ISightable, SightableData>();

	protected Dictionary<ISightable, Dictionary<EVisualState, StateContainer>> stateSettings = new Dictionary<ISightable, Dictionary<EVisualState, StateContainer>>();

	protected List<VisionEvent> activeVisionEvents = new List<VisionEvent>();

	protected List<VisionEvent> cachedVisionEvents = new List<VisionEvent>();

	protected NPC npc;

	protected bool noticeGeneralCrime;

	protected List<ISightable> sightablesSeenThisFrame = new List<ISightable>();

	protected List<ISightable> toRemove = new List<ISightable>();

	private bool NetworkInitialize___EarlyScheduleOne_002EVision_002EVisionConeAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EVision_002EVisionConeAssembly_002DCSharp_002Edll_Excuted;

	protected float effectiveRange => 25f * RangeMultiplier;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EVision_002EVisionCone_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	private void PlayerSpawned(Player plr)
	{
		Dictionary<EVisualState, StateContainer> dictionary = new Dictionary<EVisualState, StateContainer>();
		for (int i = 0; i < DefaultStatesOfInterest.Count; i++)
		{
			dictionary.Add(DefaultStatesOfInterest[i].state, DefaultStatesOfInterest[i].GetCopy());
		}
		stateSettings.Add(plr, dictionary);
		sightablesOfInterest.Add(plr);
	}

	private void OnEnable()
	{
		ClearEvents();
		sightableDatas.Clear();
	}

	private void OnDisable()
	{
		ClearEvents();
		sightableDatas.Clear();
	}

	protected virtual void VisionUpdate()
	{
		if (((Behaviour)this).enabled)
		{
			UpdateVision(0.1f);
			UpdateEvents(0.1f);
		}
	}

	protected virtual void UpdateEvents(float tickTime)
	{
		foreach (ISightable key in sightableDatas.Keys)
		{
			if ((key is Player && (Object)(object)(key as Player) != (Object)(object)Player.Local && !InstanceFinder.IsServer) || (!(key is Player) && !InstanceFinder.IsServer) || !key.IsCurrentlySightable())
			{
				continue;
			}
			foreach (EntityVisualState visualState in key.VisibilityComponent.VisualStates)
			{
				if (!stateSettings[key].ContainsKey(visualState.state) || !stateSettings[key][visualState.state].Enabled)
				{
					continue;
				}
				StateContainer stateContainer = stateSettings[key][visualState.state];
				if (GetEvent(key, visualState) == null)
				{
					VisionEvent item = new VisionEvent(this, key, visualState, stateContainer.RequiredNoticeTime, UseTremoloSound);
					activeVisionEvents.Add(item);
					if (onVisionEventStarted != null)
					{
						VisionEventReceipt visionEventReceipt = new VisionEventReceipt(key.NetworkObject, visualState.state);
						onVisionEventStarted(visionEventReceipt);
					}
				}
			}
		}
		cachedVisionEvents.Clear();
		cachedVisionEvents.AddRange(activeVisionEvents);
		foreach (VisionEvent cachedVisionEvent in cachedVisionEvents)
		{
			if (!stateSettings[cachedVisionEvent.Target].ContainsKey(cachedVisionEvent.State.state) || !stateSettings[cachedVisionEvent.Target][cachedVisionEvent.State.state].Enabled)
			{
				cachedVisionEvent.EndEvent();
			}
		}
		float num = 0f;
		ISightable local = Player.Local;
		for (int i = 0; i < activeVisionEvents.Count; i++)
		{
			VisionEvent visionEvent = activeVisionEvents[i];
			if (visionEvent.Target == local)
			{
				if (sightableDatas.ContainsKey(Player.Local))
				{
					visionEvent.UpdateEvent(sightableDatas[Player.Local].VisionDelta, tickTime);
				}
				else
				{
					visionEvent.UpdateEvent(0f, tickTime);
				}
				if (visionEvent.NormalizedNoticeLevel > num)
				{
					num = visionEvent.NormalizedNoticeLevel;
				}
			}
		}
		if (num > 0f && WorldspaceIconsEnabled)
		{
			((Behaviour)QuestionMarkPopup).enabled = true;
			QuestionMarkPopup.CurrentFillLevel = num;
		}
		else
		{
			((Behaviour)QuestionMarkPopup).enabled = false;
		}
	}

	protected virtual void UpdateVision(float tickTime)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)npc != (Object)null && !npc.IsConscious)
		{
			return;
		}
		sightablesSeenThisFrame.Clear();
		foreach (ISightable item in sightablesOfInterest)
		{
			if (item == null || !IsPointWithinSight(item.VisibilityComponent.CenterPoint, ignoreLoS: true))
			{
				continue;
			}
			float num = item.VisibilityComponent.CalculateExposureToPoint(VisionOrigin.position, effectiveRange, npc);
			if (item is Player)
			{
				Player player = (Player)item;
				if ((Object)(object)player.CurrentVehicle != (Object)null && IsPointWithinSight(((Component)player.CurrentVehicle).transform.position, ignoreLoS: false, ((Component)player.CurrentVehicle).GetComponent<LandVehicle>()))
				{
					num = 1f;
				}
			}
			if (DEBUG)
			{
				Console.Log("Sightable: " + ((Object)((Component)item.NetworkObject).gameObject).name + " Exposure: " + num);
			}
			if (!(num > 0f))
			{
				continue;
			}
			float num2 = num * VisionFalloff.Evaluate(Mathf.Clamp01(Vector3.Distance(VisionOrigin.position, item.VisibilityComponent.CenterPoint) / effectiveRange)) * item.VisibilityComponent.CurrentVisibility / 100f;
			if (DEBUG)
			{
				Console.Log("Vision delta: " + num2 + " for sightable: " + ((Object)((Component)item.NetworkObject).gameObject).name);
			}
			if (num2 > 0.075f)
			{
				sightablesSeenThisFrame.Add(item);
				if (!sightableDatas.ContainsKey(item))
				{
					SightableData sightableData = new SightableData();
					sightableData.Sightable = item;
					sightableDatas.Add(item, sightableData);
				}
				sightableDatas[item].TimeVisible += tickTime;
				sightableDatas[item].VisionDelta = num2;
			}
		}
		toRemove.AddRange(sightableDatas.Keys);
		foreach (ISightable item2 in toRemove)
		{
			if (!sightablesSeenThisFrame.Contains(item2))
			{
				sightableDatas.Remove(item2);
			}
		}
		toRemove.Clear();
	}

	public virtual void EventReachedZero(VisionEvent _event)
	{
		activeVisionEvents.Remove(_event);
		VisionEventReceipt receipt = new VisionEventReceipt(_event.Target.NetworkObject, _event.State.state);
		SendEventReceipt(receipt, EEventLevel.Zero);
	}

	public virtual void EventHalfNoticed(VisionEvent _event)
	{
		VisionEventReceipt receipt = new VisionEventReceipt(_event.Target.NetworkObject, _event.State.state);
		SendEventReceipt(receipt, EEventLevel.Half);
	}

	public virtual void EventFullyNoticed(VisionEvent _event)
	{
		activeVisionEvents.Remove(_event);
		if (WorldspaceIconsEnabled && (Object)(object)((Component)_event.Target.NetworkObject).GetComponent<Player>() == (Object)(object)Player.Local)
		{
			ExclamationPointPopup.Popup();
			if (Time.realtimeSinceStartup - TimeOnLastExclamationSound > 1f)
			{
				ExclamationSound.Play();
				TimeOnLastExclamationSound = Time.realtimeSinceStartup;
			}
		}
		VisionEventReceipt receipt = new VisionEventReceipt(_event.Target.NetworkObject, _event.State.state);
		SendEventReceipt(receipt, EEventLevel.Full);
	}

	[ServerRpc(RunLocally = true, RequireOwnership = false)]
	public void SendEventReceipt(VisionEventReceipt receipt, EEventLevel level)
	{
		RpcWriter___Server_SendEventReceipt_3486014028(receipt, level);
		RpcLogic___SendEventReceipt_3486014028(receipt, level);
	}

	[ObserversRpc(RunLocally = true, ExcludeOwner = true)]
	public virtual void ReceiveEventReceipt(VisionEventReceipt receipt, EEventLevel level)
	{
		RpcWriter___Observers_ReceiveEventReceipt_3486014028(receipt, level);
		RpcLogic___ReceiveEventReceipt_3486014028(receipt, level);
	}

	public void AddSightableOfInterest(ISightable s)
	{
		if (!sightablesOfInterest.Contains(s))
		{
			sightablesOfInterest.Add(s);
		}
	}

	public void RemoveSightableOfInterest(ISightable s)
	{
		if (!(s is Player) && sightablesOfInterest.Contains(s))
		{
			sightablesOfInterest.Remove(s);
		}
	}

	public void SetSightableStateEnabled(ISightable sightable, EVisualState state, bool enabled)
	{
		if (!stateSettings.TryGetValue(sightable, out var value))
		{
			Console.LogWarning("No state settings for sightable: " + ((Object)((Component)sightable.NetworkObject).gameObject).name);
			return;
		}
		StateContainer stateContainer = value[state];
		if (stateContainer != null && stateContainer.Enabled != enabled)
		{
			stateContainer.Enabled = enabled;
		}
	}

	[Button]
	public void PrintSightableStates()
	{
		foreach (KeyValuePair<ISightable, Dictionary<EVisualState, StateContainer>> stateSetting in stateSettings)
		{
			string text = "";
			foreach (KeyValuePair<EVisualState, StateContainer> item in stateSetting.Value)
			{
				text = text + item.Key.ToString() + ": " + item.Value.Enabled + ", ";
			}
			Console.Log("Sightable: " + ((Object)((Component)stateSetting.Key.NetworkObject).gameObject).name + " States: " + text);
		}
	}

	public virtual bool IsPointWithinSight(Vector3 point, bool ignoreLoS = false, LandVehicle vehicleToIgnore = null)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		if (Vector3.Distance(point, VisionOrigin.position) > effectiveRange)
		{
			return false;
		}
		Vector3 forward = VisionOrigin.forward;
		Vector3 val = point - VisionOrigin.position;
		if (Vector3.SignedAngle(forward, ((Vector3)(ref val)).normalized, VisionOrigin.up) > 90f)
		{
			return false;
		}
		Vector3 forward2 = VisionOrigin.forward;
		val = point - VisionOrigin.position;
		if (Vector3.SignedAngle(forward2, ((Vector3)(ref val)).normalized, VisionOrigin.right) > 90f)
		{
			return false;
		}
		Plane[] frustumPlanes = GetFrustumPlanes();
		for (int i = 0; i < 6; i++)
		{
			if (((Plane)(ref frustumPlanes[i])).GetDistanceToPoint(point) > 0f)
			{
				return false;
			}
		}
		RaycastHit val2 = default(RaycastHit);
		if (!ignoreLoS && Physics.Raycast(VisionOrigin.position, point - VisionOrigin.position, ref val2, Vector3.Distance(point, VisionOrigin.position), LayerMask.op_Implicit(VisibilityBlockingLayers)))
		{
			if ((Object)(object)vehicleToIgnore != (Object)null && (Object)(object)((Component)((RaycastHit)(ref val2)).collider).GetComponentInParent<LandVehicle>() == (Object)(object)vehicleToIgnore)
			{
				return true;
			}
			return false;
		}
		return true;
	}

	public VisionEvent GetEvent(ISightable target, EntityVisualState state)
	{
		return activeVisionEvents.Find((VisionEvent x) => x.Target == target && x.State == state);
	}

	public bool IsPlayerVisible(Player player)
	{
		return WasSightableVisibleThisFrame(player);
	}

	public bool WasSightableVisibleThisFrame(ISightable sightable)
	{
		if (sightableDatas.ContainsKey(sightable))
		{
			return sightableDatas[sightable].VisionDelta > 0.075f;
		}
		return false;
	}

	public bool IsTargetVisible(ISightable target)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (target is Player player)
		{
			return IsPlayerVisible(player);
		}
		if (target is NPC nPC)
		{
			return IsPointWithinSight(nPC.Avatar.CenterPoint, ignoreLoS: true);
		}
		return false;
	}

	public float GetPlayerVisibility(Player player)
	{
		if (sightableDatas.ContainsKey(player))
		{
			return sightableDatas[player].VisionDelta;
		}
		if (DEBUG)
		{
			Console.Log("No sight key");
		}
		return 0f;
	}

	public bool IsPlayerVisible(Player player, out SightableData data)
	{
		if (sightableDatas.ContainsKey(player))
		{
			data = sightableDatas[player];
			return true;
		}
		data = null;
		return false;
	}

	public virtual void SetNoticePlayerCrimes(Player player, bool active)
	{
		SetSightableStateEnabled(player, EVisualState.DisobeyingCurfew, active);
		SetSightableStateEnabled(player, EVisualState.DrugDealing, active);
		SetSightableStateEnabled(player, EVisualState.Vandalizing, active);
		SetSightableStateEnabled(player, EVisualState.Pickpocketing, active);
		SetSightableStateEnabled(player, EVisualState.DischargingWeapon, active);
		SetSightableStateEnabled(player, EVisualState.Brandishing, active);
	}

	private void OnDie()
	{
		ClearEvents();
	}

	public void ClearEvents()
	{
		((Behaviour)ExclamationPointPopup).enabled = false;
		((Behaviour)QuestionMarkPopup).enabled = false;
		VisionEvent[] array = activeVisionEvents.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].EndEvent();
		}
		activeVisionEvents.Clear();
	}

	private Vector3[] GetFrustumVertices()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = VisionOrigin.position;
		Quaternion rotation = VisionOrigin.rotation;
		float num = 0f;
		float num2 = effectiveRange;
		float num3 = 3f;
		float num4 = 1.5f;
		float num5 = num3 + 2f * effectiveRange * Mathf.Tan((float)System.Math.PI * 3f / 8f);
		float num6 = num4 + 2f * effectiveRange * Mathf.Tan(0.87266463f);
		Vector3[] array = (Vector3[])(object)new Vector3[8];
		Vector3 val = position + rotation * new Vector3((0f - num3) / 2f, num4 / 2f, num);
		Vector3 val2 = position + rotation * new Vector3(num3 / 2f, num4 / 2f, num);
		Vector3 val3 = position + rotation * new Vector3((0f - num3) / 2f, (0f - num4) / 2f, num);
		Vector3 val4 = position + rotation * new Vector3(num3 / 2f, (0f - num4) / 2f, num);
		Vector3 val5 = position + rotation * new Vector3((0f - num5) / 2f, num6 / 2f, num2);
		Vector3 val6 = position + rotation * new Vector3(num5 / 2f, num6 / 2f, num2);
		Vector3 val7 = position + rotation * new Vector3((0f - num5) / 2f, (0f - num6) / 2f, num2);
		Vector3 val8 = position + rotation * new Vector3(num5 / 2f, (0f - num6) / 2f, num2);
		array[0] = val;
		array[1] = val2;
		array[2] = val3;
		array[3] = val4;
		array[4] = val5;
		array[5] = val6;
		array[6] = val7;
		array[7] = val8;
		Debug.DrawLine(val, val5, Color.red);
		Debug.DrawLine(val2, val6, Color.green);
		Debug.DrawLine(val3, val7, Color.blue);
		Debug.DrawLine(val4, val8, Color.magenta);
		return array;
	}

	private Plane[] GetFrustumPlanes()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = VisionOrigin.position;
		Quaternion rotation = VisionOrigin.rotation;
		float num = 0f;
		float num2 = effectiveRange;
		float num3 = 3f;
		float num4 = 1.5f;
		float num5 = num3 + 2f * effectiveRange * Mathf.Tan((float)System.Math.PI * 3f / 8f);
		float num6 = num4 + 2f * effectiveRange * Mathf.Tan(0.87266463f);
		Plane[] array = (Plane[])(object)new Plane[6];
		Vector3 val = position + rotation * new Vector3((0f - num3) / 2f, num4 / 2f, num);
		Vector3 val2 = position + rotation * new Vector3(num3 / 2f, num4 / 2f, num);
		Vector3 val3 = position + rotation * new Vector3((0f - num3) / 2f, (0f - num4) / 2f, num);
		Vector3 val4 = position + rotation * new Vector3(num3 / 2f, (0f - num4) / 2f, num);
		Vector3 val5 = position + rotation * new Vector3((0f - num5) / 2f, num6 / 2f, num2);
		Vector3 val6 = position + rotation * new Vector3(num5 / 2f, num6 / 2f, num2);
		Vector3 val7 = position + rotation * new Vector3((0f - num5) / 2f, (0f - num6) / 2f, num2);
		Vector3 val8 = position + rotation * new Vector3(num5 / 2f, (0f - num6) / 2f, num2);
		array[0] = new Plane(val2, val, val5);
		array[1] = new Plane(val3, val4, val8);
		array[2] = new Plane(val, val3, val7);
		array[3] = new Plane(val4, val2, val6);
		array[4] = new Plane(val, val2, val4);
		array[5] = new Plane(val6, val5, val7);
		return array;
	}

	public override void NetworkInitialize___Early()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EVision_002EVisionConeAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EVision_002EVisionConeAssembly_002DCSharp_002Edll_Excuted = true;
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_SendEventReceipt_3486014028));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_ReceiveEventReceipt_3486014028));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EVision_002EVisionConeAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EVision_002EVisionConeAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_SendEventReceipt_3486014028(VisionEventReceipt receipt, EEventLevel level)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkBehaviour)this).IsClientInitialized)
		{
			NetworkManager networkManager = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager == null)
			{
				networkManager = InstanceFinder.NetworkManager;
			}
			if (networkManager != null)
			{
				networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			GeneratedWriters___Internal.Write___ScheduleOne_002EVision_002EVisionEventReceiptFishNet_002ESerializing_002EGenerated((Writer)(object)writer, receipt);
			GeneratedWriters___Internal.Write___ScheduleOne_002EVision_002EVisionCone_002FEEventLevelFishNet_002ESerializing_002EGenerated((Writer)(object)writer, level);
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendEventReceipt_3486014028(VisionEventReceipt receipt, EEventLevel level)
	{
		ReceiveEventReceipt(receipt, level);
	}

	private void RpcReader___Server_SendEventReceipt_3486014028(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		VisionEventReceipt receipt = GeneratedReaders___Internal.Read___ScheduleOne_002EVision_002EVisionEventReceiptFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		EEventLevel level = GeneratedReaders___Internal.Read___ScheduleOne_002EVision_002EVisionCone_002FEEventLevelFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendEventReceipt_3486014028(receipt, level);
		}
	}

	private void RpcWriter___Observers_ReceiveEventReceipt_3486014028(VisionEventReceipt receipt, EEventLevel level)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkBehaviour)this).IsServerInitialized)
		{
			NetworkManager networkManager = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager == null)
			{
				networkManager = InstanceFinder.NetworkManager;
			}
			if (networkManager != null)
			{
				networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			GeneratedWriters___Internal.Write___ScheduleOne_002EVision_002EVisionEventReceiptFishNet_002ESerializing_002EGenerated((Writer)(object)writer, receipt);
			GeneratedWriters___Internal.Write___ScheduleOne_002EVision_002EVisionCone_002FEEventLevelFishNet_002ESerializing_002EGenerated((Writer)(object)writer, level);
			((NetworkBehaviour)this).SendObserversRpc(1u, writer, val, (DataOrderType)0, false, false, true);
			writer.Store();
		}
	}

	public virtual void RpcLogic___ReceiveEventReceipt_3486014028(VisionEventReceipt receipt, EEventLevel level)
	{
		switch (level)
		{
		case EEventLevel.Start:
			if (onVisionEventStarted != null)
			{
				onVisionEventStarted(receipt);
			}
			break;
		case EEventLevel.Half:
			if (onVisionEventHalf != null)
			{
				onVisionEventHalf(receipt);
			}
			break;
		case EEventLevel.Full:
			if (onVisionEventFull != null)
			{
				onVisionEventFull(receipt);
			}
			break;
		case EEventLevel.Zero:
			if (onVisionEventExpired != null)
			{
				onVisionEventExpired(receipt);
			}
			break;
		}
	}

	private void RpcReader___Observers_ReceiveEventReceipt_3486014028(PooledReader PooledReader0, Channel channel)
	{
		VisionEventReceipt receipt = GeneratedReaders___Internal.Read___ScheduleOne_002EVision_002EVisionEventReceiptFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		EEventLevel level = GeneratedReaders___Internal.Read___ScheduleOne_002EVision_002EVisionCone_002FEEventLevelFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___ReceiveEventReceipt_3486014028(receipt, level);
		}
	}

	protected virtual void Awake_UserLogic_ScheduleOne_002EVision_002EVisionCone_Assembly_002DCSharp_002Edll()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Expected O, but got Unknown
		if ((Object)(object)VisionOrigin == (Object)null)
		{
			VisionOrigin = ((Component)this).transform;
		}
		npc = ((Component)this).GetComponentInParent<NPC>();
		for (int i = 0; i < Player.PlayerList.Count; i++)
		{
			PlayerSpawned(Player.PlayerList[i]);
		}
		Player.onPlayerSpawned = (Action<Player>)Delegate.Combine(Player.onPlayerSpawned, new Action<Player>(PlayerSpawned));
		if ((Object)(object)npc != (Object)null)
		{
			npc.Health.onDie.AddListener(new UnityAction(OnDie));
			npc.Health.onKnockedOut.AddListener(new UnityAction(OnDie));
		}
		((MonoBehaviour)this).InvokeRepeating("VisionUpdate", Random.Range(0f, 0.1f), 0.1f);
	}
}
