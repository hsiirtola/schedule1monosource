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
using ScheduleOne.Audio;
using ScheduleOne.Combat;
using ScheduleOne.Core;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Interaction;
using ScheduleOne.ItemFramework;
using ScheduleOne.Money;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Vision;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace ScheduleOne.ObjectScripts;

public class VendingMachine : NetworkBehaviour, IGUIDRegisterable, IGenericSaveable
{
	public static List<VendingMachine> AllMachines = new List<VendingMachine>();

	public const float COST = 2f;

	public const int REPAIR_TIME_DAYS = 0;

	public const float IMPACT_THRESHOLD_FREE_ITEM = 50f;

	public const float IMPACT_THRESHOLD_FREE_ITEM_CHANCE = 0.33f;

	public const float IMPACT_THRESHOLD_BREAK = 150f;

	public const int MIN_CASH_DROP = 1;

	public const int MAX_CASH_DROP = 4;

	[Header("Settings")]
	public int LitStartTime = 1700;

	public int LitOnEndTime = 800;

	public NetworkedItemPickup CukePrefab;

	public CashPickup CashPrefab;

	[Header("References")]
	public MeshRenderer DoorMesh;

	public MeshRenderer BodyMesh;

	public Material DoorOffMat;

	public Material DoorOnMat;

	public Material BodyOffMat;

	public Material BodyOnMat;

	public OptimizedLight[] Lights;

	public AudioSourceController PaySound;

	public AudioSourceController DispenseSound;

	public Animation Anim;

	public Transform ItemSpawnPoint;

	public InteractableObject IntObj;

	public Transform AccessPoint;

	public PhysicsDamageable Damageable;

	public Transform CashSpawnPoint;

	public UnityEvent onBreak;

	public UnityEvent onRepair;

	private bool isLit;

	private bool purchaseInProgress;

	private float timeOnLastFreeItem;

	[SerializeField]
	protected string BakedGUID = string.Empty;

	private bool NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EVendingMachineAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EObjectScripts_002EVendingMachineAssembly_002DCSharp_002Edll_Excuted;

	public bool IsBroken { get; protected set; }

	public int DaysUntilRepair { get; protected set; }

	public NetworkedItemPickup lastDroppedItem { get; protected set; }

	public Guid GUID { get; protected set; }

	[Button]
	public void RegenerateGUID()
	{
		BakedGUID = Guid.NewGuid().ToString();
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EObjectScripts_002EVendingMachine_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	private void Start()
	{
		NetworkSingleton<TimeManager>.Instance.onMinutePass += new Action(MinPass);
		TimeManager instance = NetworkSingleton<TimeManager>.Instance;
		instance.onSleepStart = (Action)Delegate.Combine(instance.onSleepStart, new Action(DayPass));
		SetLit(lit: false);
		((IGenericSaveable)this).InitializeSaveable();
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		((NetworkBehaviour)this).OnSpawnServer(connection);
		if (IsBroken)
		{
			Break(connection);
		}
	}

	public void SetGUID(Guid guid)
	{
		GUID = guid;
		GUIDManager.RegisterObject(this);
	}

	private void OnDestroy()
	{
		if (AllMachines.Contains(this))
		{
			AllMachines.Remove(this);
		}
		if (NetworkSingleton<TimeManager>.InstanceExists)
		{
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onSleepStart = (Action)Delegate.Remove(instance.onSleepStart, new Action(DayPass));
		}
	}

	private void MinPass()
	{
		if (NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(LitStartTime, LitOnEndTime) && !IsBroken)
		{
			if (!isLit)
			{
				SetLit(lit: true);
			}
		}
		else if (isLit)
		{
			SetLit(lit: false);
		}
	}

	public void DayPass()
	{
		if (IsBroken)
		{
			DaysUntilRepair--;
			if (DaysUntilRepair <= 0)
			{
				Repair();
			}
		}
	}

	public void Hovered()
	{
		if (purchaseInProgress)
		{
			IntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
		}
		else if (IsBroken)
		{
			IntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
		}
		else if (NetworkSingleton<MoneyManager>.Instance.cashBalance >= 2f)
		{
			IntObj.SetMessage("Purchase Cuke");
			IntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
		}
		else
		{
			IntObj.SetMessage("Not enough cash");
			IntObj.SetInteractableState(InteractableObject.EInteractableState.Invalid);
		}
	}

	public void Interacted()
	{
		if (!purchaseInProgress && !IsBroken && NetworkSingleton<MoneyManager>.Instance.cashBalance >= 2f)
		{
			LocalPurchase();
		}
	}

	private void LocalPurchase()
	{
		NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(-2f);
		SendPurchase();
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SendPurchase()
	{
		RpcWriter___Server_SendPurchase_2166136261();
		RpcLogic___SendPurchase_2166136261();
	}

	[ObserversRpc(RunLocally = true)]
	public void PurchaseRoutine()
	{
		RpcWriter___Observers_PurchaseRoutine_2166136261();
		RpcLogic___PurchaseRoutine_2166136261();
	}

	[ServerRpc(RequireOwnership = false)]
	public void DropItem()
	{
		RpcWriter___Server_DropItem_2166136261();
	}

	public void RemoveLastDropped()
	{
		if ((Object)(object)lastDroppedItem != (Object)null && (Object)(object)((Component)lastDroppedItem).gameObject != (Object)null)
		{
			lastDroppedItem.Destroy();
			lastDroppedItem = null;
		}
	}

	private void Impacted(Impact impact)
	{
		if (impact.ImpactForce < 50f || IsBroken)
		{
			return;
		}
		if (impact.ImpactForce >= 150f || impact.ImpactType == EImpactType.Bullet)
		{
			SendBreak();
			if ((Object)(object)impact.ImpactSource == (Object)(object)((NetworkBehaviour)Player.Local).NetworkObject)
			{
				Player.Local.VisualState.ApplyState("vandalism", EVisualState.Vandalizing);
				Player.Local.VisualState.RemoveState("vandalism", 2f);
			}
			((MonoBehaviour)this).StartCoroutine(BreakRoutine());
		}
		else if (Random.value < 0.33f && Time.time - timeOnLastFreeItem > 10f)
		{
			timeOnLastFreeItem = Time.time;
			((MonoBehaviour)this).StartCoroutine(Drop());
		}
		IEnumerator BreakRoutine()
		{
			int cashDrop = Random.Range(1, 5);
			for (int i = 0; i < cashDrop; i++)
			{
				DropCash();
				yield return (object)new WaitForSeconds(0.25f);
			}
		}
		IEnumerator Drop()
		{
			DispenseSound.Play();
			yield return (object)new WaitForSeconds(0.65f);
			DropItem();
		}
	}

	private void SetLit(bool lit)
	{
		isLit = lit;
		if (isLit)
		{
			Material[] materials = ((Renderer)DoorMesh).materials;
			materials[1] = DoorOnMat;
			((Renderer)DoorMesh).materials = materials;
			Material[] materials2 = ((Renderer)BodyMesh).materials;
			materials2[1] = BodyOnMat;
			((Renderer)BodyMesh).materials = materials2;
		}
		else
		{
			Material[] materials3 = ((Renderer)DoorMesh).materials;
			materials3[1] = DoorOffMat;
			((Renderer)DoorMesh).materials = materials3;
			Material[] materials4 = ((Renderer)BodyMesh).materials;
			materials4[1] = BodyOffMat;
			((Renderer)BodyMesh).materials = materials4;
		}
		for (int i = 0; i < Lights.Length; i++)
		{
			Lights[i].Enabled = isLit;
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	private void SendBreak()
	{
		RpcWriter___Server_SendBreak_2166136261();
		RpcLogic___SendBreak_2166136261();
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void Break(NetworkConnection conn)
	{
		if (conn == null)
		{
			RpcWriter___Observers_Break_328543758(conn);
			RpcLogic___Break_328543758(conn);
		}
		else
		{
			RpcWriter___Target_Break_328543758(conn);
		}
	}

	[ObserversRpc]
	private void Repair()
	{
		RpcWriter___Observers_Repair_2166136261();
	}

	[ServerRpc(RequireOwnership = false)]
	private void DropCash()
	{
		RpcWriter___Server_DropCash_2166136261();
	}

	public void Load(GenericSaveData data)
	{
		bool flag = data.GetBool("broken");
		if (flag)
		{
			Break(null);
		}
		IsBroken = flag;
		DaysUntilRepair = data.GetInt("daysUntilRepair");
	}

	public GenericSaveData GetSaveData()
	{
		GenericSaveData genericSaveData = new GenericSaveData(GUID.ToString());
		genericSaveData.Add("broken", IsBroken);
		genericSaveData.Add("daysUntilRepair", DaysUntilRepair);
		return genericSaveData;
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
		if (!NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EVendingMachineAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EVendingMachineAssembly_002DCSharp_002Edll_Excuted = true;
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_SendPurchase_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_PurchaseRoutine_2166136261));
			((NetworkBehaviour)this).RegisterServerRpc(2u, new ServerRpcDelegate(RpcReader___Server_DropItem_2166136261));
			((NetworkBehaviour)this).RegisterServerRpc(3u, new ServerRpcDelegate(RpcReader___Server_SendBreak_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(4u, new ClientRpcDelegate(RpcReader___Observers_Break_328543758));
			((NetworkBehaviour)this).RegisterTargetRpc(5u, new ClientRpcDelegate(RpcReader___Target_Break_328543758));
			((NetworkBehaviour)this).RegisterObserversRpc(6u, new ClientRpcDelegate(RpcReader___Observers_Repair_2166136261));
			((NetworkBehaviour)this).RegisterServerRpc(7u, new ServerRpcDelegate(RpcReader___Server_DropCash_2166136261));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EObjectScripts_002EVendingMachineAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EObjectScripts_002EVendingMachineAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_SendPurchase_2166136261()
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
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendPurchase_2166136261()
	{
		PurchaseRoutine();
	}

	private void RpcReader___Server_SendPurchase_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendPurchase_2166136261();
		}
	}

	private void RpcWriter___Observers_PurchaseRoutine_2166136261()
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
			((NetworkBehaviour)this).SendObserversRpc(1u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___PurchaseRoutine_2166136261()
	{
		if (!purchaseInProgress)
		{
			purchaseInProgress = true;
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			PaySound.Play();
			DispenseSound.Play();
			Anim.Play();
			yield return (object)new WaitForSeconds(0.65f);
			if (((NetworkBehaviour)this).IsServer)
			{
				DropItem();
			}
			purchaseInProgress = false;
		}
	}

	private void RpcReader___Observers_PurchaseRoutine_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___PurchaseRoutine_2166136261();
		}
	}

	private void RpcWriter___Server_DropItem_2166136261()
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
			((NetworkBehaviour)this).SendServerRpc(2u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___DropItem_2166136261()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		NetworkedItemPickup networkedItemPickup = Object.Instantiate<NetworkedItemPickup>(CukePrefab, ItemSpawnPoint.position, ItemSpawnPoint.rotation);
		((NetworkBehaviour)this).Spawn(((Component)networkedItemPickup).gameObject, (NetworkConnection)null, default(Scene));
		lastDroppedItem = networkedItemPickup;
	}

	private void RpcReader___Server_DropItem_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___DropItem_2166136261();
		}
	}

	private void RpcWriter___Server_SendBreak_2166136261()
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
			((NetworkBehaviour)this).SendServerRpc(3u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___SendBreak_2166136261()
	{
		DaysUntilRepair = 0;
		Break(null);
	}

	private void RpcReader___Server_SendBreak_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendBreak_2166136261();
		}
	}

	private void RpcWriter___Observers_Break_328543758(NetworkConnection conn)
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
			((NetworkBehaviour)this).SendObserversRpc(4u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___Break_328543758(NetworkConnection conn)
	{
		if (!IsBroken)
		{
			IsBroken = true;
			SetLit(lit: false);
			UnityEvent obj = onBreak;
			if (obj != null)
			{
				obj.Invoke();
			}
		}
	}

	private void RpcReader___Observers_Break_328543758(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___Break_328543758(null);
		}
	}

	private void RpcWriter___Target_Break_328543758(NetworkConnection conn)
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
			((NetworkBehaviour)this).SendTargetRpc(5u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_Break_328543758(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___Break_328543758(((NetworkBehaviour)this).LocalConnection);
		}
	}

	private void RpcWriter___Observers_Repair_2166136261()
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
			((NetworkBehaviour)this).SendObserversRpc(6u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___Repair_2166136261()
	{
		if (IsBroken)
		{
			Console.Log("Repairing...");
			IsBroken = false;
			UnityEvent obj = onRepair;
			if (obj != null)
			{
				obj.Invoke();
			}
		}
	}

	private void RpcReader___Observers_Repair_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___Repair_2166136261();
		}
	}

	private void RpcWriter___Server_DropCash_2166136261()
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
			((NetworkBehaviour)this).SendServerRpc(7u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___DropCash_2166136261()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = Object.Instantiate<GameObject>(((Component)CashPrefab).gameObject, CashSpawnPoint.position, CashSpawnPoint.rotation);
		val.GetComponent<Rigidbody>().AddForce(CashSpawnPoint.forward * Random.Range(1.5f, 2.5f), (ForceMode)2);
		val.GetComponent<Rigidbody>().AddTorque(Random.insideUnitSphere * 2f, (ForceMode)2);
		((NetworkBehaviour)this).Spawn(val.gameObject, (NetworkConnection)null, default(Scene));
		PaySound.Play();
	}

	private void RpcReader___Server_DropCash_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___DropCash_2166136261();
		}
	}

	private void Awake_UserLogic_ScheduleOne_002EObjectScripts_002EVendingMachine_Assembly_002DCSharp_002Edll()
	{
		if (!AllMachines.Contains(this))
		{
			AllMachines.Add(this);
		}
		PhysicsDamageable damageable = Damageable;
		damageable.onImpacted = (Action<Impact>)Delegate.Combine(damageable.onImpacted, new Action<Impact>(Impacted));
		GUID = new Guid(BakedGUID);
		GUIDManager.RegisterObject(this);
	}
}
