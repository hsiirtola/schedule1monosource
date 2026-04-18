using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Audio;
using ScheduleOne.Core;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Networking;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Trash;
using UnityEngine;

namespace ScheduleOne.Dragging;

public class DragManager : NetworkSingleton<DragManager>
{
	public const float DRAGGABLE_OFFSET = 1.25f;

	public AudioSourceController ThrowSound;

	[Header("Settings")]
	public float DragForce = 10f;

	public float DampingFactor = 0.5f;

	public float TorqueForce = 10f;

	public float TorqueDampingFactor = 0.5f;

	public float ThrowForce = 10f;

	public float MassInfluence = 0.6f;

	private List<Draggable> AllDraggables = new List<Draggable>();

	private List<Draggable> CurrentlyUpdating = new List<Draggable>();

	private Draggable lastThrownDraggable;

	private Draggable lastHeldDraggable;

	private bool NetworkInitialize___EarlyScheduleOne_002EDragging_002EDragManagerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EDragging_002EDragManagerAssembly_002DCSharp_002Edll_Excuted;

	public Draggable CurrentDraggable { get; protected set; }

	public bool IsDragging => (Object)(object)CurrentDraggable != (Object)null;

	public override void OnSpawnServer(NetworkConnection connection)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		((NetworkBehaviour)this).OnSpawnServer(connection);
		if (connection.IsHost)
		{
			return;
		}
		int num = 0;
		foreach (Draggable allDraggable in AllDraggables)
		{
			if (allDraggable.InitialReplicationMode != Draggable.EInitialReplicationMode.Off && (allDraggable.InitialReplicationMode == Draggable.EInitialReplicationMode.Full || Vector3.Distance(allDraggable.initialPosition, ((Component)allDraggable).transform.position) > 1f))
			{
				num++;
			}
		}
		ReplicationQueue.Enqueue(((object)this).GetType().Name, connection, SendDraggableData, 80 * num);
		void SendDraggableData(NetworkConnection conn)
		{
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			foreach (Draggable allDraggable2 in AllDraggables)
			{
				if (allDraggable2.InitialReplicationMode != Draggable.EInitialReplicationMode.Off && (allDraggable2.InitialReplicationMode == Draggable.EInitialReplicationMode.Full || Vector3.Distance(allDraggable2.initialPosition, ((Component)allDraggable2).transform.position) > 1f))
				{
					SetDraggableTransformData(conn, allDraggable2.GUID.ToString(), ((Component)allDraggable2).transform.position, ((Component)allDraggable2).transform.rotation, allDraggable2.Rigidbody.velocity);
				}
			}
		}
	}

	public void Update()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		if (IsDragging)
		{
			bool flag = false;
			LayerMask val = default(LayerMask);
			((LayerMask)(ref val)).value = 1 << LayerMask.NameToLayer("Default");
			((LayerMask)(ref val)).value = ((LayerMask)(ref val)).value | (1 << LayerMask.NameToLayer("NPC"));
			RaycastHit val2 = default(RaycastHit);
			if (Physics.Raycast(((Component)PlayerSingleton<PlayerMovement>.Instance).transform.position - PlayerSingleton<PlayerMovement>.Instance.Controller.height * Vector3.up * 0.5f, Vector3.down, ref val2, 0.5f, LayerMask.op_Implicit(val)))
			{
				flag = (Object)(object)((Component)((RaycastHit)(ref val2)).collider).GetComponentInParent<Draggable>() == (Object)(object)CurrentDraggable;
			}
			if (GameInput.GetButtonDown(GameInput.ButtonCode.Interact) || !IsDraggingAllowed() || Vector3.Distance(GetTargetPosition(), ((Component)CurrentDraggable).transform.position) > 1.5f || flag)
			{
				StopDragging(CurrentDraggable.Rigidbody.velocity);
			}
			else if (GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick))
			{
				Vector3 val3 = ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.forward * ThrowForce;
				float num = Mathf.Lerp(1f, Mathf.Sqrt(CurrentDraggable.Rigidbody.mass), MassInfluence);
				Vector3 velocity = CurrentDraggable.Rigidbody.velocity + val3 / num;
				CurrentDraggable.Rigidbody.velocity = velocity;
				lastThrownDraggable = CurrentDraggable;
				((Component)ThrowSound).transform.position = ((Component)lastThrownDraggable).transform.position;
				float num2 = Mathf.Sqrt(CurrentDraggable.Rigidbody.mass / 30f);
				ThrowSound.VolumeMultiplier = Mathf.Clamp(num2, 0.4f, 1f);
				ThrowSound.PitchMultiplier = Mathf.Lerp(0.6f, 0.4f, Mathf.Clamp01(num2));
				ThrowSound.Play();
				StopDragging(velocity);
			}
		}
	}

	public void FixedUpdate()
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		if (CurrentlyUpdating.Count > 0)
		{
			foreach (Draggable item in CurrentlyUpdating)
			{
				item.UpdateDraggable();
			}
		}
		if ((Object)(object)lastThrownDraggable != (Object)null)
		{
			((Component)ThrowSound).transform.position = ((Component)lastThrownDraggable).transform.position;
		}
		if (IsDragging)
		{
			CurrentDraggable.ApplyDragForces(GetTargetPosition());
		}
	}

	public bool IsDraggingAllowed()
	{
		if (PlayerSingleton<PlayerCamera>.Instance.activeUIElementCount > 0)
		{
			return false;
		}
		if (!Player.Local.Health.IsAlive)
		{
			return false;
		}
		if (Player.Local.IsSkating)
		{
			return false;
		}
		if (PlayerSingleton<PlayerInventory>.Instance.isAnythingEquipped)
		{
			if (((BaseItemInstance)PlayerSingleton<PlayerInventory>.Instance.equippedSlot.ItemInstance).ID == "trashgrabber")
			{
				return false;
			}
			if (((BaseItemInstance)PlayerSingleton<PlayerInventory>.Instance.equippedSlot.ItemInstance).ID == "trashbag" && TrashBag_Equippable.IsHoveringTrash)
			{
				return false;
			}
		}
		return true;
	}

	public void RegisterDraggable(Draggable draggable)
	{
		if (!AllDraggables.Contains(draggable))
		{
			AllDraggables.Add(draggable);
		}
	}

	public void Deregister(Draggable draggable)
	{
		if (AllDraggables.Contains(draggable))
		{
			AllDraggables.Remove(draggable);
		}
	}

	public void StartDragging(Draggable draggable)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)CurrentDraggable != (Object)null)
		{
			CurrentDraggable.StopDragging();
		}
		CurrentDraggable = draggable;
		lastHeldDraggable = draggable;
		draggable.StartDragging(Player.Local);
		SendDragger(draggable.GUID.ToString(), ((NetworkBehaviour)Player.Local).NetworkObject, ((Component)draggable).transform.position);
	}

	[ServerRpc(RequireOwnership = false)]
	private void SendDragger(string draggableGUID, NetworkObject dragger, Vector3 position)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		RpcWriter___Server_SendDragger_807933219(draggableGUID, dragger, position);
	}

	[ObserversRpc]
	private void SetDragger(string draggableGUID, NetworkObject dragger, Vector3 position)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		RpcWriter___Observers_SetDragger_807933219(draggableGUID, dragger, position);
	}

	public void StopDragging(Vector3 velocity)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)CurrentDraggable != (Object)null)
		{
			CurrentDraggable.StopDragging();
			SendDragger(CurrentDraggable.GUID.ToString(), null, ((Component)CurrentDraggable).transform.position);
			SendDraggableTransformData(CurrentDraggable.GUID.ToString(), ((Component)CurrentDraggable).transform.position, ((Component)CurrentDraggable).transform.rotation, velocity);
			CurrentDraggable = null;
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	private void SendDraggableTransformData(string guid, Vector3 position, Quaternion rotation, Vector3 velocity)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		RpcWriter___Server_SendDraggableTransformData_4062762274(guid, position, rotation, velocity);
		RpcLogic___SendDraggableTransformData_4062762274(guid, position, rotation, velocity);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void SetDraggableTransformData(NetworkConnection conn, string guid, Vector3 position, Quaternion rotation, Vector3 velocity)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (conn == null)
		{
			RpcWriter___Observers_SetDraggableTransformData_3831223955(conn, guid, position, rotation, velocity);
			RpcLogic___SetDraggableTransformData_3831223955(conn, guid, position, rotation, velocity);
		}
		else
		{
			RpcWriter___Target_SetDraggableTransformData_3831223955(conn, guid, position, rotation, velocity);
		}
	}

	private Vector3 GetTargetPosition()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position + ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.forward * 1.25f * CurrentDraggable.HoldDistanceMultiplier;
	}

	[Button]
	public void EnsureAllDraggableGUIDsAreValid()
	{
		List<string> list = new List<string>();
		Draggable[] array = Object.FindObjectsOfType<Draggable>();
		foreach (Draggable draggable in array)
		{
			if (draggable.BakedGUID == string.Empty || list.Contains(draggable.BakedGUID))
			{
				Console.Log("Regenerating GUID for draggable: " + ((Object)draggable).name);
				draggable.RegenerateGUID();
			}
			list.Add(draggable.BakedGUID);
		}
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EDragging_002EDragManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EDragging_002EDragManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_SendDragger_807933219));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_SetDragger_807933219));
			((NetworkBehaviour)this).RegisterServerRpc(2u, new ServerRpcDelegate(RpcReader___Server_SendDraggableTransformData_4062762274));
			((NetworkBehaviour)this).RegisterObserversRpc(3u, new ClientRpcDelegate(RpcReader___Observers_SetDraggableTransformData_3831223955));
			((NetworkBehaviour)this).RegisterTargetRpc(4u, new ClientRpcDelegate(RpcReader___Target_SetDraggableTransformData_3831223955));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EDragging_002EDragManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EDragging_002EDragManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_SendDragger_807933219(string draggableGUID, NetworkObject dragger, Vector3 position)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(draggableGUID);
			((Writer)writer).WriteNetworkObject(dragger);
			((Writer)writer).WriteVector3(position);
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___SendDragger_807933219(string draggableGUID, NetworkObject dragger, Vector3 position)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		SetDragger(draggableGUID, dragger, position);
	}

	private void RpcReader___Server_SendDragger_807933219(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		string draggableGUID = ((Reader)PooledReader0).ReadString();
		NetworkObject dragger = ((Reader)PooledReader0).ReadNetworkObject();
		Vector3 position = ((Reader)PooledReader0).ReadVector3();
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SendDragger_807933219(draggableGUID, dragger, position);
		}
	}

	private void RpcWriter___Observers_SetDragger_807933219(string draggableGUID, NetworkObject dragger, Vector3 position)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(draggableGUID);
			((Writer)writer).WriteNetworkObject(dragger);
			((Writer)writer).WriteVector3(position);
			((NetworkBehaviour)this).SendObserversRpc(1u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetDragger_807933219(string draggableGUID, NetworkObject dragger, Vector3 position)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		Draggable draggable = GUIDManager.GetObject<Draggable>(new Guid(draggableGUID));
		Player player = (((Object)(object)dragger != (Object)null) ? ((Component)dragger).GetComponent<Player>() : null);
		if (!((Object)(object)draggable != (Object)null))
		{
			return;
		}
		if ((Object)(object)CurrentDraggable != (Object)(object)draggable && (Object)(object)lastHeldDraggable != (Object)(object)draggable)
		{
			draggable.Rigidbody.position = position;
		}
		if ((Object)(object)dragger != (Object)null)
		{
			if ((Object)(object)player != (Object)null)
			{
				draggable.StartDragging(((Component)dragger).GetComponent<Player>());
				if (!CurrentlyUpdating.Contains(draggable))
				{
					CurrentlyUpdating.Add(draggable);
				}
			}
		}
		else
		{
			draggable.StopDragging();
			CurrentlyUpdating.Remove(draggable);
		}
	}

	private void RpcReader___Observers_SetDragger_807933219(PooledReader PooledReader0, Channel channel)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		string draggableGUID = ((Reader)PooledReader0).ReadString();
		NetworkObject dragger = ((Reader)PooledReader0).ReadNetworkObject();
		Vector3 position = ((Reader)PooledReader0).ReadVector3();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetDragger_807933219(draggableGUID, dragger, position);
		}
	}

	private void RpcWriter___Server_SendDraggableTransformData_4062762274(string guid, Vector3 position, Quaternion rotation, Vector3 velocity)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(guid);
			((Writer)writer).WriteVector3(position);
			((Writer)writer).WriteQuaternion(rotation, (AutoPackType)1);
			((Writer)writer).WriteVector3(velocity);
			((NetworkBehaviour)this).SendServerRpc(2u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___SendDraggableTransformData_4062762274(string guid, Vector3 position, Quaternion rotation, Vector3 velocity)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		SetDraggableTransformData(null, guid, position, rotation, velocity);
	}

	private void RpcReader___Server_SendDraggableTransformData_4062762274(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		string guid = ((Reader)PooledReader0).ReadString();
		Vector3 position = ((Reader)PooledReader0).ReadVector3();
		Quaternion rotation = ((Reader)PooledReader0).ReadQuaternion((AutoPackType)1);
		Vector3 velocity = ((Reader)PooledReader0).ReadVector3();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendDraggableTransformData_4062762274(guid, position, rotation, velocity);
		}
	}

	private void RpcWriter___Observers_SetDraggableTransformData_3831223955(NetworkConnection conn, string guid, Vector3 position, Quaternion rotation, Vector3 velocity)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(guid);
			((Writer)writer).WriteVector3(position);
			((Writer)writer).WriteQuaternion(rotation, (AutoPackType)1);
			((Writer)writer).WriteVector3(velocity);
			((NetworkBehaviour)this).SendObserversRpc(3u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetDraggableTransformData_3831223955(NetworkConnection conn, string guid, Vector3 position, Quaternion rotation, Vector3 velocity)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		Draggable draggable = GUIDManager.GetObject<Draggable>(new Guid(guid));
		if ((Object)(object)draggable == (Object)null)
		{
			Console.LogWarning("Failed to find draggable with GUID " + guid);
		}
		if (!((Object)(object)draggable == (Object)(object)lastThrownDraggable) && !((Object)(object)draggable == (Object)(object)lastHeldDraggable) && (Object)(object)draggable != (Object)null)
		{
			draggable.Rigidbody.position = position;
			draggable.Rigidbody.rotation = rotation;
			draggable.Rigidbody.velocity = velocity;
		}
	}

	private void RpcReader___Observers_SetDraggableTransformData_3831223955(PooledReader PooledReader0, Channel channel)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		string guid = ((Reader)PooledReader0).ReadString();
		Vector3 position = ((Reader)PooledReader0).ReadVector3();
		Quaternion rotation = ((Reader)PooledReader0).ReadQuaternion((AutoPackType)1);
		Vector3 velocity = ((Reader)PooledReader0).ReadVector3();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetDraggableTransformData_3831223955(null, guid, position, rotation, velocity);
		}
	}

	private void RpcWriter___Target_SetDraggableTransformData_3831223955(NetworkConnection conn, string guid, Vector3 position, Quaternion rotation, Vector3 velocity)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(guid);
			((Writer)writer).WriteVector3(position);
			((Writer)writer).WriteQuaternion(rotation, (AutoPackType)1);
			((Writer)writer).WriteVector3(velocity);
			((NetworkBehaviour)this).SendTargetRpc(4u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetDraggableTransformData_3831223955(PooledReader PooledReader0, Channel channel)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		string guid = ((Reader)PooledReader0).ReadString();
		Vector3 position = ((Reader)PooledReader0).ReadVector3();
		Quaternion rotation = ((Reader)PooledReader0).ReadQuaternion((AutoPackType)1);
		Vector3 velocity = ((Reader)PooledReader0).ReadVector3();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetDraggableTransformData_3831223955(((NetworkBehaviour)this).LocalConnection, guid, position, rotation, velocity);
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
