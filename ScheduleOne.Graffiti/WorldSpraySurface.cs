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
using ScheduleOne.Cartel;
using ScheduleOne.Core;
using ScheduleOne.DevUtilities;
using ScheduleOne.Levelling;
using ScheduleOne.Map;
using ScheduleOne.Persistence.Datas;
using UnityEngine;

namespace ScheduleOne.Graffiti;

public class WorldSpraySurface : SpraySurface, IGUIDRegisterable
{
	public const int RemoveCartelGraffitiXP = 25;

	private const float RemoveCartelGraffitiInfluenceChange = -0.05f;

	private const float CartelInfluenceChange = -0.05f;

	[Header("Settings")]
	public string BakedGUID = string.Empty;

	[SerializeField]
	private float StandPointWallOffset = 0.6f;

	private bool NetworkInitialize___EarlyScheduleOne_002EGraffiti_002EWorldSpraySurfaceAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EGraffiti_002EWorldSpraySurfaceAssembly_002DCSharp_002Edll_Excuted;

	public Guid GUID { get; protected set; }

	public EMapRegion Region { get; private set; }

	public bool HasEverBeenMarkedByPlayer { get; private set; }

	[field: SerializeField]
	public Transform NPCStandPoint { get; private set; }

	[field: SerializeField]
	public bool CanBeSprayedByNPCs { get; private set; } = true;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EGraffiti_002EWorldSpraySurface_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	private void Start()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		Region = Singleton<ScheduleOne.Map.Map>.Instance.GetRegionFromPosition(BottomLeftPoint.position);
		NetworkSingleton<GraffitiManager>.Instance.WorldSpraySurfaces.Remove(this);
		NetworkSingleton<GraffitiManager>.Instance.WorldSpraySurfaces.Add(this);
	}

	private void OnDrawGizmos()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)NPCStandPoint != (Object)null)
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawSphere(NPCStandPoint.position, 0.1f);
			Gizmos.DrawLine(NPCStandPoint.position, NPCStandPoint.position + Vector3.up * 0.5f);
		}
	}

	public override void OnEditingFinished()
	{
		base.OnEditingFinished();
		if (base.DrawingStrokeCount > 0 && !HasEverBeenMarkedByPlayer)
		{
			MarkDrawingFinalized();
			Reward();
			if (NetworkSingleton<GraffitiManager>.InstanceExists && NetworkSingleton<GraffitiManager>.Instance.WorldSpraySurfaces.FindAll((WorldSpraySurface s) => s.HasEverBeenMarkedByPlayer).Count >= 25)
			{
				AchievementManager.UnlockAchievement(AchievementManager.EAchievement.URBAN_ARTIST);
			}
		}
	}

	public override void CleanGraffiti()
	{
		if (base.ContainsCartelGraffiti && NetworkSingleton<ScheduleOne.Cartel.Cartel>.InstanceExists && NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance.Status == ECartelStatus.Hostile)
		{
			Debug.Log((object)"Giving player XP and influence for removing cartel graffiti.");
			NetworkSingleton<LevelManager>.Instance.AddXP(25);
			NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance.Influence.ChangeInfluence(Region, -0.05f);
		}
		base.CleanGraffiti();
	}

	private void Reward()
	{
		NetworkSingleton<LevelManager>.Instance.AddXP(50);
		if (NetworkSingleton<ScheduleOne.Cartel.Cartel>.InstanceExists && NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance.Status == ECartelStatus.Hostile)
		{
			NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance.Influence.ChangeInfluence(Region, -0.05f);
		}
	}

	public override void ReplicateTo(NetworkConnection conn)
	{
		if (drawing != null)
		{
			Set(conn, drawing.GetStrokes().ToArray(), HasEverBeenMarkedByPlayer, base.ContainsCartelGraffiti);
		}
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void Set(NetworkConnection conn, SprayStroke[] strokes, bool hasBeenFinalized, bool isCartelGraffiti)
	{
		if (conn == null)
		{
			RpcWriter___Observers_Set_3759704962(conn, strokes, hasBeenFinalized, isCartelGraffiti);
			RpcLogic___Set_3759704962(conn, strokes, hasBeenFinalized, isCartelGraffiti);
		}
		else
		{
			RpcWriter___Target_Set_3759704962(conn, strokes, hasBeenFinalized, isCartelGraffiti);
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void MarkDrawingFinalized()
	{
		RpcWriter___Server_MarkDrawingFinalized_2166136261();
		RpcLogic___MarkDrawingFinalized_2166136261();
	}

	[ObserversRpc(RunLocally = true)]
	private void SetFinalized()
	{
		RpcWriter___Observers_SetFinalized_2166136261();
		RpcLogic___SetFinalized_2166136261();
	}

	public override bool ShouldSave()
	{
		if (HasEverBeenMarkedByPlayer)
		{
			return true;
		}
		return base.ShouldSave();
	}

	public void SetGUID(Guid guid)
	{
		GUID = guid;
		GUIDManager.RegisterObject(this);
	}

	[Button]
	public void RegenerateGUID()
	{
		BakedGUID = Guid.NewGuid().ToString();
	}

	[Button]
	private void GroundNPCStandPoint()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		LayerMask val = default(LayerMask);
		((LayerMask)(ref val)).value = 1 << LayerMask.NameToLayer("Default");
		((LayerMask)(ref val)).value = ((LayerMask)(ref val)).value | (1 << LayerMask.NameToLayer("Terrain"));
		NPCStandPoint.localPosition = new Vector3(BottomLeftPoint.localPosition.x - (float)Width * 0.006666671f / 2f, 0f, StandPointWallOffset);
		RaycastHit val2 = default(RaycastHit);
		if (Physics.Raycast(NPCStandPoint.position + Vector3.up * 1f, Vector3.down, ref val2, 1000f, LayerMask.op_Implicit(val)))
		{
			NPCStandPoint.position = ((RaycastHit)(ref val2)).point;
		}
	}

	public new WorldSpraySurfaceData GetSaveData()
	{
		return new WorldSpraySurfaceData((drawing != null) ? drawing.GetStrokes() : new List<SprayStroke>(), base.ContainsCartelGraffiti, GUID.ToString(), HasEverBeenMarkedByPlayer);
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
		if (!NetworkInitialize___EarlyScheduleOne_002EGraffiti_002EWorldSpraySurfaceAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EGraffiti_002EWorldSpraySurfaceAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterObserversRpc(12u, new ClientRpcDelegate(RpcReader___Observers_Set_3759704962));
			((NetworkBehaviour)this).RegisterTargetRpc(13u, new ClientRpcDelegate(RpcReader___Target_Set_3759704962));
			((NetworkBehaviour)this).RegisterServerRpc(14u, new ServerRpcDelegate(RpcReader___Server_MarkDrawingFinalized_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(15u, new ClientRpcDelegate(RpcReader___Observers_SetFinalized_2166136261));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EGraffiti_002EWorldSpraySurfaceAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EGraffiti_002EWorldSpraySurfaceAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_Set_3759704962(NetworkConnection conn, SprayStroke[] strokes, bool hasBeenFinalized, bool isCartelGraffiti)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EGraffiti_002ESprayStroke_005B_005DFishNet_002ESerializing_002EGenerated((Writer)(object)writer, strokes);
			((Writer)writer).WriteBoolean(hasBeenFinalized);
			((Writer)writer).WriteBoolean(isCartelGraffiti);
			((NetworkBehaviour)this).SendObserversRpc(12u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___Set_3759704962(NetworkConnection conn, SprayStroke[] strokes, bool hasBeenFinalized, bool isCartelGraffiti)
	{
		CreateNewDrawing();
		drawing.AddStrokes(strokes.ToList());
		HasEverBeenMarkedByPlayer = hasBeenFinalized;
		base.ContainsCartelGraffiti = isCartelGraffiti;
	}

	private void RpcReader___Observers_Set_3759704962(PooledReader PooledReader0, Channel channel)
	{
		SprayStroke[] strokes = GeneratedReaders___Internal.Read___ScheduleOne_002EGraffiti_002ESprayStroke_005B_005DFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		bool hasBeenFinalized = ((Reader)PooledReader0).ReadBoolean();
		bool isCartelGraffiti = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___Set_3759704962(null, strokes, hasBeenFinalized, isCartelGraffiti);
		}
	}

	private void RpcWriter___Target_Set_3759704962(NetworkConnection conn, SprayStroke[] strokes, bool hasBeenFinalized, bool isCartelGraffiti)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EGraffiti_002ESprayStroke_005B_005DFishNet_002ESerializing_002EGenerated((Writer)(object)writer, strokes);
			((Writer)writer).WriteBoolean(hasBeenFinalized);
			((Writer)writer).WriteBoolean(isCartelGraffiti);
			((NetworkBehaviour)this).SendTargetRpc(13u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_Set_3759704962(PooledReader PooledReader0, Channel channel)
	{
		SprayStroke[] strokes = GeneratedReaders___Internal.Read___ScheduleOne_002EGraffiti_002ESprayStroke_005B_005DFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		bool hasBeenFinalized = ((Reader)PooledReader0).ReadBoolean();
		bool isCartelGraffiti = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___Set_3759704962(((NetworkBehaviour)this).LocalConnection, strokes, hasBeenFinalized, isCartelGraffiti);
		}
	}

	private void RpcWriter___Server_MarkDrawingFinalized_2166136261()
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
			((NetworkBehaviour)this).SendServerRpc(14u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___MarkDrawingFinalized_2166136261()
	{
		SetFinalized();
	}

	private void RpcReader___Server_MarkDrawingFinalized_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___MarkDrawingFinalized_2166136261();
		}
	}

	private void RpcWriter___Observers_SetFinalized_2166136261()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
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
			((NetworkBehaviour)this).SendObserversRpc(15u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetFinalized_2166136261()
	{
		HasEverBeenMarkedByPlayer = true;
	}

	private void RpcReader___Observers_SetFinalized_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetFinalized_2166136261();
		}
	}

	protected override void Awake_UserLogic_ScheduleOne_002EGraffiti_002EWorldSpraySurface_Assembly_002DCSharp_002Edll()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		base.Awake();
		GUID = new Guid(BakedGUID);
		GUIDManager.RegisterObject(this);
		if (CanBeSprayedByNPCs && !NavMeshUtility.SamplePosition(NPCStandPoint.position, out var _, 1f, -1))
		{
			Debug.LogWarning((object)("WorldSpraySurface '" + ((Object)this).name + "' has NPCStandPoint that is not on the NavMesh. Disabling NPC spraying ability."));
			CanBeSprayedByNPCs = false;
		}
	}
}
