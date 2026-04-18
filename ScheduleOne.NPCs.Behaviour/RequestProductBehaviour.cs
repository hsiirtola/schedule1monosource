using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Dialogue;
using ScheduleOne.Economy;
using ScheduleOne.GameTime;
using ScheduleOne.ItemFramework;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Product;
using ScheduleOne.Quests;
using ScheduleOne.UI;
using ScheduleOne.UI.Handover;
using ScheduleOne.VoiceOver;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace ScheduleOne.NPCs.Behaviour;

public class RequestProductBehaviour : Behaviour
{
	public enum EState
	{
		InitialApproach,
		FollowPlayer
	}

	public const float CONVERSATION_RANGE = 2.5f;

	public const float FOLLOW_MAX_RANGE = 5f;

	public const int TicksBeforeAskAgain = 180;

	private int ticksSinceLastRequest;

	private DialogueController.GreetingOverride requestGreeting;

	private DialogueController.DialogueChoice acceptRequestChoice;

	private DialogueController.DialogueChoice followChoice;

	private DialogueController.DialogueChoice rejectChoice;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ERequestProductBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ERequestProductBehaviourAssembly_002DCSharp_002Edll_Excuted;

	public Player TargetPlayer { get; private set; }

	public EState State { get; private set; }

	private Customer customer => ((Component)base.Npc).GetComponent<Customer>();

	[ObserversRpc(RunLocally = true)]
	public void AssignTarget(NetworkObject plr)
	{
		RpcWriter___Observers_AssignTarget_3323014238(plr);
		RpcLogic___AssignTarget_3323014238(plr);
	}

	protected virtual void Start()
	{
		SetUpDialogue();
	}

	public override void Activate()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		base.Activate();
		State = EState.InitialApproach;
		requestGreeting.Greeting = base.Npc.DialogueHandler.Database.GetLine(EDialogueModule.Customer, "request_product_initial");
		if (InstanceFinder.IsServer)
		{
			Transform target = NetworkSingleton<NPCManager>.Instance.GetOrderedDistanceWarpPoints(((Component)TargetPlayer).transform.position)[1];
			base.Npc.Movement.Warp(target);
			if (base.Npc.isInBuilding)
			{
				base.Npc.ExitBuilding();
			}
			base.Npc.Movement.SetAgentType(NPCMovement.EAgentType.IgnoreCosts);
			base.Npc.Movement.SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("requestproduct", 5, 0.4f));
		}
		requestGreeting.ShouldShow = (Object)(object)TargetPlayer != (Object)null && ((NetworkBehaviour)TargetPlayer).Owner.IsLocalClient;
	}

	public override void Deactivate()
	{
		base.Deactivate();
		if (requestGreeting != null)
		{
			requestGreeting.ShouldShow = false;
		}
		if (InstanceFinder.IsServer)
		{
			base.Npc.Movement.SetAgentType(NPCMovement.EAgentType.Humanoid);
			base.Npc.Movement.SpeedController.RemoveSpeedControl("requestproduct");
		}
	}

	public override void Disable()
	{
		base.Disable();
		Deactivate();
	}

	public override void OnActiveTick()
	{
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		base.OnActiveTick();
		if (base.Npc.DialogueHandler.IsDialogueInProgress)
		{
			ticksSinceLastRequest = 0;
		}
		ticksSinceLastRequest++;
		if ((Object)(object)TargetPlayer == (Object)null)
		{
			return;
		}
		if (((NetworkBehaviour)TargetPlayer).Owner.IsLocalClient)
		{
			if (State == EState.InitialApproach && CanStartDialogue())
			{
				SendStartInitialDialogue();
			}
			if (State == EState.FollowPlayer && ticksSinceLastRequest >= 180 && CanStartDialogue())
			{
				ticksSinceLastRequest = 0;
				SendStartFollowUpDialogue();
			}
		}
		if (!InstanceFinder.IsServer || (Object)(object)Singleton<HandoverScreen>.Instance.CurrentCustomer == (Object)(object)customer)
		{
			return;
		}
		if (!IsTargetValid(TargetPlayer))
		{
			Disable_Server();
		}
		else if (State == EState.InitialApproach)
		{
			if (!IsTargetDestinationValid())
			{
				if (GetNewDestination(out var dest))
				{
					base.Npc.Movement.SetDestination(dest);
				}
				else
				{
					Disable_Server();
				}
			}
		}
		else if (State == EState.FollowPlayer && !IsTargetDestinationValid())
		{
			if (GetNewDestination(out var dest2))
			{
				base.Npc.Movement.SetDestination(dest2);
			}
			else
			{
				Disable_Server();
			}
		}
	}

	private bool IsTargetDestinationValid()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (!base.Npc.Movement.IsMoving)
		{
			return false;
		}
		if (Vector3.Distance(base.Npc.Movement.CurrentDestination, ((Component)TargetPlayer).transform.position) > ((State == EState.InitialApproach) ? 2.5f : 5f))
		{
			return false;
		}
		if (base.Npc.Movement.Agent.path == null)
		{
			return false;
		}
		return true;
	}

	private bool GetNewDestination(out Vector3 dest)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		dest = ((Component)TargetPlayer).transform.position;
		if (State == EState.InitialApproach)
		{
			dest += ((Component)TargetPlayer).transform.forward * 1.5f;
		}
		else if (State == EState.InitialApproach)
		{
			Vector3 val = dest;
			Vector3 val2 = ((Component)base.Npc).transform.position - ((Component)TargetPlayer).transform.position;
			dest = val + ((Vector3)(ref val2)).normalized * 2.5f;
		}
		if (NavMeshUtility.SamplePosition(dest, out var hit, 15f, -1))
		{
			dest = ((NavMeshHit)(ref hit)).position;
			return true;
		}
		Console.LogError("Failed to find valid destination for RequestProductBehaviour: stopping");
		return false;
	}

	public static bool IsTargetValid(Player player)
	{
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		if (player.IsArrested)
		{
			return false;
		}
		if (!player.Health.IsAlive)
		{
			return false;
		}
		if (player.CrimeData.CurrentPursuitLevel != PlayerCrimeData.EPursuitLevel.None)
		{
			return false;
		}
		if (player.CrimeData.BodySearchPending)
		{
			return false;
		}
		if (player.IsSleeping)
		{
			return false;
		}
		return true;
	}

	public bool CanStartDialogue()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		if (!IsTargetValid(TargetPlayer))
		{
			return false;
		}
		if (!((NetworkBehaviour)TargetPlayer).Owner.IsLocalClient)
		{
			return false;
		}
		if (Singleton<DialogueCanvas>.Instance.isActive)
		{
			return false;
		}
		if (Vector3.Distance(((Component)base.Npc).transform.position, ((Component)TargetPlayer).transform.position) > 2.5f)
		{
			return false;
		}
		if (Singleton<HandoverScreen>.Instance.IsOpen)
		{
			return false;
		}
		if (PlayerSingleton<PlayerCamera>.Instance.activeUIElementCount > 0)
		{
			return false;
		}
		return true;
	}

	private void SetUpDialogue()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Expected O, but got Unknown
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Expected O, but got Unknown
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Expected O, but got Unknown
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Expected O, but got Unknown
		if (requestGreeting == null)
		{
			acceptRequestChoice = new DialogueController.DialogueChoice();
			acceptRequestChoice.ChoiceText = "[Make an offer]";
			acceptRequestChoice.Enabled = true;
			acceptRequestChoice.Conversation = null;
			acceptRequestChoice.onChoosen = new UnityEvent();
			acceptRequestChoice.onChoosen.AddListener(new UnityAction(RequestAccepted));
			acceptRequestChoice.shouldShowCheck = DialogueActive;
			((Component)base.Npc.DialogueHandler).GetComponent<DialogueController>().AddDialogueChoice(acceptRequestChoice);
			followChoice = new DialogueController.DialogueChoice();
			followChoice.ChoiceText = "Follow me, I need to grab it first";
			followChoice.Enabled = true;
			followChoice.Conversation = null;
			followChoice.onChoosen = new UnityEvent();
			followChoice.onChoosen.AddListener(new UnityAction(Follow));
			followChoice.shouldShowCheck = DialogueActive;
			((Component)base.Npc.DialogueHandler).GetComponent<DialogueController>().AddDialogueChoice(followChoice);
			rejectChoice = new DialogueController.DialogueChoice();
			rejectChoice.ChoiceText = "Get out of here";
			rejectChoice.Enabled = true;
			rejectChoice.Conversation = null;
			rejectChoice.onChoosen = new UnityEvent();
			rejectChoice.onChoosen.AddListener(new UnityAction(RequestRejected));
			rejectChoice.shouldShowCheck = DialogueActive;
			((Component)base.Npc.DialogueHandler).GetComponent<DialogueController>().AddDialogueChoice(rejectChoice);
			requestGreeting = new DialogueController.GreetingOverride();
			requestGreeting.Greeting = base.Npc.DialogueHandler.Database.GetLine(EDialogueModule.Customer, "request_product_initial");
			requestGreeting.ShouldShow = false;
			requestGreeting.PlayVO = true;
			requestGreeting.VOType = EVOLineType.Question;
			((Component)base.Npc.DialogueHandler).GetComponent<DialogueController>().AddGreetingOverride(requestGreeting);
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	private void SendStartInitialDialogue()
	{
		RpcWriter___Server_SendStartInitialDialogue_2166136261();
		RpcLogic___SendStartInitialDialogue_2166136261();
	}

	[ObserversRpc(RunLocally = true)]
	private void StartInitialDialogue()
	{
		RpcWriter___Observers_StartInitialDialogue_2166136261();
		RpcLogic___StartInitialDialogue_2166136261();
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	private void SendStartFollowUpDialogue()
	{
		RpcWriter___Server_SendStartFollowUpDialogue_2166136261();
		RpcLogic___SendStartFollowUpDialogue_2166136261();
	}

	[ObserversRpc(RunLocally = true)]
	private void StartFollowUpDialogue()
	{
		RpcWriter___Observers_StartFollowUpDialogue_2166136261();
		RpcLogic___StartFollowUpDialogue_2166136261();
	}

	private bool DialogueActive(bool enabled)
	{
		if (base.Active && (Object)(object)TargetPlayer != (Object)null)
		{
			return ((NetworkBehaviour)TargetPlayer).Owner.IsLocalClient;
		}
		return false;
	}

	private void RequestAccepted()
	{
		ticksSinceLastRequest = 0;
		Singleton<HandoverScreen>.Instance.Open(null, customer, HandoverScreen.EMode.Offer, HandoverClosed, customer.GetOfferSuccessChance);
	}

	private void HandoverClosed(HandoverScreen.EHandoverOutcome outcome, List<ItemInstance> items, float askingPrice)
	{
		if (outcome == HandoverScreen.EHandoverOutcome.Cancelled)
		{
			Singleton<DialogueCanvas>.Instance.SkipNextRollout = true;
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Wait());
			return;
		}
		float offerSuccessChance = customer.GetOfferSuccessChance(items, askingPrice);
		if (Random.value < offerSuccessChance)
		{
			Contract contract = new Contract();
			ProductList productList = new ProductList();
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i] is ProductItemInstance)
				{
					productList.entries.Add(new ProductList.Entry(((BaseItemInstance)items[i]).ID, customer.CustomerData.Standards.GetCorrespondingQuality(), (items[i] as ProductItemInstance).Amount * ((BaseItemInstance)items[i]).Quantity));
				}
			}
			contract.SilentlyInitializeContract("Offer", string.Empty, null, string.Empty, ((Component)base.Npc).GetComponent<Customer>(), askingPrice, productList, string.Empty, new QuestWindowConfig(), 0, NetworkSingleton<TimeManager>.Instance.GetDateTime());
			customer.ProcessHandover(HandoverScreen.EHandoverOutcome.Finalize, contract, items, handoverByPlayer: true, giveBonuses: false);
		}
		else
		{
			Singleton<HandoverScreen>.Instance.ClearCustomerSlots(returnToOriginals: true);
			customer.RejectProductRequestOffer();
		}
		Disable_Server();
		IEnumerator Wait()
		{
			yield return (object)new WaitForEndOfFrame();
			StartInitialDialogue();
		}
	}

	private void Follow()
	{
		ticksSinceLastRequest = 0;
		State = EState.FollowPlayer;
		base.Npc.Movement.SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("requestproduct", 5, 0.6f));
		requestGreeting.Greeting = base.Npc.DialogueHandler.Database.GetLine(EDialogueModule.Customer, "request_product_after_follow");
		base.Npc.DialogueHandler.ShowWorldspaceDialogue("Ok...", 3f);
	}

	private void RequestRejected()
	{
		ticksSinceLastRequest = 0;
		customer.PlayerRejectedProductRequest();
		Disable_Server();
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
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ERequestProductBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ERequestProductBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterObserversRpc(0u, new ClientRpcDelegate(RpcReader___Observers_AssignTarget_3323014238));
			((NetworkBehaviour)this).RegisterServerRpc(1u, new ServerRpcDelegate(RpcReader___Server_SendStartInitialDialogue_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(2u, new ClientRpcDelegate(RpcReader___Observers_StartInitialDialogue_2166136261));
			((NetworkBehaviour)this).RegisterServerRpc(3u, new ServerRpcDelegate(RpcReader___Server_SendStartFollowUpDialogue_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(4u, new ClientRpcDelegate(RpcReader___Observers_StartFollowUpDialogue_2166136261));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ERequestProductBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ERequestProductBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_AssignTarget_3323014238(NetworkObject plr)
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
			((Writer)writer).WriteNetworkObject(plr);
			((NetworkBehaviour)this).SendObserversRpc(0u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___AssignTarget_3323014238(NetworkObject plr)
	{
		TargetPlayer = (((Object)(object)plr != (Object)null) ? ((Component)plr).GetComponent<Player>() : null);
	}

	private void RpcReader___Observers_AssignTarget_3323014238(PooledReader PooledReader0, Channel channel)
	{
		NetworkObject plr = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___AssignTarget_3323014238(plr);
		}
	}

	private void RpcWriter___Server_SendStartInitialDialogue_2166136261()
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
			((NetworkBehaviour)this).SendServerRpc(1u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___SendStartInitialDialogue_2166136261()
	{
		StartInitialDialogue();
	}

	private void RpcReader___Server_SendStartInitialDialogue_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendStartInitialDialogue_2166136261();
		}
	}

	private void RpcWriter___Observers_StartInitialDialogue_2166136261()
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
			((NetworkBehaviour)this).SendObserversRpc(2u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___StartInitialDialogue_2166136261()
	{
		if ((Object)(object)TargetPlayer != (Object)null && ((NetworkBehaviour)TargetPlayer).IsOwner && !base.Npc.DialogueHandler.IsDialogueInProgress)
		{
			if (PlayerSingleton<PlayerCamera>.Instance.activeUIElementCount > 0)
			{
				Singleton<GameInput>.Instance.ExitAll();
			}
			((Component)base.Npc.DialogueHandler).GetComponent<DialogueController>().StartGenericDialogue(allowExit: false);
		}
	}

	private void RpcReader___Observers_StartInitialDialogue_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___StartInitialDialogue_2166136261();
		}
	}

	private void RpcWriter___Server_SendStartFollowUpDialogue_2166136261()
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

	private void RpcLogic___SendStartFollowUpDialogue_2166136261()
	{
		StartFollowUpDialogue();
	}

	private void RpcReader___Server_SendStartFollowUpDialogue_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendStartFollowUpDialogue_2166136261();
		}
	}

	private void RpcWriter___Observers_StartFollowUpDialogue_2166136261()
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

	private void RpcLogic___StartFollowUpDialogue_2166136261()
	{
		if ((Object)(object)TargetPlayer != (Object)null && ((NetworkBehaviour)TargetPlayer).IsOwner && !base.Npc.DialogueHandler.IsDialogueInProgress)
		{
			if (PlayerSingleton<PlayerCamera>.Instance.activeUIElementCount > 0)
			{
				Singleton<GameInput>.Instance.ExitAll();
			}
			((Component)base.Npc.DialogueHandler).GetComponent<DialogueController>().StartGenericDialogue(allowExit: false);
		}
	}

	private void RpcReader___Observers_StartFollowUpDialogue_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___StartFollowUpDialogue_2166136261();
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
