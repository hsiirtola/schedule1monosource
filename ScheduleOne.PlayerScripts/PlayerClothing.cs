using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.AvatarFramework;
using ScheduleOne.Clothing;
using ScheduleOne.ItemFramework;
using UnityEngine;

namespace ScheduleOne.PlayerScripts;

public class PlayerClothing : NetworkBehaviour, IItemSlotOwner
{
	public Player Player;

	public Dictionary<EClothingSlot, ItemSlot> ClothingSlots = new Dictionary<EClothingSlot, ItemSlot>();

	private List<ClothingInstance> appliedClothing = new List<ClothingInstance>();

	private bool NetworkInitialize___EarlyScheduleOne_002EPlayerScripts_002EPlayerClothingAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EPlayerScripts_002EPlayerClothingAssembly_002DCSharp_002Edll_Excuted;

	public List<ItemSlot> ItemSlots { get; set; } = new List<ItemSlot>();

	private AvatarSettings appearanceSettings => Player.Avatar.CurrentSettings;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EPlayerScripts_002EPlayerClothing_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		((NetworkBehaviour)this).OnSpawnServer(connection);
		if (!connection.IsLocalClient)
		{
			((IItemSlotOwner)this).SendItemSlotDataToClient(connection);
		}
	}

	public void InsertClothing(ClothingInstance clothing)
	{
		EClothingSlot slot = (clothing.Definition as ClothingDefinition).Slot;
		if (!ClothingSlots.ContainsKey(slot))
		{
			Console.LogError("No slot found for clothing slot type: " + slot);
		}
		else
		{
			ClothingSlots[slot].SetStoredItem(clothing);
		}
	}

	protected virtual void ClothingChanged()
	{
		RefreshAppearance();
	}

	public virtual void RefreshAppearance()
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		AvatarSettings avatarSettings = Object.Instantiate<AvatarSettings>(appearanceSettings);
		for (int i = 0; i < avatarSettings.BodyLayerSettings.Count; i++)
		{
			if (!(avatarSettings.BodyLayerSettings[i].layerPath == string.Empty))
			{
				AvatarLayer avatarLayer = Resources.Load<AvatarLayer>(avatarSettings.BodyLayerSettings[i].layerPath);
				if (!((Object)(object)avatarLayer == (Object)null) && avatarLayer.Order > 19 && !TryGetInventoryClothing(avatarLayer.AssetPath, avatarSettings.BodyLayerSettings[i].layerTint, out var _))
				{
					avatarSettings.BodyLayerSettings.RemoveAt(i);
					i--;
				}
			}
		}
		for (int j = 0; j < avatarSettings.AccessorySettings.Count; j++)
		{
			if (!(avatarSettings.AccessorySettings[j].path == string.Empty))
			{
				Accessory accessory = Resources.Load<Accessory>(avatarSettings.AccessorySettings[j].path);
				if (!((Object)(object)accessory == (Object)null) && !TryGetInventoryClothing(accessory.AssetPath, avatarSettings.AccessorySettings[j].color, out var _))
				{
					avatarSettings.AccessorySettings.RemoveAt(j);
					j--;
				}
			}
		}
		for (int k = 0; k < ItemSlots.Count; k++)
		{
			if (ItemSlots[k].Quantity > 0 && ItemSlots[k].ItemInstance is ClothingInstance clothing3 && !IsClothingApplied(avatarSettings, clothing3))
			{
				ApplyClothing(avatarSettings, clothing3);
			}
		}
		Player.SetAvatarSettings(avatarSettings);
	}

	private bool TryGetInventoryClothing(string assetPath, Color color, out ClothingInstance clothing)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		clothing = null;
		for (int i = 0; i < ItemSlots.Count; i++)
		{
			if (ItemSlots[i].Quantity > 0 && ItemSlots[i].ItemInstance is ClothingInstance clothingInstance)
			{
				ClothingDefinition clothingDefinition = clothingInstance.Definition as ClothingDefinition;
				if (!((Object)(object)clothingDefinition == (Object)null) && clothingInstance.Color.GetActualColor() == color && clothingDefinition.ClothingAssetPath == assetPath)
				{
					clothing = clothingInstance;
					return true;
				}
			}
		}
		return false;
	}

	private bool IsClothingApplied(AvatarSettings settings, ClothingInstance clothing)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		if (clothing == null)
		{
			return false;
		}
		ClothingDefinition clothingDefinition = clothing.Definition as ClothingDefinition;
		for (int i = 0; i < settings.BodyLayerSettings.Count; i++)
		{
			if (settings.BodyLayerSettings[i].layerPath == clothingDefinition.ClothingAssetPath && settings.BodyLayerSettings[i].layerTint == clothing.Color.GetActualColor())
			{
				return true;
			}
		}
		for (int j = 0; j < settings.AccessorySettings.Count; j++)
		{
			if (settings.AccessorySettings[j].path == clothingDefinition.ClothingAssetPath && settings.AccessorySettings[j].color == clothing.Color.GetActualColor())
			{
				return true;
			}
		}
		return false;
	}

	private void ApplyClothing(AvatarSettings settings, ClothingInstance clothing)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if (clothing != null)
		{
			ClothingDefinition clothingDefinition = clothing.Definition as ClothingDefinition;
			if (clothingDefinition.ApplicationType == EClothingApplicationType.BodyLayer)
			{
				settings.BodyLayerSettings.Add(new AvatarSettings.LayerSetting
				{
					layerPath = clothingDefinition.ClothingAssetPath,
					layerTint = clothing.Color.GetActualColor()
				});
			}
			else if (clothingDefinition.ApplicationType == EClothingApplicationType.Accessory)
			{
				settings.AccessorySettings.Add(new AvatarSettings.AccessorySetting
				{
					path = clothingDefinition.ClothingAssetPath,
					color = clothing.Color.GetActualColor()
				});
			}
			else
			{
				Console.LogError("Unknown clothing application type: " + clothingDefinition.ApplicationType);
			}
		}
	}

	[ServerRpc(RunLocally = true, RequireOwnership = false)]
	public void SetStoredInstance(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
	{
		RpcWriter___Server_SetStoredInstance_2652194801(conn, itemSlotIndex, instance);
		RpcLogic___SetStoredInstance_2652194801(conn, itemSlotIndex, instance);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void SetStoredInstance_Internal(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetStoredInstance_Internal_2652194801(conn, itemSlotIndex, instance);
			RpcLogic___SetStoredInstance_Internal_2652194801(conn, itemSlotIndex, instance);
		}
		else
		{
			RpcWriter___Target_SetStoredInstance_Internal_2652194801(conn, itemSlotIndex, instance);
		}
	}

	[ServerRpc(RunLocally = true, RequireOwnership = false)]
	public void SetItemSlotQuantity(int itemSlotIndex, int quantity)
	{
		RpcWriter___Server_SetItemSlotQuantity_1692629761(itemSlotIndex, quantity);
		RpcLogic___SetItemSlotQuantity_1692629761(itemSlotIndex, quantity);
	}

	[ObserversRpc(RunLocally = true)]
	private void SetItemSlotQuantity_Internal(int itemSlotIndex, int quantity)
	{
		RpcWriter___Observers_SetItemSlotQuantity_Internal_1692629761(itemSlotIndex, quantity);
		RpcLogic___SetItemSlotQuantity_Internal_1692629761(itemSlotIndex, quantity);
	}

	[ServerRpc(RunLocally = true, RequireOwnership = false)]
	public void SetSlotLocked(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
	{
		RpcWriter___Server_SetSlotLocked_3170825843(conn, itemSlotIndex, locked, lockOwner, lockReason);
		RpcLogic___SetSlotLocked_3170825843(conn, itemSlotIndex, locked, lockOwner, lockReason);
	}

	[TargetRpc]
	[ObserversRpc(RunLocally = true)]
	private void SetSlotLocked_Internal(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetSlotLocked_Internal_3170825843(conn, itemSlotIndex, locked, lockOwner, lockReason);
			RpcLogic___SetSlotLocked_Internal_3170825843(conn, itemSlotIndex, locked, lockOwner, lockReason);
		}
		else
		{
			RpcWriter___Target_SetSlotLocked_Internal_3170825843(conn, itemSlotIndex, locked, lockOwner, lockReason);
		}
	}

	[ServerRpc(RunLocally = true, RequireOwnership = false)]
	public void SetSlotFilter(NetworkConnection conn, int itemSlotIndex, SlotFilter filter)
	{
		RpcWriter___Server_SetSlotFilter_527532783(conn, itemSlotIndex, filter);
		RpcLogic___SetSlotFilter_527532783(conn, itemSlotIndex, filter);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void SetSlotFilter_Internal(NetworkConnection conn, int itemSlotIndex, SlotFilter filter)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetSlotFilter_Internal_527532783(conn, itemSlotIndex, filter);
			RpcLogic___SetSlotFilter_Internal_527532783(conn, itemSlotIndex, filter);
		}
		else
		{
			RpcWriter___Target_SetSlotFilter_Internal_527532783(conn, itemSlotIndex, filter);
		}
	}

	public override void NetworkInitialize___Early()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
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
		if (!NetworkInitialize___EarlyScheduleOne_002EPlayerScripts_002EPlayerClothingAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EPlayerScripts_002EPlayerClothingAssembly_002DCSharp_002Edll_Excuted = true;
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_SetStoredInstance_2652194801));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_SetStoredInstance_Internal_2652194801));
			((NetworkBehaviour)this).RegisterTargetRpc(2u, new ClientRpcDelegate(RpcReader___Target_SetStoredInstance_Internal_2652194801));
			((NetworkBehaviour)this).RegisterServerRpc(3u, new ServerRpcDelegate(RpcReader___Server_SetItemSlotQuantity_1692629761));
			((NetworkBehaviour)this).RegisterObserversRpc(4u, new ClientRpcDelegate(RpcReader___Observers_SetItemSlotQuantity_Internal_1692629761));
			((NetworkBehaviour)this).RegisterServerRpc(5u, new ServerRpcDelegate(RpcReader___Server_SetSlotLocked_3170825843));
			((NetworkBehaviour)this).RegisterTargetRpc(6u, new ClientRpcDelegate(RpcReader___Target_SetSlotLocked_Internal_3170825843));
			((NetworkBehaviour)this).RegisterObserversRpc(7u, new ClientRpcDelegate(RpcReader___Observers_SetSlotLocked_Internal_3170825843));
			((NetworkBehaviour)this).RegisterServerRpc(8u, new ServerRpcDelegate(RpcReader___Server_SetSlotFilter_527532783));
			((NetworkBehaviour)this).RegisterObserversRpc(9u, new ClientRpcDelegate(RpcReader___Observers_SetSlotFilter_Internal_527532783));
			((NetworkBehaviour)this).RegisterTargetRpc(10u, new ClientRpcDelegate(RpcReader___Target_SetSlotFilter_Internal_527532783));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EPlayerScripts_002EPlayerClothingAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EPlayerScripts_002EPlayerClothingAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_SetStoredInstance_2652194801(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteNetworkConnection(conn);
			((Writer)writer).WriteInt32(itemSlotIndex, (AutoPackType)1);
			((Writer)(object)writer).WriteItemInstance(instance);
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetStoredInstance_2652194801(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
	{
		if (conn == (NetworkConnection)null || conn.ClientId == -1)
		{
			SetStoredInstance_Internal(null, itemSlotIndex, instance);
		}
		else
		{
			SetStoredInstance_Internal(conn, itemSlotIndex, instance);
		}
	}

	private void RpcReader___Server_SetStoredInstance_2652194801(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkConnection conn2 = ((Reader)PooledReader0).ReadNetworkConnection();
		int itemSlotIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		ItemInstance instance = ((Reader)(object)PooledReader0).ReadItemInstance();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetStoredInstance_2652194801(conn2, itemSlotIndex, instance);
		}
	}

	private void RpcWriter___Observers_SetStoredInstance_Internal_2652194801(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
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
			((Writer)writer).WriteInt32(itemSlotIndex, (AutoPackType)1);
			((Writer)(object)writer).WriteItemInstance(instance);
			((NetworkBehaviour)this).SendObserversRpc(1u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetStoredInstance_Internal_2652194801(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
	{
		if (instance != null)
		{
			ItemSlots[itemSlotIndex].SetStoredItem(instance, _internal: true);
		}
		else
		{
			ItemSlots[itemSlotIndex].ClearStoredInstance(_internal: true);
		}
	}

	private void RpcReader___Observers_SetStoredInstance_Internal_2652194801(PooledReader PooledReader0, Channel channel)
	{
		int itemSlotIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		ItemInstance instance = ((Reader)(object)PooledReader0).ReadItemInstance();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetStoredInstance_Internal_2652194801(null, itemSlotIndex, instance);
		}
	}

	private void RpcWriter___Target_SetStoredInstance_Internal_2652194801(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
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
			((Writer)writer).WriteInt32(itemSlotIndex, (AutoPackType)1);
			((Writer)(object)writer).WriteItemInstance(instance);
			((NetworkBehaviour)this).SendTargetRpc(2u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetStoredInstance_Internal_2652194801(PooledReader PooledReader0, Channel channel)
	{
		int itemSlotIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		ItemInstance instance = ((Reader)(object)PooledReader0).ReadItemInstance();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetStoredInstance_Internal_2652194801(((NetworkBehaviour)this).LocalConnection, itemSlotIndex, instance);
		}
	}

	private void RpcWriter___Server_SetItemSlotQuantity_1692629761(int itemSlotIndex, int quantity)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(itemSlotIndex, (AutoPackType)1);
			((Writer)writer).WriteInt32(quantity, (AutoPackType)1);
			((NetworkBehaviour)this).SendServerRpc(3u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetItemSlotQuantity_1692629761(int itemSlotIndex, int quantity)
	{
		SetItemSlotQuantity_Internal(itemSlotIndex, quantity);
	}

	private void RpcReader___Server_SetItemSlotQuantity_1692629761(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int itemSlotIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		int quantity = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetItemSlotQuantity_1692629761(itemSlotIndex, quantity);
		}
	}

	private void RpcWriter___Observers_SetItemSlotQuantity_Internal_1692629761(int itemSlotIndex, int quantity)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(itemSlotIndex, (AutoPackType)1);
			((Writer)writer).WriteInt32(quantity, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(4u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetItemSlotQuantity_Internal_1692629761(int itemSlotIndex, int quantity)
	{
		ItemSlots[itemSlotIndex].SetQuantity(quantity, _internal: true);
	}

	private void RpcReader___Observers_SetItemSlotQuantity_Internal_1692629761(PooledReader PooledReader0, Channel channel)
	{
		int itemSlotIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		int quantity = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetItemSlotQuantity_Internal_1692629761(itemSlotIndex, quantity);
		}
	}

	private void RpcWriter___Server_SetSlotLocked_3170825843(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteNetworkConnection(conn);
			((Writer)writer).WriteInt32(itemSlotIndex, (AutoPackType)1);
			((Writer)writer).WriteBoolean(locked);
			((Writer)writer).WriteNetworkObject(lockOwner);
			((Writer)writer).WriteString(lockReason);
			((NetworkBehaviour)this).SendServerRpc(5u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetSlotLocked_3170825843(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
	{
		if (conn == (NetworkConnection)null || conn.ClientId == -1)
		{
			SetSlotLocked_Internal(null, itemSlotIndex, locked, lockOwner, lockReason);
		}
		else
		{
			SetSlotLocked_Internal(conn, itemSlotIndex, locked, lockOwner, lockReason);
		}
	}

	private void RpcReader___Server_SetSlotLocked_3170825843(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkConnection conn2 = ((Reader)PooledReader0).ReadNetworkConnection();
		int itemSlotIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		bool locked = ((Reader)PooledReader0).ReadBoolean();
		NetworkObject lockOwner = ((Reader)PooledReader0).ReadNetworkObject();
		string lockReason = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetSlotLocked_3170825843(conn2, itemSlotIndex, locked, lockOwner, lockReason);
		}
	}

	private void RpcWriter___Target_SetSlotLocked_Internal_3170825843(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(itemSlotIndex, (AutoPackType)1);
			((Writer)writer).WriteBoolean(locked);
			((Writer)writer).WriteNetworkObject(lockOwner);
			((Writer)writer).WriteString(lockReason);
			((NetworkBehaviour)this).SendTargetRpc(6u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcLogic___SetSlotLocked_Internal_3170825843(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
	{
		if (locked)
		{
			ItemSlots[itemSlotIndex].ApplyLock(lockOwner, lockReason, _internal: true);
		}
		else
		{
			ItemSlots[itemSlotIndex].RemoveLock(_internal: true);
		}
	}

	private void RpcReader___Target_SetSlotLocked_Internal_3170825843(PooledReader PooledReader0, Channel channel)
	{
		int itemSlotIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		bool locked = ((Reader)PooledReader0).ReadBoolean();
		NetworkObject lockOwner = ((Reader)PooledReader0).ReadNetworkObject();
		string lockReason = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetSlotLocked_Internal_3170825843(((NetworkBehaviour)this).LocalConnection, itemSlotIndex, locked, lockOwner, lockReason);
		}
	}

	private void RpcWriter___Observers_SetSlotLocked_Internal_3170825843(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(itemSlotIndex, (AutoPackType)1);
			((Writer)writer).WriteBoolean(locked);
			((Writer)writer).WriteNetworkObject(lockOwner);
			((Writer)writer).WriteString(lockReason);
			((NetworkBehaviour)this).SendObserversRpc(7u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcReader___Observers_SetSlotLocked_Internal_3170825843(PooledReader PooledReader0, Channel channel)
	{
		int itemSlotIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		bool locked = ((Reader)PooledReader0).ReadBoolean();
		NetworkObject lockOwner = ((Reader)PooledReader0).ReadNetworkObject();
		string lockReason = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetSlotLocked_Internal_3170825843(null, itemSlotIndex, locked, lockOwner, lockReason);
		}
	}

	private void RpcWriter___Server_SetSlotFilter_527532783(NetworkConnection conn, int itemSlotIndex, SlotFilter filter)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteNetworkConnection(conn);
			((Writer)writer).WriteInt32(itemSlotIndex, (AutoPackType)1);
			GeneratedWriters___Internal.Write___ScheduleOne_002EItemFramework_002ESlotFilterFishNet_002ESerializing_002EGenerated((Writer)(object)writer, filter);
			((NetworkBehaviour)this).SendServerRpc(8u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetSlotFilter_527532783(NetworkConnection conn, int itemSlotIndex, SlotFilter filter)
	{
		if (conn == (NetworkConnection)null || conn.ClientId == -1)
		{
			SetSlotFilter_Internal(null, itemSlotIndex, filter);
		}
		else
		{
			SetSlotFilter_Internal(conn, itemSlotIndex, filter);
		}
	}

	private void RpcReader___Server_SetSlotFilter_527532783(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkConnection conn2 = ((Reader)PooledReader0).ReadNetworkConnection();
		int itemSlotIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		SlotFilter filter = GeneratedReaders___Internal.Read___ScheduleOne_002EItemFramework_002ESlotFilterFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetSlotFilter_527532783(conn2, itemSlotIndex, filter);
		}
	}

	private void RpcWriter___Observers_SetSlotFilter_Internal_527532783(NetworkConnection conn, int itemSlotIndex, SlotFilter filter)
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
			((Writer)writer).WriteInt32(itemSlotIndex, (AutoPackType)1);
			GeneratedWriters___Internal.Write___ScheduleOne_002EItemFramework_002ESlotFilterFishNet_002ESerializing_002EGenerated((Writer)(object)writer, filter);
			((NetworkBehaviour)this).SendObserversRpc(9u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetSlotFilter_Internal_527532783(NetworkConnection conn, int itemSlotIndex, SlotFilter filter)
	{
		ItemSlots[itemSlotIndex].SetPlayerFilter(filter, _internal: true);
	}

	private void RpcReader___Observers_SetSlotFilter_Internal_527532783(PooledReader PooledReader0, Channel channel)
	{
		int itemSlotIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		SlotFilter filter = GeneratedReaders___Internal.Read___ScheduleOne_002EItemFramework_002ESlotFilterFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetSlotFilter_Internal_527532783(null, itemSlotIndex, filter);
		}
	}

	private void RpcWriter___Target_SetSlotFilter_Internal_527532783(NetworkConnection conn, int itemSlotIndex, SlotFilter filter)
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
			((Writer)writer).WriteInt32(itemSlotIndex, (AutoPackType)1);
			GeneratedWriters___Internal.Write___ScheduleOne_002EItemFramework_002ESlotFilterFishNet_002ESerializing_002EGenerated((Writer)(object)writer, filter);
			((NetworkBehaviour)this).SendTargetRpc(10u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetSlotFilter_Internal_527532783(PooledReader PooledReader0, Channel channel)
	{
		int itemSlotIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		SlotFilter filter = GeneratedReaders___Internal.Read___ScheduleOne_002EItemFramework_002ESlotFilterFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetSlotFilter_Internal_527532783(((NetworkBehaviour)this).LocalConnection, itemSlotIndex, filter);
		}
	}

	protected virtual void Awake_UserLogic_ScheduleOne_002EPlayerScripts_002EPlayerClothing_Assembly_002DCSharp_002Edll()
	{
		foreach (EClothingSlot value in Enum.GetValues(typeof(EClothingSlot)))
		{
			ItemSlot itemSlot = new ItemSlot();
			itemSlot.SetSlotOwner(this);
			itemSlot.AddFilter(new ItemFilter_ClothingSlot(value));
			itemSlot.onItemDataChanged = (Action)Delegate.Combine(itemSlot.onItemDataChanged, new Action(ClothingChanged));
			ClothingSlots.Add(value, itemSlot);
		}
	}
}
