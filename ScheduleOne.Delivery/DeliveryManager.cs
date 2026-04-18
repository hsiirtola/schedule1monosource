using System;
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
using ScheduleOne.Configuration;
using ScheduleOne.Core.Settings.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Networking;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using ScheduleOne.Property;
using ScheduleOne.UI;
using ScheduleOne.UI.Phone.Delivery;
using ScheduleOne.UI.Shop;
using UnityEngine;

namespace ScheduleOne.Delivery;

public class DeliveryManager : NetworkSingleton<DeliveryManager>, IBaseSaveable, ISaveable
{
	public List<DeliveryInstance> Deliveries = new List<DeliveryInstance>();

	private DeliveriesLoader loader = new DeliveriesLoader();

	private List<string> writtenVehicles = new List<string>();

	[SyncObject]
	private readonly SyncList<DeliveryReceipt> _deliveryHistory = new SyncList<DeliveryReceipt>();

	[SyncObject]
	private readonly SyncList<DeliveryReceipt> _displayedDeliveryHistory = new SyncList<DeliveryReceipt>();

	private Dictionary<DeliveryInstance, int> _minsSinceVehicleEmpty = new Dictionary<DeliveryInstance, int>();

	private bool NetworkInitialize___EarlyScheduleOne_002EDelivery_002EDeliveryManagerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EDelivery_002EDeliveryManagerAssembly_002DCSharp_002Edll_Excuted;

	public string SaveFolderName => "Deliveries";

	public string SaveFileName => "Deliveries";

	public Loader Loader => loader;

	public bool ShouldSaveUnderFolder => false;

	public List<string> LocalExtraFiles { get; set; } = new List<string>();

	public List<string> LocalExtraFolders { get; set; } = new List<string>();

	public bool HasChanged { get; set; }

	public int LoadOrder { get; }

	public List<DeliveryReceipt> DisplayedDeliveryHistory => ((IEnumerable<DeliveryReceipt>)_displayedDeliveryHistory).ToList();

	public event Action<DeliveryInstance> onDeliveryCreated;

	public event Action<DeliveryInstance> onDeliveryCompleted;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EDelivery_002EDeliveryManager_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	protected override void Start()
	{
		base.Start();
		NetworkSingleton<TimeManager>.Instance.onMinutePass += (Action)delegate
		{
			OnTimePass(1);
		};
		TimeManager timeManager = NetworkSingleton<TimeManager>.Instance;
		timeManager.onTimeSkip = (Action<int>)Delegate.Combine(timeManager.onTimeSkip, new Action<int>(OnTimePass));
	}

	public virtual void InitializeSaveable()
	{
		Singleton<SaveManager>.Instance.RegisterSaveable(this);
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		((NetworkBehaviour)this).OnSpawnServer(connection);
		DeliveryInstance[] deliveryCache;
		if (!connection.IsHost)
		{
			deliveryCache = Deliveries.ToArray();
			ReplicationQueue.Enqueue(((object)this).GetType().Name, connection, SendAllDeliveries, 200 * deliveryCache.Length);
		}
		void SendAllDeliveries(NetworkConnection conn)
		{
			DeliveryInstance[] array = deliveryCache;
			foreach (DeliveryInstance deliveryInstance in array)
			{
				if (Deliveries.Contains(deliveryInstance))
				{
					ReceiveDelivery(conn, deliveryInstance);
				}
			}
		}
	}

	private void OnTimePass(int minutes)
	{
		if (Singleton<LoadManager>.Instance.IsLoading)
		{
			return;
		}
		DeliveryInstance[] array = Deliveries.ToArray();
		foreach (DeliveryInstance deliveryInstance in array)
		{
			deliveryInstance.OnTimePass(minutes);
			if (!InstanceFinder.IsServer)
			{
				continue;
			}
			if (deliveryInstance.TimeUntilArrival == 0 && deliveryInstance.Status != EDeliveryStatus.Arrived)
			{
				if (IsLoadingBayFree(deliveryInstance.Destination, deliveryInstance.LoadingDockIndex))
				{
					deliveryInstance.AddItemsToDeliveryVehicle();
					SetDeliveryState(deliveryInstance.DeliveryID, EDeliveryStatus.Arrived);
				}
				else if (deliveryInstance.Status != EDeliveryStatus.Waiting)
				{
					SetDeliveryState(deliveryInstance.DeliveryID, EDeliveryStatus.Waiting);
				}
			}
			if (deliveryInstance.Status != EDeliveryStatus.Arrived)
			{
				continue;
			}
			if (!_minsSinceVehicleEmpty.ContainsKey(deliveryInstance))
			{
				_minsSinceVehicleEmpty.Add(deliveryInstance, 0);
			}
			if (deliveryInstance.ActiveVehicle.Vehicle.Storage.ItemCount == 0 && (Object)(object)deliveryInstance.ActiveVehicle.Vehicle.Storage.CurrentPlayerAccessor == (Object)null)
			{
				_minsSinceVehicleEmpty[deliveryInstance]++;
				if (_minsSinceVehicleEmpty[deliveryInstance] >= 3)
				{
					SetDeliveryState(deliveryInstance.DeliveryID, EDeliveryStatus.Completed);
				}
			}
			else
			{
				_minsSinceVehicleEmpty[deliveryInstance] = 0;
			}
		}
	}

	public bool IsLoadingBayFree(ScheduleOne.Property.Property destination, int loadingDockIndex)
	{
		return !destination.LoadingDocks[loadingDockIndex].IsInUse;
	}

	[ServerRpc(RequireOwnership = false)]
	public void SendDelivery(DeliveryInstance delivery)
	{
		RpcWriter___Server_SendDelivery_2813439055(delivery);
	}

	[ServerRpc(RequireOwnership = false)]
	public void RecordDeliveryReceipt_Server(DeliveryReceipt receipt, string originalOrderID = "")
	{
		RpcWriter___Server_RecordDeliveryReceipt_Server_2582461062(receipt, originalOrderID);
	}

	[ObserversRpc]
	[TargetRpc]
	private void ReceiveDelivery(NetworkConnection conn, DeliveryInstance delivery)
	{
		if (conn == null)
		{
			RpcWriter___Observers_ReceiveDelivery_2795369214(conn, delivery);
		}
		else
		{
			RpcWriter___Target_ReceiveDelivery_2795369214(conn, delivery);
		}
	}

	[ObserversRpc(RunLocally = true)]
	private void SetDeliveryState(string deliveryID, EDeliveryStatus status)
	{
		RpcWriter___Observers_SetDeliveryState_316609003(deliveryID, status);
		RpcLogic___SetDeliveryState_316609003(deliveryID, status);
	}

	private DeliveryInstance GetDelivery(string deliveryID)
	{
		return Deliveries.FirstOrDefault((DeliveryInstance d) => d.DeliveryID == deliveryID);
	}

	public DeliveryInstance GetDelivery(ScheduleOne.Property.Property destination)
	{
		return Deliveries.FirstOrDefault((DeliveryInstance d) => d.DestinationCode == destination.PropertyCode);
	}

	public DeliveryInstance GetActiveShopDelivery(DeliveryShop shop)
	{
		return Deliveries.FirstOrDefault((DeliveryInstance d) => d.StoreName == shop.MatchingShopInterfaceName);
	}

	public ShopInterface GetShopInterface(string shopName)
	{
		return ShopInterface.AllShops.Find((ShopInterface x) => x.ShopName == shopName);
	}

	public virtual string GetSaveString()
	{
		List<VehicleData> list = new List<VehicleData>();
		foreach (DeliveryInstance delivery in Deliveries)
		{
			if (!((Object)(object)delivery.ActiveVehicle == (Object)null))
			{
				list.Add(delivery.ActiveVehicle.Vehicle.GetVehicleData());
			}
		}
		return new DeliveriesData(Deliveries.ToArray(), list.ToArray(), ((IEnumerable<DeliveryReceipt>)_deliveryHistory).ToArray(), ((IEnumerable<DeliveryReceipt>)_displayedDeliveryHistory).ToArray()).GetJson();
	}

	public void Load(DeliveriesData data)
	{
		if (data == null)
		{
			return;
		}
		if (data.ActiveDeliveries != null)
		{
			DeliveryInstance[] activeDeliveries = data.ActiveDeliveries;
			foreach (DeliveryInstance delivery in activeDeliveries)
			{
				SendDelivery(delivery);
			}
		}
		if (data.DeliveryHistory != null)
		{
			_deliveryHistory.AddRange((IEnumerable<DeliveryReceipt>)data.DeliveryHistory);
		}
		if (data.DisplayedDeliveryHistory != null)
		{
			_displayedDeliveryHistory.AddRange((IEnumerable<DeliveryReceipt>)data.DisplayedDeliveryHistory);
		}
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Expected O, but got Unknown
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EDelivery_002EDeliveryManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EDelivery_002EDeliveryManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((SyncBase)_displayedDeliveryHistory).InitializeInstance((NetworkBehaviour)(object)this, 1u, (WritePermission)0, (ReadPermission)0, -1f, (Channel)0, true);
			((SyncBase)_deliveryHistory).InitializeInstance((NetworkBehaviour)(object)this, 0u, (WritePermission)0, (ReadPermission)0, -1f, (Channel)0, true);
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_SendDelivery_2813439055));
			((NetworkBehaviour)this).RegisterServerRpc(1u, new ServerRpcDelegate(RpcReader___Server_RecordDeliveryReceipt_Server_2582461062));
			((NetworkBehaviour)this).RegisterObserversRpc(2u, new ClientRpcDelegate(RpcReader___Observers_ReceiveDelivery_2795369214));
			((NetworkBehaviour)this).RegisterTargetRpc(3u, new ClientRpcDelegate(RpcReader___Target_ReceiveDelivery_2795369214));
			((NetworkBehaviour)this).RegisterObserversRpc(4u, new ClientRpcDelegate(RpcReader___Observers_SetDeliveryState_316609003));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EDelivery_002EDeliveryManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EDelivery_002EDeliveryManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
			((SyncBase)_deliveryHistory).SetRegistered();
			((SyncBase)_displayedDeliveryHistory).SetRegistered();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_SendDelivery_2813439055(DeliveryInstance delivery)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EDelivery_002EDeliveryInstanceFishNet_002ESerializing_002EGenerated((Writer)(object)writer, delivery);
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendDelivery_2813439055(DeliveryInstance delivery)
	{
		ReceiveDelivery(null, delivery);
	}

	private void RpcReader___Server_SendDelivery_2813439055(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		DeliveryInstance delivery = GeneratedReaders___Internal.Read___ScheduleOne_002EDelivery_002EDeliveryInstanceFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SendDelivery_2813439055(delivery);
		}
	}

	private void RpcWriter___Server_RecordDeliveryReceipt_Server_2582461062(DeliveryReceipt receipt, string originalOrderID = "")
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EDelivery_002EDeliveryReceiptFishNet_002ESerializing_002EGenerated((Writer)(object)writer, receipt);
			((Writer)writer).WriteString(originalOrderID);
			((NetworkBehaviour)this).SendServerRpc(1u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___RecordDeliveryReceipt_Server_2582461062(DeliveryReceipt receipt, string originalOrderID = "")
	{
		if (receipt == null)
		{
			Debug.LogError((object)"Received null delivery receipt");
			return;
		}
		_deliveryHistory.Add(receipt);
		if (!string.IsNullOrEmpty(originalOrderID))
		{
			for (int i = 0; i < _displayedDeliveryHistory.Count; i++)
			{
				if (_displayedDeliveryHistory[i].DeliveryID == originalOrderID)
				{
					_displayedDeliveryHistory.Add(DisplayedDeliveryHistory[i]);
					_displayedDeliveryHistory.RemoveAt(i);
					Debug.Log((object)("Moved original delivery receipt with ID " + originalOrderID + " to end of displayed delivery history due to reorder"));
					break;
				}
			}
		}
		else
		{
			_displayedDeliveryHistory.Add(receipt);
		}
		Singleton<ConfigurationService>.Instance.TryGetConfiguration<DeliveryConfiguration>(out var configuration);
		while (_deliveryHistory.Count > ((SettingsField<int>)(object)configuration.Settings.OrderHistoryLength).Value)
		{
			_deliveryHistory.RemoveAt(0);
		}
		while (_displayedDeliveryHistory.Count > ((SettingsField<int>)(object)configuration.Settings.OrderHistoryLength).Value)
		{
			_displayedDeliveryHistory.RemoveAt(0);
		}
		Debug.Log((object)("Recorded delivery receipt for " + receipt.StoreName + " to " + receipt.DestinationCode));
		Debug.Log((object)("Current delivery history count: " + _deliveryHistory.Count));
	}

	private void RpcReader___Server_RecordDeliveryReceipt_Server_2582461062(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		DeliveryReceipt receipt = GeneratedReaders___Internal.Read___ScheduleOne_002EDelivery_002EDeliveryReceiptFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		string originalOrderID = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___RecordDeliveryReceipt_Server_2582461062(receipt, originalOrderID);
		}
	}

	private void RpcWriter___Observers_ReceiveDelivery_2795369214(NetworkConnection conn, DeliveryInstance delivery)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EDelivery_002EDeliveryInstanceFishNet_002ESerializing_002EGenerated((Writer)(object)writer, delivery);
			((NetworkBehaviour)this).SendObserversRpc(2u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___ReceiveDelivery_2795369214(NetworkConnection conn, DeliveryInstance delivery)
	{
		if (GetDelivery(delivery.DeliveryID) == null)
		{
			Deliveries.Add(delivery);
			delivery.SetStatus(delivery.Status);
			if (this.onDeliveryCreated != null)
			{
				this.onDeliveryCreated(delivery);
			}
			HasChanged = true;
		}
	}

	private void RpcReader___Observers_ReceiveDelivery_2795369214(PooledReader PooledReader0, Channel channel)
	{
		DeliveryInstance delivery = GeneratedReaders___Internal.Read___ScheduleOne_002EDelivery_002EDeliveryInstanceFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ReceiveDelivery_2795369214(null, delivery);
		}
	}

	private void RpcWriter___Target_ReceiveDelivery_2795369214(NetworkConnection conn, DeliveryInstance delivery)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EDelivery_002EDeliveryInstanceFishNet_002ESerializing_002EGenerated((Writer)(object)writer, delivery);
			((NetworkBehaviour)this).SendTargetRpc(3u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_ReceiveDelivery_2795369214(PooledReader PooledReader0, Channel channel)
	{
		DeliveryInstance delivery = GeneratedReaders___Internal.Read___ScheduleOne_002EDelivery_002EDeliveryInstanceFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ReceiveDelivery_2795369214(((NetworkBehaviour)this).LocalConnection, delivery);
		}
	}

	private void RpcWriter___Observers_SetDeliveryState_316609003(string deliveryID, EDeliveryStatus status)
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
			((Writer)writer).WriteString(deliveryID);
			GeneratedWriters___Internal.Write___ScheduleOne_002EDelivery_002EEDeliveryStatusFishNet_002ESerializing_002EGenerated((Writer)(object)writer, status);
			((NetworkBehaviour)this).SendObserversRpc(4u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetDeliveryState_316609003(string deliveryID, EDeliveryStatus status)
	{
		DeliveryInstance delivery = GetDelivery(deliveryID);
		if (delivery != null)
		{
			delivery.SetStatus(status);
			if (status == EDeliveryStatus.Arrived)
			{
				Singleton<NotificationsManager>.Instance.SendNotification("Delivery Arrived", Singleton<PropertyManager>.Instance.GetProperty(delivery.DestinationCode).PropertyName, PlayerSingleton<DeliveryApp>.Instance.AppIcon);
			}
		}
		if (status == EDeliveryStatus.Completed)
		{
			if (this.onDeliveryCompleted != null)
			{
				this.onDeliveryCompleted(delivery);
			}
			Deliveries.Remove(delivery);
		}
		HasChanged = true;
	}

	private void RpcReader___Observers_SetDeliveryState_316609003(PooledReader PooledReader0, Channel channel)
	{
		string deliveryID = ((Reader)PooledReader0).ReadString();
		EDeliveryStatus status = GeneratedReaders___Internal.Read___ScheduleOne_002EDelivery_002EEDeliveryStatusFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetDeliveryState_316609003(deliveryID, status);
		}
	}

	protected override void Awake_UserLogic_ScheduleOne_002EDelivery_002EDeliveryManager_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		InitializeSaveable();
	}
}
