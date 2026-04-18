using System;
using System.Collections;
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
using ScheduleOne.Management;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Tiles;
using ScheduleOne.UI;
using ScheduleOne.UI.Management;
using ScheduleOne.Variables;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ScheduleOne.ObjectScripts;

public class MushroomBed : GrowContainer, IConfigurable
{
	public enum EMushroomBedSoilAppearance
	{
		NoSpores,
		MaskedSpores,
		FullSpores
	}

	[Header("Mushroom Bed")]
	[SerializeField]
	private float _internalSideLength = 1f;

	[SerializeField]
	private ConfigurationReplicator _configurationReplicator;

	[SerializeField]
	private Sprite _typeIcon;

	[SerializeField]
	private MushroomBedUIElement _worldspaceUIPrefab;

	[SerializeField]
	private ParticleSystem _poofParticles;

	[SerializeField]
	private AudioSourceController _poofSound;

	[SerializeField]
	private Transform _colonyAlignment;

	[SerializeField]
	private Transform _mixFXContainer;

	[SerializeField]
	private ParticleSystem[] _mixParticles;

	[SerializeField]
	private AudioSourceController _mixSound;

	[CompilerGenerated]
	[SyncVar]
	[HideInInspector]
	public NetworkObject _003CCurrentPlayerConfigurer_003Ek__BackingField;

	private Material _soilMaterialInstance;

	private EMushroomBedSoilAppearance _currentSoilAppearance;

	private bool _mushroomBedColdAtLeastOnce;

	public SyncVar<NetworkObject> syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField;

	private bool NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EMushroomBedAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EObjectScripts_002EMushroomBedAssembly_002DCSharp_002Edll_Excuted;

	public ShroomColony CurrentColony { get; set; }

	public EntityConfiguration Configuration => _configuration;

	public ConfigurationReplicator ConfigReplicator => _configurationReplicator;

	public EConfigurableType ConfigurableType => EConfigurableType.MushroomBed;

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

	public Sprite TypeIcon => _typeIcon;

	public Transform Transform => ((Component)this).transform;

	public Transform UIPoint => _uiPoint;

	public bool CanBeSelected => true;

	protected MushroomBedConfiguration _configuration { get; set; }

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

	public override void OnSpawnServer(NetworkConnection connection)
	{
		base.OnSpawnServer(connection);
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
		_configuration = new MushroomBedConfiguration(ConfigReplicator, this, this);
		CreateWorldspaceUI();
	}

	public override string GetManagementName()
	{
		return Configuration.Name.Value;
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

	public override bool CanBeDestroyed(out string reason)
	{
		if ((Object)(object)CurrentColony != (Object)null)
		{
			reason = "Contains mushrooms";
			return false;
		}
		if (((IUsable)this).IsInUse)
		{
			reason = "In use by other player";
			return false;
		}
		return base.CanBeDestroyed(out reason);
	}

	public override bool IsPointAboveGrowSurface(Vector3 point)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = point - ((Component)this).transform.position;
		if (Mathf.Abs(val.x) <= _internalSideLength * 0.5f)
		{
			return Mathf.Abs(val.z) <= _internalSideLength * 0.5f;
		}
		return false;
	}

	public override void SetGrowableVisible(bool visible)
	{
		if ((Object)(object)CurrentColony != (Object)null)
		{
			CurrentColony.SetColonyVisible(visible);
		}
	}

	public override bool CanApplyAdditive(AdditiveDefinition additiveDef, out string invalidReason)
	{
		if ((Object)(object)CurrentColony == (Object)null)
		{
			invalidReason = "No mushrooms";
			return false;
		}
		if (CurrentColony.IsFullyGrown)
		{
			invalidReason = "Fully grown";
			return false;
		}
		return base.CanApplyAdditive(additiveDef, out invalidReason);
	}

	protected override Vector3 GetRandomPourTargetPosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 result = ((Component)this).transform.position + new Vector3(Random.Range((0f - _internalSideLength) * 0.5f, _internalSideLength * 0.5f) * 0.85f, 0f, Random.Range((0f - _internalSideLength) * 0.5f, _internalSideLength * 0.5f) * 0.85f);
		result.y = _pourTarget.position.y;
		return result;
	}

	public override float GetGrowSurfaceSideLength()
	{
		return _internalSideLength;
	}

	protected override Material GetSoilMaterial()
	{
		return _soilMaterialInstance;
	}

	public override void SetSoil(SoilDefinition soil)
	{
		if ((Object)(object)soil != (Object)null)
		{
			_soilMaterialInstance = Object.Instantiate<Material>(soil.DrySoilMat);
			_soilMaterialInstance.SetFloat("_Moisture", base.NormalizedMoistureAmount);
			ConfigureSoilAppearance(_currentSoilAppearance);
		}
		base.SetSoil(soil);
	}

	public override void SetMoistureAmount(float amount)
	{
		base.SetMoistureAmount(amount);
		if ((Object)(object)_soilMaterialInstance != (Object)null)
		{
			_soilMaterialInstance.SetFloat("_Moisture", base.NormalizedMoistureAmount);
		}
	}

	public void ConfigureSoilAppearance(EMushroomBedSoilAppearance appearance, Texture2D sporeMask = null)
	{
		_currentSoilAppearance = appearance;
		if (!((Object)(object)_soilMaterialInstance == (Object)null))
		{
			switch (appearance)
			{
			case EMushroomBedSoilAppearance.NoSpores:
				_soilMaterialInstance.SetInt("_Dots", 0);
				break;
			case EMushroomBedSoilAppearance.MaskedSpores:
				_soilMaterialInstance.SetInt("_Dots", 1);
				_soilMaterialInstance.SetTexture("_DotsMask", (Texture)(object)sporeMask);
				break;
			case EMushroomBedSoilAppearance.FullSpores:
				_soilMaterialInstance.SetInt("_Dots", 1);
				_soilMaterialInstance.SetTexture("_DotsMask", (Texture)null);
				break;
			}
		}
	}

	public bool IsReadyForHarvest(out string reason)
	{
		if ((Object)(object)CurrentColony == (Object)null)
		{
			reason = "No plant in this pot";
			return false;
		}
		if (!CurrentColony.IsFullyGrown)
		{
			reason = Mathf.Floor(CurrentColony.GrowthProgress * 100f) + "% grown";
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

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SetConfigurer(NetworkObject player)
	{
		RpcWriter___Server_SetConfigurer_3323014238(player);
		RpcLogic___SetConfigurer_3323014238(player);
	}

	protected override AdditiveDefinition ApplyAdditive(string additiveID, bool isInitialApplication)
	{
		AdditiveDefinition additiveDefinition = base.ApplyAdditive(additiveID, isInitialApplication);
		if ((Object)(object)additiveDefinition == (Object)null)
		{
			return null;
		}
		if ((Object)(object)CurrentColony != (Object)null)
		{
			CurrentColony.AdditiveApplied(additiveDefinition, isInitialApplication);
			if (additiveDefinition.InstantGrowth > 0f || additiveDefinition.YieldMultiplier > 1f)
			{
				_poofParticles.Play();
				_poofSound.Play();
			}
		}
		return additiveDefinition;
	}

	public void PlayMixFXAtPoint(Vector3 point)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		_mixFXContainer.position = point;
		ParticleSystem[] mixParticles = _mixParticles;
		for (int i = 0; i < mixParticles.Length; i++)
		{
			mixParticles[i].Play();
		}
		_mixSound.PlayOneShot();
	}

	protected override void OnTileTemperatureChanged(Tile tile, float newTemp)
	{
		base.OnTileTemperatureChanged(tile, newTemp);
		if (InstanceFinder.IsServer && !_mushroomBedColdAtLeastOnce && GetAverageTileTemperature() <= 15f && NetworkSingleton<VariableDatabase>.InstanceExists && !NetworkSingleton<VariableDatabase>.Instance.GetValue<bool>("MushroomBedCold"))
		{
			_mushroomBedColdAtLeastOnce = true;
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("MushroomBedCold", true.ToString());
		}
	}

	public override bool ContainsGrowable()
	{
		return (Object)(object)CurrentColony != (Object)null;
	}

	public override float GetGrowthProgressNormalized()
	{
		if (!((Object)(object)CurrentColony != (Object)null))
		{
			return 0f;
		}
		return CurrentColony.GrowthProgress;
	}

	[ServerRpc(RequireOwnership = false)]
	public void CreateAndAssignColony_Server(string shroomSpawnID)
	{
		RpcWriter___Server_CreateAndAssignColony_Server_3615296227(shroomSpawnID);
	}

	private void CreateAndAssignColony(ShroomSpawnDefinition shroomSpawn)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)shroomSpawn == (Object)null)
		{
			Debug.LogWarning((object)"Mushroom bed tried to create colony with null spawn definition!");
			return;
		}
		if ((Object)(object)CurrentColony != (Object)null)
		{
			Debug.LogWarning((object)"Mushroom bed already has a colony assigned!");
			return;
		}
		CheckShowTemperatureHint();
		ShroomColony shroomColony = Object.Instantiate<ShroomColony>(shroomSpawn.ColonyPrefab, ((Component)this).transform);
		((NetworkBehaviour)this).NetworkObject.Spawn(((Component)shroomColony).gameObject, (NetworkConnection)null, default(Scene));
		AssignColony(shroomColony);
	}

	public void AssignColony(ShroomColony colony)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)CurrentColony == (Object)(object)colony))
		{
			CurrentColony = colony;
			((Component)CurrentColony).transform.SetParent(((Component)this).transform);
			((NetworkBehaviour)CurrentColony).NetworkObject.SetParent((NetworkBehaviour)(object)this);
			((Component)CurrentColony).transform.localPosition = _colonyAlignment.localPosition;
			((Component)CurrentColony).transform.localRotation = _colonyAlignment.localRotation;
			ShroomColony currentColony = CurrentColony;
			currentColony.onFullyHarvested = (Action)Delegate.Combine(currentColony.onFullyHarvested, new Action(OnColonyFullyHarvested));
			ConfigureSoilAppearance(EMushroomBedSoilAppearance.FullSpores);
			for (int i = 0; i < base.AppliedAdditives.Count; i++)
			{
				CurrentColony.AdditiveApplied(base.AppliedAdditives[i], isInitialApplication: false);
			}
		}
	}

	private void OnColonyFullyHarvested()
	{
		if (!((Object)(object)CurrentColony == (Object)null))
		{
			ShroomColony currentColony = CurrentColony;
			currentColony.onFullyHarvested = (Action)Delegate.Remove(currentColony.onFullyHarvested, new Action(OnColonyFullyHarvested));
			if (InstanceFinder.IsServer)
			{
				((NetworkBehaviour)this).Despawn(((Component)CurrentColony).gameObject, (DespawnType?)(DespawnType)0);
			}
			else
			{
				Object.Destroy((Object)(object)((Component)CurrentColony).gameObject);
			}
			CurrentColony = null;
			ClearSoil();
		}
	}

	protected override void ClearSoil()
	{
		base.ClearSoil();
		ConfigureSoilAppearance(EMushroomBedSoilAppearance.NoSpores);
	}

	public void CheckShowTemperatureHint()
	{
		if (!Singleton<LoadManager>.Instance.IsLoading && NetworkSingleton<VariableDatabase>.InstanceExists && !NetworkSingleton<VariableDatabase>.Instance.GetValue<bool>("MushroomTemperatureHintShown"))
		{
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("MushroomTemperatureHintShown", true.ToString());
			Singleton<HintDisplay>.Instance.ShowHint_20s("Mushrooms only grow in cool temperatures. Use an <h1>AC Unit</h> to keep the area cool.");
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
		MushroomBedUIElement component = ((Component)Object.Instantiate<MushroomBedUIElement>(_worldspaceUIPrefab, (Transform)(object)base.ParentProperty.WorldspaceUIContainer)).GetComponent<MushroomBedUIElement>();
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
		ShroomColonyData colonyData = null;
		if ((Object)(object)CurrentColony != (Object)null)
		{
			colonyData = CurrentColony.GetSaveData();
		}
		return new MushroomBedData(base.GUID, base.ItemInstance, 0, base.OwnerGrid, _originCoordinate, _rotation, ((Object)(object)base.CurrentSoil != (Object)null) ? ((BaseItemDefinition)base.CurrentSoil).ID : string.Empty, _currentSoilAmount, _remainingSoilUses, _currentMoistureAmount, base.AppliedAdditives.ConvertAll((AdditiveDefinition x) => ((BaseItemDefinition)x).ID).ToArray(), colonyData);
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

	public virtual void Load(MushroomBedData mushroomBedData)
	{
		Load((GrowContainerData)mushroomBedData);
		if (mushroomBedData.ShroomColonyData != null)
		{
			if ((Object)(object)Registry.GetItem<ShroomSpawnDefinition>(mushroomBedData.ShroomColonyData.MushroomSpawnID) != (Object)null)
			{
				CreateAndAssignColony(Registry.GetItem<ShroomSpawnDefinition>(mushroomBedData.ShroomColonyData.MushroomSpawnID));
				CurrentColony.Load(mushroomBedData.ShroomColonyData);
			}
			else
			{
				Debug.LogWarning((object)("Mushroom bed tried to load a colony with invalid spawn ID: " + mushroomBedData.ShroomColonyData.MushroomSpawnID));
			}
		}
	}

	public override void NetworkInitialize___Early()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EMushroomBedAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EMushroomBedAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField = new SyncVar<NetworkObject>((NetworkBehaviour)(object)this, 2u, (WritePermission)0, (ReadPermission)0, -1f, (Channel)0, CurrentPlayerConfigurer);
			((NetworkBehaviour)this).RegisterServerRpc(16u, new ServerRpcDelegate(RpcReader___Server_SetConfigurer_3323014238));
			((NetworkBehaviour)this).RegisterServerRpc(17u, new ServerRpcDelegate(RpcReader___Server_CreateAndAssignColony_Server_3615296227));
			((NetworkBehaviour)this).RegisterSyncVarRead(new SyncVarReadDelegate(ReadSyncVar___ScheduleOne_002EObjectScripts_002EMushroomBed));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EObjectScripts_002EMushroomBedAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EObjectScripts_002EMushroomBedAssembly_002DCSharp_002Edll_Excuted = true;
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

	private void RpcWriter___Server_CreateAndAssignColony_Server_3615296227(string shroomSpawnID)
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
			((Writer)writer).WriteString(shroomSpawnID);
			((NetworkBehaviour)this).SendServerRpc(17u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___CreateAndAssignColony_Server_3615296227(string shroomSpawnID)
	{
		CreateAndAssignColony(Registry.GetItem<ShroomSpawnDefinition>(shroomSpawnID));
	}

	private void RpcReader___Server_CreateAndAssignColony_Server_3615296227(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string shroomSpawnID = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___CreateAndAssignColony_Server_3615296227(shroomSpawnID);
		}
	}

	public override bool ReadSyncVar___ScheduleOne_002EObjectScripts_002EMushroomBed(PooledReader PooledReader0, uint UInt321, bool Boolean2)
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

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
