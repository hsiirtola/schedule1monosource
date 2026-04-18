using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Object.Synchronizing;
using FishNet.Object.Synchronizing.Internal;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Law;
using ScheduleOne.Levelling;
using ScheduleOne.Map;
using ScheduleOne.NPCs;
using ScheduleOne.Police;
using ScheduleOne.UI;
using ScheduleOne.Vision;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.PlayerScripts;

public class PlayerCrimeData : NetworkBehaviour
{
	public class VehicleCollisionInstance
	{
		public NPC Victim;

		public float TimeSince;

		public VehicleCollisionInstance(NPC victim, float timeSince)
		{
			Victim = victim;
			TimeSince = timeSince;
		}
	}

	public enum EPursuitLevel
	{
		None,
		Investigating,
		Arresting,
		NonLethal,
		Lethal
	}

	public const float SEARCH_TIME_INVESTIGATING = 60f;

	public const float SEARCH_TIME_ARRESTING = 25f;

	public const float SEARCH_TIME_NONLETHAL = 30f;

	public const float SEARCH_TIME_LETHAL = 40f;

	public const float ESCALATION_TIME_ARRESTING = 25f;

	public const float ESCALATION_TIME_NONLETHAL = 120f;

	public const float SHOT_COOLDOWN_MIN = 2f;

	public const float SHOT_COOLDOWN_MAX = 8f;

	public const float VEHICLE_COLLISION_LIFETIME = 30f;

	public const float VEHICLE_COLLISION_LIMIT = 3f;

	public PoliceOfficer NearestOfficer;

	public Player Player;

	public AudioSourceController onPursuitEscapedSound;

	[CompilerGenerated]
	[SyncVar(/*Could not decode attribute arguments.*/)]
	public EPursuitLevel _003CCurrentPursuitLevel_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar(/*Could not decode attribute arguments.*/)]
	public Vector3 _003CLastKnownPosition_003Ek__BackingField;

	public List<PoliceOfficer> Pursuers;

	public float TimeSincePursuitStart;

	public float CurrentPursuitLevelDuration;

	public float TimeSinceSighted;

	public Dictionary<Crime, int> Crimes;

	public bool BodySearchPending;

	public Action<EPursuitLevel, EPursuitLevel> onPursuitLevelChange;

	protected List<VehicleCollisionInstance> Collisions;

	public SyncVar<EPursuitLevel> syncVar____003CCurrentPursuitLevel_003Ek__BackingField;

	public SyncVar<Vector3> syncVar____003CLastKnownPosition_003Ek__BackingField;

	private bool NetworkInitialize___EarlyScheduleOne_002EPlayerScripts_002EPlayerCrimeDataAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EPlayerScripts_002EPlayerCrimeDataAssembly_002DCSharp_002Edll_Excuted;

	public EPursuitLevel CurrentPursuitLevel
	{
		[CompilerGenerated]
		get
		{
			return SyncAccessor__003CCurrentPursuitLevel_003Ek__BackingField;
		}
		[CompilerGenerated]
		protected set
		{
			this.sync___set_value__003CCurrentPursuitLevel_003Ek__BackingField(value, true);
		}
	}

	public Vector3 LastKnownPosition
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return SyncAccessor__003CLastKnownPosition_003Ek__BackingField;
		}
		[CompilerGenerated]
		[ServerRpc(RunLocally = true)]
		protected set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			RpcWriter___Server_set_LastKnownPosition_4276783012(value);
			RpcLogic___set_LastKnownPosition_4276783012(value);
		}
	}

	public float CurrentArrestProgress { get; protected set; }

	public float CurrentBodySearchProgress { get; protected set; }

	public int MinsSinceLastArrested { get; set; }

	public float TimeSinceLastBodySearch { get; set; }

	public bool EvadedArrest { get; protected set; }

	public EPursuitLevel SyncAccessor__003CCurrentPursuitLevel_003Ek__BackingField
	{
		get
		{
			return CurrentPursuitLevel;
		}
		set
		{
			if (value || !((NetworkBehaviour)this).IsServerInitialized)
			{
				CurrentPursuitLevel = value;
			}
			if (Application.isPlaying)
			{
				syncVar____003CCurrentPursuitLevel_003Ek__BackingField.SetValue(value, value);
			}
		}
	}

	public Vector3 SyncAccessor__003CLastKnownPosition_003Ek__BackingField
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return LastKnownPosition;
		}
		set
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			if (value || !((NetworkBehaviour)this).IsServerInitialized)
			{
				LastKnownPosition = value;
			}
			if (Application.isPlaying)
			{
				syncVar____003CLastKnownPosition_003Ek__BackingField.SetValue(value, value);
			}
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EPlayerScripts_002EPlayerCrimeData_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	private void Start()
	{
		TimeManager instance = NetworkSingleton<TimeManager>.Instance;
		instance.onSleepStart = (Action)Delegate.Remove(instance.onSleepStart, new Action(OnSleepStart));
		TimeManager instance2 = NetworkSingleton<TimeManager>.Instance;
		instance2.onSleepStart = (Action)Delegate.Combine(instance2.onSleepStart, new Action(OnSleepStart));
		NetworkSingleton<TimeManager>.Instance.onMinutePass -= new Action(MinPass);
		NetworkSingleton<TimeManager>.Instance.onMinutePass += new Action(MinPass);
	}

	private void OnDestroy()
	{
		if (NetworkSingleton<TimeManager>.InstanceExists)
		{
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onSleepStart = (Action)Delegate.Remove(instance.onSleepStart, new Action(OnSleepStart));
		}
	}

	protected virtual void Update()
	{
		CurrentPursuitLevelDuration += Time.deltaTime;
		TimeSincePursuitStart += Time.deltaTime;
		TimeSinceSighted += Time.deltaTime;
		TimeSinceLastBodySearch += Time.deltaTime;
		if (!((NetworkBehaviour)Player).IsOwner)
		{
			return;
		}
		if (SyncAccessor__003CCurrentPursuitLevel_003Ek__BackingField != EPursuitLevel.None && SyncAccessor__003CCurrentPursuitLevel_003Ek__BackingField != EPursuitLevel.Lethal)
		{
			UpdateEscalation();
		}
		if (SyncAccessor__003CCurrentPursuitLevel_003Ek__BackingField != EPursuitLevel.None)
		{
			UpdateTimeout();
		}
		for (int i = 0; i < Collisions.Count; i++)
		{
			Collisions[i].TimeSince += Time.deltaTime;
			if (Collisions[i].TimeSince > 30f)
			{
				Collisions.RemoveAt(i);
				i--;
			}
		}
		Singleton<HUD>.Instance.CrimeStatusUI.UpdateStatus();
		if ((float)Collisions.Count >= 3f)
		{
			RecordLastKnownPosition(resetTimeSinceSighted: true);
			SetPursuitLevel(EPursuitLevel.Investigating);
			AddCrime(new VehicularAssault(), Collisions.Count - 1);
			Singleton<LawManager>.Instance.PoliceCalled(Player, new VehicularAssault());
			Collisions.Clear();
		}
	}

	private void MinPass()
	{
		MinsSinceLastArrested++;
	}

	protected virtual void LateUpdate()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		if (CurrentArrestProgress > 0f)
		{
			Singleton<ProgressSlider>.Instance.Configure("Cuffing...", Color32.op_Implicit(new Color32((byte)75, (byte)165, byte.MaxValue, byte.MaxValue)));
			Singleton<ProgressSlider>.Instance.ShowProgress(CurrentArrestProgress);
		}
		else if (CurrentBodySearchProgress > 0f)
		{
			Singleton<ProgressSlider>.Instance.Configure("Being searched...", Color32.op_Implicit(new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue)));
			Singleton<ProgressSlider>.Instance.ShowProgress(CurrentBodySearchProgress);
		}
		CurrentArrestProgress = 0f;
		CurrentBodySearchProgress = 0f;
	}

	public void SetPursuitLevel(EPursuitLevel level)
	{
		if (!GameManager.IS_TUTORIAL)
		{
			EPursuitLevel ePursuitLevel = SyncAccessor__003CCurrentPursuitLevel_003Ek__BackingField;
			SetPursuitLevel_Server(level);
			if (level != EPursuitLevel.None)
			{
				BodySearchPending = false;
			}
			if (ePursuitLevel == EPursuitLevel.None && level != EPursuitLevel.None)
			{
				TimeSincePursuitStart = 0f;
				TimeSinceSighted = 0f;
				Player.VisualState.ApplyState("Wanted", EVisualState.Wanted);
			}
			if (ePursuitLevel != EPursuitLevel.None && level == EPursuitLevel.None)
			{
				ClearCrimes();
				Player.VisualState.RemoveState("Wanted");
			}
			CurrentPursuitLevelDuration = 0f;
			if (onPursuitLevelChange != null)
			{
				onPursuitLevelChange(ePursuitLevel, level);
			}
			if (((NetworkBehaviour)Player).IsOwner)
			{
				Singleton<HUD>.Instance.CrimeStatusUI.UpdateStatus();
			}
		}
	}

	[ServerRpc(RunLocally = true, RequireOwnership = false)]
	private void SetPursuitLevel_Server(EPursuitLevel level)
	{
		RpcWriter___Server_SetPursuitLevel_Server_2979171596(level);
		RpcLogic___SetPursuitLevel_Server_2979171596(level);
	}

	public void Escalate()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		if (SyncAccessor__003CCurrentPursuitLevel_003Ek__BackingField == EPursuitLevel.None)
		{
			SetPursuitLevel(EPursuitLevel.Investigating);
		}
		else if (SyncAccessor__003CCurrentPursuitLevel_003Ek__BackingField == EPursuitLevel.Investigating)
		{
			SetPursuitLevel(EPursuitLevel.Arresting);
		}
		else if (SyncAccessor__003CCurrentPursuitLevel_003Ek__BackingField == EPursuitLevel.Arresting)
		{
			SetEvaded();
			SetPursuitLevel(EPursuitLevel.NonLethal);
			if (PoliceStation.GetClosestPoliceStation(Player.Avatar.MiddleSpineRB.position).TimeSinceLastDispatch > 10f)
			{
				PoliceStation.GetClosestPoliceStation(Player.Avatar.MiddleSpineRB.position).Dispatch(1, Player, PoliceStation.EDispatchType.Auto, beginAsSighted: true);
			}
		}
		else if (SyncAccessor__003CCurrentPursuitLevel_003Ek__BackingField == EPursuitLevel.NonLethal)
		{
			SetPursuitLevel(EPursuitLevel.Lethal);
			PoliceStation.GetClosestPoliceStation(Player.Avatar.MiddleSpineRB.position);
			PoliceStation.GetClosestPoliceStation(Player.Avatar.MiddleSpineRB.position).Dispatch(1, Player, PoliceStation.EDispatchType.Auto, beginAsSighted: true);
		}
	}

	public void Deescalate()
	{
		if (SyncAccessor__003CCurrentPursuitLevel_003Ek__BackingField == EPursuitLevel.Investigating)
		{
			SetPursuitLevel(EPursuitLevel.None);
		}
		else if (SyncAccessor__003CCurrentPursuitLevel_003Ek__BackingField == EPursuitLevel.Arresting)
		{
			SetPursuitLevel(EPursuitLevel.Investigating);
		}
		else if (SyncAccessor__003CCurrentPursuitLevel_003Ek__BackingField == EPursuitLevel.NonLethal)
		{
			SetPursuitLevel(EPursuitLevel.Arresting);
		}
		else if (SyncAccessor__003CCurrentPursuitLevel_003Ek__BackingField == EPursuitLevel.Lethal)
		{
			SetPursuitLevel(EPursuitLevel.NonLethal);
		}
	}

	[ObserversRpc(RunLocally = true)]
	public void RecordLastKnownPosition(bool resetTimeSinceSighted)
	{
		RpcWriter___Observers_RecordLastKnownPosition_1140765316(resetTimeSinceSighted);
		RpcLogic___RecordLastKnownPosition_1140765316(resetTimeSinceSighted);
	}

	public void SetArrestProgress(float progress)
	{
		CurrentArrestProgress = progress;
		if (progress >= 1f)
		{
			Player.Arrest_Server();
			SetPursuitLevel(EPursuitLevel.None);
		}
	}

	public void ResetBodysearchCooldown()
	{
		TimeSinceLastBodySearch = 0f;
	}

	public void SetBodySearchProgress(float progress)
	{
		CurrentBodySearchProgress = progress;
		if (CurrentBodySearchProgress >= 1f)
		{
			TimeSinceLastBodySearch = 0f;
			BodySearchPending = false;
		}
	}

	private void OnDie()
	{
		if (SyncAccessor__003CCurrentPursuitLevel_003Ek__BackingField != EPursuitLevel.None)
		{
			SetArrestProgress(1f);
		}
	}

	public void AddCrime(Crime crime, int quantity = 1)
	{
		if (crime == null)
		{
			return;
		}
		Debug.Log((object)("Adding crime: " + crime));
		Crime[] array = Crimes.Keys.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].GetType() == crime.GetType())
			{
				Crimes[array[i]] += quantity;
				return;
			}
		}
		Crimes.Add(crime, quantity);
	}

	public void ClearCrimes()
	{
		Crimes.Clear();
		EvadedArrest = false;
	}

	public bool IsCrimeOnRecord(Type crime)
	{
		Crime[] array = Crimes.Keys.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].GetType() == crime)
			{
				return true;
			}
		}
		return false;
	}

	public void SetEvaded()
	{
		EvadedArrest = true;
	}

	private void OnSleepStart()
	{
		if (SyncAccessor__003CCurrentPursuitLevel_003Ek__BackingField != EPursuitLevel.None)
		{
			SetPursuitLevel(EPursuitLevel.None);
			ClearCrimes();
		}
	}

	private void UpdateEscalation()
	{
		if (TimeSinceSighted > 1f)
		{
			return;
		}
		if (SyncAccessor__003CCurrentPursuitLevel_003Ek__BackingField == EPursuitLevel.Arresting)
		{
			if (CurrentPursuitLevelDuration > 25f)
			{
				Escalate();
			}
		}
		else if (SyncAccessor__003CCurrentPursuitLevel_003Ek__BackingField == EPursuitLevel.NonLethal && CurrentPursuitLevelDuration > 120f)
		{
			Escalate();
		}
	}

	private void UpdateTimeout()
	{
		if (((NetworkBehaviour)Player).IsOwner && TimeSinceSighted > GetSearchTime() + 3f)
		{
			TimeoutPursuit();
		}
	}

	private void TimeoutPursuit()
	{
		switch (SyncAccessor__003CCurrentPursuitLevel_003Ek__BackingField)
		{
		case EPursuitLevel.Arresting:
			NetworkSingleton<LevelManager>.Instance.AddXP(20);
			break;
		case EPursuitLevel.NonLethal:
			NetworkSingleton<LevelManager>.Instance.AddXP(40);
			break;
		case EPursuitLevel.Lethal:
			NetworkSingleton<LevelManager>.Instance.AddXP(60);
			break;
		}
		onPursuitEscapedSound.Play();
		SetPursuitLevel(EPursuitLevel.None);
		ClearCrimes();
	}

	public float GetSearchTime()
	{
		return SyncAccessor__003CCurrentPursuitLevel_003Ek__BackingField switch
		{
			EPursuitLevel.Investigating => 60f, 
			EPursuitLevel.Arresting => 25f, 
			EPursuitLevel.NonLethal => 30f, 
			EPursuitLevel.Lethal => 40f, 
			_ => 0f, 
		};
	}

	public float GetShotAccuracyMultiplier()
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		float num = 1f;
		if (Player.Health.TimeSinceLastDamage < 2f)
		{
			num = 0f;
		}
		if (Player.Health.TimeSinceLastDamage < 8f)
		{
			num = 1f - (Player.Health.TimeSinceLastDamage - 2f) / 6f;
		}
		Vector3 velocity = Player.VelocityCalculator.Velocity;
		float num2 = Mathf.Clamp01(Mathf.InverseLerp(0f, 6.1749997f, ((Vector3)(ref velocity)).magnitude));
		float num3 = Mathf.Lerp(2f, 0.5f, num2);
		int num4 = 0;
		for (int i = 0; i < PoliceOfficer.Officers.Count; i++)
		{
			if (PoliceOfficer.Officers[i].PursuitBehaviour.Active && (Object)(object)PoliceOfficer.Officers[i].PursuitBehaviour.TargetPlayer == (Object)(object)Player && Vector3.Distance(((Component)PoliceOfficer.Officers[i]).transform.position, Player.Avatar.CenterPoint) < 20f)
			{
				num4++;
			}
		}
		float num5 = Mathf.Lerp(1f, 0.6f, Mathf.Clamp01((float)num4 / 3f));
		return num * num3 * num5;
	}

	public void RecordVehicleCollision(NPC victim)
	{
		VehicleCollisionInstance item = new VehicleCollisionInstance(victim, 0f);
		Collisions.Add(item);
	}

	private void CheckNearestOfficer()
	{
		if (!((Object)(object)Player == (Object)null))
		{
			NearestOfficer = PoliceOfficer.Officers.OrderBy((PoliceOfficer x) => Vector3.Distance(x.Avatar.CenterPoint, Player.Avatar.CenterPoint)).FirstOrDefault();
		}
	}

	public PlayerCrimeData()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		LastKnownPosition = Vector3.zero;
		Pursuers = new List<PoliceOfficer>();
		MinsSinceLastArrested = 9999;
		TimeSinceSighted = 100000f;
		Crimes = new Dictionary<Crime, int>();
		TimeSinceLastBodySearch = 100000f;
		Collisions = new List<VehicleCollisionInstance>();
		((NetworkBehaviour)this)._002Ector();
	}

	public override void NetworkInitialize___Early()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Expected O, but got Unknown
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Expected O, but got Unknown
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EPlayerScripts_002EPlayerCrimeDataAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EPlayerScripts_002EPlayerCrimeDataAssembly_002DCSharp_002Edll_Excuted = true;
			syncVar____003CLastKnownPosition_003Ek__BackingField = new SyncVar<Vector3>((NetworkBehaviour)(object)this, 1u, (WritePermission)1, (ReadPermission)0, 0.5f, (Channel)0, LastKnownPosition);
			syncVar____003CCurrentPursuitLevel_003Ek__BackingField = new SyncVar<EPursuitLevel>((NetworkBehaviour)(object)this, 0u, (WritePermission)1, (ReadPermission)0, 0.5f, (Channel)0, CurrentPursuitLevel);
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_set_LastKnownPosition_4276783012));
			((NetworkBehaviour)this).RegisterServerRpc(1u, new ServerRpcDelegate(RpcReader___Server_SetPursuitLevel_Server_2979171596));
			((NetworkBehaviour)this).RegisterObserversRpc(2u, new ClientRpcDelegate(RpcReader___Observers_RecordLastKnownPosition_1140765316));
			((NetworkBehaviour)this).RegisterSyncVarRead(new SyncVarReadDelegate(ReadSyncVar___ScheduleOne_002EPlayerScripts_002EPlayerCrimeData));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EPlayerScripts_002EPlayerCrimeDataAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EPlayerScripts_002EPlayerCrimeDataAssembly_002DCSharp_002Edll_Excuted = true;
			((SyncBase)syncVar____003CLastKnownPosition_003Ek__BackingField).SetRegistered();
			((SyncBase)syncVar____003CCurrentPursuitLevel_003Ek__BackingField).SetRegistered();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_set_LastKnownPosition_4276783012(Vector3 value)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
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
		else if (!((NetworkBehaviour)this).IsOwner)
		{
			NetworkManager networkManager2 = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager2 == null)
			{
				networkManager2 = InstanceFinder.NetworkManager;
			}
			if (networkManager2 != null)
			{
				networkManager2.LogWarning("Cannot complete action because you are not the owner of this object. .");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because you are not the owner of this object. .");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((Writer)writer).WriteVector3(value);
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	[SpecialName]
	protected void RpcLogic___set_LastKnownPosition_4276783012(Vector3 value)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		this.sync___set_value__003CLastKnownPosition_003Ek__BackingField(value, true);
	}

	private void RpcReader___Server_set_LastKnownPosition_4276783012(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		Vector3 value = ((Reader)PooledReader0).ReadVector3();
		if (((NetworkBehaviour)this).IsServerInitialized && ((NetworkBehaviour)this).OwnerMatches(conn) && !conn.IsLocalClient)
		{
			RpcLogic___set_LastKnownPosition_4276783012(value);
		}
	}

	private void RpcWriter___Server_SetPursuitLevel_Server_2979171596(EPursuitLevel level)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EPlayerScripts_002EPlayerCrimeData_002FEPursuitLevelFishNet_002ESerializing_002EGenerated((Writer)(object)writer, level);
			((NetworkBehaviour)this).SendServerRpc(1u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___SetPursuitLevel_Server_2979171596(EPursuitLevel level)
	{
		CurrentPursuitLevel = level;
		Debug.Log((object)(Player.PlayerName + " new pursuit level: " + level));
	}

	private void RpcReader___Server_SetPursuitLevel_Server_2979171596(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		EPursuitLevel level = GeneratedReaders___Internal.Read___ScheduleOne_002EPlayerScripts_002EPlayerCrimeData_002FEPursuitLevelFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetPursuitLevel_Server_2979171596(level);
		}
	}

	private void RpcWriter___Observers_RecordLastKnownPosition_1140765316(bool resetTimeSinceSighted)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteBoolean(resetTimeSinceSighted);
			((NetworkBehaviour)this).SendObserversRpc(2u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___RecordLastKnownPosition_1140765316(bool resetTimeSinceSighted)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		LastKnownPosition = Player.Avatar.CenterPoint;
		if (resetTimeSinceSighted)
		{
			TimeSinceSighted = 0f;
		}
	}

	private void RpcReader___Observers_RecordLastKnownPosition_1140765316(PooledReader PooledReader0, Channel channel)
	{
		bool resetTimeSinceSighted = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___RecordLastKnownPosition_1140765316(resetTimeSinceSighted);
		}
	}

	public override bool ReadSyncVar___ScheduleOne_002EPlayerScripts_002EPlayerCrimeData(PooledReader PooledReader0, uint UInt321, bool Boolean2)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		switch (UInt321)
		{
		case 1u:
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value__003CLastKnownPosition_003Ek__BackingField(syncVar____003CLastKnownPosition_003Ek__BackingField.GetValue(true), true);
				return true;
			}
			Vector3 value2 = ((Reader)PooledReader0).ReadVector3();
			this.sync___set_value__003CLastKnownPosition_003Ek__BackingField(value2, Boolean2);
			return true;
		}
		case 0u:
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value__003CCurrentPursuitLevel_003Ek__BackingField(syncVar____003CCurrentPursuitLevel_003Ek__BackingField.GetValue(true), true);
				return true;
			}
			EPursuitLevel value = GeneratedReaders___Internal.Read___ScheduleOne_002EPlayerScripts_002EPlayerCrimeData_002FEPursuitLevelFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
			this.sync___set_value__003CCurrentPursuitLevel_003Ek__BackingField(value, Boolean2);
			return true;
		}
		default:
			return false;
		}
	}

	private void Awake_UserLogic_ScheduleOne_002EPlayerScripts_002EPlayerCrimeData_Assembly_002DCSharp_002Edll()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		Player.Health.onDie.AddListener(new UnityAction(OnDie));
		Player.onFreed.AddListener(new UnityAction(ClearCrimes));
		Player.onFreed.AddListener((UnityAction)delegate
		{
			SetPursuitLevel(EPursuitLevel.None);
		});
		((MonoBehaviour)this).InvokeRepeating("CheckNearestOfficer", 0f, 0.2f);
	}
}
