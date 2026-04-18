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
using UnityEngine;

namespace ScheduleOne.Vehicles;

[RequireComponent(typeof(LandVehicle))]
public class VehicleLights : NetworkBehaviour
{
	[SerializeField]
	private bool _debug;

	[CompilerGenerated]
	[SyncVar(/*Could not decode attribute arguments.*/)]
	public bool _003CHeadlightsOn_003Ek__BackingField;

	[Header("Headlights")]
	public MeshRenderer[] headLightMeshes;

	public OptimizedLight[] headLightSources;

	public Material headlightMat_On;

	public Material headLightMat_Off;

	private bool headLightsApplied;

	[Header("Brake lights")]
	public MeshRenderer[] brakeLightMeshes;

	public Light[] brakeLightSources;

	public Material brakeLightMat_On;

	public Material brakeLightMat_Off;

	private bool brakeLightsApplied = true;

	[Header("Reverse lights")]
	public bool hasReverseLights;

	public MeshRenderer[] reverseLightMeshes;

	public Light[] reverseLightSources;

	public Material reverseLightMat_On;

	public Material reverseLightMat_Off;

	private bool reverseLightsApplied = true;

	private LandVehicle vehicle;

	public SyncVar<bool> syncVar____003CHeadlightsOn_003Ek__BackingField;

	private bool NetworkInitialize___EarlyScheduleOne_002EVehicles_002EVehicleLightsAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EVehicles_002EVehicleLightsAssembly_002DCSharp_002Edll_Excuted;

	public bool HeadlightsOn
	{
		[CompilerGenerated]
		get
		{
			return SyncAccessor__003CHeadlightsOn_003Ek__BackingField;
		}
		[CompilerGenerated]
		[ServerRpc(RunLocally = true, RequireOwnership = false)]
		set
		{
			RpcWriter___Server_set_HeadlightsOn_1140765316(value);
			RpcLogic___set_HeadlightsOn_1140765316(value);
		}
	}

	public bool SyncAccessor__003CHeadlightsOn_003Ek__BackingField
	{
		get
		{
			return HeadlightsOn;
		}
		set
		{
			if (value || !((NetworkBehaviour)this).IsServerInitialized)
			{
				HeadlightsOn = value;
			}
			if (Application.isPlaying)
			{
				syncVar____003CHeadlightsOn_003Ek__BackingField.SetValue(value, value);
			}
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EVehicles_002EVehicleLights_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	protected virtual void Update()
	{
		if (vehicle.LocalPlayerIsDriver && GameInput.GetButtonDown(GameInput.ButtonCode.VehicleToggleLights))
		{
			HeadlightsOn = !SyncAccessor__003CHeadlightsOn_003Ek__BackingField;
		}
		UpdateVisuals();
	}

	private void UpdateVisuals()
	{
		if (SyncAccessor__003CHeadlightsOn_003Ek__BackingField != headLightsApplied)
		{
			headLightsApplied = SyncAccessor__003CHeadlightsOn_003Ek__BackingField;
			for (int i = 0; i < headLightMeshes.Length; i++)
			{
				((Renderer)headLightMeshes[i]).material = (SyncAccessor__003CHeadlightsOn_003Ek__BackingField ? headlightMat_On : headLightMat_Off);
			}
			for (int j = 0; j < headLightSources.Length; j++)
			{
				headLightSources[j].Enabled = SyncAccessor__003CHeadlightsOn_003Ek__BackingField;
			}
		}
		if (_debug)
		{
			Debug.Log((object)("Brake lights on: " + vehicle.BrakesApplied));
			Debug.Log((object)("Brake lights applied: " + brakeLightsApplied));
		}
		if (vehicle.BrakesApplied != brakeLightsApplied)
		{
			brakeLightsApplied = vehicle.BrakesApplied;
			for (int k = 0; k < brakeLightMeshes.Length; k++)
			{
				((Renderer)brakeLightMeshes[k]).material = (brakeLightsApplied ? brakeLightMat_On : brakeLightMat_Off);
			}
			for (int l = 0; l < brakeLightSources.Length; l++)
			{
				((Behaviour)brakeLightSources[l]).enabled = brakeLightsApplied;
			}
		}
		if (vehicle.IsReversing != reverseLightsApplied)
		{
			reverseLightsApplied = vehicle.IsReversing;
			for (int m = 0; m < reverseLightMeshes.Length; m++)
			{
				((Renderer)reverseLightMeshes[m]).material = (reverseLightsApplied ? reverseLightMat_On : reverseLightMat_Off);
			}
			for (int n = 0; n < reverseLightSources.Length; n++)
			{
				((Behaviour)reverseLightSources[n]).enabled = reverseLightsApplied;
			}
		}
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EVehicles_002EVehicleLightsAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EVehicles_002EVehicleLightsAssembly_002DCSharp_002Edll_Excuted = true;
			syncVar____003CHeadlightsOn_003Ek__BackingField = new SyncVar<bool>((NetworkBehaviour)(object)this, 0u, (WritePermission)1, (ReadPermission)0, 0.25f, (Channel)1, HeadlightsOn);
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_set_HeadlightsOn_1140765316));
			((NetworkBehaviour)this).RegisterSyncVarRead(new SyncVarReadDelegate(ReadSyncVar___ScheduleOne_002EVehicles_002EVehicleLights));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EVehicles_002EVehicleLightsAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EVehicles_002EVehicleLightsAssembly_002DCSharp_002Edll_Excuted = true;
			((SyncBase)syncVar____003CHeadlightsOn_003Ek__BackingField).SetRegistered();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_set_HeadlightsOn_1140765316(bool value)
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
			((Writer)writer).WriteBoolean(value);
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	[SpecialName]
	public void RpcLogic___set_HeadlightsOn_1140765316(bool value)
	{
		this.sync___set_value__003CHeadlightsOn_003Ek__BackingField(value, true);
	}

	private void RpcReader___Server_set_HeadlightsOn_1140765316(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		bool value = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___set_HeadlightsOn_1140765316(value);
		}
	}

	public override bool ReadSyncVar___ScheduleOne_002EVehicles_002EVehicleLights(PooledReader PooledReader0, uint UInt321, bool Boolean2)
	{
		if (UInt321 == 0)
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value__003CHeadlightsOn_003Ek__BackingField(syncVar____003CHeadlightsOn_003Ek__BackingField.GetValue(true), true);
				return true;
			}
			bool value = ((Reader)PooledReader0).ReadBoolean();
			this.sync___set_value__003CHeadlightsOn_003Ek__BackingField(value, Boolean2);
			return true;
		}
		return false;
	}

	private void Awake_UserLogic_ScheduleOne_002EVehicles_002EVehicleLights_Assembly_002DCSharp_002Edll()
	{
		vehicle = ((Component)this).GetComponent<LandVehicle>();
	}
}
