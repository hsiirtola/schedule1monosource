using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Building;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Property;
using UnityEngine;

namespace ScheduleOne.EntityFramework;

public class SurfaceItem : BuildableItem
{
	[Header("Settings")]
	public List<Surface.ESurfaceType> ValidSurfaceTypes = new List<Surface.ESurfaceType>
	{
		Surface.ESurfaceType.Wall,
		Surface.ESurfaceType.Roof
	};

	public bool AllowRotation = true;

	protected Vector3 RelativePosition = Vector3.zero;

	protected Quaternion RelativeRotation = Quaternion.identity;

	private bool NetworkInitialize___EarlyScheduleOne_002EEntityFramework_002ESurfaceItemAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EEntityFramework_002ESurfaceItemAssembly_002DCSharp_002Edll_Excuted;

	public Surface ParentSurface { get; protected set; }

	public float RotationIncrement { get; } = 45f;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EEntityFramework_002ESurfaceItem_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	protected override void SendInitializationToServer()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		InitializeSurfaceItem_Server(base.ItemInstance, base.GUID.ToString(), ParentSurface.GUID.ToString(), RelativePosition, RelativeRotation);
	}

	protected override void SendInitializationToClient(NetworkConnection conn)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		InitializeSurfaceItem_Client(conn, base.ItemInstance, base.GUID.ToString(), ParentSurface.GUID.ToString(), RelativePosition, RelativeRotation);
	}

	[ServerRpc(RequireOwnership = false)]
	private void InitializeSurfaceItem_Server(ItemInstance instance, string GUID, string parentSurfaceGUID, Vector3 relativePosition, Quaternion relativeRotation)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		RpcWriter___Server_InitializeSurfaceItem_Server_2652836379(instance, GUID, parentSurfaceGUID, relativePosition, relativeRotation);
	}

	[TargetRpc]
	[ObserversRpc(RunLocally = true)]
	private void InitializeSurfaceItem_Client(NetworkConnection conn, ItemInstance instance, string GUID, string parentSurfaceGUID, Vector3 relativePosition, Quaternion relativeRotation)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (conn == null)
		{
			RpcWriter___Observers_InitializeSurfaceItem_Client_2932264618(conn, instance, GUID, parentSurfaceGUID, relativePosition, relativeRotation);
			RpcLogic___InitializeSurfaceItem_Client_2932264618(conn, instance, GUID, parentSurfaceGUID, relativePosition, relativeRotation);
		}
		else
		{
			RpcWriter___Target_InitializeSurfaceItem_Client_2932264618(conn, instance, GUID, parentSurfaceGUID, relativePosition, relativeRotation);
		}
	}

	public virtual void InitializeSurfaceItem(ItemInstance instance, string GUID, string parentSurfaceGUID, Vector3 relativePosition, Quaternion relativeRotation)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (base.Initialized)
		{
			return;
		}
		SetTransformData(parentSurfaceGUID, relativePosition, relativeRotation);
		if ((Object)(object)ParentSurface == (Object)null)
		{
			Console.LogError("Failed to initialize SurfaceItem " + ((Object)((Component)this).gameObject).name + " due to missing parent surface.");
			Destroy();
			return;
		}
		ScheduleOne.Property.Property parentProperty = ParentSurface.ParentProperty;
		if ((Object)(object)parentProperty == (Object)null)
		{
			Console.LogError("Failed to find parent property for " + ((Object)((Component)this).gameObject).name);
		}
		else
		{
			InitializeBuildableItem(instance, GUID, parentProperty.PropertyCode);
		}
	}

	private void SetTransformData(string parentSurfaceGUID, Vector3 relativePosition, Quaternion relativeRotation)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		Surface surface = GUIDManager.GetObject<Surface>(new Guid(parentSurfaceGUID));
		if ((Object)(object)surface == (Object)null)
		{
			Console.LogError("Failed to find parent surface for " + ((Object)((Component)this).gameObject).name);
			return;
		}
		ParentSurface = surface;
		RelativePosition = relativePosition;
		RelativeRotation = relativeRotation;
		((Component)this).transform.position = ((Component)surface).transform.TransformPoint(relativePosition);
		((Component)this).transform.rotation = ((Component)surface).transform.rotation * relativeRotation;
		if (((NetworkBehaviour)this).NetworkObject.IsSpawned)
		{
			((Component)this).transform.SetParent(((Component)ParentSurface.Container).transform);
		}
		else
		{
			((MonoBehaviour)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			yield return (object)new WaitUntil((Func<bool>)(() => ((NetworkBehaviour)this).NetworkObject.IsSpawned));
			((Component)this).transform.SetParent(((Component)ParentSurface.Container).transform);
		}
	}

	protected override ScheduleOne.Property.Property GetProperty(Transform searchTransform = null)
	{
		return base.GetProperty(searchTransform);
	}

	public override BuildableItemData GetBaseData()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		return new SurfaceItemData(base.GUID, base.ItemInstance, 25, ParentSurface.GUID.ToString(), RelativePosition, RelativeRotation);
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EEntityFramework_002ESurfaceItemAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EEntityFramework_002ESurfaceItemAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterServerRpc(2u, new ServerRpcDelegate(RpcReader___Server_InitializeSurfaceItem_Server_2652836379));
			((NetworkBehaviour)this).RegisterTargetRpc(3u, new ClientRpcDelegate(RpcReader___Target_InitializeSurfaceItem_Client_2932264618));
			((NetworkBehaviour)this).RegisterObserversRpc(4u, new ClientRpcDelegate(RpcReader___Observers_InitializeSurfaceItem_Client_2932264618));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EEntityFramework_002ESurfaceItemAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EEntityFramework_002ESurfaceItemAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_InitializeSurfaceItem_Server_2652836379(ItemInstance instance, string GUID, string parentSurfaceGUID, Vector3 relativePosition, Quaternion relativeRotation)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)(object)writer).WriteItemInstance(instance);
			((Writer)writer).WriteString(GUID);
			((Writer)writer).WriteString(parentSurfaceGUID);
			((Writer)writer).WriteVector3(relativePosition);
			((Writer)writer).WriteQuaternion(relativeRotation, (AutoPackType)1);
			((NetworkBehaviour)this).SendServerRpc(2u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___InitializeSurfaceItem_Server_2652836379(ItemInstance instance, string GUID, string parentSurfaceGUID, Vector3 relativePosition, Quaternion relativeRotation)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		InitializeSurfaceItem_Client(null, instance, GUID, parentSurfaceGUID, relativePosition, relativeRotation);
	}

	private void RpcReader___Server_InitializeSurfaceItem_Server_2652836379(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		ItemInstance instance = ((Reader)(object)PooledReader0).ReadItemInstance();
		string gUID = ((Reader)PooledReader0).ReadString();
		string parentSurfaceGUID = ((Reader)PooledReader0).ReadString();
		Vector3 relativePosition = ((Reader)PooledReader0).ReadVector3();
		Quaternion relativeRotation = ((Reader)PooledReader0).ReadQuaternion((AutoPackType)1);
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___InitializeSurfaceItem_Server_2652836379(instance, gUID, parentSurfaceGUID, relativePosition, relativeRotation);
		}
	}

	private void RpcWriter___Target_InitializeSurfaceItem_Client_2932264618(NetworkConnection conn, ItemInstance instance, string GUID, string parentSurfaceGUID, Vector3 relativePosition, Quaternion relativeRotation)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)(object)writer).WriteItemInstance(instance);
			((Writer)writer).WriteString(GUID);
			((Writer)writer).WriteString(parentSurfaceGUID);
			((Writer)writer).WriteVector3(relativePosition);
			((Writer)writer).WriteQuaternion(relativeRotation, (AutoPackType)1);
			((NetworkBehaviour)this).SendTargetRpc(3u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcLogic___InitializeSurfaceItem_Client_2932264618(NetworkConnection conn, ItemInstance instance, string GUID, string parentSurfaceGUID, Vector3 relativePosition, Quaternion relativeRotation)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (!base.Initialized)
		{
			InitializeSurfaceItem(instance, GUID, parentSurfaceGUID, relativePosition, relativeRotation);
		}
	}

	private void RpcReader___Target_InitializeSurfaceItem_Client_2932264618(PooledReader PooledReader0, Channel channel)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		ItemInstance instance = ((Reader)(object)PooledReader0).ReadItemInstance();
		string gUID = ((Reader)PooledReader0).ReadString();
		string parentSurfaceGUID = ((Reader)PooledReader0).ReadString();
		Vector3 relativePosition = ((Reader)PooledReader0).ReadVector3();
		Quaternion relativeRotation = ((Reader)PooledReader0).ReadQuaternion((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___InitializeSurfaceItem_Client_2932264618(((NetworkBehaviour)this).LocalConnection, instance, gUID, parentSurfaceGUID, relativePosition, relativeRotation);
		}
	}

	private void RpcWriter___Observers_InitializeSurfaceItem_Client_2932264618(NetworkConnection conn, ItemInstance instance, string GUID, string parentSurfaceGUID, Vector3 relativePosition, Quaternion relativeRotation)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)(object)writer).WriteItemInstance(instance);
			((Writer)writer).WriteString(GUID);
			((Writer)writer).WriteString(parentSurfaceGUID);
			((Writer)writer).WriteVector3(relativePosition);
			((Writer)writer).WriteQuaternion(relativeRotation, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(4u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcReader___Observers_InitializeSurfaceItem_Client_2932264618(PooledReader PooledReader0, Channel channel)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		ItemInstance instance = ((Reader)(object)PooledReader0).ReadItemInstance();
		string gUID = ((Reader)PooledReader0).ReadString();
		string parentSurfaceGUID = ((Reader)PooledReader0).ReadString();
		Vector3 relativePosition = ((Reader)PooledReader0).ReadVector3();
		Quaternion relativeRotation = ((Reader)PooledReader0).ReadQuaternion((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___InitializeSurfaceItem_Client_2932264618(null, instance, gUID, parentSurfaceGUID, relativePosition, relativeRotation);
		}
	}

	protected override void Awake_UserLogic_ScheduleOne_002EEntityFramework_002ESurfaceItem_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
	}
}
