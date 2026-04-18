using System;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.Core;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence.Datas;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ScheduleOne.Graffiti;

public class SpraySurface : NetworkBehaviour
{
	public const float PIXEL_SIZE = 0.006666671f;

	[Header("Settings")]
	public bool Editable = true;

	[Range(1f, 1000f)]
	public int Width = 450;

	[Range(1f, 1000f)]
	public int Height = 300;

	public AnimationCurve FalloffCurve;

	[SerializeField]
	public bool IsVandalismSurface = true;

	[Header("References")]
	public Transform BottomLeftPoint;

	public DecalProjector Projector;

	protected Drawing drawing;

	private Drawing cachedDrawing;

	public Action onDrawingChanged;

	private List<int> pastRequestIDs = new List<int>();

	private bool NetworkInitialize___EarlyScheduleOne_002EGraffiti_002ESpraySurfaceAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EGraffiti_002ESpraySurfaceAssembly_002DCSharp_002Edll_Excuted;

	public NetworkObject CurrentEditor { get; private set; }

	public int DrawingStrokeCount
	{
		get
		{
			if (drawing == null)
			{
				return 0;
			}
			return drawing.StrokeCount;
		}
	}

	public Texture DrawingOutputTexture
	{
		get
		{
			if (drawing == null)
			{
				return null;
			}
			return (Texture)(object)drawing.OutputTexture;
		}
	}

	public int DrawingPaintedPixelCount
	{
		get
		{
			if (drawing == null)
			{
				return 0;
			}
			return drawing.PaintedPixelCount;
		}
		set
		{
			if (drawing != null)
			{
				drawing.PaintedPixelCount = value;
			}
		}
	}

	public int RoundedWidth => Mathf.NextPowerOfTwo(Width);

	public int RoundedHeight => Mathf.NextPowerOfTwo(Height);

	public bool ContainsCartelGraffiti { get; set; }

	public Vector3 TopRightPoint => BottomLeftPoint.TransformPoint(new Vector3((float)(-Width) * 0.006666671f, (float)Height * 0.006666671f, 0f));

	public Vector3 CenterPoint => BottomLeftPoint.TransformPoint(new Vector3((float)(-Width) * 0.006666671f / 2f, (float)Height * 0.006666671f / 2f, 0f));

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EGraffiti_002ESpraySurface_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	protected override void OnValidate()
	{
		((NetworkBehaviour)this).OnValidate();
		ResizeProjector();
	}

	private void ResizeProjector()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		((Component)Projector).transform.localPosition = new Vector3(BottomLeftPoint.localPosition.x - (float)RoundedWidth * 0.006666671f / 2f, BottomLeftPoint.localPosition.y + (float)RoundedHeight * 0.006666671f / 2f, ((Component)Projector).transform.localPosition.z);
		Projector.size = new Vector3((float)RoundedWidth * 0.006666671f, (float)RoundedHeight * 0.006666671f, Projector.size.z);
	}

	public bool CanBeEdited(bool checkEditor)
	{
		if (checkEditor && (Object)(object)CurrentEditor != (Object)null)
		{
			return false;
		}
		if (drawing != null && DrawingStrokeCount > 0)
		{
			return false;
		}
		return Editable;
	}

	public bool CanUndo()
	{
		if (drawing != null)
		{
			return drawing.CanUndo();
		}
		return false;
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		((NetworkBehaviour)this).OnSpawnServer(connection);
		if (!connection.IsHost)
		{
			if ((Object)(object)CurrentEditor != (Object)null)
			{
				SetCurrentEditor_Client(connection, CurrentEditor);
			}
			if (drawing != null && DrawingStrokeCount > 0)
			{
				NetworkSingleton<GraffitiManager>.Instance.QueueSurfaceToReplicate(this, connection);
			}
		}
	}

	public virtual void ReplicateTo(NetworkConnection conn)
	{
		if (drawing != null)
		{
			Set(conn, drawing.GetStrokes().ToArray(), ContainsCartelGraffiti);
		}
	}

	[ServerRpc(RequireOwnership = false)]
	public void SetCurrentEditor_Server(NetworkObject player)
	{
		RpcWriter___Server_SetCurrentEditor_Server_3323014238(player);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void SetCurrentEditor_Client(NetworkConnection conn, NetworkObject player)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetCurrentEditor_Client_1824087381(conn, player);
			RpcLogic___SetCurrentEditor_Client_1824087381(conn, player);
		}
		else
		{
			RpcWriter___Target_SetCurrentEditor_Client_1824087381(conn, player);
		}
	}

	public virtual void OnEditingFinished()
	{
		SetCurrentEditor_Server(null);
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void AddStrokes_Server(List<SprayStroke> newStrokes, int requestID)
	{
		RpcWriter___Server_AddStrokes_Server_1511871282(newStrokes, requestID);
		RpcLogic___AddStrokes_Server_1511871282(newStrokes, requestID);
	}

	[ObserversRpc(RunLocally = true)]
	private void AddStrokes_Client(List<SprayStroke> newStrokes, int requestID)
	{
		RpcWriter___Observers_AddStrokes_Client_1511871282(newStrokes, requestID);
		RpcLogic___AddStrokes_Client_1511871282(newStrokes, requestID);
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void AddTextureToHistory_Server(int requestID)
	{
		RpcWriter___Server_AddTextureToHistory_Server_3316948804(requestID);
		RpcLogic___AddTextureToHistory_Server_3316948804(requestID);
	}

	[ObserversRpc(RunLocally = true)]
	private void AddTextureToHistory_Client(int requestID)
	{
		RpcWriter___Observers_AddTextureToHistory_Client_3316948804(requestID);
		RpcLogic___AddTextureToHistory_Client_3316948804(requestID);
	}

	public void CacheDrawing()
	{
		drawing.CacheDrawing();
	}

	public void PrintHistoryCount()
	{
		Console.Log($"SpraySurface History Count: {drawing.HistoryCount}");
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void Undo_Server(int requestID)
	{
		RpcWriter___Server_Undo_Server_3316948804(requestID);
		RpcLogic___Undo_Server_3316948804(requestID);
	}

	[ObserversRpc(RunLocally = true)]
	private void Undo_Client(int requestID)
	{
		RpcWriter___Observers_Undo_Client_3316948804(requestID);
		RpcLogic___Undo_Client_3316948804(requestID);
	}

	public virtual void CleanGraffiti()
	{
		ClearDrawing();
	}

	[ServerRpc(RequireOwnership = false)]
	public void ClearDrawing()
	{
		RpcWriter___Server_ClearDrawing_2166136261();
	}

	public void EnsureDrawingExists()
	{
		if (drawing == null)
		{
			CreateNewDrawing();
		}
	}

	protected void CreateNewDrawing()
	{
		if (drawing != null)
		{
			Drawing obj = drawing;
			obj.onTextureChanged = (Action)Delegate.Remove(obj.onTextureChanged, new Action(DrawingChanged));
		}
		drawing = new Drawing(Width, Height, initPixels: true);
		Drawing obj2 = drawing;
		obj2.onTextureChanged = (Action)Delegate.Combine(obj2.onTextureChanged, new Action(DrawingChanged));
		DrawingChanged();
	}

	public void RestoreFromCache()
	{
		drawing.RestoreFromCache();
	}

	public Vector3 ToWorldPosition(UShort2 coordinate, float offset = 0f)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		return BottomLeftPoint.TransformPoint(new Vector3((float)(-coordinate.X) * 0.006666671f, (float)(int)coordinate.Y * 0.006666671f, offset));
	}

	public void DrawPaintedPixel(PixelData data, bool applyTexture)
	{
		drawing.DrawPaintedPixel(data, applyTexture);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void Set(NetworkConnection conn, SprayStroke[] strokes, bool isCartelGraffiti)
	{
		if (conn == null)
		{
			RpcWriter___Observers_Set_4105842735(conn, strokes, isCartelGraffiti);
			RpcLogic___Set_4105842735(conn, strokes, isCartelGraffiti);
		}
		else
		{
			RpcWriter___Target_Set_4105842735(conn, strokes, isCartelGraffiti);
		}
	}

	private void DrawingChanged()
	{
		if (onDrawingChanged != null)
		{
			onDrawingChanged();
		}
	}

	public SerializedGraffitiDrawing GetSerializedDrawing()
	{
		SerializedGraffitiDrawing serializedGraffitiDrawing = ScriptableObject.CreateInstance<SerializedGraffitiDrawing>();
		List<SprayStroke> strokes = drawing.GetStrokes();
		SprayStroke.GetBounds(strokes, out var min, out var _);
		List<SprayStroke> strokes2 = SprayStroke.CopyAndShiftStrokes(strokes, new UShort2((ushort)(-min.X), (ushort)(-min.Y)));
		serializedGraffitiDrawing.SetStrokes(strokes2);
		return serializedGraffitiDrawing;
	}

	[Button]
	public void LoadSerializedDrawing(SerializedGraffitiDrawing serializedDrawing, bool isCartelGraffiti)
	{
		if (!WillDrawingFit(serializedDrawing.Width, serializedDrawing.Height))
		{
			Console.LogError($"Cannot load drawing onto spray surface - drawing size ({serializedDrawing.Width}x{serializedDrawing.Height}) exceeds surface size ({Width}x{Height}).");
		}
		else
		{
			List<SprayStroke> list = SprayStroke.CopyAndShiftStrokes(shift: new UShort2((ushort)((Width - serializedDrawing.Width) / 2), (ushort)((Height - serializedDrawing.Height) / 2)), strokes: serializedDrawing.Strokes);
			Set(null, list.ToArray(), isCartelGraffiti);
		}
	}

	public bool WillDrawingFit(int width, int height)
	{
		if (width <= Width)
		{
			return height <= Height;
		}
		return false;
	}

	public static int GetPadding(byte strokeSize)
	{
		return Mathf.CeilToInt((float)(int)strokeSize / 2f);
	}

	public virtual bool ShouldSave()
	{
		return DrawingStrokeCount > 0;
	}

	public virtual SpraySurfaceData GetSaveData()
	{
		return new SpraySurfaceData((drawing != null) ? drawing.GetStrokes() : new List<SprayStroke>(), ContainsCartelGraffiti);
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
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EGraffiti_002ESpraySurfaceAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EGraffiti_002ESpraySurfaceAssembly_002DCSharp_002Edll_Excuted = true;
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_SetCurrentEditor_Server_3323014238));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_SetCurrentEditor_Client_1824087381));
			((NetworkBehaviour)this).RegisterTargetRpc(2u, new ClientRpcDelegate(RpcReader___Target_SetCurrentEditor_Client_1824087381));
			((NetworkBehaviour)this).RegisterServerRpc(3u, new ServerRpcDelegate(RpcReader___Server_AddStrokes_Server_1511871282));
			((NetworkBehaviour)this).RegisterObserversRpc(4u, new ClientRpcDelegate(RpcReader___Observers_AddStrokes_Client_1511871282));
			((NetworkBehaviour)this).RegisterServerRpc(5u, new ServerRpcDelegate(RpcReader___Server_AddTextureToHistory_Server_3316948804));
			((NetworkBehaviour)this).RegisterObserversRpc(6u, new ClientRpcDelegate(RpcReader___Observers_AddTextureToHistory_Client_3316948804));
			((NetworkBehaviour)this).RegisterServerRpc(7u, new ServerRpcDelegate(RpcReader___Server_Undo_Server_3316948804));
			((NetworkBehaviour)this).RegisterObserversRpc(8u, new ClientRpcDelegate(RpcReader___Observers_Undo_Client_3316948804));
			((NetworkBehaviour)this).RegisterServerRpc(9u, new ServerRpcDelegate(RpcReader___Server_ClearDrawing_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(10u, new ClientRpcDelegate(RpcReader___Observers_Set_4105842735));
			((NetworkBehaviour)this).RegisterTargetRpc(11u, new ClientRpcDelegate(RpcReader___Target_Set_4105842735));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EGraffiti_002ESpraySurfaceAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EGraffiti_002ESpraySurfaceAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_SetCurrentEditor_Server_3323014238(NetworkObject player)
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
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetCurrentEditor_Server_3323014238(NetworkObject player)
	{
		SetCurrentEditor_Client(null, player);
	}

	private void RpcReader___Server_SetCurrentEditor_Server_3323014238(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkObject player = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SetCurrentEditor_Server_3323014238(player);
		}
	}

	private void RpcWriter___Observers_SetCurrentEditor_Client_1824087381(NetworkConnection conn, NetworkObject player)
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
			((Writer)writer).WriteNetworkObject(player);
			((NetworkBehaviour)this).SendObserversRpc(1u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetCurrentEditor_Client_1824087381(NetworkConnection conn, NetworkObject player)
	{
		CurrentEditor = player;
	}

	private void RpcReader___Observers_SetCurrentEditor_Client_1824087381(PooledReader PooledReader0, Channel channel)
	{
		NetworkObject player = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetCurrentEditor_Client_1824087381(null, player);
		}
	}

	private void RpcWriter___Target_SetCurrentEditor_Client_1824087381(NetworkConnection conn, NetworkObject player)
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
			((Writer)writer).WriteNetworkObject(player);
			((NetworkBehaviour)this).SendTargetRpc(2u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetCurrentEditor_Client_1824087381(PooledReader PooledReader0, Channel channel)
	{
		NetworkObject player = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetCurrentEditor_Client_1824087381(((NetworkBehaviour)this).LocalConnection, player);
		}
	}

	private void RpcWriter___Server_AddStrokes_Server_1511871282(List<SprayStroke> newStrokes, int requestID)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
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
			GeneratedWriters___Internal.Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EGraffiti_002ESprayStroke_003EFishNet_002ESerializing_002EGenerated((Writer)(object)writer, newStrokes);
			((Writer)writer).WriteInt32(requestID, (AutoPackType)1);
			((NetworkBehaviour)this).SendServerRpc(3u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___AddStrokes_Server_1511871282(List<SprayStroke> newStrokes, int requestID)
	{
		AddStrokes_Client(newStrokes, requestID);
	}

	private void RpcReader___Server_AddStrokes_Server_1511871282(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		List<SprayStroke> newStrokes = GeneratedReaders___Internal.Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EGraffiti_002ESprayStroke_003EFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		int requestID = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___AddStrokes_Server_1511871282(newStrokes, requestID);
		}
	}

	private void RpcWriter___Observers_AddStrokes_Client_1511871282(List<SprayStroke> newStrokes, int requestID)
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
			GeneratedWriters___Internal.Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EGraffiti_002ESprayStroke_003EFishNet_002ESerializing_002EGenerated((Writer)(object)writer, newStrokes);
			((Writer)writer).WriteInt32(requestID, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(4u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___AddStrokes_Client_1511871282(List<SprayStroke> newStrokes, int requestID)
	{
		if (!pastRequestIDs.Contains(requestID))
		{
			pastRequestIDs.Add(requestID);
			EnsureDrawingExists();
			drawing.AddStrokes(newStrokes);
		}
	}

	private void RpcReader___Observers_AddStrokes_Client_1511871282(PooledReader PooledReader0, Channel channel)
	{
		List<SprayStroke> newStrokes = GeneratedReaders___Internal.Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EGraffiti_002ESprayStroke_003EFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		int requestID = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___AddStrokes_Client_1511871282(newStrokes, requestID);
		}
	}

	private void RpcWriter___Server_AddTextureToHistory_Server_3316948804(int requestID)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(requestID, (AutoPackType)1);
			((NetworkBehaviour)this).SendServerRpc(5u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___AddTextureToHistory_Server_3316948804(int requestID)
	{
		AddTextureToHistory_Client(requestID);
	}

	private void RpcReader___Server_AddTextureToHistory_Server_3316948804(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int requestID = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___AddTextureToHistory_Server_3316948804(requestID);
		}
	}

	private void RpcWriter___Observers_AddTextureToHistory_Client_3316948804(int requestID)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(requestID, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(6u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___AddTextureToHistory_Client_3316948804(int requestID)
	{
		if (!pastRequestIDs.Contains(requestID))
		{
			pastRequestIDs.Add(requestID);
			EnsureDrawingExists();
			drawing.AddTextureToHistory();
		}
	}

	private void RpcReader___Observers_AddTextureToHistory_Client_3316948804(PooledReader PooledReader0, Channel channel)
	{
		int requestID = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___AddTextureToHistory_Client_3316948804(requestID);
		}
	}

	private void RpcWriter___Server_Undo_Server_3316948804(int requestID)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(requestID, (AutoPackType)1);
			((NetworkBehaviour)this).SendServerRpc(7u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___Undo_Server_3316948804(int requestID)
	{
		Undo_Client(requestID);
	}

	private void RpcReader___Server_Undo_Server_3316948804(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int requestID = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___Undo_Server_3316948804(requestID);
		}
	}

	private void RpcWriter___Observers_Undo_Client_3316948804(int requestID)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(requestID, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(8u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___Undo_Client_3316948804(int requestID)
	{
		if (!pastRequestIDs.Contains(requestID))
		{
			pastRequestIDs.Add(requestID);
			drawing.Undo();
			DrawingChanged();
		}
	}

	private void RpcReader___Observers_Undo_Client_3316948804(PooledReader PooledReader0, Channel channel)
	{
		int requestID = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___Undo_Client_3316948804(requestID);
		}
	}

	private void RpcWriter___Server_ClearDrawing_2166136261()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
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
			((NetworkBehaviour)this).SendServerRpc(9u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___ClearDrawing_2166136261()
	{
		ContainsCartelGraffiti = false;
		if (DrawingStrokeCount != 0)
		{
			Set(null, new SprayStroke[0], isCartelGraffiti: false);
		}
	}

	private void RpcReader___Server_ClearDrawing_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___ClearDrawing_2166136261();
		}
	}

	private void RpcWriter___Observers_Set_4105842735(NetworkConnection conn, SprayStroke[] strokes, bool isCartelGraffiti)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EGraffiti_002ESprayStroke_005B_005DFishNet_002ESerializing_002EGenerated((Writer)(object)writer, strokes);
			((Writer)writer).WriteBoolean(isCartelGraffiti);
			((NetworkBehaviour)this).SendObserversRpc(10u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___Set_4105842735(NetworkConnection conn, SprayStroke[] strokes, bool isCartelGraffiti)
	{
		CreateNewDrawing();
		drawing.AddStrokes(strokes.ToList());
		ContainsCartelGraffiti = isCartelGraffiti;
	}

	private void RpcReader___Observers_Set_4105842735(PooledReader PooledReader0, Channel channel)
	{
		SprayStroke[] strokes = GeneratedReaders___Internal.Read___ScheduleOne_002EGraffiti_002ESprayStroke_005B_005DFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		bool isCartelGraffiti = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___Set_4105842735(null, strokes, isCartelGraffiti);
		}
	}

	private void RpcWriter___Target_Set_4105842735(NetworkConnection conn, SprayStroke[] strokes, bool isCartelGraffiti)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EGraffiti_002ESprayStroke_005B_005DFishNet_002ESerializing_002EGenerated((Writer)(object)writer, strokes);
			((Writer)writer).WriteBoolean(isCartelGraffiti);
			((NetworkBehaviour)this).SendTargetRpc(11u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_Set_4105842735(PooledReader PooledReader0, Channel channel)
	{
		SprayStroke[] strokes = GeneratedReaders___Internal.Read___ScheduleOne_002EGraffiti_002ESprayStroke_005B_005DFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		bool isCartelGraffiti = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___Set_4105842735(((NetworkBehaviour)this).LocalConnection, strokes, isCartelGraffiti);
		}
	}

	protected virtual void Awake_UserLogic_ScheduleOne_002EGraffiti_002ESpraySurface_Assembly_002DCSharp_002Edll()
	{
		ResizeProjector();
	}
}
