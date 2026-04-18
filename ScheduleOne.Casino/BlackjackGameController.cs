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
using ScheduleOne.Casino.UI;
using ScheduleOne.DevUtilities;
using ScheduleOne.Money;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Casino;

public class BlackjackGameController : CasinoGameController
{
	public enum EStage
	{
		WaitingForPlayers,
		Dealing,
		PlayerTurn,
		DealerTurn,
		Ending
	}

	public enum EPayoutType
	{
		None,
		Blackjack,
		Win,
		Push
	}

	public const int BET_MINIMUM = 10;

	public const int BET_MAXIMUM = 1000;

	public const float PAYOUT_RATIO = 1f;

	public const float BLACKJACK_PAYOUT_RATIO = 1.5f;

	[Header("References")]
	public PlayingCard[] Cards;

	public Transform[] DefaultCardPositions;

	public Transform[] FocusedCameraTransforms;

	public Transform[] FinalCameraTransforms;

	public Transform[] Player1CardPositions;

	public Transform[] Player2CardPositions;

	public Transform[] Player3CardPositions;

	public Transform[] Player4CardPositions;

	public Transform[] DealerCardPositions;

	private List<Player> playersInCurrentRound = new List<Player>();

	private List<PlayingCard> playStack = new List<PlayingCard>();

	private List<PlayingCard> player1Hand = new List<PlayingCard>();

	private List<PlayingCard> player2Hand = new List<PlayingCard>();

	private List<PlayingCard> player3Hand = new List<PlayingCard>();

	private List<PlayingCard> player4Hand = new List<PlayingCard>();

	private List<PlayingCard> dealerHand = new List<PlayingCard>();

	private List<PlayingCard.CardData> cardValuesInDeck = new List<PlayingCard.CardData>();

	private List<PlayingCard.CardData> drawnCardsValues = new List<PlayingCard.CardData>();

	protected Transform localFocusCameraTransform;

	protected Transform localFinalCameraTransform;

	public Action onLocalPlayerBetChange;

	public Action onLocalPlayerExitRound;

	public Action onInitialCardsDealt;

	public Action onLocalPlayerReadyForInput;

	public Action onLocalPlayerBust;

	public Action<EPayoutType> onLocalPlayerRoundCompleted;

	private bool roundEnded;

	private Coroutine gameRoutine;

	private bool NetworkInitialize___EarlyScheduleOne_002ECasino_002EBlackjackGameControllerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ECasino_002EBlackjackGameControllerAssembly_002DCSharp_002Edll_Excuted;

	public EStage CurrentStage { get; private set; }

	public Player PlayerTurn { get; private set; }

	public float LocalPlayerBet { get; private set; } = 10f;

	public int DealerScore { get; private set; }

	public int LocalPlayerScore { get; private set; }

	public bool IsLocalPlayerBlackjack { get; private set; }

	public bool IsLocalPlayerBust { get; private set; }

	public bool IsLocalPlayerInCurrentRound => playersInCurrentRound.Contains(Player.Local);

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ECasino_002EBlackjackGameController_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	protected override void Open()
	{
		base.Open();
		Singleton<BlackjackInterface>.Instance.Open(this);
		localFocusCameraTransform = FocusedCameraTransforms[Players.GetPlayerIndex(Player.Local)];
		localFinalCameraTransform = FinalCameraTransforms[Players.GetPlayerIndex(Player.Local)];
	}

	protected override void Close()
	{
		if (IsLocalPlayerInCurrentRound)
		{
			RemoveLocalPlayerFromGame(EPayoutType.None);
		}
		Singleton<BlackjackInterface>.Instance.Close();
		base.Close();
	}

	protected override void Exit(ExitAction action)
	{
		if (!action.Used && base.IsOpen)
		{
			if (action.exitType == ExitType.Escape && IsLocalPlayerInCurrentRound)
			{
				action.Used = true;
				RemoveLocalPlayerFromGame(EPayoutType.None);
			}
			base.Exit(action);
		}
	}

	private List<Player> GetClockwisePlayers()
	{
		List<Player> list = new List<Player>();
		Player player = Players.GetPlayer(3);
		Player player2 = Players.GetPlayer(1);
		Player player3 = Players.GetPlayer(0);
		Player player4 = Players.GetPlayer(2);
		if ((Object)(object)player != (Object)null)
		{
			list.Add(player);
		}
		if ((Object)(object)player2 != (Object)null)
		{
			list.Add(player2);
		}
		if ((Object)(object)player3 != (Object)null)
		{
			list.Add(player3);
		}
		if ((Object)(object)player4 != (Object)null)
		{
			list.Add(player4);
		}
		return list;
	}

	[ObserversRpc(RunLocally = true)]
	private void StartGame()
	{
		RpcWriter___Observers_StartGame_2166136261();
		RpcLogic___StartGame_2166136261();
	}

	[ObserversRpc(RunLocally = true)]
	private void NotifyPlayerScore(NetworkObject player, int score, bool blackjack)
	{
		RpcWriter___Observers_NotifyPlayerScore_2864061566(player, score, blackjack);
		RpcLogic___NotifyPlayerScore_2864061566(player, score, blackjack);
	}

	private Transform[] GetPlayerCardPositions(int playerIndex)
	{
		return playerIndex switch
		{
			0 => Player1CardPositions, 
			1 => Player2CardPositions, 
			2 => Player3CardPositions, 
			3 => Player4CardPositions, 
			_ => null, 
		};
	}

	[ObserversRpc(RunLocally = true)]
	private void SetRoundEnded(bool ended)
	{
		RpcWriter___Observers_SetRoundEnded_1140765316(ended);
		RpcLogic___SetRoundEnded_1140765316(ended);
	}

	private void AddCardToPlayerHand(int playerIndex, PlayingCard card)
	{
		AddCardToPlayerHand(playerIndex, card.CardID);
	}

	[ObserversRpc(RunLocally = true)]
	private void AddCardToPlayerHand(int playerindex, string cardID)
	{
		RpcWriter___Observers_AddCardToPlayerHand_2801973956(playerindex, cardID);
		RpcLogic___AddCardToPlayerHand_2801973956(playerindex, cardID);
	}

	[ObserversRpc(RunLocally = true)]
	private void AddCardToDealerHand(string cardID)
	{
		RpcWriter___Observers_AddCardToDealerHand_3615296227(cardID);
		RpcLogic___AddCardToDealerHand_3615296227(cardID);
	}

	private List<PlayingCard> GetPlayerCards(int playerIndex)
	{
		return playerIndex switch
		{
			0 => player1Hand, 
			1 => player2Hand, 
			2 => player3Hand, 
			3 => player4Hand, 
			_ => null, 
		};
	}

	private int GetHandScore(List<PlayingCard> cards, bool countFaceDown = true)
	{
		int num = 0;
		foreach (PlayingCard card in cards)
		{
			if (countFaceDown || card.IsFaceUp)
			{
				num += GetCardValue(card);
			}
		}
		if (num > 21)
		{
			foreach (PlayingCard card2 in cards)
			{
				if (card2.Value == PlayingCard.ECardValue.Ace)
				{
					num -= 10;
				}
				if (num <= 21)
				{
					break;
				}
			}
		}
		return num;
	}

	private int GetCardValue(PlayingCard card, bool aceAsEleven = true)
	{
		if (card.Value == PlayingCard.ECardValue.Ace)
		{
			if (!aceAsEleven)
			{
				return 1;
			}
			return 11;
		}
		if (card.Value == PlayingCard.ECardValue.Jack || card.Value == PlayingCard.ECardValue.Queen || card.Value == PlayingCard.ECardValue.King)
		{
			return 10;
		}
		return (int)card.Value;
	}

	private PlayingCard DrawCard()
	{
		PlayingCard playingCard = playStack[0];
		playStack.RemoveAt(0);
		PlayingCard.CardData item = cardValuesInDeck[Random.Range(0, cardValuesInDeck.Count)];
		cardValuesInDeck.Remove(item);
		drawnCardsValues.Add(item);
		playingCard.SetCard(item.Suit, item.Value);
		return playingCard;
	}

	private void ResetCards()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (InstanceFinder.IsServer)
		{
			for (int i = 0; i < Cards.Length; i++)
			{
				Cards[i].SetFaceUp(faceUp: false);
				Cards[i].GlideTo(DefaultCardPositions[i].position, DefaultCardPositions[i].rotation);
			}
		}
		cardValuesInDeck = new List<PlayingCard.CardData>();
		for (int j = 0; j < 4; j++)
		{
			for (int k = 0; k < 13; k++)
			{
				PlayingCard.CardData item = new PlayingCard.CardData
				{
					Suit = (PlayingCard.ECardSuit)j,
					Value = (PlayingCard.ECardValue)(k + 1)
				};
				cardValuesInDeck.Add(item);
			}
		}
		playStack = new List<PlayingCard>();
		playStack.AddRange(Cards);
		player1Hand.Clear();
		player2Hand.Clear();
		player3Hand.Clear();
		player4Hand.Clear();
		dealerHand.Clear();
		drawnCardsValues.Clear();
	}

	[ObserversRpc(RunLocally = true)]
	private void EndGame()
	{
		RpcWriter___Observers_EndGame_2166136261();
		RpcLogic___EndGame_2166136261();
	}

	public void RemoveLocalPlayerFromGame(EPayoutType payout, float cameraDelay = 0f)
	{
		RequestRemovePlayerFromCurrentRound(((NetworkBehaviour)Player.Local).NetworkObject);
		Players.SetPlayerScore(Player.Local, 0);
		float payout2 = GetPayout(LocalPlayerBet, payout);
		if (payout2 > 0f)
		{
			NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(payout2);
		}
		if (onLocalPlayerRoundCompleted != null)
		{
			onLocalPlayerRoundCompleted(payout);
		}
		if (onLocalPlayerExitRound != null)
		{
			onLocalPlayerExitRound();
		}
		((MonoBehaviour)this).StartCoroutine(Wait());
		IEnumerator Wait()
		{
			yield return (object)new WaitForSeconds(cameraDelay);
			if (base.IsOpen && !IsLocalPlayerInCurrentRound)
			{
				PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(localDefaultCameraTransform.position, localDefaultCameraTransform.rotation, 0.5f);
			}
		}
	}

	public float GetPayout(float bet, EPayoutType payout)
	{
		return payout switch
		{
			EPayoutType.Blackjack => bet * 2.5f, 
			EPayoutType.Win => bet * 2f, 
			EPayoutType.Push => bet, 
			_ => 0f, 
		};
	}

	private bool IsCurrentRoundEmpty()
	{
		return playersInCurrentRound.Count == 0;
	}

	[ObserversRpc(RunLocally = true)]
	private void AddPlayerToCurrentRound(NetworkObject player)
	{
		RpcWriter___Observers_AddPlayerToCurrentRound_3323014238(player);
		RpcLogic___AddPlayerToCurrentRound_3323014238(player);
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	private void RequestRemovePlayerFromCurrentRound(NetworkObject player)
	{
		RpcWriter___Server_RequestRemovePlayerFromCurrentRound_3323014238(player);
		RpcLogic___RequestRemovePlayerFromCurrentRound_3323014238(player);
	}

	[ObserversRpc(RunLocally = true)]
	private void RemovePlayerFromCurrentRound(NetworkObject player)
	{
		RpcWriter___Observers_RemovePlayerFromCurrentRound_3323014238(player);
		RpcLogic___RemovePlayerFromCurrentRound_3323014238(player);
	}

	public void SetLocalPlayerBet(float bet)
	{
		if (base.IsOpen)
		{
			LocalPlayerBet = bet;
			if (onLocalPlayerBetChange != null)
			{
				onLocalPlayerBetChange();
			}
		}
	}

	public bool AreAllPlayersReady()
	{
		if (Players.CurrentPlayerCount == 0)
		{
			return false;
		}
		return GetPlayersReadyCount() == Players.CurrentPlayerCount;
	}

	public int GetPlayersReadyCount()
	{
		int num = 0;
		for (int i = 0; i < Players.CurrentPlayerCount; i++)
		{
			if (!((Object)(object)Players.GetPlayer(i) == (Object)null) && Players.GetPlayerData(i).GetData<bool>("Ready"))
			{
				num++;
			}
		}
		return num;
	}

	public void ToggleLocalPlayerReady()
	{
		bool data = base.LocalPlayerData.GetData<bool>("Ready");
		data = !data;
		base.LocalPlayerData.SetData("Ready", data);
		TryStartGame();
	}

	[ObserversRpc(RunLocally = true)]
	private void TryStartGame()
	{
		RpcWriter___Observers_TryStartGame_2166136261();
		RpcLogic___TryStartGame_2166136261();
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
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Expected O, but got Unknown
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Expected O, but got Unknown
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ECasino_002EBlackjackGameControllerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ECasino_002EBlackjackGameControllerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterObserversRpc(0u, new ClientRpcDelegate(RpcReader___Observers_StartGame_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_NotifyPlayerScore_2864061566));
			((NetworkBehaviour)this).RegisterObserversRpc(2u, new ClientRpcDelegate(RpcReader___Observers_SetRoundEnded_1140765316));
			((NetworkBehaviour)this).RegisterObserversRpc(3u, new ClientRpcDelegate(RpcReader___Observers_AddCardToPlayerHand_2801973956));
			((NetworkBehaviour)this).RegisterObserversRpc(4u, new ClientRpcDelegate(RpcReader___Observers_AddCardToDealerHand_3615296227));
			((NetworkBehaviour)this).RegisterObserversRpc(5u, new ClientRpcDelegate(RpcReader___Observers_EndGame_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(6u, new ClientRpcDelegate(RpcReader___Observers_AddPlayerToCurrentRound_3323014238));
			((NetworkBehaviour)this).RegisterServerRpc(7u, new ServerRpcDelegate(RpcReader___Server_RequestRemovePlayerFromCurrentRound_3323014238));
			((NetworkBehaviour)this).RegisterObserversRpc(8u, new ClientRpcDelegate(RpcReader___Observers_RemovePlayerFromCurrentRound_3323014238));
			((NetworkBehaviour)this).RegisterObserversRpc(9u, new ClientRpcDelegate(RpcReader___Observers_TryStartGame_2166136261));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ECasino_002EBlackjackGameControllerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ECasino_002EBlackjackGameControllerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_StartGame_2166136261()
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
			((NetworkBehaviour)this).SendObserversRpc(0u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___StartGame_2166136261()
	{
		ResetCards();
		CurrentStage = EStage.Dealing;
		PlayerTurn = null;
		IsLocalPlayerBlackjack = false;
		IsLocalPlayerBust = false;
		if (InstanceFinder.IsServer)
		{
			SetRoundEnded(ended: false);
		}
		if (IsLocalPlayerInCurrentRound)
		{
			NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(0f - LocalPlayerBet);
			Players.SetPlayerScore(Player.Local, Mathf.RoundToInt(LocalPlayerBet));
			base.LocalPlayerData.SetData("Ready", value: false);
		}
		List<Player> clockwisePlayers = GetClockwisePlayers();
		if (gameRoutine != null)
		{
			Console.LogWarning("Game routine already running, stopping...");
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StopCoroutine(gameRoutine);
		}
		gameRoutine = ((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(GameRoutine());
		IEnumerator GameRoutine()
		{
			float drawSpacing = Mathf.Lerp(0.5f, 0.2f, (float)clockwisePlayers.Count / 4f);
			Console.Log("Dealing...");
			for (int i = 0; i < clockwisePlayers.Count; i++)
			{
				if (playersInCurrentRound.Contains(clockwisePlayers[i]))
				{
					int playerIndex = Players.GetPlayerIndex(clockwisePlayers[i]);
					Transform[] playerCardPositions = GetPlayerCardPositions(playerIndex);
					if (InstanceFinder.IsServer)
					{
						PlayingCard playingCard = DrawCard();
						playingCard.GlideTo(playerCardPositions[0].position, playerCardPositions[0].rotation);
						playingCard.SetFaceUp(faceUp: true);
						AddCardToPlayerHand(playerIndex, playingCard);
					}
					yield return (object)new WaitForSeconds(drawSpacing);
				}
			}
			if (InstanceFinder.IsServer)
			{
				PlayingCard playingCard2 = DrawCard();
				playingCard2.GlideTo(DealerCardPositions[0].position, DealerCardPositions[0].rotation);
				playingCard2.SetFaceUp(faceUp: true);
				AddCardToDealerHand(playingCard2.CardID);
			}
			yield return (object)new WaitForSeconds(drawSpacing);
			DealerScore = GetHandScore(dealerHand, countFaceDown: false);
			Console.Log("Partial dealer score: " + DealerScore);
			for (int i = 0; i < clockwisePlayers.Count; i++)
			{
				if (playersInCurrentRound.Contains(clockwisePlayers[i]))
				{
					int playerIndex2 = Players.GetPlayerIndex(clockwisePlayers[i]);
					Transform[] playerCardPositions2 = GetPlayerCardPositions(playerIndex2);
					if (InstanceFinder.IsServer)
					{
						PlayingCard playingCard3 = DrawCard();
						playingCard3.GlideTo(playerCardPositions2[1].position, playerCardPositions2[1].rotation);
						playingCard3.SetFaceUp(faceUp: true);
						AddCardToPlayerHand(playerIndex2, playingCard3);
					}
					yield return (object)new WaitForSeconds(drawSpacing);
				}
			}
			if (IsLocalPlayerInCurrentRound)
			{
				List<PlayingCard> playerCards = GetPlayerCards(Players.GetPlayerIndex(Player.Local));
				LocalPlayerScore = GetHandScore(playerCards);
			}
			if (InstanceFinder.IsServer)
			{
				PlayingCard playingCard4 = DrawCard();
				playingCard4.GlideTo(DealerCardPositions[1].position, DealerCardPositions[1].rotation);
				playingCard4.SetFaceUp(faceUp: false);
				AddCardToDealerHand(playingCard4.CardID);
			}
			if (onInitialCardsDealt != null)
			{
				onInitialCardsDealt();
			}
			yield return (object)new WaitForSeconds(0.5f);
			Console.Log("Player turns...");
			CurrentStage = EStage.PlayerTurn;
			clockwisePlayers = GetClockwisePlayers();
			for (int i = 0; i < clockwisePlayers.Count; i++)
			{
				Player player = clockwisePlayers[i];
				PlayerTurn = player;
				if (playersInCurrentRound.Contains(player))
				{
					int playerIndex3 = Players.GetPlayerIndex(player);
					Console.Log("Player " + playerIndex3 + " turn");
					if (player.IsLocalPlayer)
					{
						PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(localFocusCameraTransform.position, localFocusCameraTransform.rotation, 0.6f);
					}
					int turn = 0;
					while (true)
					{
						List<PlayingCard> playerCards2 = GetPlayerCards(playerIndex3);
						int handScore = GetHandScore(playerCards2);
						if (player.IsLocalPlayer)
						{
							LocalPlayerScore = handScore;
						}
						Console.Log("Player " + playerIndex3 + " score: " + handScore);
						if (handScore == 21)
						{
							NotifyPlayerScore(((NetworkBehaviour)player).NetworkObject, handScore, turn == 0);
							Console.Log("21!");
							break;
						}
						if (handScore > 21)
						{
							NotifyPlayerScore(((NetworkBehaviour)player).NetworkObject, handScore, blackjack: false);
							Console.Log("Bust");
							if (player.IsLocalPlayer)
							{
								if (onLocalPlayerBust != null)
								{
									onLocalPlayerBust();
								}
								RemoveLocalPlayerFromGame(EPayoutType.None, 1.5f);
							}
							break;
						}
						Players.GetPlayerData(player).SetData("Action", 0f);
						if (player.IsLocalPlayer && onLocalPlayerReadyForInput != null)
						{
							onLocalPlayerReadyForInput();
						}
						yield return (object)new WaitForSeconds(0.5f);
						yield return (object)new WaitUntil((Func<bool>)(() => Players.GetPlayerData(player).GetData<float>("Action") > 0.1f || !playersInCurrentRound.Contains(player)));
						Console.Log(player.PlayerName + " action: " + Players.GetPlayerData(player).GetData<float>("Action"));
						Console.Log("Player in round: " + playersInCurrentRound.Contains(player));
						if (Players.GetPlayerData(player).GetData<float>("Action") != 1f || GetPlayerCards(playerIndex3).Count >= Player1CardPositions.Length)
						{
							break;
						}
						if (InstanceFinder.IsServer)
						{
							Transform[] playerCardPositions3 = GetPlayerCardPositions(playerIndex3);
							PlayingCard playingCard5 = DrawCard();
							playingCard5.GlideTo(playerCardPositions3[2 + turn].position, playerCardPositions3[2 + turn].rotation);
							playingCard5.SetFaceUp(faceUp: true);
							AddCardToPlayerHand(playerIndex3, playingCard5);
						}
						yield return (object)new WaitForSeconds(0.6f);
						turn++;
					}
					if (player.IsLocalPlayer)
					{
						PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(localDefaultCameraTransform.position, localDefaultCameraTransform.rotation, 0.6f);
					}
				}
			}
			yield return (object)new WaitForSeconds(1f);
			if (IsCurrentRoundEmpty() && InstanceFinder.IsServer)
			{
				Console.Log("No players left in round");
				EndGame();
				gameRoutine = null;
			}
			else
			{
				CurrentStage = EStage.DealerTurn;
				if (IsLocalPlayerInCurrentRound)
				{
					PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(localFinalCameraTransform.position, localFinalCameraTransform.rotation, 0.6f);
				}
				yield return (object)new WaitForSeconds(0.5f);
				if (InstanceFinder.IsServer)
				{
					dealerHand[1].SetFaceUp(faceUp: true);
				}
				yield return (object)new WaitForSeconds(1f);
				int dealerTurn = 0;
				while (!roundEnded)
				{
					int handScore2 = GetHandScore(dealerHand);
					DealerScore = handScore2;
					Console.Log("Updated dealer score: " + DealerScore);
					if (handScore2 >= 17 || dealerHand.Count >= DealerCardPositions.Length)
					{
						break;
					}
					if (InstanceFinder.IsServer)
					{
						PlayingCard playingCard6 = DrawCard();
						playingCard6.GlideTo(DealerCardPositions[2 + dealerTurn].position, DealerCardPositions[2 + dealerTurn].rotation);
						playingCard6.SetFaceUp(faceUp: true);
						AddCardToDealerHand(playingCard6.CardID);
					}
					yield return (object)new WaitForSeconds(0.6f);
					dealerTurn++;
				}
				CurrentStage = EStage.Ending;
				if (InstanceFinder.IsServer)
				{
					SetRoundEnded(ended: true);
				}
				yield return (object)new WaitForSeconds(0.2f);
				yield return (object)new WaitUntil((Func<bool>)(() => roundEnded));
				if (IsLocalPlayerInCurrentRound)
				{
					Console.Log("Final dealer score: " + DealerScore);
					Console.Log("Final player score: " + LocalPlayerScore);
					bool flag = DealerScore > 21;
					EPayoutType payout = EPayoutType.None;
					if (LocalPlayerScore <= 21)
					{
						if (LocalPlayerScore == 21 && IsLocalPlayerBlackjack && LocalPlayerScore > DealerScore)
						{
							payout = EPayoutType.Blackjack;
						}
						else if (flag)
						{
							payout = EPayoutType.Win;
						}
						else if (LocalPlayerScore > DealerScore)
						{
							payout = EPayoutType.Win;
						}
						else if (LocalPlayerScore == DealerScore)
						{
							payout = EPayoutType.Push;
						}
					}
					Console.Log("Payout: " + payout);
					RemoveLocalPlayerFromGame(payout, 1f);
				}
				yield return (object)new WaitForSeconds(1.5f);
				gameRoutine = null;
				if (InstanceFinder.IsServer)
				{
					EndGame();
				}
			}
		}
	}

	private void RpcReader___Observers_StartGame_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___StartGame_2166136261();
		}
	}

	private void RpcWriter___Observers_NotifyPlayerScore_2864061566(NetworkObject player, int score, bool blackjack)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(score, (AutoPackType)1);
			((Writer)writer).WriteBoolean(blackjack);
			((NetworkBehaviour)this).SendObserversRpc(1u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___NotifyPlayerScore_2864061566(NetworkObject player, int score, bool blackjack)
	{
		Player component = ((Component)player).GetComponent<Player>();
		if (!((Object)(object)component == (Object)null) && component.IsLocalPlayer)
		{
			LocalPlayerScore = score;
			IsLocalPlayerBlackjack = blackjack;
			if (score > 21)
			{
				IsLocalPlayerBust = true;
			}
		}
	}

	private void RpcReader___Observers_NotifyPlayerScore_2864061566(PooledReader PooledReader0, Channel channel)
	{
		NetworkObject player = ((Reader)PooledReader0).ReadNetworkObject();
		int score = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		bool blackjack = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___NotifyPlayerScore_2864061566(player, score, blackjack);
		}
	}

	private void RpcWriter___Observers_SetRoundEnded_1140765316(bool ended)
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
			((Writer)writer).WriteBoolean(ended);
			((NetworkBehaviour)this).SendObserversRpc(2u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetRoundEnded_1140765316(bool ended)
	{
		roundEnded = ended;
	}

	private void RpcReader___Observers_SetRoundEnded_1140765316(PooledReader PooledReader0, Channel channel)
	{
		bool ended = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetRoundEnded_1140765316(ended);
		}
	}

	private void RpcWriter___Observers_AddCardToPlayerHand_2801973956(int playerindex, string cardID)
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
			((Writer)writer).WriteInt32(playerindex, (AutoPackType)1);
			((Writer)writer).WriteString(cardID);
			((NetworkBehaviour)this).SendObserversRpc(3u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___AddCardToPlayerHand_2801973956(int playerindex, string cardID)
	{
		PlayingCard playingCard = Cards.FirstOrDefault((PlayingCard x) => x.CardID == cardID);
		if ((Object)(object)playingCard == (Object)null)
		{
			return;
		}
		switch (playerindex)
		{
		case 0:
			if (!player1Hand.Contains(playingCard))
			{
				player1Hand.Add(playingCard);
			}
			break;
		case 1:
			if (!player2Hand.Contains(playingCard))
			{
				player2Hand.Add(playingCard);
			}
			break;
		case 2:
			if (!player3Hand.Contains(playingCard))
			{
				player3Hand.Add(playingCard);
			}
			break;
		case 3:
			if (!player4Hand.Contains(playingCard))
			{
				player4Hand.Add(playingCard);
			}
			break;
		}
	}

	private void RpcReader___Observers_AddCardToPlayerHand_2801973956(PooledReader PooledReader0, Channel channel)
	{
		int playerindex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		string cardID = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___AddCardToPlayerHand_2801973956(playerindex, cardID);
		}
	}

	private void RpcWriter___Observers_AddCardToDealerHand_3615296227(string cardID)
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
			((Writer)writer).WriteString(cardID);
			((NetworkBehaviour)this).SendObserversRpc(4u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___AddCardToDealerHand_3615296227(string cardID)
	{
		PlayingCard playingCard = Cards.FirstOrDefault((PlayingCard x) => x.CardID == cardID);
		if (!((Object)(object)playingCard == (Object)null) && !dealerHand.Contains(playingCard))
		{
			dealerHand.Add(playingCard);
		}
	}

	private void RpcReader___Observers_AddCardToDealerHand_3615296227(PooledReader PooledReader0, Channel channel)
	{
		string cardID = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___AddCardToDealerHand_3615296227(cardID);
		}
	}

	private void RpcWriter___Observers_EndGame_2166136261()
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
			((NetworkBehaviour)this).SendObserversRpc(5u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___EndGame_2166136261()
	{
		PlayerTurn = null;
		CurrentStage = EStage.WaitingForPlayers;
		ResetCards();
	}

	private void RpcReader___Observers_EndGame_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___EndGame_2166136261();
		}
	}

	private void RpcWriter___Observers_AddPlayerToCurrentRound_3323014238(NetworkObject player)
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
			((NetworkBehaviour)this).SendObserversRpc(6u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___AddPlayerToCurrentRound_3323014238(NetworkObject player)
	{
		Player component = ((Component)player).GetComponent<Player>();
		if (!((Object)(object)component == (Object)null))
		{
			Console.Log("Adding player to current round: " + component.PlayerName);
			if (!playersInCurrentRound.Contains(component))
			{
				playersInCurrentRound.Add(component);
			}
		}
	}

	private void RpcReader___Observers_AddPlayerToCurrentRound_3323014238(PooledReader PooledReader0, Channel channel)
	{
		NetworkObject player = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___AddPlayerToCurrentRound_3323014238(player);
		}
	}

	private void RpcWriter___Server_RequestRemovePlayerFromCurrentRound_3323014238(NetworkObject player)
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
			((NetworkBehaviour)this).SendServerRpc(7u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___RequestRemovePlayerFromCurrentRound_3323014238(NetworkObject player)
	{
		RemovePlayerFromCurrentRound(player);
	}

	private void RpcReader___Server_RequestRemovePlayerFromCurrentRound_3323014238(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkObject player = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___RequestRemovePlayerFromCurrentRound_3323014238(player);
		}
	}

	private void RpcWriter___Observers_RemovePlayerFromCurrentRound_3323014238(NetworkObject player)
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
			((NetworkBehaviour)this).SendObserversRpc(8u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___RemovePlayerFromCurrentRound_3323014238(NetworkObject player)
	{
		Player component = ((Component)player).GetComponent<Player>();
		if (!((Object)(object)component == (Object)null))
		{
			Console.Log("Removing player from current round: " + component.PlayerName);
			if (playersInCurrentRound.Contains(component))
			{
				playersInCurrentRound.Remove(component);
			}
		}
	}

	private void RpcReader___Observers_RemovePlayerFromCurrentRound_3323014238(PooledReader PooledReader0, Channel channel)
	{
		NetworkObject player = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___RemovePlayerFromCurrentRound_3323014238(player);
		}
	}

	private void RpcWriter___Observers_TryStartGame_2166136261()
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
			((NetworkBehaviour)this).SendObserversRpc(9u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___TryStartGame_2166136261()
	{
		if (InstanceFinder.IsServer && CurrentStage == EStage.WaitingForPlayers && AreAllPlayersReady())
		{
			for (int i = 0; i < Players.CurrentPlayerCount; i++)
			{
				AddPlayerToCurrentRound(((NetworkBehaviour)Players.GetPlayer(i)).NetworkObject);
			}
			StartGame();
		}
	}

	private void RpcReader___Observers_TryStartGame_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___TryStartGame_2166136261();
		}
	}

	protected override void Awake_UserLogic_ScheduleOne_002ECasino_002EBlackjackGameController_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		ResetCards();
	}
}
