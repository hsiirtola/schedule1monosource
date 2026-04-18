using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using UnityEngine;

namespace ScheduleOne.Casino;

public class CardController : NetworkBehaviour
{
	private List<PlayingCard> cards = new List<PlayingCard>();

	private Dictionary<string, PlayingCard> cardDictionary = new Dictionary<string, PlayingCard>();

	private bool NetworkInitialize___EarlyScheduleOne_002ECasino_002ECardControllerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ECasino_002ECardControllerAssembly_002DCSharp_002Edll_Excuted;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ECasino_002ECardController_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SendCardValue(string cardId, PlayingCard.ECardSuit suit, PlayingCard.ECardValue value)
	{
		RpcWriter___Server_SendCardValue_3709737967(cardId, suit, value);
		RpcLogic___SendCardValue_3709737967(cardId, suit, value);
	}

	[ObserversRpc(RunLocally = true)]
	private void SetCardValue(string cardId, PlayingCard.ECardSuit suit, PlayingCard.ECardValue value)
	{
		RpcWriter___Observers_SetCardValue_3709737967(cardId, suit, value);
		RpcLogic___SetCardValue_3709737967(cardId, suit, value);
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SendCardFaceUp(string cardId, bool faceUp)
	{
		RpcWriter___Server_SendCardFaceUp_310431262(cardId, faceUp);
		RpcLogic___SendCardFaceUp_310431262(cardId, faceUp);
	}

	[ObserversRpc(RunLocally = true)]
	private void SetCardFaceUp(string cardId, bool faceUp)
	{
		RpcWriter___Observers_SetCardFaceUp_310431262(cardId, faceUp);
		RpcLogic___SetCardFaceUp_310431262(cardId, faceUp);
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SendCardGlide(string cardId, Vector3 position, Quaternion rotation, float glideTime)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		RpcWriter___Server_SendCardGlide_2833372058(cardId, position, rotation, glideTime);
		RpcLogic___SendCardGlide_2833372058(cardId, position, rotation, glideTime);
	}

	[ObserversRpc(RunLocally = true)]
	private void SetCardGlide(string cardId, Vector3 position, Quaternion rotation, float glideTime)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		RpcWriter___Observers_SetCardGlide_2833372058(cardId, position, rotation, glideTime);
		RpcLogic___SetCardGlide_2833372058(cardId, position, rotation, glideTime);
	}

	private PlayingCard GetCard(string cardId)
	{
		return cardDictionary[cardId];
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
		if (!NetworkInitialize___EarlyScheduleOne_002ECasino_002ECardControllerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ECasino_002ECardControllerAssembly_002DCSharp_002Edll_Excuted = true;
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_SendCardValue_3709737967));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_SetCardValue_3709737967));
			((NetworkBehaviour)this).RegisterServerRpc(2u, new ServerRpcDelegate(RpcReader___Server_SendCardFaceUp_310431262));
			((NetworkBehaviour)this).RegisterObserversRpc(3u, new ClientRpcDelegate(RpcReader___Observers_SetCardFaceUp_310431262));
			((NetworkBehaviour)this).RegisterServerRpc(4u, new ServerRpcDelegate(RpcReader___Server_SendCardGlide_2833372058));
			((NetworkBehaviour)this).RegisterObserversRpc(5u, new ClientRpcDelegate(RpcReader___Observers_SetCardGlide_2833372058));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ECasino_002ECardControllerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ECasino_002ECardControllerAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_SendCardValue_3709737967(string cardId, PlayingCard.ECardSuit suit, PlayingCard.ECardValue value)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(cardId);
			GeneratedWriters___Internal.Write___ScheduleOne_002ECasino_002EPlayingCard_002FECardSuitFishNet_002ESerializing_002EGenerated((Writer)(object)writer, suit);
			GeneratedWriters___Internal.Write___ScheduleOne_002ECasino_002EPlayingCard_002FECardValueFishNet_002ESerializing_002EGenerated((Writer)(object)writer, value);
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendCardValue_3709737967(string cardId, PlayingCard.ECardSuit suit, PlayingCard.ECardValue value)
	{
		SetCardValue(cardId, suit, value);
	}

	private void RpcReader___Server_SendCardValue_3709737967(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string cardId = ((Reader)PooledReader0).ReadString();
		PlayingCard.ECardSuit suit = GeneratedReaders___Internal.Read___ScheduleOne_002ECasino_002EPlayingCard_002FECardSuitFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		PlayingCard.ECardValue value = GeneratedReaders___Internal.Read___ScheduleOne_002ECasino_002EPlayingCard_002FECardValueFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendCardValue_3709737967(cardId, suit, value);
		}
	}

	private void RpcWriter___Observers_SetCardValue_3709737967(string cardId, PlayingCard.ECardSuit suit, PlayingCard.ECardValue value)
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
			((Writer)writer).WriteString(cardId);
			GeneratedWriters___Internal.Write___ScheduleOne_002ECasino_002EPlayingCard_002FECardSuitFishNet_002ESerializing_002EGenerated((Writer)(object)writer, suit);
			GeneratedWriters___Internal.Write___ScheduleOne_002ECasino_002EPlayingCard_002FECardValueFishNet_002ESerializing_002EGenerated((Writer)(object)writer, value);
			((NetworkBehaviour)this).SendObserversRpc(1u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetCardValue_3709737967(string cardId, PlayingCard.ECardSuit suit, PlayingCard.ECardValue value)
	{
		PlayingCard card = GetCard(cardId);
		if ((Object)(object)card != (Object)null)
		{
			card.SetCard(suit, value, network: false);
		}
	}

	private void RpcReader___Observers_SetCardValue_3709737967(PooledReader PooledReader0, Channel channel)
	{
		string cardId = ((Reader)PooledReader0).ReadString();
		PlayingCard.ECardSuit suit = GeneratedReaders___Internal.Read___ScheduleOne_002ECasino_002EPlayingCard_002FECardSuitFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		PlayingCard.ECardValue value = GeneratedReaders___Internal.Read___ScheduleOne_002ECasino_002EPlayingCard_002FECardValueFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetCardValue_3709737967(cardId, suit, value);
		}
	}

	private void RpcWriter___Server_SendCardFaceUp_310431262(string cardId, bool faceUp)
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
			((Writer)writer).WriteString(cardId);
			((Writer)writer).WriteBoolean(faceUp);
			((NetworkBehaviour)this).SendServerRpc(2u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendCardFaceUp_310431262(string cardId, bool faceUp)
	{
		SetCardFaceUp(cardId, faceUp);
	}

	private void RpcReader___Server_SendCardFaceUp_310431262(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string cardId = ((Reader)PooledReader0).ReadString();
		bool faceUp = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendCardFaceUp_310431262(cardId, faceUp);
		}
	}

	private void RpcWriter___Observers_SetCardFaceUp_310431262(string cardId, bool faceUp)
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
			((Writer)writer).WriteString(cardId);
			((Writer)writer).WriteBoolean(faceUp);
			((NetworkBehaviour)this).SendObserversRpc(3u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetCardFaceUp_310431262(string cardId, bool faceUp)
	{
		PlayingCard card = GetCard(cardId);
		if ((Object)(object)card != (Object)null)
		{
			card.SetFaceUp(faceUp, network: false);
		}
	}

	private void RpcReader___Observers_SetCardFaceUp_310431262(PooledReader PooledReader0, Channel channel)
	{
		string cardId = ((Reader)PooledReader0).ReadString();
		bool faceUp = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetCardFaceUp_310431262(cardId, faceUp);
		}
	}

	private void RpcWriter___Server_SendCardGlide_2833372058(string cardId, Vector3 position, Quaternion rotation, float glideTime)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(cardId);
			((Writer)writer).WriteVector3(position);
			((Writer)writer).WriteQuaternion(rotation, (AutoPackType)1);
			((Writer)writer).WriteSingle(glideTime, (AutoPackType)0);
			((NetworkBehaviour)this).SendServerRpc(4u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendCardGlide_2833372058(string cardId, Vector3 position, Quaternion rotation, float glideTime)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		SetCardGlide(cardId, position, rotation, glideTime);
	}

	private void RpcReader___Server_SendCardGlide_2833372058(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		string cardId = ((Reader)PooledReader0).ReadString();
		Vector3 position = ((Reader)PooledReader0).ReadVector3();
		Quaternion rotation = ((Reader)PooledReader0).ReadQuaternion((AutoPackType)1);
		float glideTime = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendCardGlide_2833372058(cardId, position, rotation, glideTime);
		}
	}

	private void RpcWriter___Observers_SetCardGlide_2833372058(string cardId, Vector3 position, Quaternion rotation, float glideTime)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(cardId);
			((Writer)writer).WriteVector3(position);
			((Writer)writer).WriteQuaternion(rotation, (AutoPackType)1);
			((Writer)writer).WriteSingle(glideTime, (AutoPackType)0);
			((NetworkBehaviour)this).SendObserversRpc(5u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetCardGlide_2833372058(string cardId, Vector3 position, Quaternion rotation, float glideTime)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		PlayingCard card = GetCard(cardId);
		if ((Object)(object)card != (Object)null)
		{
			card.GlideTo(position, rotation, glideTime, network: false);
		}
	}

	private void RpcReader___Observers_SetCardGlide_2833372058(PooledReader PooledReader0, Channel channel)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		string cardId = ((Reader)PooledReader0).ReadString();
		Vector3 position = ((Reader)PooledReader0).ReadVector3();
		Quaternion rotation = ((Reader)PooledReader0).ReadQuaternion((AutoPackType)1);
		float glideTime = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetCardGlide_2833372058(cardId, position, rotation, glideTime);
		}
	}

	private void Awake_UserLogic_ScheduleOne_002ECasino_002ECardController_Assembly_002DCSharp_002Edll()
	{
		cards = new List<PlayingCard>(((Component)this).GetComponentsInChildren<PlayingCard>());
		foreach (PlayingCard card in cards)
		{
			card.SetCardController(this);
			if (cardDictionary.ContainsKey(card.CardID))
			{
				Debug.LogError((object)("Card ID " + card.CardID + " already exists in the dictionary."));
			}
			else
			{
				cardDictionary.Add(card.CardID, card);
			}
		}
	}
}
