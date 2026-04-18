using System;
using System.Collections;
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
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Property;
using ScheduleOne.Tiles;
using UnityEngine;

namespace ScheduleOne.EntityFramework;

public class ProceduralGridItem : BuildableItem
{
	public class FootprintTileMatch
	{
		public FootprintTile footprint;

		public ProceduralTile matchedTile;
	}

	[Header("Grid item data")]
	public List<CoordinateFootprintTilePair> CoordinateFootprintTilePairs = new List<CoordinateFootprintTilePair>();

	public ProceduralTile.EProceduralTileType ProceduralTileType;

	[SyncVar]
	[HideInInspector]
	public int Rotation;

	[SyncVar]
	[HideInInspector]
	public List<CoordinateProceduralTilePair> footprintTileMatches = new List<CoordinateProceduralTilePair>();

	public SyncVar<int> syncVar___Rotation;

	public SyncVar<List<CoordinateProceduralTilePair>> syncVar___footprintTileMatches;

	private bool NetworkInitialize___EarlyScheduleOne_002EEntityFramework_002EProceduralGridItemAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EEntityFramework_002EProceduralGridItemAssembly_002DCSharp_002Edll_Excuted;

	public int FootprintXSize => CoordinateFootprintTilePairs.OrderByDescending((CoordinateFootprintTilePair c) => c.coord.x).FirstOrDefault().coord.x + 1;

	public int FootprintYSize => CoordinateFootprintTilePairs.OrderByDescending((CoordinateFootprintTilePair c) => c.coord.y).FirstOrDefault().coord.y + 1;

	public int SyncAccessor_Rotation
	{
		get
		{
			return Rotation;
		}
		set
		{
			if (value || !((NetworkBehaviour)this).IsServerInitialized)
			{
				Rotation = value;
			}
			if (Application.isPlaying)
			{
				syncVar___Rotation.SetValue(value, value);
			}
		}
	}

	public List<CoordinateProceduralTilePair> SyncAccessor_footprintTileMatches
	{
		get
		{
			return footprintTileMatches;
		}
		set
		{
			if (value || !((NetworkBehaviour)this).IsServerInitialized)
			{
				footprintTileMatches = value;
			}
			if (Application.isPlaying)
			{
				syncVar___footprintTileMatches.SetValue(value, value);
			}
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EEntityFramework_002EProceduralGridItem_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	protected override void SendInitializationToServer()
	{
		InitializeProceduralGridItem_Server(base.ItemInstance, SyncAccessor_Rotation, SyncAccessor_footprintTileMatches, base.GUID.ToString());
	}

	protected override void SendInitializationToClient(NetworkConnection conn)
	{
		InitializeProceduralGridItem_Client(conn, base.ItemInstance, SyncAccessor_Rotation, SyncAccessor_footprintTileMatches, base.GUID.ToString());
	}

	[ServerRpc(RequireOwnership = false)]
	public void InitializeProceduralGridItem_Server(ItemInstance instance, int _rotation, List<CoordinateProceduralTilePair> _footprintTileMatches, string GUID)
	{
		RpcWriter___Server_InitializeProceduralGridItem_Server_638911643(instance, _rotation, _footprintTileMatches, GUID);
	}

	[TargetRpc]
	[ObserversRpc(RunLocally = true)]
	public virtual void InitializeProceduralGridItem_Client(NetworkConnection conn, ItemInstance instance, int _rotation, List<CoordinateProceduralTilePair> _footprintTileMatches, string GUID)
	{
		if (conn == null)
		{
			RpcWriter___Observers_InitializeProceduralGridItem_Client_3164718044(conn, instance, _rotation, _footprintTileMatches, GUID);
			RpcLogic___InitializeProceduralGridItem_Client_3164718044(conn, instance, _rotation, _footprintTileMatches, GUID);
		}
		else
		{
			RpcWriter___Target_InitializeProceduralGridItem_Client_3164718044(conn, instance, _rotation, _footprintTileMatches, GUID);
		}
	}

	public virtual void InitializeProceduralGridItem(ItemInstance instance, int _rotation, List<CoordinateProceduralTilePair> _footprintTileMatches, string GUID)
	{
		if (base.Initialized)
		{
			return;
		}
		if (_footprintTileMatches.Count == 0)
		{
			Console.LogError(((Object)((Component)this).gameObject).name + " initialized with zero footprint tile matches!");
			return;
		}
		SetProceduralGridData(_rotation, _footprintTileMatches);
		NetworkObject tileParent = _footprintTileMatches[0].tileParent;
		if ((Object)(object)tileParent == (Object)null)
		{
			Console.LogError("Base object is null for " + ((Object)((Component)this).gameObject).name);
			return;
		}
		ScheduleOne.Property.Property property = GetProperty(((Component)tileParent).transform);
		if ((Object)(object)property == (Object)null)
		{
			Console.LogError("Failed to find property from base " + ((Object)((Component)tileParent).gameObject).name);
		}
		else
		{
			InitializeBuildableItem(instance, GUID, property.PropertyCode);
		}
	}

	protected virtual void SetProceduralGridData(int _rotation, List<CoordinateProceduralTilePair> _footprintTileMatches)
	{
		this.sync___set_value_Rotation(_rotation, true);
		this.sync___set_value_footprintTileMatches(_footprintTileMatches, true);
		for (int i = 0; i < SyncAccessor_footprintTileMatches.Count; i++)
		{
			_footprintTileMatches[i].tile.AddOccupant(GetFootprintTile(SyncAccessor_footprintTileMatches[i].coord), this);
		}
		if (((NetworkBehaviour)this).NetworkObject.IsSpawned)
		{
			((Component)this).transform.SetParent(((Component)SyncAccessor_footprintTileMatches[0].tile.ParentBuildableItem).transform.parent);
			RefreshTransform();
		}
		else
		{
			((MonoBehaviour)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			yield return (object)new WaitUntil((Func<bool>)(() => ((NetworkBehaviour)this).NetworkObject.IsSpawned));
			((Component)this).transform.SetParent(((Component)SyncAccessor_footprintTileMatches[0].tile.ParentBuildableItem).transform.parent);
			RefreshTransform();
		}
	}

	private void RefreshTransform()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		ProceduralTile tile = SyncAccessor_footprintTileMatches[0].tile;
		((Component)this).transform.forward = ((Component)tile).transform.forward;
		((Component)this).transform.Rotate(((Component)tile).transform.up, (float)SyncAccessor_Rotation);
		((Component)this).transform.position = ((Component)tile).transform.position - (((Component)GetFootprintTile(SyncAccessor_footprintTileMatches[0].coord)).transform.position - ((Component)this).transform.position);
	}

	private void ClearPositionData()
	{
		for (int i = 0; i < SyncAccessor_footprintTileMatches.Count; i++)
		{
			SyncAccessor_footprintTileMatches[i].tile.RemoveOccupant(GetFootprintTile(SyncAccessor_footprintTileMatches[i].coord), this);
		}
	}

	protected override void Destroy()
	{
		ClearPositionData();
		base.Destroy();
	}

	protected override ScheduleOne.Property.Property GetProperty(Transform searchTransform = null)
	{
		if ((Object)(object)searchTransform != (Object)null && (Object)(object)((Component)searchTransform).GetComponent<GridItem>() != (Object)null)
		{
			return ((Component)searchTransform).GetComponent<GridItem>().ParentProperty;
		}
		return base.GetProperty(searchTransform);
	}

	public virtual void CalculateFootprintTileIntersections()
	{
		for (int i = 0; i < CoordinateFootprintTilePairs.Count; i++)
		{
			CoordinateFootprintTilePairs[i].footprintTile.tileDetector.CheckIntersections();
		}
	}

	public void SetFootprintTileVisiblity(bool visible)
	{
		for (int i = 0; i < CoordinateFootprintTilePairs.Count; i++)
		{
			CoordinateFootprintTilePairs[i].footprintTile.tileAppearance.SetVisible(visible);
		}
	}

	public FootprintTile GetFootprintTile(Coordinate coord)
	{
		for (int i = 0; i < CoordinateFootprintTilePairs.Count; i++)
		{
			if (CoordinateFootprintTilePairs[i].coord.Equals(coord))
			{
				return CoordinateFootprintTilePairs[i].footprintTile;
			}
		}
		return null;
	}

	public override BuildableItemData GetBaseData()
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		FootprintMatchData[] array = new FootprintMatchData[SyncAccessor_footprintTileMatches.Count];
		Vector2 footprintCoordinate = default(Vector2);
		for (int i = 0; i < SyncAccessor_footprintTileMatches.Count; i++)
		{
			string tileOwnerGUID = ((IGUIDRegisterable)((Component)SyncAccessor_footprintTileMatches[i].tileParent).GetComponent<BuildableItem>()).GUID.ToString();
			int tileIndex = SyncAccessor_footprintTileMatches[i].tileIndex;
			((Vector2)(ref footprintCoordinate))._002Ector((float)SyncAccessor_footprintTileMatches[i].coord.x, (float)SyncAccessor_footprintTileMatches[i].coord.y);
			array[i] = new FootprintMatchData(tileOwnerGUID, tileIndex, footprintCoordinate);
		}
		return new ProceduralGridItemData(base.GUID, base.ItemInstance, 50, SyncAccessor_Rotation, array);
	}

	public override void NetworkInitialize___Early()
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected O, but got Unknown
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected O, but got Unknown
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EEntityFramework_002EProceduralGridItemAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EEntityFramework_002EProceduralGridItemAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			syncVar___footprintTileMatches = new SyncVar<List<CoordinateProceduralTilePair>>((NetworkBehaviour)(object)this, 1u, (WritePermission)0, (ReadPermission)0, -1f, (Channel)0, footprintTileMatches);
			syncVar___Rotation = new SyncVar<int>((NetworkBehaviour)(object)this, 0u, (WritePermission)0, (ReadPermission)0, -1f, (Channel)0, Rotation);
			((NetworkBehaviour)this).RegisterServerRpc(2u, new ServerRpcDelegate(RpcReader___Server_InitializeProceduralGridItem_Server_638911643));
			((NetworkBehaviour)this).RegisterTargetRpc(3u, new ClientRpcDelegate(RpcReader___Target_InitializeProceduralGridItem_Client_3164718044));
			((NetworkBehaviour)this).RegisterObserversRpc(4u, new ClientRpcDelegate(RpcReader___Observers_InitializeProceduralGridItem_Client_3164718044));
			((NetworkBehaviour)this).RegisterSyncVarRead(new SyncVarReadDelegate(ReadSyncVar___ScheduleOne_002EEntityFramework_002EProceduralGridItem));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EEntityFramework_002EProceduralGridItemAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EEntityFramework_002EProceduralGridItemAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
			((SyncBase)syncVar___footprintTileMatches).SetRegistered();
			((SyncBase)syncVar___Rotation).SetRegistered();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_InitializeProceduralGridItem_Server_638911643(ItemInstance instance, int _rotation, List<CoordinateProceduralTilePair> _footprintTileMatches, string GUID)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)(object)writer).WriteItemInstance(instance);
			((Writer)writer).WriteInt32(_rotation, (AutoPackType)1);
			GeneratedWriters___Internal.Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002ETiles_002ECoordinateProceduralTilePair_003EFishNet_002ESerializing_002EGenerated((Writer)(object)writer, _footprintTileMatches);
			((Writer)writer).WriteString(GUID);
			((NetworkBehaviour)this).SendServerRpc(2u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___InitializeProceduralGridItem_Server_638911643(ItemInstance instance, int _rotation, List<CoordinateProceduralTilePair> _footprintTileMatches, string GUID)
	{
		InitializeProceduralGridItem_Client(null, instance, _rotation, _footprintTileMatches, GUID);
	}

	private void RpcReader___Server_InitializeProceduralGridItem_Server_638911643(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		ItemInstance instance = ((Reader)(object)PooledReader0).ReadItemInstance();
		int rotation = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		List<CoordinateProceduralTilePair> list = GeneratedReaders___Internal.Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002ETiles_002ECoordinateProceduralTilePair_003EFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		string gUID = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___InitializeProceduralGridItem_Server_638911643(instance, rotation, list, gUID);
		}
	}

	private void RpcWriter___Target_InitializeProceduralGridItem_Client_3164718044(NetworkConnection conn, ItemInstance instance, int _rotation, List<CoordinateProceduralTilePair> _footprintTileMatches, string GUID)
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
			((Writer)(object)writer).WriteItemInstance(instance);
			((Writer)writer).WriteInt32(_rotation, (AutoPackType)1);
			GeneratedWriters___Internal.Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002ETiles_002ECoordinateProceduralTilePair_003EFishNet_002ESerializing_002EGenerated((Writer)(object)writer, _footprintTileMatches);
			((Writer)writer).WriteString(GUID);
			((NetworkBehaviour)this).SendTargetRpc(3u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	public virtual void RpcLogic___InitializeProceduralGridItem_Client_3164718044(NetworkConnection conn, ItemInstance instance, int _rotation, List<CoordinateProceduralTilePair> _footprintTileMatches, string GUID)
	{
		if (!base.Initialized)
		{
			InitializeProceduralGridItem(instance, _rotation, _footprintTileMatches, GUID);
		}
	}

	private void RpcReader___Target_InitializeProceduralGridItem_Client_3164718044(PooledReader PooledReader0, Channel channel)
	{
		ItemInstance instance = ((Reader)(object)PooledReader0).ReadItemInstance();
		int rotation = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		List<CoordinateProceduralTilePair> list = GeneratedReaders___Internal.Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002ETiles_002ECoordinateProceduralTilePair_003EFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		string gUID = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___InitializeProceduralGridItem_Client_3164718044(((NetworkBehaviour)this).LocalConnection, instance, rotation, list, gUID);
		}
	}

	private void RpcWriter___Observers_InitializeProceduralGridItem_Client_3164718044(NetworkConnection conn, ItemInstance instance, int _rotation, List<CoordinateProceduralTilePair> _footprintTileMatches, string GUID)
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
			((Writer)(object)writer).WriteItemInstance(instance);
			((Writer)writer).WriteInt32(_rotation, (AutoPackType)1);
			GeneratedWriters___Internal.Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002ETiles_002ECoordinateProceduralTilePair_003EFishNet_002ESerializing_002EGenerated((Writer)(object)writer, _footprintTileMatches);
			((Writer)writer).WriteString(GUID);
			((NetworkBehaviour)this).SendObserversRpc(4u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcReader___Observers_InitializeProceduralGridItem_Client_3164718044(PooledReader PooledReader0, Channel channel)
	{
		ItemInstance instance = ((Reader)(object)PooledReader0).ReadItemInstance();
		int rotation = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		List<CoordinateProceduralTilePair> list = GeneratedReaders___Internal.Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002ETiles_002ECoordinateProceduralTilePair_003EFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		string gUID = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___InitializeProceduralGridItem_Client_3164718044(null, instance, rotation, list, gUID);
		}
	}

	public override bool ReadSyncVar___ScheduleOne_002EEntityFramework_002EProceduralGridItem(PooledReader PooledReader0, uint UInt321, bool Boolean2)
	{
		switch (UInt321)
		{
		case 1u:
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value_footprintTileMatches(syncVar___footprintTileMatches.GetValue(true), true);
				return true;
			}
			List<CoordinateProceduralTilePair> value2 = GeneratedReaders___Internal.Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002ETiles_002ECoordinateProceduralTilePair_003EFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
			this.sync___set_value_footprintTileMatches(value2, Boolean2);
			return true;
		}
		case 0u:
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value_Rotation(syncVar___Rotation.GetValue(true), true);
				return true;
			}
			int value = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
			this.sync___set_value_Rotation(value, Boolean2);
			return true;
		}
		default:
			return false;
		}
	}

	protected override void Awake_UserLogic_ScheduleOne_002EEntityFramework_002EProceduralGridItem_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		SetFootprintTileVisiblity(visible: false);
	}
}
