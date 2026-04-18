using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using UnityEngine;

namespace ScheduleOne.UI.Shop;

public class ShopManager : NetworkSingleton<ShopManager>, IBaseSaveable, ISaveable
{
	private ShopManagerLoader loader = new ShopManagerLoader();

	private bool NetworkInitialize___EarlyScheduleOne_002EUI_002EShop_002EShopManagerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EUI_002EShop_002EShopManagerAssembly_002DCSharp_002Edll_Excuted;

	public string SaveFolderName => "Shops";

	public string SaveFileName => "Shops";

	public Loader Loader => loader;

	public bool ShouldSaveUnderFolder => false;

	public List<string> LocalExtraFiles { get; set; } = new List<string>();

	public List<string> LocalExtraFolders { get; set; } = new List<string>();

	public bool HasChanged { get; set; } = true;

	public int LoadOrder { get; }

	protected override void Start()
	{
		base.Start();
		InitializeSaveable();
	}

	public virtual void InitializeSaveable()
	{
		Singleton<SaveManager>.Instance.RegisterSaveable(this);
	}

	public virtual string GetSaveString()
	{
		List<ShopData> list = new List<ShopData>();
		for (int i = 0; i < ShopInterface.AllShops.Count; i++)
		{
			if (!((Object)(object)ShopInterface.AllShops[i] == (Object)null) && ShopInterface.AllShops[i].ShouldSave())
			{
				list.Add(ShopInterface.AllShops[i].GetSaveData());
			}
		}
		return new ShopManagerData(list.ToArray()).GetJson();
	}

	[ServerRpc(RequireOwnership = false)]
	public void SendStock(string shopCode, string itemID, int stock)
	{
		RpcWriter___Server_SendStock_15643032(shopCode, itemID, stock);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void SetStock(NetworkConnection conn, string shopCode, string itemID, int stock)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetStock_3509965635(conn, shopCode, itemID, stock);
			RpcLogic___SetStock_3509965635(conn, shopCode, itemID, stock);
		}
		else
		{
			RpcWriter___Target_SetStock_3509965635(conn, shopCode, itemID, stock);
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
		if (!NetworkInitialize___EarlyScheduleOne_002EUI_002EShop_002EShopManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EUI_002EShop_002EShopManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_SendStock_15643032));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_SetStock_3509965635));
			((NetworkBehaviour)this).RegisterTargetRpc(2u, new ClientRpcDelegate(RpcReader___Target_SetStock_3509965635));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EUI_002EShop_002EShopManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EUI_002EShop_002EShopManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_SendStock_15643032(string shopCode, string itemID, int stock)
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
			((Writer)writer).WriteString(shopCode);
			((Writer)writer).WriteString(itemID);
			((Writer)writer).WriteInt32(stock, (AutoPackType)1);
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendStock_15643032(string shopCode, string itemID, int stock)
	{
		SetStock(null, shopCode, itemID, stock);
	}

	private void RpcReader___Server_SendStock_15643032(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string shopCode = ((Reader)PooledReader0).ReadString();
		string itemID = ((Reader)PooledReader0).ReadString();
		int stock = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SendStock_15643032(shopCode, itemID, stock);
		}
	}

	private void RpcWriter___Observers_SetStock_3509965635(NetworkConnection conn, string shopCode, string itemID, int stock)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(shopCode);
			((Writer)writer).WriteString(itemID);
			((Writer)writer).WriteInt32(stock, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(1u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___SetStock_3509965635(NetworkConnection conn, string shopCode, string itemID, int stock)
	{
		ShopInterface shopInterface = ShopInterface.AllShops.Find((ShopInterface x) => x.ShopCode == shopCode);
		if ((Object)(object)shopInterface == (Object)null)
		{
			Debug.LogError((object)("Failed to set stock: Shop not found: " + shopCode));
			return;
		}
		ShopListing listing = shopInterface.GetListing(itemID);
		if (listing == null)
		{
			Debug.LogError((object)("Failed to set stock: Listing not found: " + itemID));
		}
		else
		{
			listing.SetStock(stock, network: false);
		}
	}

	private void RpcReader___Observers_SetStock_3509965635(PooledReader PooledReader0, Channel channel)
	{
		string shopCode = ((Reader)PooledReader0).ReadString();
		string itemID = ((Reader)PooledReader0).ReadString();
		int stock = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetStock_3509965635(null, shopCode, itemID, stock);
		}
	}

	private void RpcWriter___Target_SetStock_3509965635(NetworkConnection conn, string shopCode, string itemID, int stock)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(shopCode);
			((Writer)writer).WriteString(itemID);
			((Writer)writer).WriteInt32(stock, (AutoPackType)1);
			((NetworkBehaviour)this).SendTargetRpc(2u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetStock_3509965635(PooledReader PooledReader0, Channel channel)
	{
		string shopCode = ((Reader)PooledReader0).ReadString();
		string itemID = ((Reader)PooledReader0).ReadString();
		int stock = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetStock_3509965635(((NetworkBehaviour)this).LocalConnection, shopCode, itemID, stock);
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
