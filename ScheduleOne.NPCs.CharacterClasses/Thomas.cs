using System;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Cartel;
using ScheduleOne.DevUtilities;
using ScheduleOne.Dialogue;
using ScheduleOne.Messaging;
using ScheduleOne.UI.Phone.Messages;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.NPCs.CharacterClasses;

public class Thomas : NPC
{
	public Sprite MessagingIcon;

	public UnityEvent onMeetingEnded;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002EThomasAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002EThomasAssembly_002DCSharp_002Edll_Excuted;

	public override Sprite GetMessagingIcon()
	{
		return MessagingIcon;
	}

	public void SendIntroMessage()
	{
		if (base.MSGConversation.messageHistory.Count <= 0)
		{
			base.MSGConversation.SetIsKnown(known: false);
			if (InstanceFinder.IsServer)
			{
				MessageChain messageChain = DialogueHandler.Database.GetChain(EDialogueModule.Generic, "thomas_intro").GetMessageChain();
				base.MSGConversation.SendMessageChain(messageChain);
			}
		}
	}

	[ServerRpc(RequireOwnership = false)]
	public void MeetingEnded_Server()
	{
		RpcWriter___Server_MeetingEnded_Server_2166136261();
	}

	[ObserversRpc]
	private void MeetingEnded()
	{
		RpcWriter___Observers_MeetingEnded_2166136261();
	}

	protected override void CreateMessageConversation()
	{
		base.CreateMessageConversation();
		SendableMessage sendableMessage = base.MSGConversation.CreateSendableMessage("We're not working together any more - our agreement is off.");
		sendableMessage.ShouldShowCheck = ShowCancelAgreement;
		sendableMessage.disableDefaultSendBehaviour = true;
		sendableMessage.onSelected = (Action)Delegate.Combine(sendableMessage.onSelected, new Action(ConfirmCancelAgreement));
		void CancelAgreementCallback(ConfirmationPopup.EResponse response)
		{
			if (response == ConfirmationPopup.EResponse.Confirm)
			{
				string text = "We're not working together any more - our agreement is off.";
				base.MSGConversation.SendMessage(new Message(text, Message.ESenderType.Player, _endOfGroup: true));
				CancelAgreement_Server();
			}
		}
		void ConfirmCancelAgreement()
		{
			PlayerSingleton<MessagesApp>.Instance.ConfirmationPopup.Open("Are you sure?", "Calling off the agreement with the Benzies is irreversible.\n\nThey will immediately become hostile.", base.MSGConversation, CancelAgreementCallback);
		}
		static bool ShowCancelAgreement(SendableMessage msg)
		{
			return NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance.Status == ECartelStatus.Truced;
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void CancelAgreement_Server()
	{
		RpcWriter___Server_CancelAgreement_Server_2166136261();
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002EThomasAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002EThomasAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterServerRpc(39u, new ServerRpcDelegate(RpcReader___Server_MeetingEnded_Server_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(40u, new ClientRpcDelegate(RpcReader___Observers_MeetingEnded_2166136261));
			((NetworkBehaviour)this).RegisterServerRpc(41u, new ServerRpcDelegate(RpcReader___Server_CancelAgreement_Server_2166136261));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002EThomasAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002EThomasAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_MeetingEnded_Server_2166136261()
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
			((NetworkBehaviour)this).SendServerRpc(39u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___MeetingEnded_Server_2166136261()
	{
		MeetingEnded();
	}

	private void RpcReader___Server_MeetingEnded_Server_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___MeetingEnded_Server_2166136261();
		}
	}

	private void RpcWriter___Observers_MeetingEnded_2166136261()
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
			((NetworkBehaviour)this).SendObserversRpc(40u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___MeetingEnded_2166136261()
	{
		if (onMeetingEnded != null)
		{
			onMeetingEnded.Invoke();
		}
	}

	private void RpcReader___Observers_MeetingEnded_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___MeetingEnded_2166136261();
		}
	}

	private void RpcWriter___Server_CancelAgreement_Server_2166136261()
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
			((NetworkBehaviour)this).SendServerRpc(41u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___CancelAgreement_Server_2166136261()
	{
		NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance.SetStatus(null, ECartelStatus.Hostile, resetStatusChangeTimer: true);
		MessageChain messageChain = DialogueHandler.Database.GetChain(EDialogueModule.Generic, "thomas_acknowledge_cancel_agreement").GetMessageChain();
		base.MSGConversation.SendMessageChain(messageChain, 1f, notify: false);
	}

	private void RpcReader___Server_CancelAgreement_Server_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___CancelAgreement_Server_2166136261();
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
