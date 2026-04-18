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
using ScheduleOne.DevUtilities;
using ScheduleOne.Interaction;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Property;

public class Tap : NetworkBehaviour, IUsable
{
	private const float FlowRateMultiplier = 6f;

	private const float HandleMoveSpeed = 2f;

	[SerializeField]
	private InteractableObject _interactable;

	[SerializeField]
	private Transform _handleTransform;

	[SerializeField]
	private Clickable _handleClickable;

	[SerializeField]
	private ParticleSystem _waterParticles;

	[SerializeField]
	private AudioSourceController _squeakSound;

	[SerializeField]
	private AudioSourceController _waterRunningSound;

	[CompilerGenerated]
	[SyncVar(/*Could not decode attribute arguments.*/)]
	[HideInInspector]
	public bool _003CIsHeldOpen_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar(/*Could not decode attribute arguments.*/)]
	[HideInInspector]
	public NetworkObject _003CNPCUserObject_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar(/*Could not decode attribute arguments.*/)]
	[HideInInspector]
	public NetworkObject _003CPlayerUserObject_003Ek__BackingField;

	private float _normalizedTapFlow;

	private Vector2 _defaultParticleStartSize;

	private float _maxTapOpenValue = 1f;

	public SyncVar<bool> syncVar____003CIsHeldOpen_003Ek__BackingField;

	public SyncVar<NetworkObject> syncVar____003CNPCUserObject_003Ek__BackingField;

	public SyncVar<NetworkObject> syncVar____003CPlayerUserObject_003Ek__BackingField;

	private bool NetworkInitialize___EarlyScheduleOne_002EProperty_002ETapAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EProperty_002ETapAssembly_002DCSharp_002Edll_Excuted;

	[field: SerializeField]
	public Transform CameraPos { get; private set; }

	[field: SerializeField]
	public Transform FillableModelContainer { get; private set; }

	public bool IsHeldOpen
	{
		[CompilerGenerated]
		get
		{
			return SyncAccessor__003CIsHeldOpen_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			this.sync___set_value__003CIsHeldOpen_003Ek__BackingField(value, true);
		}
	}

	public float ActualFlowRate => 6f * _normalizedTapFlow;

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

	public bool SyncAccessor__003CIsHeldOpen_003Ek__BackingField
	{
		get
		{
			return IsHeldOpen;
		}
		set
		{
			if (value || !((NetworkBehaviour)this).IsServerInitialized)
			{
				IsHeldOpen = value;
			}
			if (Application.isPlaying)
			{
				syncVar____003CIsHeldOpen_003Ek__BackingField.SetValue(value, value);
			}
		}
	}

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

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EProperty_002ETap_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	protected virtual void LateUpdate()
	{
		if (SyncAccessor__003CIsHeldOpen_003Ek__BackingField)
		{
			_normalizedTapFlow = Mathf.Clamp(_normalizedTapFlow + Time.deltaTime * 2f, 0f, _maxTapOpenValue);
		}
		else
		{
			_normalizedTapFlow = Mathf.Clamp(_normalizedTapFlow - Time.deltaTime * 2f, 0f, _maxTapOpenValue);
		}
		UpdateTapVisuals();
		UpdateWaterSound();
	}

	private void UpdateTapVisuals()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		((Component)_handleTransform).transform.localEulerAngles = new Vector3(0f, (0f - _normalizedTapFlow) * 360f, 0f);
		if (_normalizedTapFlow > 0f)
		{
			MainModule main = _waterParticles.main;
			((MainModule)(ref main)).startSize = new MinMaxCurve(_defaultParticleStartSize.x * _normalizedTapFlow, _defaultParticleStartSize.y * _normalizedTapFlow);
			if (!_waterParticles.isPlaying)
			{
				_waterParticles.Play();
			}
		}
		else if (_waterParticles.isPlaying)
		{
			_waterParticles.Stop();
		}
	}

	private void UpdateWaterSound()
	{
		if (_normalizedTapFlow > 0.01f)
		{
			_waterRunningSound.VolumeMultiplier = _normalizedTapFlow;
			if (!_waterRunningSound.IsPlaying)
			{
				_waterRunningSound.Play();
			}
		}
		else if (_waterRunningSound.IsPlaying)
		{
			_waterRunningSound.Stop();
		}
	}

	private void Hovered()
	{
		if (CanInteract(out var invalidReason))
		{
			_interactable.SetMessage("Use tap");
			_interactable.SetInteractableState(InteractableObject.EInteractableState.Default);
		}
		else if (!string.IsNullOrEmpty(invalidReason))
		{
			_interactable.SetMessage(invalidReason);
			_interactable.SetInteractableState(InteractableObject.EInteractableState.Invalid);
		}
		else
		{
			_interactable.SetInteractableState(InteractableObject.EInteractableState.Disabled);
		}
	}

	private void Interacted()
	{
		if (CanInteract(out var _))
		{
			new FillWaterContainer(this, PlayerSingleton<PlayerInventory>.Instance.EquippedItem as WaterContainerInstance);
		}
	}

	public void SetHandleEnabled(bool enabled)
	{
		_handleClickable.ClickableEnabled = enabled;
	}

	[ServerRpc(RunLocally = true, RequireOwnership = false)]
	private void SetHeldOpen(bool open)
	{
		RpcWriter___Server_SetHeldOpen_1140765316(open);
		RpcLogic___SetHeldOpen_1140765316(open);
	}

	private void OnHandleClickStart(RaycastHit hit)
	{
		SetHeldOpen(open: true);
	}

	private void OnHandleClickEnd()
	{
		SetHeldOpen(open: false);
	}

	private bool CanInteract(out string invalidReason)
	{
		invalidReason = string.Empty;
		if (!PlayerSingleton<PlayerInventory>.Instance.isAnythingEquipped)
		{
			return false;
		}
		if (!(PlayerSingleton<PlayerInventory>.Instance.EquippedItem is WaterContainerInstance waterContainerInstance))
		{
			return false;
		}
		if (waterContainerInstance.NormalizedFillAmount >= 1f)
		{
			invalidReason = "Already full";
			return false;
		}
		if (((IUsable)this).IsInUse)
		{
			invalidReason = "In use by " + ((IUsable)this).UserName;
			return false;
		}
		return true;
	}

	public void SetMaxTapOpen(float max)
	{
		_maxTapOpenValue = Mathf.Clamp01(max);
	}

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

	public override void NetworkInitialize___Early()
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Expected O, but got Unknown
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EProperty_002ETapAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EProperty_002ETapAssembly_002DCSharp_002Edll_Excuted = true;
			syncVar____003CPlayerUserObject_003Ek__BackingField = new SyncVar<NetworkObject>((NetworkBehaviour)(object)this, 2u, (WritePermission)1, (ReadPermission)0, -1f, (Channel)0, PlayerUserObject);
			syncVar____003CNPCUserObject_003Ek__BackingField = new SyncVar<NetworkObject>((NetworkBehaviour)(object)this, 1u, (WritePermission)1, (ReadPermission)0, -1f, (Channel)0, NPCUserObject);
			syncVar____003CIsHeldOpen_003Ek__BackingField = new SyncVar<bool>((NetworkBehaviour)(object)this, 0u, (WritePermission)1, (ReadPermission)0, -1f, (Channel)0, IsHeldOpen);
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_SetHeldOpen_1140765316));
			((NetworkBehaviour)this).RegisterServerRpc(1u, new ServerRpcDelegate(RpcReader___Server_SetPlayerUser_3323014238));
			((NetworkBehaviour)this).RegisterServerRpc(2u, new ServerRpcDelegate(RpcReader___Server_SetNPCUser_3323014238));
			((NetworkBehaviour)this).RegisterSyncVarRead(new SyncVarReadDelegate(ReadSyncVar___ScheduleOne_002EProperty_002ETap));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EProperty_002ETapAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EProperty_002ETapAssembly_002DCSharp_002Edll_Excuted = true;
			((SyncBase)syncVar____003CPlayerUserObject_003Ek__BackingField).SetRegistered();
			((SyncBase)syncVar____003CNPCUserObject_003Ek__BackingField).SetRegistered();
			((SyncBase)syncVar____003CIsHeldOpen_003Ek__BackingField).SetRegistered();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_SetHeldOpen_1140765316(bool open)
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
			((Writer)writer).WriteBoolean(open);
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___SetHeldOpen_1140765316(bool open)
	{
		if (open && !SyncAccessor__003CIsHeldOpen_003Ek__BackingField)
		{
			_squeakSound.Play();
		}
		IsHeldOpen = open;
	}

	private void RpcReader___Server_SetHeldOpen_1140765316(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		bool open = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetHeldOpen_1140765316(open);
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
			((NetworkBehaviour)this).SendServerRpc(1u, writer, val, (DataOrderType)0);
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
		if (SyncAccessor__003CIsHeldOpen_003Ek__BackingField && !((IUsable)this).IsInUse)
		{
			IsHeldOpen = false;
		}
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
			((NetworkBehaviour)this).SendServerRpc(2u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetNPCUser_3323014238(NetworkObject npcObject)
	{
		NPCUserObject = npcObject;
		if (SyncAccessor__003CIsHeldOpen_003Ek__BackingField && !((IUsable)this).IsInUse)
		{
			IsHeldOpen = false;
		}
	}

	private void RpcReader___Server_SetNPCUser_3323014238(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkObject npcObject = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetNPCUser_3323014238(npcObject);
		}
	}

	public override bool ReadSyncVar___ScheduleOne_002EProperty_002ETap(PooledReader PooledReader0, uint UInt321, bool Boolean2)
	{
		switch (UInt321)
		{
		case 2u:
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value__003CPlayerUserObject_003Ek__BackingField(syncVar____003CPlayerUserObject_003Ek__BackingField.GetValue(true), true);
				return true;
			}
			NetworkObject value3 = ((Reader)PooledReader0).ReadNetworkObject();
			this.sync___set_value__003CPlayerUserObject_003Ek__BackingField(value3, Boolean2);
			return true;
		}
		case 1u:
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value__003CNPCUserObject_003Ek__BackingField(syncVar____003CNPCUserObject_003Ek__BackingField.GetValue(true), true);
				return true;
			}
			NetworkObject value2 = ((Reader)PooledReader0).ReadNetworkObject();
			this.sync___set_value__003CNPCUserObject_003Ek__BackingField(value2, Boolean2);
			return true;
		}
		case 0u:
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value__003CIsHeldOpen_003Ek__BackingField(syncVar____003CIsHeldOpen_003Ek__BackingField.GetValue(true), true);
				return true;
			}
			bool value = ((Reader)PooledReader0).ReadBoolean();
			this.sync___set_value__003CIsHeldOpen_003Ek__BackingField(value, Boolean2);
			return true;
		}
		default:
			return false;
		}
	}

	private void Awake_UserLogic_ScheduleOne_002EProperty_002ETap_Assembly_002DCSharp_002Edll()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		_interactable.onHovered.AddListener(new UnityAction(Hovered));
		_interactable.onInteractStart.AddListener(new UnityAction(Interacted));
		_handleClickable.onClickStart.AddListener((UnityAction<RaycastHit>)OnHandleClickStart);
		_handleClickable.onClickEnd.AddListener(new UnityAction(OnHandleClickEnd));
		MainModule main = _waterParticles.main;
		MinMaxCurve startSize = ((MainModule)(ref main)).startSize;
		float constantMin = ((MinMaxCurve)(ref startSize)).constantMin;
		main = _waterParticles.main;
		startSize = ((MainModule)(ref main)).startSize;
		_defaultParticleStartSize = new Vector2(constantMin, ((MinMaxCurve)(ref startSize)).constantMax);
	}
}
