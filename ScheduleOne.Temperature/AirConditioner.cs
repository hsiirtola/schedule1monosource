using System;
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
using ScheduleOne.Core;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.Heatmap;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Property;
using ScheduleOne.Tiles;
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.Temperature;

public class AirConditioner : GridItem
{
	public enum EMode
	{
		Off,
		Cooling,
		Heating
	}

	private const float CoolingTemperature = 0f;

	private const float HeatingTemperature = 40f;

	[SerializeField]
	private Light _coolingLight;

	[SerializeField]
	private Light _heatingLight;

	[SerializeField]
	private AudioSourceController _beepSound;

	[SerializeField]
	private AudioSourceController _loopSound;

	[SerializeField]
	private ParticleSystem _heatParticles;

	[SerializeField]
	private ParticleSystem _coolParticles;

	[CompilerGenerated]
	[SyncVar(/*Could not decode attribute arguments.*/)]
	[HideInInspector]
	public EMode _003CCurrentMode_003Ek__BackingField;

	public SyncVar<EMode> syncVar____003CCurrentMode_003Ek__BackingField;

	private bool NetworkInitialize___EarlyScheduleOne_002ETemperature_002EAirConditionerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ETemperature_002EAirConditionerAssembly_002DCSharp_002Edll_Excuted;

	[field: SerializeField]
	public TemperatureEmitter TemperatureEmitter { get; private set; }

	[field: SerializeField]
	public TemperatureDisplay TemperatureDisplay { get; private set; }

	public EMode CurrentMode
	{
		[CompilerGenerated]
		get
		{
			return SyncAccessor__003CCurrentMode_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			this.sync___set_value__003CCurrentMode_003Ek__BackingField(value, true);
		}
	}

	public EMode SyncAccessor__003CCurrentMode_003Ek__BackingField
	{
		get
		{
			return CurrentMode;
		}
		set
		{
			if (value || !((NetworkBehaviour)this).IsServerInitialized)
			{
				CurrentMode = value;
			}
			if (Application.isPlaying)
			{
				syncVar____003CCurrentMode_003Ek__BackingField.SetValue(value, value);
			}
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ETemperature_002EAirConditioner_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public override void InitializeGridItem(ItemInstance instance, Grid grid, Vector2 originCoordinate, int rotation, string GUID)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		base.InitializeGridItem(instance, grid, originCoordinate, rotation, GUID);
		TemperatureDisplay.SetTemperatureGetter(() => TemperatureEmitter.Temperature);
		TemperatureDisplay.SetVisibilityGetter(() => SyncAccessor__003CCurrentMode_003Ek__BackingField != EMode.Off);
		HeatmapManager instance2 = Singleton<HeatmapManager>.Instance;
		instance2.onHeatmapVisibilityChanged = (Action<ScheduleOne.Property.Property, bool>)Delegate.Combine(instance2.onHeatmapVisibilityChanged, new Action<ScheduleOne.Property.Property, bool>(HeatmapVisibilityChanged));
		TemperatureDisplay.SetEnabled(Singleton<HeatmapManager>.Instance.IsHeatmapActive(base.OwnerGrid.ParentProperty));
		ApplyMode();
	}

	private void HeatmapVisibilityChanged(ScheduleOne.Property.Property property, bool visible)
	{
		if ((Object)(object)property == (Object)(object)base.OwnerGrid.ParentProperty)
		{
			TemperatureDisplay.SetEnabled(visible);
		}
	}

	protected override void Destroy()
	{
		foreach (Grid grid in base.ParentProperty.Grids)
		{
			grid.RemoveTemperatureEmitter(TemperatureEmitter, onlyCosmetic: false);
		}
		HeatmapManager instance = Singleton<HeatmapManager>.Instance;
		instance.onHeatmapVisibilityChanged = (Action<ScheduleOne.Property.Property, bool>)Delegate.Remove(instance.onHeatmapVisibilityChanged, new Action<ScheduleOne.Property.Property, bool>(HeatmapVisibilityChanged));
		base.Destroy();
	}

	private void Update()
	{
		UpdateLoopSound();
	}

	private void UpdateLoopSound()
	{
		if (base.IsCulled)
		{
			return;
		}
		if (SyncAccessor__003CCurrentMode_003Ek__BackingField == EMode.Off)
		{
			if (_loopSound.IsPlaying)
			{
				_loopSound.VolumeMultiplier = Mathf.MoveTowards(_loopSound.VolumeMultiplier, 0f, Time.deltaTime * 1f);
				_loopSound.PitchMultiplier = Mathf.MoveTowards(_loopSound.PitchMultiplier, 0.6f, Time.deltaTime * 1f);
			}
			if (_loopSound.VolumeMultiplier <= 0.01f && _loopSound.IsPlaying)
			{
				_loopSound.Stop();
			}
		}
		else
		{
			if (!_loopSound.IsPlaying)
			{
				_loopSound.Play();
			}
			else
			{
				_loopSound.PitchMultiplier = Mathf.MoveTowards(_loopSound.PitchMultiplier, (SyncAccessor__003CCurrentMode_003Ek__BackingField == EMode.Heating) ? 0.8f : 1f, Time.deltaTime * 0.5f);
			}
			_loopSound.VolumeMultiplier = Mathf.MoveTowards(_loopSound.VolumeMultiplier, 1f, Time.deltaTime * 1f);
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SetMode_Server(EMode mode)
	{
		RpcWriter___Server_SetMode_Server_3835190203(mode);
		RpcLogic___SetMode_Server_3835190203(mode);
	}

	public void SetMode(EMode mode)
	{
		CurrentMode = mode;
		ApplyMode();
	}

	private void ApplyMode()
	{
		bool onlyCosmetic = isGhost;
		((Behaviour)_coolingLight).enabled = SyncAccessor__003CCurrentMode_003Ek__BackingField == EMode.Cooling && !isGhost;
		((Behaviour)_heatingLight).enabled = SyncAccessor__003CCurrentMode_003Ek__BackingField == EMode.Heating && !isGhost;
		if (!isGhost)
		{
			if (SyncAccessor__003CCurrentMode_003Ek__BackingField == EMode.Cooling)
			{
				if (!_coolParticles.isEmitting)
				{
					_coolParticles.Play();
				}
			}
			else if (_coolParticles.isEmitting)
			{
				_coolParticles.Stop();
			}
			if (SyncAccessor__003CCurrentMode_003Ek__BackingField == EMode.Heating)
			{
				if (!_heatParticles.isEmitting)
				{
					_heatParticles.Play();
				}
			}
			else if (_heatParticles.isEmitting)
			{
				_heatParticles.Stop();
			}
		}
		if (SyncAccessor__003CCurrentMode_003Ek__BackingField == EMode.Off)
		{
			if (isGhost || !((Object)(object)base.ParentProperty != (Object)null))
			{
				return;
			}
			{
				foreach (Grid grid in base.ParentProperty.Grids)
				{
					grid.RemoveTemperatureEmitter(TemperatureEmitter, onlyCosmetic);
				}
				return;
			}
		}
		if (SyncAccessor__003CCurrentMode_003Ek__BackingField == EMode.Cooling)
		{
			TemperatureEmitter.SetTemperature(0f);
		}
		else
		{
			TemperatureEmitter.SetTemperature(40f);
		}
		if (isGhost || !((Object)(object)base.ParentProperty != (Object)null))
		{
			return;
		}
		foreach (Grid grid2 in base.ParentProperty.Grids)
		{
			grid2.AddTemperatureEmitter(TemperatureEmitter, onlyCosmetic);
		}
	}

	private void OnModeChanged(EMode previous, EMode current, bool asServer)
	{
		if (previous == EMode.Off && current != EMode.Off && !_beepSound.IsPlaying && !isGhost)
		{
			_beepSound.Play();
		}
		ApplyMode();
	}

	[Button]
	public void SetOff()
	{
		SetMode_Server(EMode.Off);
	}

	[Button]
	public void SetCooling()
	{
		SetMode_Server(EMode.Cooling);
	}

	[Button]
	public void SetHeating()
	{
		SetMode_Server(EMode.Heating);
	}

	public override BuildableItemData GetBaseData()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		return new AirConditionerData(base.GUID, base.ItemInstance, 0, base.OwnerGrid, _originCoordinate, _rotation, SyncAccessor__003CCurrentMode_003Ek__BackingField);
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ETemperature_002EAirConditionerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ETemperature_002EAirConditionerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			syncVar____003CCurrentMode_003Ek__BackingField = new SyncVar<EMode>((NetworkBehaviour)(object)this, 0u, (WritePermission)1, (ReadPermission)0, -1f, (Channel)0, CurrentMode);
			syncVar____003CCurrentMode_003Ek__BackingField.OnChange += OnModeChanged;
			((NetworkBehaviour)this).RegisterServerRpc(5u, new ServerRpcDelegate(RpcReader___Server_SetMode_Server_3835190203));
			((NetworkBehaviour)this).RegisterSyncVarRead(new SyncVarReadDelegate(ReadSyncVar___ScheduleOne_002ETemperature_002EAirConditioner));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ETemperature_002EAirConditionerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ETemperature_002EAirConditionerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
			((SyncBase)syncVar____003CCurrentMode_003Ek__BackingField).SetRegistered();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_SetMode_Server_3835190203(EMode mode)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002ETemperature_002EAirConditioner_002FEModeFishNet_002ESerializing_002EGenerated((Writer)(object)writer, mode);
			((NetworkBehaviour)this).SendServerRpc(5u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetMode_Server_3835190203(EMode mode)
	{
		CurrentMode = mode;
		ApplyMode();
	}

	private void RpcReader___Server_SetMode_Server_3835190203(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		EMode mode = GeneratedReaders___Internal.Read___ScheduleOne_002ETemperature_002EAirConditioner_002FEModeFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetMode_Server_3835190203(mode);
		}
	}

	public override bool ReadSyncVar___ScheduleOne_002ETemperature_002EAirConditioner(PooledReader PooledReader0, uint UInt321, bool Boolean2)
	{
		if (UInt321 == 0)
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value__003CCurrentMode_003Ek__BackingField(syncVar____003CCurrentMode_003Ek__BackingField.GetValue(true), true);
				return true;
			}
			EMode value = GeneratedReaders___Internal.Read___ScheduleOne_002ETemperature_002EAirConditioner_002FEModeFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
			this.sync___set_value__003CCurrentMode_003Ek__BackingField(value, Boolean2);
			return true;
		}
		return false;
	}

	protected override void Awake_UserLogic_ScheduleOne_002ETemperature_002EAirConditioner_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		_loopSound.VolumeMultiplier = 0f;
	}
}
