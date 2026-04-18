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
using ScheduleOne.Combat;
using ScheduleOne.DevUtilities;
using ScheduleOne.Doors;
using ScheduleOne.GameTime;
using ScheduleOne.ItemFramework;
using ScheduleOne.Map;
using ScheduleOne.Networking;
using ScheduleOne.Product;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.NPCs.Behaviour;

public class NPCBehaviour : NetworkBehaviour
{
	public bool DEBUG_MODE;

	[Header("References")]
	public NPCScheduleManager ScheduleManager;

	[Header("Default Behaviours")]
	public CoweringBehaviour CoweringBehaviour;

	public RagdollBehaviour RagdollBehaviour;

	public CallPoliceBehaviour CallPoliceBehaviour;

	public GenericDialogueBehaviour GenericDialogueBehaviour;

	public HeavyFlinchBehaviour HeavyFlinchBehaviour;

	public FaceTargetBehaviour FaceTargetBehaviour;

	public DeadBehaviour DeadBehaviour;

	public UnconsciousBehaviour UnconsciousBehaviour;

	public Behaviour SummonBehaviour;

	public ConsumeProductBehaviour ConsumeProductBehaviour;

	public CombatBehaviour CombatBehaviour;

	public FleeBehaviour FleeBehaviour;

	public StationaryBehaviour StationaryBehaviour;

	public RequestProductBehaviour RequestProductBehaviour;

	[SerializeField]
	protected List<Behaviour> behaviourStack = new List<Behaviour>();

	private Coroutine summonRoutine;

	[SerializeField]
	private List<Behaviour> enabledBehaviours = new List<Behaviour>();

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ENPCBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ENPCBehaviourAssembly_002DCSharp_002Edll_Excuted;

	public Behaviour activeBehaviour { get; set; }

	public NPC Npc { get; private set; }

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002ENPCBehaviour_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	protected virtual void Start()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Expected O, but got Unknown
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Expected O, but got Unknown
		Npc.Avatar.Animation.onHeavyFlinch.AddListener(new UnityAction(HeavyFlinchBehaviour.Flinch));
		NetworkSingleton<TimeManager>.Instance.onTick -= new Action(OnTick);
		NetworkSingleton<TimeManager>.Instance.onTick += new Action(OnTick);
		NetworkSingleton<TimeManager>.Instance.onUncappedMinutePass -= new Action(OnUncappedMinutePass);
		NetworkSingleton<TimeManager>.Instance.onUncappedMinutePass += new Action(OnUncappedMinutePass);
		for (int i = 0; i < behaviourStack.Count; i++)
		{
			Behaviour b = behaviourStack[i];
			if (b.Enabled)
			{
				enabledBehaviours.Add(b);
			}
			b.onEnable.AddListener((UnityAction)delegate
			{
				AddEnabledBehaviour(b);
			});
			b.onDisable.AddListener((UnityAction)delegate
			{
				RemoveEnabledBehaviour(b);
			});
		}
	}

	private void OnDestroy()
	{
		if (NetworkSingleton<TimeManager>.InstanceExists)
		{
			NetworkSingleton<TimeManager>.Instance.onTick -= new Action(OnTick);
			NetworkSingleton<TimeManager>.Instance.onUncappedMinutePass -= new Action(OnUncappedMinutePass);
		}
	}

	protected override void OnValidate()
	{
		((NetworkBehaviour)this).OnValidate();
		behaviourStack = ((Component)this).GetComponentsInChildren<Behaviour>().ToList();
		SortBehaviourStack();
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		((NetworkBehaviour)this).OnSpawnServer(connection);
		if (!connection.IsHost)
		{
			ReplicationQueue.Enqueue(((object)this).GetType().Name, connection, Replicate, 512);
		}
		void Replicate(NetworkConnection conn)
		{
			for (int i = 0; i < behaviourStack.Count; i++)
			{
				if (behaviourStack[i].Enabled)
				{
					EnableBehaviour_Client(conn, i);
				}
			}
			if ((Object)(object)activeBehaviour != (Object)null)
			{
				activeBehaviour.Activate_Server(conn);
			}
		}
	}

	[ServerRpc(RequireOwnership = false)]
	public void Summon(string buildingGUID, int doorIndex, float duration)
	{
		RpcWriter___Server_Summon_900355577(buildingGUID, doorIndex, duration);
	}

	[ServerRpc(RequireOwnership = false)]
	public void ConsumeProduct(ProductItemInstance product, bool removeFromInventory = false)
	{
		RpcWriter___Server_ConsumeProduct_3964170259(product, removeFromInventory);
	}

	private void OnKnockOut()
	{
		foreach (Behaviour item in behaviourStack)
		{
			if (!((Object)(object)item == (Object)(object)DeadBehaviour) && !((Object)(object)item == (Object)(object)UnconsciousBehaviour))
			{
				item.Disable_Networked(null);
				if (item.Active)
				{
					item.Deactivate_Networked(null);
				}
			}
		}
	}

	private void OnRevive()
	{
		if (!InstanceFinder.IsServer)
		{
			return;
		}
		foreach (Behaviour item in behaviourStack)
		{
			if (item.EnabledOnAwake)
			{
				item.Enable_Server();
			}
		}
	}

	protected virtual void OnDie()
	{
		OnKnockOut();
		UnconsciousBehaviour.Disable_Networked(null);
	}

	public Behaviour GetBehaviour(string BehaviourName)
	{
		Behaviour behaviour = behaviourStack.Find((Behaviour x) => x.Name.ToLower() == BehaviourName.ToLower());
		if ((Object)(object)behaviour == (Object)null)
		{
			Console.LogWarning("No behaviour found with name '" + BehaviourName + "'");
		}
		return behaviour;
	}

	public T GetBehaviour<T>() where T : Behaviour
	{
		return behaviourStack.FirstOrDefault((Behaviour x) => x is T) as T;
	}

	public virtual void Update()
	{
		if (DEBUG_MODE)
		{
			Debug.Log((object)("Enabled behaviours: " + string.Join(", ", enabledBehaviours.Select((Behaviour x) => x.Name).ToArray())));
			if ((Object)(object)activeBehaviour != (Object)null)
			{
				Debug.Log((object)("Active behaviour: " + activeBehaviour.Name));
			}
		}
		if (InstanceFinder.IsHost)
		{
			Behaviour enabledBehaviour = GetEnabledBehaviour();
			if ((Object)(object)enabledBehaviour != (Object)(object)activeBehaviour)
			{
				if ((Object)(object)activeBehaviour != (Object)null)
				{
					activeBehaviour.Pause_Server();
				}
				if ((Object)(object)enabledBehaviour != (Object)null)
				{
					if (enabledBehaviour.Started)
					{
						enabledBehaviour.Resume_Server();
					}
					else
					{
						enabledBehaviour.Activate_Server(null);
					}
				}
			}
		}
		if ((Object)(object)activeBehaviour != (Object)null && activeBehaviour.Active)
		{
			activeBehaviour.BehaviourUpdate();
		}
	}

	public virtual void LateUpdate()
	{
		if ((Object)(object)activeBehaviour != (Object)null && activeBehaviour.Active)
		{
			activeBehaviour.BehaviourLateUpdate();
		}
	}

	protected virtual void OnTick()
	{
		if ((Object)(object)activeBehaviour != (Object)null && activeBehaviour.Active)
		{
			activeBehaviour.OnActiveTick();
		}
	}

	protected virtual void OnUncappedMinutePass()
	{
		if ((Object)(object)activeBehaviour != (Object)null && activeBehaviour.Active)
		{
			activeBehaviour.OnActiveUncappedMinutePass();
		}
	}

	public void SortBehaviourStack()
	{
		behaviourStack = behaviourStack.OrderByDescending((Behaviour x) => x.Priority).ToList();
		for (int num = 0; num < behaviourStack.Count; num++)
		{
			behaviourStack[num].BehaviourIndex = num;
		}
	}

	private Behaviour GetEnabledBehaviour()
	{
		return enabledBehaviours.FirstOrDefault();
	}

	private void AddEnabledBehaviour(Behaviour b)
	{
		if (!enabledBehaviours.Contains(b))
		{
			enabledBehaviours.Add(b);
			enabledBehaviours = enabledBehaviours.OrderByDescending((Behaviour x) => x.Priority).ToList();
		}
	}

	private void RemoveEnabledBehaviour(Behaviour b)
	{
		if (enabledBehaviours.Contains(b))
		{
			enabledBehaviours.Remove(b);
			enabledBehaviours = enabledBehaviours.OrderByDescending((Behaviour x) => x.Priority).ToList();
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void EnableBehaviour_Server(int behaviourIndex)
	{
		RpcWriter___Server_EnableBehaviour_Server_3316948804(behaviourIndex);
		RpcLogic___EnableBehaviour_Server_3316948804(behaviourIndex);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void EnableBehaviour_Client(NetworkConnection conn, int behaviourIndex)
	{
		if (conn == null)
		{
			RpcWriter___Observers_EnableBehaviour_Client_2681120339(conn, behaviourIndex);
			RpcLogic___EnableBehaviour_Client_2681120339(conn, behaviourIndex);
		}
		else
		{
			RpcWriter___Target_EnableBehaviour_Client_2681120339(conn, behaviourIndex);
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void DisableBehaviour_Server(int behaviourIndex)
	{
		RpcWriter___Server_DisableBehaviour_Server_3316948804(behaviourIndex);
		RpcLogic___DisableBehaviour_Server_3316948804(behaviourIndex);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void DisableBehaviour_Client(NetworkConnection conn, int behaviourIndex)
	{
		if (conn == null)
		{
			RpcWriter___Observers_DisableBehaviour_Client_2681120339(conn, behaviourIndex);
			RpcLogic___DisableBehaviour_Client_2681120339(conn, behaviourIndex);
		}
		else
		{
			RpcWriter___Target_DisableBehaviour_Client_2681120339(conn, behaviourIndex);
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void ActivateBehaviour_Server(int behaviourIndex)
	{
		RpcWriter___Server_ActivateBehaviour_Server_3316948804(behaviourIndex);
		RpcLogic___ActivateBehaviour_Server_3316948804(behaviourIndex);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void ActivateBehaviour_Client(NetworkConnection conn, int behaviourIndex)
	{
		if (conn == null)
		{
			RpcWriter___Observers_ActivateBehaviour_Client_2681120339(conn, behaviourIndex);
			RpcLogic___ActivateBehaviour_Client_2681120339(conn, behaviourIndex);
		}
		else
		{
			RpcWriter___Target_ActivateBehaviour_Client_2681120339(conn, behaviourIndex);
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void DeactivateBehaviour_Server(int behaviourIndex)
	{
		RpcWriter___Server_DeactivateBehaviour_Server_3316948804(behaviourIndex);
		RpcLogic___DeactivateBehaviour_Server_3316948804(behaviourIndex);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void DeactivateBehaviour_Client(NetworkConnection conn, int behaviourIndex)
	{
		if (conn == null)
		{
			RpcWriter___Observers_DeactivateBehaviour_Client_2681120339(conn, behaviourIndex);
			RpcLogic___DeactivateBehaviour_Client_2681120339(conn, behaviourIndex);
		}
		else
		{
			RpcWriter___Target_DeactivateBehaviour_Client_2681120339(conn, behaviourIndex);
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void PauseBehaviour_Server(int behaviourIndex)
	{
		RpcWriter___Server_PauseBehaviour_Server_3316948804(behaviourIndex);
		RpcLogic___PauseBehaviour_Server_3316948804(behaviourIndex);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void PauseBehaviour_Client(NetworkConnection conn, int behaviourIndex)
	{
		if (conn == null)
		{
			RpcWriter___Observers_PauseBehaviour_Client_2681120339(conn, behaviourIndex);
			RpcLogic___PauseBehaviour_Client_2681120339(conn, behaviourIndex);
		}
		else
		{
			RpcWriter___Target_PauseBehaviour_Client_2681120339(conn, behaviourIndex);
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void ResumeBehaviour_Server(int behaviourIndex)
	{
		RpcWriter___Server_ResumeBehaviour_Server_3316948804(behaviourIndex);
		RpcLogic___ResumeBehaviour_Server_3316948804(behaviourIndex);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void ResumeBehaviour_Client(NetworkConnection conn, int behaviourIndex)
	{
		if (conn == null)
		{
			RpcWriter___Observers_ResumeBehaviour_Client_2681120339(conn, behaviourIndex);
			RpcLogic___ResumeBehaviour_Client_2681120339(conn, behaviourIndex);
		}
		else
		{
			RpcWriter___Target_ResumeBehaviour_Client_2681120339(conn, behaviourIndex);
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
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Expected O, but got Unknown
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Expected O, but got Unknown
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Expected O, but got Unknown
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Expected O, but got Unknown
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Expected O, but got Unknown
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Expected O, but got Unknown
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Expected O, but got Unknown
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Expected O, but got Unknown
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ENPCBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ENPCBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_Summon_900355577));
			((NetworkBehaviour)this).RegisterServerRpc(1u, new ServerRpcDelegate(RpcReader___Server_ConsumeProduct_3964170259));
			((NetworkBehaviour)this).RegisterServerRpc(2u, new ServerRpcDelegate(RpcReader___Server_EnableBehaviour_Server_3316948804));
			((NetworkBehaviour)this).RegisterObserversRpc(3u, new ClientRpcDelegate(RpcReader___Observers_EnableBehaviour_Client_2681120339));
			((NetworkBehaviour)this).RegisterTargetRpc(4u, new ClientRpcDelegate(RpcReader___Target_EnableBehaviour_Client_2681120339));
			((NetworkBehaviour)this).RegisterServerRpc(5u, new ServerRpcDelegate(RpcReader___Server_DisableBehaviour_Server_3316948804));
			((NetworkBehaviour)this).RegisterObserversRpc(6u, new ClientRpcDelegate(RpcReader___Observers_DisableBehaviour_Client_2681120339));
			((NetworkBehaviour)this).RegisterTargetRpc(7u, new ClientRpcDelegate(RpcReader___Target_DisableBehaviour_Client_2681120339));
			((NetworkBehaviour)this).RegisterServerRpc(8u, new ServerRpcDelegate(RpcReader___Server_ActivateBehaviour_Server_3316948804));
			((NetworkBehaviour)this).RegisterObserversRpc(9u, new ClientRpcDelegate(RpcReader___Observers_ActivateBehaviour_Client_2681120339));
			((NetworkBehaviour)this).RegisterTargetRpc(10u, new ClientRpcDelegate(RpcReader___Target_ActivateBehaviour_Client_2681120339));
			((NetworkBehaviour)this).RegisterServerRpc(11u, new ServerRpcDelegate(RpcReader___Server_DeactivateBehaviour_Server_3316948804));
			((NetworkBehaviour)this).RegisterObserversRpc(12u, new ClientRpcDelegate(RpcReader___Observers_DeactivateBehaviour_Client_2681120339));
			((NetworkBehaviour)this).RegisterTargetRpc(13u, new ClientRpcDelegate(RpcReader___Target_DeactivateBehaviour_Client_2681120339));
			((NetworkBehaviour)this).RegisterServerRpc(14u, new ServerRpcDelegate(RpcReader___Server_PauseBehaviour_Server_3316948804));
			((NetworkBehaviour)this).RegisterObserversRpc(15u, new ClientRpcDelegate(RpcReader___Observers_PauseBehaviour_Client_2681120339));
			((NetworkBehaviour)this).RegisterTargetRpc(16u, new ClientRpcDelegate(RpcReader___Target_PauseBehaviour_Client_2681120339));
			((NetworkBehaviour)this).RegisterServerRpc(17u, new ServerRpcDelegate(RpcReader___Server_ResumeBehaviour_Server_3316948804));
			((NetworkBehaviour)this).RegisterObserversRpc(18u, new ClientRpcDelegate(RpcReader___Observers_ResumeBehaviour_Client_2681120339));
			((NetworkBehaviour)this).RegisterTargetRpc(19u, new ClientRpcDelegate(RpcReader___Target_ResumeBehaviour_Client_2681120339));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ENPCBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ENPCBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_Summon_900355577(string buildingGUID, int doorIndex, float duration)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(buildingGUID);
			((Writer)writer).WriteInt32(doorIndex, (AutoPackType)1);
			((Writer)writer).WriteSingle(duration, (AutoPackType)0);
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___Summon_900355577(string buildingGUID, int doorIndex, float duration)
	{
		NPCEnterableBuilding nPCEnterableBuilding = GUIDManager.GetObject<NPCEnterableBuilding>(new Guid(buildingGUID));
		if ((Object)(object)nPCEnterableBuilding == (Object)null)
		{
			Console.LogError("Failed to find building with GUID: " + buildingGUID);
			return;
		}
		if (doorIndex >= nPCEnterableBuilding.Doors.Length || doorIndex < 0)
		{
			Console.LogError("Door index out of range: " + doorIndex + " / " + nPCEnterableBuilding.Doors.Length);
			return;
		}
		StaticDoor lastEnteredDoor = nPCEnterableBuilding.Doors[doorIndex];
		Npc.LastEnteredDoor = lastEnteredDoor;
		SummonBehaviour.Enable_Networked();
		if (summonRoutine != null)
		{
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StopCoroutine(summonRoutine);
		}
		summonRoutine = ((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			float t = 0f;
			while (Npc.IsConscious)
			{
				if (SummonBehaviour.Active)
				{
					t += Time.deltaTime;
					if (t >= duration)
					{
						break;
					}
				}
				yield return (object)new WaitForEndOfFrame();
			}
			SummonBehaviour.Disable_Networked(null);
		}
	}

	private void RpcReader___Server_Summon_900355577(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string buildingGUID = ((Reader)PooledReader0).ReadString();
		int doorIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		float duration = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___Summon_900355577(buildingGUID, doorIndex, duration);
		}
	}

	private void RpcWriter___Server_ConsumeProduct_3964170259(ProductItemInstance product, bool removeFromInventory = false)
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
			((Writer)(object)writer).WriteProductItemInstance(product);
			((Writer)writer).WriteBoolean(removeFromInventory);
			((NetworkBehaviour)this).SendServerRpc(1u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___ConsumeProduct_3964170259(ProductItemInstance product, bool removeFromInventory = false)
	{
		if (product == null)
		{
			Console.LogError("Product is null");
			return;
		}
		ConsumeProductBehaviour.SendProduct(product, removeFromInventory);
		ConsumeProductBehaviour.Enable_Networked();
	}

	private void RpcReader___Server_ConsumeProduct_3964170259(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		ProductItemInstance product = ((Reader)(object)PooledReader0).ReadProductItemInstance();
		bool removeFromInventory = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___ConsumeProduct_3964170259(product, removeFromInventory);
		}
	}

	private void RpcWriter___Server_EnableBehaviour_Server_3316948804(int behaviourIndex)
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
			((Writer)writer).WriteInt32(behaviourIndex, (AutoPackType)1);
			((NetworkBehaviour)this).SendServerRpc(2u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___EnableBehaviour_Server_3316948804(int behaviourIndex)
	{
		EnableBehaviour_Client(null, behaviourIndex);
	}

	private void RpcReader___Server_EnableBehaviour_Server_3316948804(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int behaviourIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___EnableBehaviour_Server_3316948804(behaviourIndex);
		}
	}

	private void RpcWriter___Observers_EnableBehaviour_Client_2681120339(NetworkConnection conn, int behaviourIndex)
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
			((Writer)writer).WriteInt32(behaviourIndex, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(3u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___EnableBehaviour_Client_2681120339(NetworkConnection conn, int behaviourIndex)
	{
		behaviourStack[behaviourIndex].Enable();
	}

	private void RpcReader___Observers_EnableBehaviour_Client_2681120339(PooledReader PooledReader0, Channel channel)
	{
		int behaviourIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___EnableBehaviour_Client_2681120339(null, behaviourIndex);
		}
	}

	private void RpcWriter___Target_EnableBehaviour_Client_2681120339(NetworkConnection conn, int behaviourIndex)
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
			((Writer)writer).WriteInt32(behaviourIndex, (AutoPackType)1);
			((NetworkBehaviour)this).SendTargetRpc(4u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_EnableBehaviour_Client_2681120339(PooledReader PooledReader0, Channel channel)
	{
		int behaviourIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___EnableBehaviour_Client_2681120339(((NetworkBehaviour)this).LocalConnection, behaviourIndex);
		}
	}

	private void RpcWriter___Server_DisableBehaviour_Server_3316948804(int behaviourIndex)
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
			((Writer)writer).WriteInt32(behaviourIndex, (AutoPackType)1);
			((NetworkBehaviour)this).SendServerRpc(5u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___DisableBehaviour_Server_3316948804(int behaviourIndex)
	{
		DisableBehaviour_Client(null, behaviourIndex);
	}

	private void RpcReader___Server_DisableBehaviour_Server_3316948804(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int behaviourIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___DisableBehaviour_Server_3316948804(behaviourIndex);
		}
	}

	private void RpcWriter___Observers_DisableBehaviour_Client_2681120339(NetworkConnection conn, int behaviourIndex)
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
			((Writer)writer).WriteInt32(behaviourIndex, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(6u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___DisableBehaviour_Client_2681120339(NetworkConnection conn, int behaviourIndex)
	{
		behaviourStack[behaviourIndex].Disable();
	}

	private void RpcReader___Observers_DisableBehaviour_Client_2681120339(PooledReader PooledReader0, Channel channel)
	{
		int behaviourIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___DisableBehaviour_Client_2681120339(null, behaviourIndex);
		}
	}

	private void RpcWriter___Target_DisableBehaviour_Client_2681120339(NetworkConnection conn, int behaviourIndex)
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
			((Writer)writer).WriteInt32(behaviourIndex, (AutoPackType)1);
			((NetworkBehaviour)this).SendTargetRpc(7u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_DisableBehaviour_Client_2681120339(PooledReader PooledReader0, Channel channel)
	{
		int behaviourIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___DisableBehaviour_Client_2681120339(((NetworkBehaviour)this).LocalConnection, behaviourIndex);
		}
	}

	private void RpcWriter___Server_ActivateBehaviour_Server_3316948804(int behaviourIndex)
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
			((Writer)writer).WriteInt32(behaviourIndex, (AutoPackType)1);
			((NetworkBehaviour)this).SendServerRpc(8u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___ActivateBehaviour_Server_3316948804(int behaviourIndex)
	{
		ActivateBehaviour_Client(null, behaviourIndex);
	}

	private void RpcReader___Server_ActivateBehaviour_Server_3316948804(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int behaviourIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___ActivateBehaviour_Server_3316948804(behaviourIndex);
		}
	}

	private void RpcWriter___Observers_ActivateBehaviour_Client_2681120339(NetworkConnection conn, int behaviourIndex)
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
			((Writer)writer).WriteInt32(behaviourIndex, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(9u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___ActivateBehaviour_Client_2681120339(NetworkConnection conn, int behaviourIndex)
	{
		behaviourStack[behaviourIndex].Activate();
	}

	private void RpcReader___Observers_ActivateBehaviour_Client_2681120339(PooledReader PooledReader0, Channel channel)
	{
		int behaviourIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___ActivateBehaviour_Client_2681120339(null, behaviourIndex);
		}
	}

	private void RpcWriter___Target_ActivateBehaviour_Client_2681120339(NetworkConnection conn, int behaviourIndex)
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
			((Writer)writer).WriteInt32(behaviourIndex, (AutoPackType)1);
			((NetworkBehaviour)this).SendTargetRpc(10u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_ActivateBehaviour_Client_2681120339(PooledReader PooledReader0, Channel channel)
	{
		int behaviourIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ActivateBehaviour_Client_2681120339(((NetworkBehaviour)this).LocalConnection, behaviourIndex);
		}
	}

	private void RpcWriter___Server_DeactivateBehaviour_Server_3316948804(int behaviourIndex)
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
			((Writer)writer).WriteInt32(behaviourIndex, (AutoPackType)1);
			((NetworkBehaviour)this).SendServerRpc(11u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___DeactivateBehaviour_Server_3316948804(int behaviourIndex)
	{
		DeactivateBehaviour_Client(null, behaviourIndex);
	}

	private void RpcReader___Server_DeactivateBehaviour_Server_3316948804(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int behaviourIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___DeactivateBehaviour_Server_3316948804(behaviourIndex);
		}
	}

	private void RpcWriter___Observers_DeactivateBehaviour_Client_2681120339(NetworkConnection conn, int behaviourIndex)
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
			((Writer)writer).WriteInt32(behaviourIndex, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(12u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___DeactivateBehaviour_Client_2681120339(NetworkConnection conn, int behaviourIndex)
	{
		behaviourStack[behaviourIndex].Deactivate();
	}

	private void RpcReader___Observers_DeactivateBehaviour_Client_2681120339(PooledReader PooledReader0, Channel channel)
	{
		int behaviourIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___DeactivateBehaviour_Client_2681120339(null, behaviourIndex);
		}
	}

	private void RpcWriter___Target_DeactivateBehaviour_Client_2681120339(NetworkConnection conn, int behaviourIndex)
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
			((Writer)writer).WriteInt32(behaviourIndex, (AutoPackType)1);
			((NetworkBehaviour)this).SendTargetRpc(13u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_DeactivateBehaviour_Client_2681120339(PooledReader PooledReader0, Channel channel)
	{
		int behaviourIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___DeactivateBehaviour_Client_2681120339(((NetworkBehaviour)this).LocalConnection, behaviourIndex);
		}
	}

	private void RpcWriter___Server_PauseBehaviour_Server_3316948804(int behaviourIndex)
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
			((Writer)writer).WriteInt32(behaviourIndex, (AutoPackType)1);
			((NetworkBehaviour)this).SendServerRpc(14u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___PauseBehaviour_Server_3316948804(int behaviourIndex)
	{
		PauseBehaviour_Client(null, behaviourIndex);
	}

	private void RpcReader___Server_PauseBehaviour_Server_3316948804(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int behaviourIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___PauseBehaviour_Server_3316948804(behaviourIndex);
		}
	}

	private void RpcWriter___Observers_PauseBehaviour_Client_2681120339(NetworkConnection conn, int behaviourIndex)
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
			((Writer)writer).WriteInt32(behaviourIndex, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(15u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___PauseBehaviour_Client_2681120339(NetworkConnection conn, int behaviourIndex)
	{
		behaviourStack[behaviourIndex].Pause();
	}

	private void RpcReader___Observers_PauseBehaviour_Client_2681120339(PooledReader PooledReader0, Channel channel)
	{
		int behaviourIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___PauseBehaviour_Client_2681120339(null, behaviourIndex);
		}
	}

	private void RpcWriter___Target_PauseBehaviour_Client_2681120339(NetworkConnection conn, int behaviourIndex)
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
			((Writer)writer).WriteInt32(behaviourIndex, (AutoPackType)1);
			((NetworkBehaviour)this).SendTargetRpc(16u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_PauseBehaviour_Client_2681120339(PooledReader PooledReader0, Channel channel)
	{
		int behaviourIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___PauseBehaviour_Client_2681120339(((NetworkBehaviour)this).LocalConnection, behaviourIndex);
		}
	}

	private void RpcWriter___Server_ResumeBehaviour_Server_3316948804(int behaviourIndex)
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
			((Writer)writer).WriteInt32(behaviourIndex, (AutoPackType)1);
			((NetworkBehaviour)this).SendServerRpc(17u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___ResumeBehaviour_Server_3316948804(int behaviourIndex)
	{
		ResumeBehaviour_Client(null, behaviourIndex);
	}

	private void RpcReader___Server_ResumeBehaviour_Server_3316948804(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int behaviourIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___ResumeBehaviour_Server_3316948804(behaviourIndex);
		}
	}

	private void RpcWriter___Observers_ResumeBehaviour_Client_2681120339(NetworkConnection conn, int behaviourIndex)
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
			((Writer)writer).WriteInt32(behaviourIndex, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(18u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___ResumeBehaviour_Client_2681120339(NetworkConnection conn, int behaviourIndex)
	{
		behaviourStack[behaviourIndex].Resume();
	}

	private void RpcReader___Observers_ResumeBehaviour_Client_2681120339(PooledReader PooledReader0, Channel channel)
	{
		int behaviourIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___ResumeBehaviour_Client_2681120339(null, behaviourIndex);
		}
	}

	private void RpcWriter___Target_ResumeBehaviour_Client_2681120339(NetworkConnection conn, int behaviourIndex)
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
			((Writer)writer).WriteInt32(behaviourIndex, (AutoPackType)1);
			((NetworkBehaviour)this).SendTargetRpc(19u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_ResumeBehaviour_Client_2681120339(PooledReader PooledReader0, Channel channel)
	{
		int behaviourIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ResumeBehaviour_Client_2681120339(((NetworkBehaviour)this).LocalConnection, behaviourIndex);
		}
	}

	protected virtual void Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002ENPCBehaviour_Assembly_002DCSharp_002Edll()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		Npc = ((Component)this).GetComponentInParent<NPC>();
		Npc.Health.onKnockedOut.AddListener(new UnityAction(OnKnockOut));
		Npc.Health.onDie.AddListener(new UnityAction(OnDie));
		Npc.Health.onRevive.AddListener(new UnityAction(OnRevive));
		for (int i = 0; i < behaviourStack.Count; i++)
		{
			behaviourStack[i].BehaviourIndex = i;
		}
	}
}
