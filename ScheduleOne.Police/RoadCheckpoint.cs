using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Object.Synchronizing;
using FishNet.Object.Synchronizing.Internal;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.Misc;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Product.Packaging;
using ScheduleOne.Vehicles;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Police;

public class RoadCheckpoint : NetworkBehaviour
{
	public enum ECheckpointState
	{
		Disabled,
		Enabled
	}

	public const float MAX_TIME_OPEN = 15f;

	protected ECheckpointState appliedState;

	[CompilerGenerated]
	[SyncVar(SendRate = 0.25f)]
	public bool _003CGate1Open_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar(SendRate = 0.25f)]
	public bool _003CGate2Open_003Ek__BackingField;

	public List<NPC> AssignedNPCs = new List<NPC>();

	[Header("Settings")]
	public EStealthLevel MaxStealthLevel;

	public bool OpenForNPCs = true;

	public bool EnabledOnStart;

	[Header("References")]
	[SerializeField]
	protected GameObject container;

	public CarStopper Stopper1;

	public CarStopper Stopper2;

	public VehicleDetector SearchArea1;

	public VehicleDetector SearchArea2;

	public VehicleObstacle VehicleObstacle1;

	public VehicleObstacle VehicleObstacle2;

	public VehicleDetector NPCVehicleDetectionArea1;

	public VehicleDetector NPCVehicleDetectionArea2;

	public VehicleDetector ImmediateVehicleDetector;

	public Rigidbody[] TrafficCones;

	public Transform[] StandPoints;

	protected Dictionary<Rigidbody, Tuple<Vector3, Quaternion>> trafficConeOriginalTransforms = new Dictionary<Rigidbody, Tuple<Vector3, Quaternion>>();

	private float timeSinceGate1Open;

	private bool vehicleDetectedSinceGate1Open;

	private float timeSinceGate2Open;

	private bool vehicleDetectedSinceGate2Open;

	public UnityEvent<Player> onPlayerWalkThrough;

	public SyncVar<bool> syncVar____003CGate1Open_003Ek__BackingField;

	public SyncVar<bool> syncVar____003CGate2Open_003Ek__BackingField;

	private bool NetworkInitialize___EarlyScheduleOne_002EPolice_002ERoadCheckpointAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EPolice_002ERoadCheckpointAssembly_002DCSharp_002Edll_Excuted;

	public ECheckpointState ActivationState { get; protected set; }

	public bool Gate1Open
	{
		[CompilerGenerated]
		get
		{
			return SyncAccessor__003CGate1Open_003Ek__BackingField;
		}
		[CompilerGenerated]
		protected set
		{
			this.sync___set_value__003CGate1Open_003Ek__BackingField(value, true);
		}
	}

	public bool Gate2Open
	{
		[CompilerGenerated]
		get
		{
			return SyncAccessor__003CGate2Open_003Ek__BackingField;
		}
		[CompilerGenerated]
		protected set
		{
			this.sync___set_value__003CGate2Open_003Ek__BackingField(value, true);
		}
	}

	public bool SyncAccessor__003CGate1Open_003Ek__BackingField
	{
		get
		{
			return Gate1Open;
		}
		set
		{
			if (value || !((NetworkBehaviour)this).IsServerInitialized)
			{
				Gate1Open = value;
			}
			if (Application.isPlaying)
			{
				syncVar____003CGate1Open_003Ek__BackingField.SetValue(value, value);
			}
		}
	}

	public bool SyncAccessor__003CGate2Open_003Ek__BackingField
	{
		get
		{
			return Gate2Open;
		}
		set
		{
			if (value || !((NetworkBehaviour)this).IsServerInitialized)
			{
				Gate2Open = value;
			}
			if (Application.isPlaying)
			{
				syncVar____003CGate2Open_003Ek__BackingField.SetValue(value, value);
			}
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EPolice_002ERoadCheckpoint_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	protected virtual void Update()
	{
		if (ActivationState != ECheckpointState.Disabled)
		{
			((Component)VehicleObstacle1).gameObject.SetActive(!SyncAccessor__003CGate1Open_003Ek__BackingField);
			((Component)VehicleObstacle2).gameObject.SetActive(!SyncAccessor__003CGate2Open_003Ek__BackingField);
			Stopper1.isActive = !SyncAccessor__003CGate1Open_003Ek__BackingField;
			Stopper2.isActive = !SyncAccessor__003CGate2Open_003Ek__BackingField;
		}
		if (ActivationState != appliedState)
		{
			ApplyState();
		}
		if (!InstanceFinder.IsServer)
		{
			return;
		}
		if (!TryGetNearestAssignedNPC(out var _, out var distance) || distance > 20f)
		{
			if (SyncAccessor__003CGate1Open_003Ek__BackingField)
			{
				SetGate1Open(o: false);
			}
			if (SyncAccessor__003CGate2Open_003Ek__BackingField)
			{
				SetGate2Open(o: false);
			}
			return;
		}
		if (OpenForNPCs)
		{
			if ((Object)(object)NPCVehicleDetectionArea1.closestVehicle != (Object)null && (Object)(object)NPCVehicleDetectionArea1.closestVehicle.OccupantNPCs[0] != (Object)null)
			{
				if (!SyncAccessor__003CGate1Open_003Ek__BackingField)
				{
					SetGate1Open(o: true);
				}
				if (!SyncAccessor__003CGate2Open_003Ek__BackingField)
				{
					SetGate2Open(o: true);
				}
			}
			if ((Object)(object)NPCVehicleDetectionArea2.closestVehicle != (Object)null && (Object)(object)NPCVehicleDetectionArea2.closestVehicle.OccupantNPCs[0] != (Object)null)
			{
				if (!SyncAccessor__003CGate1Open_003Ek__BackingField)
				{
					SetGate1Open(o: true);
				}
				if (!SyncAccessor__003CGate2Open_003Ek__BackingField)
				{
					SetGate2Open(o: true);
				}
			}
		}
		if (ActivationState != ECheckpointState.Disabled)
		{
			if (SyncAccessor__003CGate1Open_003Ek__BackingField)
			{
				timeSinceGate1Open += Time.deltaTime;
				if (ImmediateVehicleDetector.vehicles.Count > 0)
				{
					vehicleDetectedSinceGate1Open = true;
				}
				if (timeSinceGate1Open > 15f || (vehicleDetectedSinceGate1Open && ImmediateVehicleDetector.vehicles.Count == 0))
				{
					SetGate1Open(o: false);
				}
			}
			else
			{
				timeSinceGate1Open = 0f;
				vehicleDetectedSinceGate1Open = false;
			}
			if (SyncAccessor__003CGate2Open_003Ek__BackingField)
			{
				timeSinceGate2Open += Time.deltaTime;
				if (ImmediateVehicleDetector.vehicles.Count > 0)
				{
					vehicleDetectedSinceGate2Open = true;
				}
				if (timeSinceGate2Open > 15f || (vehicleDetectedSinceGate2Open && ImmediateVehicleDetector.vehicles.Count == 0))
				{
					SetGate2Open(o: false);
				}
			}
			else
			{
				timeSinceGate2Open = 0f;
				vehicleDetectedSinceGate2Open = false;
			}
		}
		else
		{
			timeSinceGate1Open = 0f;
			vehicleDetectedSinceGate1Open = false;
			timeSinceGate2Open = 0f;
			vehicleDetectedSinceGate2Open = false;
		}
	}

	protected virtual void ApplyState()
	{
		appliedState = ActivationState;
		if (ActivationState == ECheckpointState.Disabled)
		{
			container.SetActive(false);
		}
		else
		{
			container.SetActive(true);
		}
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void Enable(NetworkConnection conn)
	{
		if (conn == null)
		{
			RpcWriter___Observers_Enable_328543758(conn);
			RpcLogic___Enable_328543758(conn);
		}
		else
		{
			RpcWriter___Target_Enable_328543758(conn);
		}
	}

	[ObserversRpc(RunLocally = true)]
	public void Disable()
	{
		RpcWriter___Observers_Disable_2166136261();
		RpcLogic___Disable_2166136261();
	}

	public void SetGate1Open(bool o)
	{
		Gate1Open = o;
	}

	public void SetGate2Open(bool o)
	{
		Gate2Open = o;
	}

	private void ResetTrafficCones()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if (trafficConeOriginalTransforms.Count != 0)
		{
			for (int i = 0; i < TrafficCones.Length; i++)
			{
				((Component)TrafficCones[i]).transform.position = trafficConeOriginalTransforms[TrafficCones[i]].Item1;
				((Component)TrafficCones[i]).transform.rotation = trafficConeOriginalTransforms[TrafficCones[i]].Item2;
			}
		}
	}

	public void PlayerDetected(Player player)
	{
		if (onPlayerWalkThrough != null)
		{
			onPlayerWalkThrough.Invoke(player);
		}
	}

	private bool TryGetNearestAssignedNPC(out NPC npc, out float distance)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		NPC nPC = null;
		float num = float.MaxValue;
		Vector3 position = ((Component)Stopper1).transform.position;
		for (int i = 0; i < AssignedNPCs.Count; i++)
		{
			if (!((Object)(object)AssignedNPCs[i] == (Object)null))
			{
				float num2 = Vector3.Distance(((Component)AssignedNPCs[i]).transform.position, position);
				if (num2 < num)
				{
					num = num2;
					nPC = AssignedNPCs[i];
				}
			}
		}
		npc = nPC;
		distance = num;
		return (Object)(object)nPC != (Object)null;
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Expected O, but got Unknown
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Expected O, but got Unknown
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EPolice_002ERoadCheckpointAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EPolice_002ERoadCheckpointAssembly_002DCSharp_002Edll_Excuted = true;
			syncVar____003CGate2Open_003Ek__BackingField = new SyncVar<bool>((NetworkBehaviour)(object)this, 1u, (WritePermission)0, (ReadPermission)0, 0.25f, (Channel)0, Gate2Open);
			syncVar____003CGate1Open_003Ek__BackingField = new SyncVar<bool>((NetworkBehaviour)(object)this, 0u, (WritePermission)0, (ReadPermission)0, 0.25f, (Channel)0, Gate1Open);
			((NetworkBehaviour)this).RegisterObserversRpc(0u, new ClientRpcDelegate(RpcReader___Observers_Enable_328543758));
			((NetworkBehaviour)this).RegisterTargetRpc(1u, new ClientRpcDelegate(RpcReader___Target_Enable_328543758));
			((NetworkBehaviour)this).RegisterObserversRpc(2u, new ClientRpcDelegate(RpcReader___Observers_Disable_2166136261));
			((NetworkBehaviour)this).RegisterSyncVarRead(new SyncVarReadDelegate(ReadSyncVar___ScheduleOne_002EPolice_002ERoadCheckpoint));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EPolice_002ERoadCheckpointAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EPolice_002ERoadCheckpointAssembly_002DCSharp_002Edll_Excuted = true;
			((SyncBase)syncVar____003CGate2Open_003Ek__BackingField).SetRegistered();
			((SyncBase)syncVar____003CGate1Open_003Ek__BackingField).SetRegistered();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_Enable_328543758(NetworkConnection conn)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
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
			((NetworkBehaviour)this).SendObserversRpc(0u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___Enable_328543758(NetworkConnection conn)
	{
		ResetTrafficCones();
		ActivationState = ECheckpointState.Enabled;
	}

	private void RpcReader___Observers_Enable_328543758(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___Enable_328543758(null);
		}
	}

	private void RpcWriter___Target_Enable_328543758(NetworkConnection conn)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
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
			((NetworkBehaviour)this).SendTargetRpc(1u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_Enable_328543758(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___Enable_328543758(((NetworkBehaviour)this).LocalConnection);
		}
	}

	private void RpcWriter___Observers_Disable_2166136261()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
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
			((NetworkBehaviour)this).SendObserversRpc(2u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___Disable_2166136261()
	{
		ActivationState = ECheckpointState.Disabled;
		if (InstanceFinder.IsServer)
		{
			for (int i = 0; i < AssignedNPCs.Count; i++)
			{
				(AssignedNPCs[i] as PoliceOfficer).UnassignFromCheckpoint();
			}
		}
	}

	private void RpcReader___Observers_Disable_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___Disable_2166136261();
		}
	}

	public override bool ReadSyncVar___ScheduleOne_002EPolice_002ERoadCheckpoint(PooledReader PooledReader0, uint UInt321, bool Boolean2)
	{
		switch (UInt321)
		{
		case 1u:
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value__003CGate2Open_003Ek__BackingField(syncVar____003CGate2Open_003Ek__BackingField.GetValue(true), true);
				return true;
			}
			bool value2 = ((Reader)PooledReader0).ReadBoolean();
			this.sync___set_value__003CGate2Open_003Ek__BackingField(value2, Boolean2);
			return true;
		}
		case 0u:
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value__003CGate1Open_003Ek__BackingField(syncVar____003CGate1Open_003Ek__BackingField.GetValue(true), true);
				return true;
			}
			bool value = ((Reader)PooledReader0).ReadBoolean();
			this.sync___set_value__003CGate1Open_003Ek__BackingField(value, Boolean2);
			return true;
		}
		default:
			return false;
		}
	}

	protected virtual void Awake_UserLogic_ScheduleOne_002EPolice_002ERoadCheckpoint_Assembly_002DCSharp_002Edll()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (EnabledOnStart)
		{
			ActivationState = ECheckpointState.Enabled;
		}
		ApplyState();
		if (trafficConeOriginalTransforms.Count == 0)
		{
			for (int i = 0; i < TrafficCones.Length; i++)
			{
				trafficConeOriginalTransforms.Add(TrafficCones[i], new Tuple<Vector3, Quaternion>(((Component)TrafficCones[i]).transform.position, ((Component)TrafficCones[i]).transform.rotation));
			}
		}
	}
}
