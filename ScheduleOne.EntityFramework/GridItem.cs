using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Tiles;
using UnityEngine;

namespace ScheduleOne.EntityFramework;

public class GridItem : BuildableItem
{
	[HideInInspector]
	public List<CoordinateFootprintTilePair> CoordinateFootprintTilePairs = new List<CoordinateFootprintTilePair>();

	protected Guid _ownerGridGUID;

	protected Vector2 _originCoordinate;

	protected int _rotation;

	public List<CoordinatePair> CoordinatePairs = new List<CoordinatePair>();

	private bool NetworkInitialize___EarlyScheduleOne_002EEntityFramework_002EGridItemAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EEntityFramework_002EGridItemAssembly_002DCSharp_002Edll_Excuted;

	public FootprintTile OriginFootprint => CoordinateFootprintTilePairs[0].footprintTile;

	public int FootprintX => CoordinateFootprintTilePairs.OrderByDescending((CoordinateFootprintTilePair c) => c.coord.x).FirstOrDefault().coord.x + 1;

	public int FootprintY => CoordinateFootprintTilePairs.OrderByDescending((CoordinateFootprintTilePair c) => c.coord.y).FirstOrDefault().coord.y + 1;

	public Grid OwnerGrid { get; protected set; }

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EEntityFramework_002EGridItem_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	protected override void SendInitializationToServer()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		InitializeGridItem_Server(base.ItemInstance, _ownerGridGUID.ToString(), _originCoordinate, _rotation, base.GUID.ToString());
	}

	protected override void SendInitializationToClient(NetworkConnection conn)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		InitializeGridItem_Client(conn, base.ItemInstance, _ownerGridGUID.ToString(), _originCoordinate, _rotation, base.GUID.ToString());
	}

	[ServerRpc(RequireOwnership = false)]
	public void InitializeGridItem_Server(ItemInstance instance, string gridGUID, Vector2 originCoordinate, int rotation, string GUID)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		RpcWriter___Server_InitializeGridItem_Server_2821640832(instance, gridGUID, originCoordinate, rotation, GUID);
	}

	[TargetRpc]
	[ObserversRpc(RunLocally = true)]
	private void InitializeGridItem_Client(NetworkConnection conn, ItemInstance instance, string gridGUID, Vector2 originCoordinate, int rotation, string GUID)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (conn == null)
		{
			RpcWriter___Observers_InitializeGridItem_Client_1883577149(conn, instance, gridGUID, originCoordinate, rotation, GUID);
			RpcLogic___InitializeGridItem_Client_1883577149(conn, instance, gridGUID, originCoordinate, rotation, GUID);
		}
		else
		{
			RpcWriter___Target_InitializeGridItem_Client_1883577149(conn, instance, gridGUID, originCoordinate, rotation, GUID);
		}
	}

	public virtual void InitializeGridItem(ItemInstance instance, Grid grid, Vector2 originCoordinate, int rotation, string GUID)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (!base.Initialized)
		{
			InitializeBuildableItem(instance, GUID, GetProperty(((Component)grid).transform).PropertyCode);
			SetGridData(grid.GUID, originCoordinate, rotation);
		}
	}

	protected void SetGridData(Guid gridGUID, Vector2 originCoordinate, int rotation)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		Grid grid = GUIDManager.GetObject<Grid>(gridGUID);
		if ((Object)(object)grid == (Object)null)
		{
			Console.LogError("InitializeConstructable_GridBased: grid is null");
			Destroy();
			return;
		}
		_ownerGridGUID = gridGUID;
		OwnerGrid = grid;
		_originCoordinate = originCoordinate;
		_rotation = ValidateRotation(rotation);
		ProcessGridData();
	}

	private int ValidateRotation(int rotation)
	{
		if (float.IsNaN(rotation) || float.IsInfinity(rotation))
		{
			Console.LogWarning("Invalid rotation value: " + rotation + " resetting to 0");
			return 0;
		}
		if (rotation != 0 && rotation != 90 && rotation != 180 && rotation != 270)
		{
			return Mathf.RoundToInt((float)(rotation / 90)) * 90;
		}
		return rotation;
	}

	private void ProcessGridData()
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		OwnerGrid = GUIDManager.GetObject<Grid>(_ownerGridGUID);
		if ((Object)(object)OwnerGrid == (Object)null)
		{
			Console.LogWarning("GridItem OwnerGrid is null");
			return;
		}
		base.ParentProperty = GetProperty(((Component)OwnerGrid).transform);
		if (((NetworkBehaviour)this).NetworkObject.IsSpawned)
		{
			((Component)this).transform.SetParent(OwnerGrid.Container);
		}
		else
		{
			((MonoBehaviour)this).StartCoroutine(Routine());
		}
		List<CoordinatePair> list = Coordinate.BuildCoordinateMatches(new Coordinate(_originCoordinate), FootprintX, FootprintY, _rotation);
		for (int i = 0; i < list.Count; i++)
		{
			if ((Object)(object)OwnerGrid.GetTile(list[i].coord2) == (Object)null)
			{
				Console.LogError("ReceiveData: grid does not contain tile at " + list[i].coord2);
				Destroy();
				return;
			}
		}
		ClearPositionData();
		CoordinatePairs.AddRange(list);
		RefreshTransform();
		for (int j = 0; j < CoordinatePairs.Count; j++)
		{
			Tile tile = OwnerGrid.GetTile(CoordinatePairs[j].coord2);
			tile.AddOccupant(this, GetFootprintTile(CoordinatePairs[j].coord1));
			tile.onTileTemperatureChanged = (Action<Tile, float>)Delegate.Combine(tile.onTileTemperatureChanged, new Action<Tile, float>(OnTileTemperatureChanged));
			GetFootprintTile(CoordinatePairs[j].coord1).Initialize(OwnerGrid.GetTile(CoordinatePairs[j].coord2));
		}
		IEnumerator Routine()
		{
			yield return (object)new WaitUntil((Func<bool>)(() => ((NetworkBehaviour)this).NetworkObject.IsSpawned));
			((Component)this).transform.SetParent(OwnerGrid.Container);
		}
	}

	private void RefreshTransform()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.rotation = ((Component)OwnerGrid).transform.rotation * (Quaternion.Inverse(((Component)BuildPoint).transform.rotation) * ((Component)this).transform.rotation);
		((Component)this).transform.Rotate(BuildPoint.up, (float)_rotation);
		((Component)this).transform.position = ((Component)OwnerGrid.GetTile(CoordinatePairs[0].coord2)).transform.position - (((Component)OriginFootprint).transform.position - ((Component)this).transform.position);
	}

	private void ClearPositionData()
	{
		if ((Object)(object)OwnerGrid != (Object)null)
		{
			for (int i = 0; i < CoordinatePairs.Count; i++)
			{
				Tile tile = OwnerGrid.GetTile(CoordinatePairs[i].coord2);
				tile.RemoveOccupant(this, GetFootprintTile(CoordinatePairs[i].coord1));
				tile.onTileTemperatureChanged = (Action<Tile, float>)Delegate.Remove(tile.onTileTemperatureChanged, new Action<Tile, float>(OnTileTemperatureChanged));
			}
		}
		CoordinatePairs.Clear();
	}

	protected override void Destroy()
	{
		ClearPositionData();
		base.Destroy();
	}

	protected virtual void OnTileTemperatureChanged(Tile tile, float newTemp)
	{
	}

	public float GetAverageTileTemperature()
	{
		float num = 0f;
		for (int i = 0; i < CoordinatePairs.Count; i++)
		{
			num += OwnerGrid.GetTile(CoordinatePairs[i].coord2).TileTemperature;
		}
		return num / (float)CoordinatePairs.Count;
	}

	public float GetAverageCosmeticTileTemperature()
	{
		float num = 0f;
		for (int i = 0; i < CoordinatePairs.Count; i++)
		{
			num += OwnerGrid.GetTile(CoordinatePairs[i].coord2).CosmeticTileTemperature;
		}
		return num / (float)CoordinatePairs.Count;
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

	public Tile GetParentTileAtFootprintCoordinate(Coordinate footprintCoord)
	{
		return OwnerGrid.GetTile(CoordinatePairs.Find((CoordinatePair x) => x.coord1 == footprintCoord).coord2);
	}

	public virtual bool CanShareTileWith(List<GridItem> obstacles)
	{
		for (int i = 0; i < obstacles.Count; i++)
		{
			if (!(obstacles[i] is FloorRack))
			{
				return false;
			}
		}
		return true;
	}

	public override BuildableItemData GetBaseData()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		return new GridItemData(base.GUID, base.ItemInstance, 0, OwnerGrid, _originCoordinate, _rotation);
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EEntityFramework_002EGridItemAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EEntityFramework_002EGridItemAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterServerRpc(2u, new ServerRpcDelegate(RpcReader___Server_InitializeGridItem_Server_2821640832));
			((NetworkBehaviour)this).RegisterTargetRpc(3u, new ClientRpcDelegate(RpcReader___Target_InitializeGridItem_Client_1883577149));
			((NetworkBehaviour)this).RegisterObserversRpc(4u, new ClientRpcDelegate(RpcReader___Observers_InitializeGridItem_Client_1883577149));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EEntityFramework_002EGridItemAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EEntityFramework_002EGridItemAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_InitializeGridItem_Server_2821640832(ItemInstance instance, string gridGUID, Vector2 originCoordinate, int rotation, string GUID)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(gridGUID);
			((Writer)writer).WriteVector2(originCoordinate);
			((Writer)writer).WriteInt32(rotation, (AutoPackType)1);
			((Writer)writer).WriteString(GUID);
			((NetworkBehaviour)this).SendServerRpc(2u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___InitializeGridItem_Server_2821640832(ItemInstance instance, string gridGUID, Vector2 originCoordinate, int rotation, string GUID)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		InitializeGridItem_Client(null, instance, gridGUID, originCoordinate, rotation, GUID);
	}

	private void RpcReader___Server_InitializeGridItem_Server_2821640832(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		ItemInstance instance = ((Reader)(object)PooledReader0).ReadItemInstance();
		string gridGUID = ((Reader)PooledReader0).ReadString();
		Vector2 originCoordinate = ((Reader)PooledReader0).ReadVector2();
		int rotation = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		string gUID = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___InitializeGridItem_Server_2821640832(instance, gridGUID, originCoordinate, rotation, gUID);
		}
	}

	private void RpcWriter___Target_InitializeGridItem_Client_1883577149(NetworkConnection conn, ItemInstance instance, string gridGUID, Vector2 originCoordinate, int rotation, string GUID)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(gridGUID);
			((Writer)writer).WriteVector2(originCoordinate);
			((Writer)writer).WriteInt32(rotation, (AutoPackType)1);
			((Writer)writer).WriteString(GUID);
			((NetworkBehaviour)this).SendTargetRpc(3u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcLogic___InitializeGridItem_Client_1883577149(NetworkConnection conn, ItemInstance instance, string gridGUID, Vector2 originCoordinate, int rotation, string GUID)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (!base.Initialized)
		{
			InitializeGridItem(instance, GUIDManager.GetObject<Grid>(new Guid(gridGUID)), originCoordinate, rotation, GUID);
		}
	}

	private void RpcReader___Target_InitializeGridItem_Client_1883577149(PooledReader PooledReader0, Channel channel)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		ItemInstance instance = ((Reader)(object)PooledReader0).ReadItemInstance();
		string gridGUID = ((Reader)PooledReader0).ReadString();
		Vector2 originCoordinate = ((Reader)PooledReader0).ReadVector2();
		int rotation = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		string gUID = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___InitializeGridItem_Client_1883577149(((NetworkBehaviour)this).LocalConnection, instance, gridGUID, originCoordinate, rotation, gUID);
		}
	}

	private void RpcWriter___Observers_InitializeGridItem_Client_1883577149(NetworkConnection conn, ItemInstance instance, string gridGUID, Vector2 originCoordinate, int rotation, string GUID)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(gridGUID);
			((Writer)writer).WriteVector2(originCoordinate);
			((Writer)writer).WriteInt32(rotation, (AutoPackType)1);
			((Writer)writer).WriteString(GUID);
			((NetworkBehaviour)this).SendObserversRpc(4u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcReader___Observers_InitializeGridItem_Client_1883577149(PooledReader PooledReader0, Channel channel)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		ItemInstance instance = ((Reader)(object)PooledReader0).ReadItemInstance();
		string gridGUID = ((Reader)PooledReader0).ReadString();
		Vector2 originCoordinate = ((Reader)PooledReader0).ReadVector2();
		int rotation = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		string gUID = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___InitializeGridItem_Client_1883577149(null, instance, gridGUID, originCoordinate, rotation, gUID);
		}
	}

	protected override void Awake_UserLogic_ScheduleOne_002EEntityFramework_002EGridItem_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		SetFootprintTileVisiblity(visible: false);
	}
}
