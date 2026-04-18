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
using ScheduleOne.Audio;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.GameTime;
using ScheduleOne.Heatmap;
using ScheduleOne.Interaction;
using ScheduleOne.ItemFramework;
using ScheduleOne.Lighting;
using ScheduleOne.Management;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Property;
using ScheduleOne.Tiles;
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.Growing;

public abstract class GrowContainer : GridItem, IUsable, ITransitEntity
{
	public const float DryThreshold = 0f;

	[SerializeField]
	private float _moistureDrainPerHour = 1f;

	[SerializeField]
	public SoilDefinition[] AllowedSoils;

	[SerializeField]
	public AdditiveDefinition[] AllowedAdditives;

	[Header("Grow Container References")]
	[SerializeField]
	private GrowContainerInteraction _interactionHandler;

	[SerializeField]
	protected MeshRenderer[] _soilMeshRenderers;

	[SerializeField]
	protected Transform _soilMinTransform;

	[SerializeField]
	protected Transform _soilMaxTransform;

	[SerializeField]
	private MeshRenderer _additiveDisplayTemplate;

	[SerializeField]
	protected Transform _pourTarget;

	[SerializeField]
	protected Transform _uiPoint;

	[SerializeField]
	protected Transform[] _accessPoints;

	[SerializeField]
	private ParticleSystem[] _soilClearedParticles;

	[SerializeField]
	private AudioSourceController _soilClearedSound;

	[Header("Optional References")]
	[SerializeField]
	private UsableLightSource _lightSourceOverride;

	[CompilerGenerated]
	[HideInInspector]
	[SyncVar(/*Could not decode attribute arguments.*/)]
	public NetworkObject _003CNPCUserObject_003Ek__BackingField;

	[CompilerGenerated]
	[HideInInspector]
	[SyncVar(/*Could not decode attribute arguments.*/)]
	public NetworkObject _003CPlayerUserObject_003Ek__BackingField;

	public Action onMinPass;

	public Action<int> onTimeSkip;

	protected float _currentSoilAmount;

	protected float _currentMoistureAmount;

	protected int _remainingSoilUses;

	private List<MeshRenderer> _activeAdditiveDisplays = new List<MeshRenderer>();

	public SyncVar<NetworkObject> syncVar____003CNPCUserObject_003Ek__BackingField;

	public SyncVar<NetworkObject> syncVar____003CPlayerUserObject_003Ek__BackingField;

	private bool NetworkInitialize___EarlyScheduleOne_002EGrowing_002EGrowContainerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EGrowing_002EGrowContainerAssembly_002DCSharp_002Edll_Excuted;

	[field: Header("Grow Container Settings")]
	[field: SerializeField]
	public float SoilCapacity { get; private set; } = 30f;

	[field: SerializeField]
	public float MoistureCapacity { get; private set; } = 5f;

	[field: SerializeField]
	public bool HidePlantDuringPourTasks { get; private set; } = true;

	[field: SerializeField]
	public Transform SoilContainer { get; private set; }

	[field: SerializeField]
	public Transform PourableStartPoint { get; private set; }

	[field: SerializeField]
	public GrowContainerSurfaceCover SurfaceCover { get; private set; }

	[field: SerializeField]
	public GrowContainerCameraHandler CameraHandler { get; private set; }

	[field: SerializeField]
	public TemperatureDisplay TemperatureDisplay { get; private set; }

	public float NormalizedSoilAmount => _currentSoilAmount / SoilCapacity;

	public bool IsFullyFilledWithSoil
	{
		get
		{
			if (NormalizedSoilAmount >= 1f)
			{
				return (Object)(object)CurrentSoil != (Object)null;
			}
			return false;
		}
	}

	public float NormalizedMoistureAmount => _currentMoistureAmount / MoistureCapacity;

	public SoilDefinition CurrentSoil { get; private set; }

	public List<AdditiveDefinition> AppliedAdditives { get; private set; } = new List<AdditiveDefinition>();

	public NetworkObject NPCUserObject
	{
		[CompilerGenerated]
		get
		{
			return SyncAccessor__003CNPCUserObject_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			this.sync___set_value__003CNPCUserObject_003Ek__BackingField(value, true);
		}
	}

	public NetworkObject PlayerUserObject
	{
		[CompilerGenerated]
		get
		{
			return SyncAccessor__003CPlayerUserObject_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			this.sync___set_value__003CPlayerUserObject_003Ek__BackingField(value, true);
		}
	}

	public string Name => ((BaseItemInstance)base.ItemInstance).Name;

	public List<ItemSlot> InputSlots { get; set; } = new List<ItemSlot>();

	public List<ItemSlot> OutputSlots { get; set; } = new List<ItemSlot>();

	public Transform LinkOrigin => _uiPoint;

	public Transform[] AccessPoints => _accessPoints;

	public bool Selectable { get; }

	public bool IsAcceptingItems { get; set; } = true;

	public NetworkObject SyncAccessor__003CNPCUserObject_003Ek__BackingField
	{
		get
		{
			return NPCUserObject;
		}
		set
		{
			if (value || !((NetworkBehaviour)this).IsServerInitialized)
			{
				NPCUserObject = value;
			}
			if (Application.isPlaying)
			{
				syncVar____003CNPCUserObject_003Ek__BackingField.SetValue(value, value);
			}
		}
	}

	public NetworkObject SyncAccessor__003CPlayerUserObject_003Ek__BackingField
	{
		get
		{
			return PlayerUserObject;
		}
		set
		{
			if (value || !((NetworkBehaviour)this).IsServerInitialized)
			{
				PlayerUserObject = value;
			}
			if (Application.isPlaying)
			{
				syncVar____003CPlayerUserObject_003Ek__BackingField.SetValue(value, value);
			}
		}
	}

	public void ConfigureInteraction(string labelText, InteractableObject.EInteractableState interactionState)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		_interactionHandler.ConfigureInteraction(labelText, interactionState);
	}

	public void ConfigureInteraction(string labelText, InteractableObject.EInteractableState interactionState, Vector3 labelPosition)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		_interactionHandler.ConfigureInteraction(labelText, interactionState, setLabelPosition: true, labelPosition);
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EGrowing_002EGrowContainer_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public override void InitializeGridItem(ItemInstance instance, Grid grid, Vector2 originCoordinate, int rotation, string GUID)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		base.InitializeGridItem(instance, grid, originCoordinate, rotation, GUID);
		TemperatureDisplay.SetTemperatureGetter(() => GetAverageCosmeticTileTemperature());
		HeatmapManager instance2 = Singleton<HeatmapManager>.Instance;
		instance2.onHeatmapVisibilityChanged = (Action<ScheduleOne.Property.Property, bool>)Delegate.Combine(instance2.onHeatmapVisibilityChanged, new Action<ScheduleOne.Property.Property, bool>(HeatmapVisibilityChanged));
		TemperatureDisplay.SetEnabled(Singleton<HeatmapManager>.Instance.IsHeatmapActive(base.OwnerGrid.ParentProperty));
		NetworkSingleton<TimeManager>.Instance.onMinutePass += new Action(OnMinPass);
		TimeManager instance3 = NetworkSingleton<TimeManager>.Instance;
		instance3.onTimeSkip = (Action<int>)Delegate.Combine(instance3.onTimeSkip, new Action<int>(OnTimeSkipped));
		OutputSlots.Add(new ItemSlot());
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
		NetworkSingleton<TimeManager>.Instance.onMinutePass -= new Action(OnMinPass);
		TimeManager instance = NetworkSingleton<TimeManager>.Instance;
		instance.onTimeSkip = (Action<int>)Delegate.Remove(instance.onTimeSkip, new Action<int>(OnTimeSkipped));
		HeatmapManager instance2 = Singleton<HeatmapManager>.Instance;
		instance2.onHeatmapVisibilityChanged = (Action<ScheduleOne.Property.Property, bool>)Delegate.Remove(instance2.onHeatmapVisibilityChanged, new Action<ScheduleOne.Property.Property, bool>(HeatmapVisibilityChanged));
		base.Destroy();
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		base.OnSpawnServer(connection);
		if (connection.IsHost)
		{
			return;
		}
		if ((Object)(object)CurrentSoil != (Object)null)
		{
			SetSoilData_Client(connection, ((BaseItemDefinition)CurrentSoil).ID, _currentSoilAmount, _remainingSoilUses);
		}
		if (_currentMoistureAmount > 0f)
		{
			SetMoistureData_Client(connection, _currentMoistureAmount);
		}
		if (AppliedAdditives == null)
		{
			return;
		}
		foreach (AdditiveDefinition appliedAdditive in AppliedAdditives)
		{
			ApplyAdditive_Client(connection, ((BaseItemDefinition)appliedAdditive).ID, initialApplication: false);
		}
	}

	protected virtual void OnMinPass()
	{
		if (onMinPass != null)
		{
			onMinPass();
		}
		if (!NetworkSingleton<TimeManager>.Instance.IsEndOfDay)
		{
			DrainMoisture(1);
		}
	}

	protected virtual void OnTimeSkipped(int minsSkipped)
	{
		if (InstanceFinder.IsServer)
		{
			if (onTimeSkip != null)
			{
				onTimeSkip(minsSkipped);
			}
			DrainMoisture(minsSkipped);
		}
	}

	private void DrainMoisture(int minutes)
	{
		ChangeMoistureAmount((0f - _moistureDrainPerHour / 60f) * (float)minutes);
	}

	public float GetAverageLightExposure(out float growSpeedMultiplier)
	{
		growSpeedMultiplier = 1f;
		if ((Object)(object)_lightSourceOverride != (Object)null)
		{
			return _lightSourceOverride.GrowSpeedMultiplier;
		}
		float num = 0f;
		for (int i = 0; i < CoordinatePairs.Count; i++)
		{
			num += base.OwnerGrid.GetTile(CoordinatePairs[i].coord2).LightExposureNode.GetTotalExposure(out var growSpeedMultiplier2);
			growSpeedMultiplier += growSpeedMultiplier2;
		}
		growSpeedMultiplier /= CoordinatePairs.Count;
		return num / (float)CoordinatePairs.Count;
	}

	public abstract bool IsPointAboveGrowSurface(Vector3 point);

	public abstract void SetGrowableVisible(bool visible);

	public abstract float GetGrowSurfaceSideLength();

	public abstract bool ContainsGrowable();

	public abstract float GetGrowthProgressNormalized();

	public virtual void SetSoil(SoilDefinition soil)
	{
		CurrentSoil = soil;
		RefreshSoilVisuals();
	}

	public void ChangeSoilAmount(float amount)
	{
		SetSoilAmount(_currentSoilAmount + amount);
	}

	public void SetSoilAmount(float amount)
	{
		_currentSoilAmount = Mathf.Clamp(amount, 0f, SoilCapacity);
		RefreshSoilVisuals();
	}

	public void SetRemainingSoilUses(int uses)
	{
		_remainingSoilUses = uses;
	}

	public void SyncSoilData()
	{
		SetSoilData_Server(((Object)(object)CurrentSoil != (Object)null) ? ((BaseItemDefinition)CurrentSoil).ID : string.Empty, _currentSoilAmount, _remainingSoilUses);
	}

	[ServerRpc(RequireOwnership = false)]
	private void SetSoilData_Server(string soilID, float amount, int uses)
	{
		RpcWriter___Server_SetSoilData_Server_3104499779(soilID, amount, uses);
	}

	[ObserversRpc]
	[TargetRpc]
	private void SetSoilData_Client(NetworkConnection conn, string soilID, float amount, int uses)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetSoilData_Client_433593356(conn, soilID, amount, uses);
		}
		else
		{
			RpcWriter___Target_SetSoilData_Client_433593356(conn, soilID, amount, uses);
		}
	}

	protected virtual void RefreshSoilVisuals()
	{
		SoilContainer.SetLocalTransformData(TransformData.Lerp(_soilMinTransform.GetLocalTransformData(), _soilMaxTransform.GetLocalTransformData(), NormalizedSoilAmount));
		((Component)SoilContainer).gameObject.SetActive(_currentSoilAmount > 0f);
		if ((Object)(object)CurrentSoil != (Object)null)
		{
			Material soilMaterial = GetSoilMaterial();
			MeshRenderer[] soilMeshRenderers = _soilMeshRenderers;
			for (int i = 0; i < soilMeshRenderers.Length; i++)
			{
				((Renderer)soilMeshRenderers[i]).material = soilMaterial;
			}
		}
	}

	protected virtual void ClearSoil()
	{
		SetSoil(null);
		SetSoilAmount(0f);
		SetMoistureAmount(0f);
		ClearAdditives();
		ParticleSystem[] soilClearedParticles = _soilClearedParticles;
		for (int i = 0; i < soilClearedParticles.Length; i++)
		{
			soilClearedParticles[i].Play();
		}
		_soilClearedSound.Play();
	}

	public bool IsSoilAllowed(SoilDefinition soil)
	{
		if (AllowedSoils == null || AllowedSoils.Length == 0)
		{
			return true;
		}
		SoilDefinition[] allowedSoils = AllowedSoils;
		for (int i = 0; i < allowedSoils.Length; i++)
		{
			if ((Object)(object)allowedSoils[i] == (Object)(object)soil)
			{
				return true;
			}
		}
		return false;
	}

	protected virtual Material GetSoilMaterial()
	{
		if ((Object)(object)CurrentSoil == (Object)null)
		{
			return null;
		}
		if (!(NormalizedMoistureAmount <= 0f))
		{
			return CurrentSoil.WetSoilMat;
		}
		return CurrentSoil.DrySoilMat;
	}

	public void ChangeMoistureAmount(float amount)
	{
		SetMoistureAmount(_currentMoistureAmount + amount);
	}

	public virtual void SetMoistureAmount(float amount)
	{
		_currentMoistureAmount = Mathf.Clamp(amount, 0f, MoistureCapacity);
		RefreshSoilVisuals();
	}

	public void SyncMoistureData()
	{
		SetMoistureData_Server(_currentMoistureAmount);
	}

	[ServerRpc(RequireOwnership = false)]
	private void SetMoistureData_Server(float amount)
	{
		RpcWriter___Server_SetMoistureData_Server_431000436(amount);
	}

	[ObserversRpc]
	[TargetRpc]
	private void SetMoistureData_Client(NetworkConnection conn, float amount)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetMoistureData_Client_530160725(conn, amount);
		}
		else
		{
			RpcWriter___Target_SetMoistureData_Client_530160725(conn, amount);
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void ApplyAdditive_Server(string additiveID)
	{
		RpcWriter___Server_ApplyAdditive_Server_3615296227(additiveID);
		RpcLogic___ApplyAdditive_Server_3615296227(additiveID);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void ApplyAdditive_Client(NetworkConnection conn, string additiveID, bool initialApplication)
	{
		if (conn == null)
		{
			RpcWriter___Observers_ApplyAdditive_Client_619441887(conn, additiveID, initialApplication);
			RpcLogic___ApplyAdditive_Client_619441887(conn, additiveID, initialApplication);
		}
		else
		{
			RpcWriter___Target_ApplyAdditive_Client_619441887(conn, additiveID, initialApplication);
		}
	}

	protected virtual AdditiveDefinition ApplyAdditive(string additiveID, bool isInitialApplication)
	{
		AdditiveDefinition item = Registry.GetItem<AdditiveDefinition>(additiveID);
		if ((Object)(object)item == (Object)null)
		{
			Console.LogError("Failed to find additive definition for ID: " + additiveID);
			return null;
		}
		if (IsAdditiveApplied(((BaseItemDefinition)item).ID))
		{
			Console.LogWarning("Already contains additive: " + additiveID);
			return null;
		}
		AppliedAdditives.Add(item);
		MeshRenderer val = Object.Instantiate<MeshRenderer>(_additiveDisplayTemplate, ((Component)_additiveDisplayTemplate).transform.parent);
		((Renderer)val).sharedMaterial = item.DisplayMaterial;
		((Component)val).gameObject.SetActive(true);
		_activeAdditiveDisplays.Add(val);
		return item;
	}

	public virtual float GetTemperatureGrowthMultiplier()
	{
		return 1f;
	}

	public bool IsAdditiveApplied(string additiveID)
	{
		return (Object)(object)AppliedAdditives.Find((AdditiveDefinition x) => ((BaseItemDefinition)x).ID == additiveID) != (Object)null;
	}

	protected void ClearAdditives()
	{
		AppliedAdditives.Clear();
		foreach (MeshRenderer activeAdditiveDisplay in _activeAdditiveDisplays)
		{
			Object.Destroy((Object)(object)((Component)activeAdditiveDisplay).gameObject);
		}
		_activeAdditiveDisplays.Clear();
	}

	public virtual bool CanApplyAdditive(AdditiveDefinition additiveDef, out string invalidReason)
	{
		if (NormalizedSoilAmount < 1f)
		{
			invalidReason = "Must be filled with soil";
			return false;
		}
		if ((Object)(object)additiveDef == (Object)null)
		{
			invalidReason = "Invalid additive";
			return false;
		}
		if (IsAdditiveApplied(((BaseItemDefinition)additiveDef).ID))
		{
			invalidReason = "Already contains " + ((BaseItemDefinition)additiveDef).Name;
			return false;
		}
		invalidReason = string.Empty;
		return true;
	}

	public void SetPourTargetActive(bool active)
	{
		((Component)_pourTarget).gameObject.SetActive(active);
	}

	public void RandomizePourTargetPosition()
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		Vector3 randomPourTargetPosition;
		do
		{
			randomPourTargetPosition = GetRandomPourTargetPosition();
			num++;
		}
		while (Vector3.Distance(_pourTarget.position, randomPourTargetPosition) < 0.15f && num < 100);
		_pourTarget.position = randomPourTargetPosition;
	}

	public Vector3 GetCurrentTargetPosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return _pourTarget.position;
	}

	protected abstract Vector3 GetRandomPourTargetPosition();

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SetPlayerUser(NetworkObject playerObject)
	{
		RpcWriter___Server_SetPlayerUser_3323014238(playerObject);
		RpcLogic___SetPlayerUser_3323014238(playerObject);
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SetNPCUser(NetworkObject npcObject)
	{
		RpcWriter___Server_SetNPCUser_3323014238(npcObject);
		RpcLogic___SetNPCUser_3323014238(npcObject);
	}

	protected void Load(GrowContainerData data)
	{
		if (!string.IsNullOrEmpty(data.SoilID))
		{
			SetSoil(Registry.GetItem<SoilDefinition>(data.SoilID));
			SetSoilAmount(data.SoilLevel);
			SetRemainingSoilUses(data.RemainingSoilUses);
		}
		SetMoistureAmount(data.WaterLevel);
		data.ConvertOldAdditiveFormatToNew();
		for (int i = 0; i < data.AppliedAdditives.Length; i++)
		{
			ApplyAdditive_Client(null, data.AppliedAdditives[i], initialApplication: false);
		}
	}

	public override void NetworkInitialize___Early()
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected O, but got Unknown
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected O, but got Unknown
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Expected O, but got Unknown
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Expected O, but got Unknown
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Expected O, but got Unknown
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Expected O, but got Unknown
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Expected O, but got Unknown
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Expected O, but got Unknown
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Expected O, but got Unknown
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EGrowing_002EGrowContainerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EGrowing_002EGrowContainerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			syncVar____003CPlayerUserObject_003Ek__BackingField = new SyncVar<NetworkObject>((NetworkBehaviour)(object)this, 1u, (WritePermission)1, (ReadPermission)0, -1f, (Channel)0, PlayerUserObject);
			syncVar____003CNPCUserObject_003Ek__BackingField = new SyncVar<NetworkObject>((NetworkBehaviour)(object)this, 0u, (WritePermission)1, (ReadPermission)0, -1f, (Channel)0, NPCUserObject);
			((NetworkBehaviour)this).RegisterServerRpc(5u, new ServerRpcDelegate(RpcReader___Server_SetSoilData_Server_3104499779));
			((NetworkBehaviour)this).RegisterObserversRpc(6u, new ClientRpcDelegate(RpcReader___Observers_SetSoilData_Client_433593356));
			((NetworkBehaviour)this).RegisterTargetRpc(7u, new ClientRpcDelegate(RpcReader___Target_SetSoilData_Client_433593356));
			((NetworkBehaviour)this).RegisterServerRpc(8u, new ServerRpcDelegate(RpcReader___Server_SetMoistureData_Server_431000436));
			((NetworkBehaviour)this).RegisterObserversRpc(9u, new ClientRpcDelegate(RpcReader___Observers_SetMoistureData_Client_530160725));
			((NetworkBehaviour)this).RegisterTargetRpc(10u, new ClientRpcDelegate(RpcReader___Target_SetMoistureData_Client_530160725));
			((NetworkBehaviour)this).RegisterServerRpc(11u, new ServerRpcDelegate(RpcReader___Server_ApplyAdditive_Server_3615296227));
			((NetworkBehaviour)this).RegisterObserversRpc(12u, new ClientRpcDelegate(RpcReader___Observers_ApplyAdditive_Client_619441887));
			((NetworkBehaviour)this).RegisterTargetRpc(13u, new ClientRpcDelegate(RpcReader___Target_ApplyAdditive_Client_619441887));
			((NetworkBehaviour)this).RegisterServerRpc(14u, new ServerRpcDelegate(RpcReader___Server_SetPlayerUser_3323014238));
			((NetworkBehaviour)this).RegisterServerRpc(15u, new ServerRpcDelegate(RpcReader___Server_SetNPCUser_3323014238));
			((NetworkBehaviour)this).RegisterSyncVarRead(new SyncVarReadDelegate(ReadSyncVar___ScheduleOne_002EGrowing_002EGrowContainer));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EGrowing_002EGrowContainerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EGrowing_002EGrowContainerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
			((SyncBase)syncVar____003CPlayerUserObject_003Ek__BackingField).SetRegistered();
			((SyncBase)syncVar____003CNPCUserObject_003Ek__BackingField).SetRegistered();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_SetSoilData_Server_3104499779(string soilID, float amount, int uses)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(soilID);
			((Writer)writer).WriteSingle(amount, (AutoPackType)0);
			((Writer)writer).WriteInt32(uses, (AutoPackType)1);
			((NetworkBehaviour)this).SendServerRpc(5u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___SetSoilData_Server_3104499779(string soilID, float amount, int uses)
	{
		SetSoilData_Client(null, soilID, amount, uses);
	}

	private void RpcReader___Server_SetSoilData_Server_3104499779(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string soilID = ((Reader)PooledReader0).ReadString();
		float amount = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		int uses = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SetSoilData_Server_3104499779(soilID, amount, uses);
		}
	}

	private void RpcWriter___Observers_SetSoilData_Client_433593356(NetworkConnection conn, string soilID, float amount, int uses)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(soilID);
			((Writer)writer).WriteSingle(amount, (AutoPackType)0);
			((Writer)writer).WriteInt32(uses, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(6u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetSoilData_Client_433593356(NetworkConnection conn, string soilID, float amount, int uses)
	{
		SetSoil(Registry.GetItem<SoilDefinition>(soilID));
		SetSoilAmount(amount);
		_remainingSoilUses = uses;
	}

	private void RpcReader___Observers_SetSoilData_Client_433593356(PooledReader PooledReader0, Channel channel)
	{
		string soilID = ((Reader)PooledReader0).ReadString();
		float amount = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		int uses = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetSoilData_Client_433593356(null, soilID, amount, uses);
		}
	}

	private void RpcWriter___Target_SetSoilData_Client_433593356(NetworkConnection conn, string soilID, float amount, int uses)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(soilID);
			((Writer)writer).WriteSingle(amount, (AutoPackType)0);
			((Writer)writer).WriteInt32(uses, (AutoPackType)1);
			((NetworkBehaviour)this).SendTargetRpc(7u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetSoilData_Client_433593356(PooledReader PooledReader0, Channel channel)
	{
		string soilID = ((Reader)PooledReader0).ReadString();
		float amount = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		int uses = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetSoilData_Client_433593356(((NetworkBehaviour)this).LocalConnection, soilID, amount, uses);
		}
	}

	private void RpcWriter___Server_SetMoistureData_Server_431000436(float amount)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteSingle(amount, (AutoPackType)0);
			((NetworkBehaviour)this).SendServerRpc(8u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___SetMoistureData_Server_431000436(float amount)
	{
		SetMoistureData_Client(null, amount);
	}

	private void RpcReader___Server_SetMoistureData_Server_431000436(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		float amount = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SetMoistureData_Server_431000436(amount);
		}
	}

	private void RpcWriter___Observers_SetMoistureData_Client_530160725(NetworkConnection conn, float amount)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteSingle(amount, (AutoPackType)0);
			((NetworkBehaviour)this).SendObserversRpc(9u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetMoistureData_Client_530160725(NetworkConnection conn, float amount)
	{
		SetMoistureAmount(amount);
	}

	private void RpcReader___Observers_SetMoistureData_Client_530160725(PooledReader PooledReader0, Channel channel)
	{
		float amount = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetMoistureData_Client_530160725(null, amount);
		}
	}

	private void RpcWriter___Target_SetMoistureData_Client_530160725(NetworkConnection conn, float amount)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteSingle(amount, (AutoPackType)0);
			((NetworkBehaviour)this).SendTargetRpc(10u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetMoistureData_Client_530160725(PooledReader PooledReader0, Channel channel)
	{
		float amount = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetMoistureData_Client_530160725(((NetworkBehaviour)this).LocalConnection, amount);
		}
	}

	private void RpcWriter___Server_ApplyAdditive_Server_3615296227(string additiveID)
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
			((Writer)writer).WriteString(additiveID);
			((NetworkBehaviour)this).SendServerRpc(11u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___ApplyAdditive_Server_3615296227(string additiveID)
	{
		ApplyAdditive_Client(null, additiveID, initialApplication: true);
	}

	private void RpcReader___Server_ApplyAdditive_Server_3615296227(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string additiveID = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___ApplyAdditive_Server_3615296227(additiveID);
		}
	}

	private void RpcWriter___Observers_ApplyAdditive_Client_619441887(NetworkConnection conn, string additiveID, bool initialApplication)
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
			((Writer)writer).WriteString(additiveID);
			((Writer)writer).WriteBoolean(initialApplication);
			((NetworkBehaviour)this).SendObserversRpc(12u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___ApplyAdditive_Client_619441887(NetworkConnection conn, string additiveID, bool initialApplication)
	{
		ApplyAdditive(additiveID, initialApplication);
	}

	private void RpcReader___Observers_ApplyAdditive_Client_619441887(PooledReader PooledReader0, Channel channel)
	{
		string additiveID = ((Reader)PooledReader0).ReadString();
		bool initialApplication = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___ApplyAdditive_Client_619441887(null, additiveID, initialApplication);
		}
	}

	private void RpcWriter___Target_ApplyAdditive_Client_619441887(NetworkConnection conn, string additiveID, bool initialApplication)
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
			((Writer)writer).WriteString(additiveID);
			((Writer)writer).WriteBoolean(initialApplication);
			((NetworkBehaviour)this).SendTargetRpc(13u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_ApplyAdditive_Client_619441887(PooledReader PooledReader0, Channel channel)
	{
		string additiveID = ((Reader)PooledReader0).ReadString();
		bool initialApplication = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ApplyAdditive_Client_619441887(((NetworkBehaviour)this).LocalConnection, additiveID, initialApplication);
		}
	}

	private void RpcWriter___Server_SetPlayerUser_3323014238(NetworkObject playerObject)
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
			((Writer)writer).WriteNetworkObject(playerObject);
			((NetworkBehaviour)this).SendServerRpc(14u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetPlayerUser_3323014238(NetworkObject playerObject)
	{
		if ((Object)(object)SyncAccessor__003CPlayerUserObject_003Ek__BackingField != (Object)null && SyncAccessor__003CPlayerUserObject_003Ek__BackingField.Owner.IsLocalClient && (Object)(object)playerObject != (Object)null && !playerObject.Owner.IsLocalClient)
		{
			Singleton<GameInput>.Instance.ExitAll();
		}
		PlayerUserObject = playerObject;
	}

	private void RpcReader___Server_SetPlayerUser_3323014238(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkObject playerObject = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetPlayerUser_3323014238(playerObject);
		}
	}

	private void RpcWriter___Server_SetNPCUser_3323014238(NetworkObject npcObject)
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
			((Writer)writer).WriteNetworkObject(npcObject);
			((NetworkBehaviour)this).SendServerRpc(15u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetNPCUser_3323014238(NetworkObject npcObject)
	{
		NPCUserObject = npcObject;
	}

	private void RpcReader___Server_SetNPCUser_3323014238(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkObject npcObject = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetNPCUser_3323014238(npcObject);
		}
	}

	public override bool ReadSyncVar___ScheduleOne_002EGrowing_002EGrowContainer(PooledReader PooledReader0, uint UInt321, bool Boolean2)
	{
		switch (UInt321)
		{
		case 1u:
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value__003CPlayerUserObject_003Ek__BackingField(syncVar____003CPlayerUserObject_003Ek__BackingField.GetValue(true), true);
				return true;
			}
			NetworkObject value2 = ((Reader)PooledReader0).ReadNetworkObject();
			this.sync___set_value__003CPlayerUserObject_003Ek__BackingField(value2, Boolean2);
			return true;
		}
		case 0u:
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value__003CNPCUserObject_003Ek__BackingField(syncVar____003CNPCUserObject_003Ek__BackingField.GetValue(true), true);
				return true;
			}
			NetworkObject value = ((Reader)PooledReader0).ReadNetworkObject();
			this.sync___set_value__003CNPCUserObject_003Ek__BackingField(value, Boolean2);
			return true;
		}
		default:
			return false;
		}
	}

	protected override void Awake_UserLogic_ScheduleOne_002EGrowing_002EGrowContainer_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		RefreshSoilVisuals();
		((Component)SurfaceCover).gameObject.SetActive(false);
		((Component)_additiveDisplayTemplate).gameObject.SetActive(false);
	}
}
