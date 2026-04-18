using System;
using System.Collections;
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
using ScheduleOne.Growing;
using ScheduleOne.ItemFramework;
using ScheduleOne.Levelling;
using ScheduleOne.Management;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Tiles;
using ScheduleOne.UI.Management;
using ScheduleOne.Variables;
using UnityEngine;

namespace ScheduleOne.ObjectScripts;

public class Pot : GrowContainer, IConfigurable
{
	public enum ESoilState
	{
		Flat,
		Parted,
		Packed
	}

	public const float MaxWarmthGrowthMultiplier = 1.5f;

	public const float WarmthMinThreshold = 20f;

	public const float WarmthMaxThreshold = 40f;

	public const float RotationSpeed = 10f;

	[Header("References")]
	public Transform ModelTransform;

	public Transform SeedStartPoint;

	public Transform SeedRestingPoint;

	public Transform LookAtPoint;

	public Transform PlantContainer;

	public Transform TaskBounds;

	public Transform LeafDropPoint;

	public ParticleSystem PoofParticles;

	public AudioSourceController PoofSound;

	public ConfigurationReplicator ConfigurationReplicator;

	public Transform Dirt_Flat;

	public Transform Dirt_Parted;

	public SoilChunk[] SoilChunks;

	[Header("UI")]
	public PotUIElement WorldspaceUIPrefab;

	public Sprite typeIcon;

	[Header("Pot Settings")]
	public float PotRadius = 0.2f;

	[Range(0.2f, 2f)]
	public float YieldMultiplier = 1f;

	[Range(0.2f, 2f)]
	public float GrowSpeedMultiplier = 1f;

	[CompilerGenerated]
	[SyncVar]
	[HideInInspector]
	public NetworkObject _003CCurrentPlayerConfigurer_003Ek__BackingField;

	private float rotation;

	private bool rotationOverridden;

	public SyncVar<NetworkObject> syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField;

	private bool NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EPotAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EObjectScripts_002EPotAssembly_002DCSharp_002Edll_Excuted;

	public Plant Plant { get; protected set; }

	public EntityConfiguration Configuration => potConfiguration;

	protected PotConfiguration potConfiguration { get; set; }

	public ConfigurationReplicator ConfigReplicator => ConfigurationReplicator;

	public EConfigurableType ConfigurableType => EConfigurableType.Pot;

	public WorldspaceUIElement WorldspaceUI { get; set; }

	public NetworkObject CurrentPlayerConfigurer
	{
		[CompilerGenerated]
		get
		{
			return SyncAccessor__003CCurrentPlayerConfigurer_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			this.sync___set_value__003CCurrentPlayerConfigurer_003Ek__BackingField(value, true);
		}
	}

	public Sprite TypeIcon => typeIcon;

	public Transform Transform => ((Component)this).transform;

	public Transform UIPoint => _uiPoint;

	public bool CanBeSelected => true;

	public NetworkObject SyncAccessor__003CCurrentPlayerConfigurer_003Ek__BackingField
	{
		get
		{
			return CurrentPlayerConfigurer;
		}
		set
		{
			if (value || !((NetworkBehaviour)this).IsServerInitialized)
			{
				CurrentPlayerConfigurer = value;
			}
			if (Application.isPlaying)
			{
				syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField.SetValue(value, value);
			}
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SetConfigurer(NetworkObject player)
	{
		RpcWriter___Server_SetConfigurer_3323014238(player);
		RpcLogic___SetConfigurer_3323014238(player);
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EObjectScripts_002EPot_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	protected override void Start()
	{
		base.Start();
		SetSoilState(ESoilState.Flat);
		((Component)TaskBounds).gameObject.SetActive(false);
		SoilChunk[] soilChunks = SoilChunks;
		for (int i = 0; i < soilChunks.Length; i++)
		{
			soilChunks[i].ClickableEnabled = false;
		}
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		base.OnSpawnServer(connection);
		if (connection.IsHost)
		{
			return;
		}
		if ((Object)(object)Plant != (Object)null)
		{
			PlantSeed_Client(connection, ((BaseItemDefinition)Plant.SeedDefinition).ID, Plant.NormalizedGrowthProgress);
			for (int i = 0; i < Plant.ActiveHarvestables.Count; i++)
			{
				SetHarvestableActive_Client(connection, Plant.ActiveHarvestables[i], active: true);
			}
		}
		SendConfigurationToClient(connection);
	}

	public void SendConfigurationToClient(NetworkConnection conn)
	{
		if (!conn.IsHost)
		{
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(WaitForConfig());
		}
		IEnumerator WaitForConfig()
		{
			yield return (object)new WaitUntil((Func<bool>)(() => Configuration != null));
			Configuration.ReplicateAllFields(conn);
		}
	}

	public override void InitializeGridItem(ItemInstance instance, Grid grid, Vector2 originCoordinate, int rotation, string GUID)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		base.InitializeGridItem(instance, grid, originCoordinate, rotation, GUID);
		base.ParentProperty.AddConfigurable(this);
		potConfiguration = new PotConfiguration(ConfigReplicator, this, this);
		CreateWorldspaceUI();
	}

	public override string GetManagementName()
	{
		return Configuration.Name.Value;
	}

	public override string GetDefaultManagementName()
	{
		return ScheduleOne.Management.ConfigurableType.GetTypeName(ConfigurableType);
	}

	protected override void Destroy()
	{
		if (Configuration != null)
		{
			Configuration.Destroy();
			DestroyWorldspaceUI();
			base.ParentProperty.RemoveConfigurable(this);
		}
		base.Destroy();
	}

	protected virtual void LateUpdate()
	{
		UpdateRotation();
	}

	private void UpdateRotation()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		if (rotationOverridden)
		{
			ModelTransform.localRotation = Quaternion.Lerp(ModelTransform.localRotation, Quaternion.Euler(0f, rotation, 0f), Time.deltaTime * 10f);
		}
		else if (Mathf.Abs(ModelTransform.localEulerAngles.y) > 0.1f)
		{
			ModelTransform.localRotation = Quaternion.Lerp(ModelTransform.localRotation, Quaternion.Euler(0f, 0f, 0f), Time.deltaTime * 10f);
		}
		rotationOverridden = false;
	}

	protected override void OnMinPass()
	{
		base.OnMinPass();
		if ((Object)(object)Plant != (Object)null)
		{
			Plant.MinPass(1);
		}
	}

	protected override void OnTimeSkipped(int minsSkipped)
	{
		if (InstanceFinder.IsServer && (Object)(object)Plant != (Object)null)
		{
			Plant.MinPass(minsSkipped);
			SetGrowthProgress_Client(Plant.NormalizedGrowthProgress);
		}
		base.OnTimeSkipped(minsSkipped);
	}

	public bool CanAcceptSeed(out string reason)
	{
		reason = string.Empty;
		if (!base.IsFullyFilledWithSoil)
		{
			reason = "Must be filled with soil";
			return false;
		}
		if ((Object)(object)Plant != (Object)null)
		{
			reason = "Already contains seed";
			return false;
		}
		return true;
	}

	public bool IsReadyForHarvest(out string reason)
	{
		if ((Object)(object)Plant == (Object)null)
		{
			reason = "No plant in this pot";
			return false;
		}
		if (!Plant.IsFullyGrown)
		{
			reason = Mathf.Floor(Plant.NormalizedGrowthProgress * 100f) + "% grown";
			return false;
		}
		if (((IUsable)this).IsInUse)
		{
			reason = "In use by " + ((IUsable)this).UserName;
			return false;
		}
		reason = string.Empty;
		return true;
	}

	public override bool CanBeDestroyed(out string reason)
	{
		if ((Object)(object)Plant != (Object)null)
		{
			reason = "Contains plant";
			return false;
		}
		if (((IUsable)this).IsInUse)
		{
			reason = "In use by other player";
			return false;
		}
		return base.CanBeDestroyed(out reason);
	}

	public void OverrideRotation(float angle)
	{
		rotationOverridden = true;
		rotation = angle;
	}

	protected override AdditiveDefinition ApplyAdditive(string additiveID, bool isInitialApplication)
	{
		AdditiveDefinition additiveDefinition = base.ApplyAdditive(additiveID, isInitialApplication);
		if ((Object)(object)additiveDefinition == (Object)null)
		{
			return null;
		}
		if ((Object)(object)Plant != (Object)null)
		{
			Plant.AdditiveApplied(additiveDefinition, isInitialApplication);
			if (additiveDefinition.InstantGrowth > 0f)
			{
				PoofParticles.Play();
				PoofSound.Play();
			}
		}
		return additiveDefinition;
	}

	public override bool CanApplyAdditive(AdditiveDefinition additiveDef, out string invalidReason)
	{
		if ((Object)(object)Plant == (Object)null)
		{
			invalidReason = "No plant";
			return false;
		}
		if (Plant.IsFullyGrown)
		{
			invalidReason = "Fully grown";
			return false;
		}
		return base.CanApplyAdditive(additiveDef, out invalidReason);
	}

	public override bool IsPointAboveGrowSurface(Vector3 point)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = point - ((Component)this).transform.position;
		val.y = 0f;
		return ((Vector3)(ref val)).magnitude <= PotRadius;
	}

	public override void SetGrowableVisible(bool visible)
	{
		if ((Object)(object)Plant != (Object)null)
		{
			Plant.SetVisible(visible);
		}
	}

	protected override Vector3 GetRandomPourTargetPosition()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 insideUnitSphere = Random.insideUnitSphere;
		insideUnitSphere.y = 0f;
		Vector3 result = ((Component)this).transform.position + insideUnitSphere * (PotRadius * 0.85f);
		result.y = _pourTarget.position.y;
		return result;
	}

	public override float GetGrowSurfaceSideLength()
	{
		return PotRadius * 2f;
	}

	public override float GetTemperatureGrowthMultiplier()
	{
		float num = 1f;
		float averageTileTemperature = GetAverageTileTemperature();
		if (averageTileTemperature > 20f)
		{
			num = Mathf.Lerp(num, 1.5f, (averageTileTemperature - 20f) / 20f);
		}
		return num;
	}

	public override bool ContainsGrowable()
	{
		return (Object)(object)Plant != (Object)null;
	}

	public override float GetGrowthProgressNormalized()
	{
		if (!((Object)(object)Plant != (Object)null))
		{
			return 0f;
		}
		return Plant.NormalizedGrowthProgress;
	}

	public void SetSoilState(ESoilState state)
	{
		if (state == ESoilState.Flat && (Object)(object)Plant == (Object)null)
		{
			((Component)Dirt_Parted).gameObject.SetActive(false);
			((Component)Dirt_Flat).gameObject.SetActive(true);
		}
		else
		{
			if (state != ESoilState.Parted && state != ESoilState.Packed)
			{
				return;
			}
			((Component)Dirt_Parted).gameObject.SetActive(true);
			((Component)Dirt_Flat).gameObject.SetActive(false);
			if (state == ESoilState.Packed)
			{
				for (int i = 0; i < SoilChunks.Length; i++)
				{
					SoilChunks[i].SetLerpedTransform(1f);
				}
			}
			else
			{
				for (int j = 0; j < SoilChunks.Length; j++)
				{
					SoilChunks[j].SetLerpedTransform(0f);
				}
			}
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void PlantSeed_Server(string seedID, float normalizedSeedProgress)
	{
		RpcWriter___Server_PlantSeed_Server_606697822(seedID, normalizedSeedProgress);
		RpcLogic___PlantSeed_Server_606697822(seedID, normalizedSeedProgress);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void PlantSeed_Client(NetworkConnection conn, string seedID, float normalizedSeedProgress)
	{
		if (conn == null)
		{
			RpcWriter___Observers_PlantSeed_Client_4077118173(conn, seedID, normalizedSeedProgress);
			RpcLogic___PlantSeed_Client_4077118173(conn, seedID, normalizedSeedProgress);
		}
		else
		{
			RpcWriter___Target_PlantSeed_Client_4077118173(conn, seedID, normalizedSeedProgress);
		}
	}

	[ServerRpc(RequireOwnership = false)]
	public void SetGrowthProgress_Server(float progress)
	{
		RpcWriter___Server_SetGrowthProgress_Server_431000436(progress);
	}

	[ObserversRpc]
	private void SetGrowthProgress_Client(float progress)
	{
		RpcWriter___Observers_SetGrowthProgress_Client_431000436(progress);
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SetHarvestableActive_Server(int harvestableIndex, bool active)
	{
		RpcWriter___Server_SetHarvestableActive_Server_3658436649(harvestableIndex, active);
		RpcLogic___SetHarvestableActive_Server_3658436649(harvestableIndex, active);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void SetHarvestableActive_Client(NetworkConnection conn, int harvestableIndex, bool active)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetHarvestableActive_Client_338960014(conn, harvestableIndex, active);
			RpcLogic___SetHarvestableActive_Client_338960014(conn, harvestableIndex, active);
		}
		else
		{
			RpcWriter___Target_SetHarvestableActive_Client_338960014(conn, harvestableIndex, active);
		}
	}

	private void OnPlantFullyHarvested()
	{
		if (!((Object)(object)Plant == (Object)null))
		{
			if (InstanceFinder.IsServer)
			{
				float value = NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("HarvestedPlantCount");
				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("HarvestedPlantCount", (value + 1f).ToString());
				NetworkSingleton<LevelManager>.Instance.AddXP(5);
			}
			Plant = null;
			SetRemainingSoilUses(_remainingSoilUses - 1);
			ClearAdditives();
			SetSoilState(ESoilState.Flat);
			if (_remainingSoilUses <= 0)
			{
				ClearSoil();
			}
		}
	}

	public WorldspaceUIElement CreateWorldspaceUI()
	{
		if ((Object)(object)WorldspaceUI != (Object)null)
		{
			Console.LogWarning(((Object)((Component)this).gameObject).name + " already has a worldspace UI element!");
		}
		if ((Object)(object)base.ParentProperty == (Object)null)
		{
			Console.LogError(((object)base.ParentProperty)?.ToString() + " is not a child of a property!");
			return null;
		}
		PotUIElement component = ((Component)Object.Instantiate<PotUIElement>(WorldspaceUIPrefab, (Transform)(object)base.ParentProperty.WorldspaceUIContainer)).GetComponent<PotUIElement>();
		component.Initialize(this);
		WorldspaceUI = component;
		return component;
	}

	public void DestroyWorldspaceUI()
	{
		if ((Object)(object)WorldspaceUI != (Object)null)
		{
			WorldspaceUI.Destroy();
		}
	}

	public override BuildableItemData GetBaseData()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		PlantData plantData = null;
		if ((Object)(object)Plant != (Object)null)
		{
			plantData = Plant.GetPlantData();
		}
		return new PotData(base.GUID, base.ItemInstance, 0, base.OwnerGrid, _originCoordinate, _rotation, ((Object)(object)base.CurrentSoil != (Object)null) ? ((BaseItemDefinition)base.CurrentSoil).ID : string.Empty, _currentSoilAmount, _remainingSoilUses, _currentMoistureAmount, base.AppliedAdditives.ConvertAll((AdditiveDefinition x) => ((BaseItemDefinition)x).ID).ToArray(), plantData);
	}

	public override DynamicSaveData GetSaveData()
	{
		DynamicSaveData saveData = base.GetSaveData();
		if (Configuration.ShouldSave())
		{
			saveData.AddData("Configuration", Configuration.GetSaveString());
		}
		return saveData;
	}

	public virtual void Load(PotData potData)
	{
		Load((GrowContainerData)potData);
		LoadPlant(potData.PlantData);
	}

	private void LoadPlant(PlantData data)
	{
		if (data != null && !string.IsNullOrEmpty(data.SeedID))
		{
			((MonoBehaviour)this).StartCoroutine(Wait());
		}
		IEnumerator Wait()
		{
			yield return (object)new WaitUntil((Func<bool>)(() => ((NetworkBehaviour)this).NetworkObject.IsSpawned));
			PlantSeed_Server(data.SeedID, data.GrowthProgress);
			if (data.ActiveBuds != null)
			{
				List<int> list = new List<int>(data.ActiveBuds);
				Plant.ActiveHarvestables.ToArray();
				for (int num = 0; num < Plant.FinalGrowthStage.GrowthSites.Length; num++)
				{
					Plant.SetHarvestableActive(num, list.Contains(num));
				}
			}
		}
	}

	public override void NetworkInitialize___Early()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Expected O, but got Unknown
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Expected O, but got Unknown
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Expected O, but got Unknown
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Expected O, but got Unknown
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Expected O, but got Unknown
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Expected O, but got Unknown
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EPotAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EPotAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField = new SyncVar<NetworkObject>((NetworkBehaviour)(object)this, 2u, (WritePermission)0, (ReadPermission)0, -1f, (Channel)0, CurrentPlayerConfigurer);
			((NetworkBehaviour)this).RegisterServerRpc(16u, new ServerRpcDelegate(RpcReader___Server_SetConfigurer_3323014238));
			((NetworkBehaviour)this).RegisterServerRpc(17u, new ServerRpcDelegate(RpcReader___Server_PlantSeed_Server_606697822));
			((NetworkBehaviour)this).RegisterObserversRpc(18u, new ClientRpcDelegate(RpcReader___Observers_PlantSeed_Client_4077118173));
			((NetworkBehaviour)this).RegisterTargetRpc(19u, new ClientRpcDelegate(RpcReader___Target_PlantSeed_Client_4077118173));
			((NetworkBehaviour)this).RegisterServerRpc(20u, new ServerRpcDelegate(RpcReader___Server_SetGrowthProgress_Server_431000436));
			((NetworkBehaviour)this).RegisterObserversRpc(21u, new ClientRpcDelegate(RpcReader___Observers_SetGrowthProgress_Client_431000436));
			((NetworkBehaviour)this).RegisterServerRpc(22u, new ServerRpcDelegate(RpcReader___Server_SetHarvestableActive_Server_3658436649));
			((NetworkBehaviour)this).RegisterObserversRpc(23u, new ClientRpcDelegate(RpcReader___Observers_SetHarvestableActive_Client_338960014));
			((NetworkBehaviour)this).RegisterTargetRpc(24u, new ClientRpcDelegate(RpcReader___Target_SetHarvestableActive_Client_338960014));
			((NetworkBehaviour)this).RegisterSyncVarRead(new SyncVarReadDelegate(ReadSyncVar___ScheduleOne_002EObjectScripts_002EPot));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EObjectScripts_002EPotAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EObjectScripts_002EPotAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
			((SyncBase)syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField).SetRegistered();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_SetConfigurer_3323014238(NetworkObject player)
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
			((Writer)writer).WriteNetworkObject(player);
			((NetworkBehaviour)this).SendServerRpc(16u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetConfigurer_3323014238(NetworkObject player)
	{
		CurrentPlayerConfigurer = player;
	}

	private void RpcReader___Server_SetConfigurer_3323014238(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkObject player = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetConfigurer_3323014238(player);
		}
	}

	private void RpcWriter___Server_PlantSeed_Server_606697822(string seedID, float normalizedSeedProgress)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(seedID);
			((Writer)writer).WriteSingle(normalizedSeedProgress, (AutoPackType)0);
			((NetworkBehaviour)this).SendServerRpc(17u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___PlantSeed_Server_606697822(string seedID, float normalizedSeedProgress)
	{
		PlantSeed_Client(null, seedID, normalizedSeedProgress);
	}

	private void RpcReader___Server_PlantSeed_Server_606697822(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string seedID = ((Reader)PooledReader0).ReadString();
		float normalizedSeedProgress = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___PlantSeed_Server_606697822(seedID, normalizedSeedProgress);
		}
	}

	private void RpcWriter___Observers_PlantSeed_Client_4077118173(NetworkConnection conn, string seedID, float normalizedSeedProgress)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(seedID);
			((Writer)writer).WriteSingle(normalizedSeedProgress, (AutoPackType)0);
			((NetworkBehaviour)this).SendObserversRpc(18u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___PlantSeed_Client_4077118173(NetworkConnection conn, string seedID, float normalizedSeedProgress)
	{
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		if (!CanAcceptSeed(out var reason))
		{
			Console.LogWarning("PlantSeed: cannot accept seed: " + reason);
			return;
		}
		SeedDefinition seedDefinition = Registry.GetItem(seedID) as SeedDefinition;
		if ((Object)(object)seedDefinition == (Object)null)
		{
			Console.LogWarning("PlantSeed: seed not found with ID '" + ((object)seedDefinition)?.ToString() + "'");
			return;
		}
		SetSoilState(ESoilState.Packed);
		Plant = Object.Instantiate<GameObject>(((Component)seedDefinition.PlantPrefab).gameObject, PlantContainer).GetComponent<Plant>();
		((Component)Plant).transform.localEulerAngles = new Vector3(0f, Random.Range(0f, 360f), 0f);
		Plant.Initialize(((NetworkBehaviour)this).NetworkObject, normalizedSeedProgress);
		Plant plant = Plant;
		plant.onFullyHarvested = (Action)Delegate.Combine(plant.onFullyHarvested, new Action(OnPlantFullyHarvested));
		for (int i = 0; i < base.AppliedAdditives.Count; i++)
		{
			Plant.AdditiveApplied(base.AppliedAdditives[i], isInitialApplication: false);
		}
	}

	private void RpcReader___Observers_PlantSeed_Client_4077118173(PooledReader PooledReader0, Channel channel)
	{
		string seedID = ((Reader)PooledReader0).ReadString();
		float normalizedSeedProgress = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___PlantSeed_Client_4077118173(null, seedID, normalizedSeedProgress);
		}
	}

	private void RpcWriter___Target_PlantSeed_Client_4077118173(NetworkConnection conn, string seedID, float normalizedSeedProgress)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(seedID);
			((Writer)writer).WriteSingle(normalizedSeedProgress, (AutoPackType)0);
			((NetworkBehaviour)this).SendTargetRpc(19u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_PlantSeed_Client_4077118173(PooledReader PooledReader0, Channel channel)
	{
		string seedID = ((Reader)PooledReader0).ReadString();
		float normalizedSeedProgress = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___PlantSeed_Client_4077118173(((NetworkBehaviour)this).LocalConnection, seedID, normalizedSeedProgress);
		}
	}

	private void RpcWriter___Server_SetGrowthProgress_Server_431000436(float progress)
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
			((Writer)writer).WriteSingle(progress, (AutoPackType)0);
			((NetworkBehaviour)this).SendServerRpc(20u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetGrowthProgress_Server_431000436(float progress)
	{
		SetGrowthProgress_Client(progress);
	}

	private void RpcReader___Server_SetGrowthProgress_Server_431000436(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		float progress = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SetGrowthProgress_Server_431000436(progress);
		}
	}

	private void RpcWriter___Observers_SetGrowthProgress_Client_431000436(float progress)
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
			((Writer)writer).WriteSingle(progress, (AutoPackType)0);
			((NetworkBehaviour)this).SendObserversRpc(21u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetGrowthProgress_Client_431000436(float progress)
	{
		if ((Object)(object)Plant == (Object)null)
		{
			Console.LogWarning("SetGrowProgress called but plant is null!");
		}
		else
		{
			Plant.SetNormalizedGrowthProgress(progress);
		}
	}

	private void RpcReader___Observers_SetGrowthProgress_Client_431000436(PooledReader PooledReader0, Channel channel)
	{
		float progress = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetGrowthProgress_Client_431000436(progress);
		}
	}

	private void RpcWriter___Server_SetHarvestableActive_Server_3658436649(int harvestableIndex, bool active)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(harvestableIndex, (AutoPackType)1);
			((Writer)writer).WriteBoolean(active);
			((NetworkBehaviour)this).SendServerRpc(22u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetHarvestableActive_Server_3658436649(int harvestableIndex, bool active)
	{
		SetHarvestableActive_Client(null, harvestableIndex, active);
	}

	private void RpcReader___Server_SetHarvestableActive_Server_3658436649(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int harvestableIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		bool active = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetHarvestableActive_Server_3658436649(harvestableIndex, active);
		}
	}

	private void RpcWriter___Observers_SetHarvestableActive_Client_338960014(NetworkConnection conn, int harvestableIndex, bool active)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(harvestableIndex, (AutoPackType)1);
			((Writer)writer).WriteBoolean(active);
			((NetworkBehaviour)this).SendObserversRpc(23u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetHarvestableActive_Client_338960014(NetworkConnection conn, int harvestableIndex, bool active)
	{
		if ((Object)(object)Plant == (Object)null)
		{
			Console.LogWarning("SetHarvestableActive called but plant is null!");
		}
		else if (Plant.IsHarvestableActive(harvestableIndex) != active)
		{
			_ = Plant.ActiveHarvestables.Count;
			Plant.SetHarvestableActive(harvestableIndex, active);
		}
	}

	private void RpcReader___Observers_SetHarvestableActive_Client_338960014(PooledReader PooledReader0, Channel channel)
	{
		int harvestableIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		bool active = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetHarvestableActive_Client_338960014(null, harvestableIndex, active);
		}
	}

	private void RpcWriter___Target_SetHarvestableActive_Client_338960014(NetworkConnection conn, int harvestableIndex, bool active)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(harvestableIndex, (AutoPackType)1);
			((Writer)writer).WriteBoolean(active);
			((NetworkBehaviour)this).SendTargetRpc(24u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetHarvestableActive_Client_338960014(PooledReader PooledReader0, Channel channel)
	{
		int harvestableIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		bool active = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetHarvestableActive_Client_338960014(((NetworkBehaviour)this).LocalConnection, harvestableIndex, active);
		}
	}

	public override bool ReadSyncVar___ScheduleOne_002EObjectScripts_002EPot(PooledReader PooledReader0, uint UInt321, bool Boolean2)
	{
		if (UInt321 == 2)
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value__003CCurrentPlayerConfigurer_003Ek__BackingField(syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField.GetValue(true), true);
				return true;
			}
			NetworkObject value = ((Reader)PooledReader0).ReadNetworkObject();
			this.sync___set_value__003CCurrentPlayerConfigurer_003Ek__BackingField(value, Boolean2);
			return true;
		}
		return false;
	}

	protected override void Awake_UserLogic_ScheduleOne_002EObjectScripts_002EPot_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		SetPourTargetActive(active: false);
	}
}
