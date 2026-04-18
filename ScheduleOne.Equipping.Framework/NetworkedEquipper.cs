using System.Collections.Generic;
using System.Linq;
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
using ScheduleOne.Core;
using ScheduleOne.Core.Equipping.Framework;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.ItemFramework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ScheduleOne.Equipping.Framework;

public abstract class NetworkedEquipper : NetworkBehaviour
{
	[SyncObject]
	private readonly SyncList<EquippedItemHandler> _networkEquippedItems = new SyncList<EquippedItemHandler>();

	private List<EquippedItemHandler> _allEquippedItems = new List<EquippedItemHandler>();

	private bool NetworkInitialize___EarlyScheduleOne_002EEquipping_002EFramework_002ENetworkedEquipperAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EEquipping_002EFramework_002ENetworkedEquipperAssembly_002DCSharp_002Edll_Excuted;

	protected abstract IEquippableUser GetUser();

	public override void OnStartClient()
	{
		((NetworkBehaviour)this).OnStartClient();
		_networkEquippedItems.OnChange += NetworkEquippedItems_OnChange;
	}

	public IEquippedItemHandler Equip(EquippableData equippable, bool networked = true)
	{
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkBehaviour)this).IsOwner && !((NetworkBehaviour)this).IsServer && networked)
		{
			Console.LogWarning("Only the owner can equip networked items.");
			return null;
		}
		if (!CanEquip(equippable))
		{
			Console.LogWarning("Cannot equip " + ((object)equippable)?.ToString() + " because required hand(s) are already occupied.");
			return null;
		}
		EquippedItemHandler equippedItemHandler = CreateHandlerForEquippable(equippable) as EquippedItemHandler;
		((NetworkBehaviour)equippedItemHandler).NetworkObject.SetIsNetworked(networked);
		equippedItemHandler.Equipped(GetUser(), equippable);
		if (networked)
		{
			((NetworkBehaviour)this).NetworkObject.Spawn(((Component)equippedItemHandler).gameObject, (NetworkConnection)null, default(Scene));
		}
		AddEquippedItem(equippedItemHandler);
		return (IEquippedItemHandler)(object)equippedItemHandler;
	}

	public IEquippedItemHandler Equip(BaseItemInstance item, bool networked = true)
	{
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkBehaviour)this).IsOwner && !((NetworkBehaviour)this).IsServer && networked)
		{
			Console.LogWarning("Only the owner can equip networked items.");
			return null;
		}
		if (!(item is ItemInstance itemInstance))
		{
			Console.LogWarning("Cannot equip item instance of type " + ((object)item).GetType().Name + " that does not inherit from ItemInstance!");
			return null;
		}
		if ((Object)(object)itemInstance.Equippable == (Object)null)
		{
			Console.LogWarning("Item " + ((object)item)?.ToString() + " is not equippable. Ignoring equip request.");
			return null;
		}
		EquippableData equippableData = ((BaseItemInstance)itemInstance).EquippableData;
		if (!CanEquip(equippableData))
		{
			Console.LogWarning("Cannot equip " + ((object)equippableData)?.ToString() + " because required hand(s) are already occupied.");
			return null;
		}
		EquippedItemHandler equippedItemHandler = CreateHandlerForEquippable(equippableData) as EquippedItemHandler;
		((NetworkBehaviour)equippedItemHandler).NetworkObject.SetIsNetworked(networked);
		equippedItemHandler.EquippedWithItem(GetUser(), equippableData, (BaseItemInstance)(object)itemInstance);
		if (networked)
		{
			((NetworkBehaviour)this).NetworkObject.Spawn(((Component)equippedItemHandler).gameObject, (NetworkConnection)null, default(Scene));
		}
		AddEquippedItem(equippedItemHandler);
		return (IEquippedItemHandler)(object)equippedItemHandler;
	}

	public void Unequip(IEquippedItemHandler equippedItem)
	{
		if (!((NetworkBehaviour)this).IsOwner && !((NetworkBehaviour)this).IsServer && ((NetworkBehaviour)(EquippedItemHandler)(object)equippedItem).IsNetworked)
		{
			Console.LogWarning("Only the owner can unequip networked items. Ignoring unequip request for " + (object)equippedItem);
			return;
		}
		if (equippedItem == null)
		{
			Console.LogWarning("Cannot unequip null handler!");
			return;
		}
		if (!IsItemEquipped((EquippedItemHandler)(object)equippedItem))
		{
			Console.LogWarning("Cannot unequip handler that is not currently equipped: " + ((Object)equippedItem.gameObject).name);
			return;
		}
		RemoveEquippedItem((EquippedItemHandler)(object)equippedItem);
		Unequip_Server((EquippedItemHandler)(object)equippedItem);
	}

	private void AddEquippedItem(EquippedItemHandler handler)
	{
		if (((NetworkBehaviour)handler).IsNetworked)
		{
			AddNetworkedEquippedItem_Server(handler);
		}
		else if (!IsItemEquipped(handler))
		{
			_allEquippedItems.Add(handler);
		}
	}

	private void RemoveEquippedItem(EquippedItemHandler handler)
	{
		if (((NetworkBehaviour)handler).IsNetworked)
		{
			RemoveNetworkedEquippedItem_Server(handler);
		}
		else if (IsItemEquipped(handler))
		{
			_allEquippedItems.Remove(handler);
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	private void Unequip_Server(EquippedItemHandler handler)
	{
		RpcWriter___Server_Unequip_Server_897730888(handler);
		RpcLogic___Unequip_Server_897730888(handler);
	}

	[ObserversRpc(RunLocally = true)]
	private void Unequip_Client(EquippedItemHandler handler)
	{
		RpcWriter___Observers_Unequip_Client_897730888(handler);
		RpcLogic___Unequip_Client_897730888(handler);
	}

	[ServerRpc(RunLocally = true, RequireOwnership = false)]
	private void AddNetworkedEquippedItem_Server(EquippedItemHandler handler)
	{
		RpcWriter___Server_AddNetworkedEquippedItem_Server_897730888(handler);
		RpcLogic___AddNetworkedEquippedItem_Server_897730888(handler);
	}

	[ServerRpc(RunLocally = true, RequireOwnership = false)]
	private void RemoveNetworkedEquippedItem_Server(EquippedItemHandler handler)
	{
		RpcWriter___Server_RemoveNetworkedEquippedItem_Server_897730888(handler);
		RpcLogic___RemoveNetworkedEquippedItem_Server_897730888(handler);
	}

	private IEquippedItemHandler CreateHandlerForEquippable(EquippableData equippable)
	{
		IEquippedItemHandler handlerPrefab = EquippableHandlerService.GetHandlerPrefab(equippable);
		if (handlerPrefab == null)
		{
			Console.LogError("No handler found for equippable: " + (object)equippable);
			return null;
		}
		return Object.Instantiate<GameObject>(handlerPrefab.gameObject).GetComponent<IEquippedItemHandler>();
	}

	private void NetworkEquippedItems_OnChange(SyncListOperation op, int index, EquippedItemHandler oldItem, EquippedItemHandler newItem, bool asServer)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between Unknown and I4
		if ((int)op == 0)
		{
			if (!IsItemEquipped(newItem))
			{
				_allEquippedItems.Add(newItem);
			}
		}
		else if ((int)op == 3 && IsItemEquipped(oldItem))
		{
			_allEquippedItems.Remove(oldItem);
		}
	}

	public void UnequipAll()
	{
		if (!((NetworkBehaviour)this).IsOwner && !((NetworkBehaviour)this).IsServer)
		{
			Console.LogWarning("Only the owner can unequip items. Ignoring unequip request.");
			return;
		}
		IEquippedItemHandler[] array = (IEquippedItemHandler[])(object)_allEquippedItems.ToArray();
		array = array;
		foreach (IEquippedItemHandler equippedItem in array)
		{
			Unequip(equippedItem);
		}
	}

	private bool CanEquip(EquippableData equippable)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between Unknown and I4
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Invalid comparison between Unknown and I4
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Invalid comparison between Unknown and I4
		if ((Object)(object)equippable == (Object)null)
		{
			return false;
		}
		if (((int)equippable.Hand == 0 || (int)equippable.Hand == 2) && IsRightHandOccupied())
		{
			return false;
		}
		if (((int)equippable.Hand == 1 || (int)equippable.Hand == 2) && IsLeftHandOccupied())
		{
			return false;
		}
		return true;
	}

	private bool IsRightHandOccupied()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Invalid comparison between Unknown and I4
		foreach (EquippedItemHandler allEquippedItem in _allEquippedItems)
		{
			if ((int)allEquippedItem.EquippableData.Hand == 0 || (int)allEquippedItem.EquippableData.Hand == 2)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsLeftHandOccupied()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Invalid comparison between Unknown and I4
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Invalid comparison between Unknown and I4
		foreach (EquippedItemHandler allEquippedItem in _allEquippedItems)
		{
			if ((int)allEquippedItem.EquippableData.Hand == 1 || (int)allEquippedItem.EquippableData.Hand == 2)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsItemEquipped(EquippedItemHandler handler)
	{
		foreach (EquippedItemHandler allEquippedItem in _allEquippedItems)
		{
			if (((NetworkBehaviour)allEquippedItem).NetworkObject.ObjectId == ((NetworkBehaviour)handler).NetworkObject.ObjectId)
			{
				return true;
			}
		}
		return false;
	}

	[Button]
	public void PrintLists()
	{
		Debug.Log((object)(((Object)((Component)this).gameObject).name + " Equipped Items: " + string.Join(", ", _allEquippedItems.Select((EquippedItemHandler i) => ((Object)((Component)i).gameObject).name + (((NetworkBehaviour)i).IsNetworked ? " (Networked)" : " (Local)")))));
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Expected O, but got Unknown
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EEquipping_002EFramework_002ENetworkedEquipperAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EEquipping_002EFramework_002ENetworkedEquipperAssembly_002DCSharp_002Edll_Excuted = true;
			((SyncBase)_networkEquippedItems).InitializeInstance((NetworkBehaviour)(object)this, 0u, (WritePermission)0, (ReadPermission)0, -1f, (Channel)0, true);
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_Unequip_Server_897730888));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_Unequip_Client_897730888));
			((NetworkBehaviour)this).RegisterServerRpc(2u, new ServerRpcDelegate(RpcReader___Server_AddNetworkedEquippedItem_Server_897730888));
			((NetworkBehaviour)this).RegisterServerRpc(3u, new ServerRpcDelegate(RpcReader___Server_RemoveNetworkedEquippedItem_Server_897730888));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EEquipping_002EFramework_002ENetworkedEquipperAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EEquipping_002EFramework_002ENetworkedEquipperAssembly_002DCSharp_002Edll_Excuted = true;
			((SyncBase)_networkEquippedItems).SetRegistered();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_Unequip_Server_897730888(EquippedItemHandler handler)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EEquipping_002EEquippedItemHandlerFishNet_002ESerializing_002EGenerated((Writer)(object)writer, handler);
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___Unequip_Server_897730888(EquippedItemHandler handler)
	{
		Unequip_Client(handler);
	}

	private void RpcReader___Server_Unequip_Server_897730888(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		EquippedItemHandler handler = GeneratedReaders___Internal.Read___ScheduleOne_002EEquipping_002EEquippedItemHandlerFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___Unequip_Server_897730888(handler);
		}
	}

	private void RpcWriter___Observers_Unequip_Client_897730888(EquippedItemHandler handler)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EEquipping_002EEquippedItemHandlerFishNet_002ESerializing_002EGenerated((Writer)(object)writer, handler);
			((NetworkBehaviour)this).SendObserversRpc(1u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___Unequip_Client_897730888(EquippedItemHandler handler)
	{
		if (handler.IsEquipped)
		{
			handler.Unequipped();
		}
	}

	private void RpcReader___Observers_Unequip_Client_897730888(PooledReader PooledReader0, Channel channel)
	{
		EquippedItemHandler handler = GeneratedReaders___Internal.Read___ScheduleOne_002EEquipping_002EEquippedItemHandlerFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___Unequip_Client_897730888(handler);
		}
	}

	private void RpcWriter___Server_AddNetworkedEquippedItem_Server_897730888(EquippedItemHandler handler)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EEquipping_002EEquippedItemHandlerFishNet_002ESerializing_002EGenerated((Writer)(object)writer, handler);
			((NetworkBehaviour)this).SendServerRpc(2u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___AddNetworkedEquippedItem_Server_897730888(EquippedItemHandler handler)
	{
		if (!_networkEquippedItems.Contains(handler))
		{
			_networkEquippedItems.Add(handler);
		}
	}

	private void RpcReader___Server_AddNetworkedEquippedItem_Server_897730888(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		EquippedItemHandler handler = GeneratedReaders___Internal.Read___ScheduleOne_002EEquipping_002EEquippedItemHandlerFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___AddNetworkedEquippedItem_Server_897730888(handler);
		}
	}

	private void RpcWriter___Server_RemoveNetworkedEquippedItem_Server_897730888(EquippedItemHandler handler)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EEquipping_002EEquippedItemHandlerFishNet_002ESerializing_002EGenerated((Writer)(object)writer, handler);
			((NetworkBehaviour)this).SendServerRpc(3u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___RemoveNetworkedEquippedItem_Server_897730888(EquippedItemHandler handler)
	{
		if (_networkEquippedItems.Contains(handler))
		{
			_networkEquippedItems.Remove(handler);
		}
	}

	private void RpcReader___Server_RemoveNetworkedEquippedItem_Server_897730888(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		EquippedItemHandler handler = GeneratedReaders___Internal.Read___ScheduleOne_002EEquipping_002EEquippedItemHandlerFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___RemoveNetworkedEquippedItem_Server_897730888(handler);
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}
}
