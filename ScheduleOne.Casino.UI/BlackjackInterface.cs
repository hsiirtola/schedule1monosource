using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.Money;
using ScheduleOne.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.Casino.UI;

public class BlackjackInterface : Singleton<BlackjackInterface>
{
	[Header("References")]
	public Canvas Canvas;

	public CasinoGamePlayerDisplay PlayerDisplay;

	public RectTransform BetContainer;

	public TextMeshProUGUI BetTitleLabel;

	public Slider BetSlider;

	public TextMeshProUGUI BetAmount;

	public Button ReadyButton;

	public TextMeshProUGUI ReadyLabel;

	public RectTransform WaitingContainer;

	public TextMeshProUGUI WaitingLabel;

	public TextMeshProUGUI DealerScoreLabel;

	public TextMeshProUGUI PlayerScoreLabel;

	public Button HitButton;

	public Button StandButton;

	public Animation InputContainerAnimation;

	public CanvasGroup InputContainerCanvasGroup;

	public AnimationClip InputContainerFadeIn;

	public AnimationClip InputContainerFadeOut;

	public RectTransform SelectionIndicator;

	public Animation ScoresContainerAnimation;

	public CanvasGroup ScoresContainerCanvasGroup;

	public TextMeshProUGUI PositiveOutcomeLabel;

	public TextMeshProUGUI PayoutLabel;

	public UnityEvent onBust;

	public UnityEvent onBlackjack;

	public UnityEvent onWin;

	public UnityEvent onLose;

	public UnityEvent onPush;

	public BlackjackGameController CurrentGame { get; private set; }

	protected override void Awake()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected O, but got Unknown
		base.Awake();
		((UnityEvent<float>)(object)BetSlider.onValueChanged).AddListener((UnityAction<float>)BetSliderChanged);
		((UnityEvent)ReadyButton.onClick).AddListener(new UnityAction(ReadyButtonClicked));
		((UnityEvent)HitButton.onClick).AddListener(new UnityAction(HitClicked));
		((UnityEvent)StandButton.onClick).AddListener(new UnityAction(StandClicked));
		InputContainerCanvasGroup.alpha = 0f;
		InputContainerCanvasGroup.interactable = false;
		ScoresContainerCanvasGroup.alpha = 0f;
		((Behaviour)Canvas).enabled = false;
	}

	private void FixedUpdate()
	{
		if ((Object)(object)CurrentGame == (Object)null)
		{
			return;
		}
		bool data = CurrentGame.LocalPlayerData.GetData<bool>("Ready");
		((Selectable)BetSlider).interactable = CurrentGame.CurrentStage == BlackjackGameController.EStage.WaitingForPlayers && !data;
		if (data)
		{
			((TMP_Text)BetTitleLabel).text = "Waiting for other players...";
		}
		else
		{
			((TMP_Text)BetTitleLabel).text = "Place your bet and press 'ready'";
		}
		if (CurrentGame.CurrentStage == BlackjackGameController.EStage.WaitingForPlayers)
		{
			((Component)BetContainer).gameObject.SetActive(true);
			RefreshReadyButton();
		}
		else
		{
			((Component)BetContainer).gameObject.SetActive(false);
		}
		((TMP_Text)PlayerScoreLabel).text = CurrentGame.LocalPlayerScore.ToString();
		if (CurrentGame.CurrentStage == BlackjackGameController.EStage.DealerTurn || CurrentGame.CurrentStage == BlackjackGameController.EStage.Ending)
		{
			((TMP_Text)DealerScoreLabel).text = CurrentGame.DealerScore.ToString();
		}
		else
		{
			((TMP_Text)DealerScoreLabel).text = CurrentGame.DealerScore + "+?";
		}
		if (CurrentGame.CurrentStage == BlackjackGameController.EStage.PlayerTurn && (Object)(object)CurrentGame.PlayerTurn != (Object)null)
		{
			if (CurrentGame.PlayerTurn.IsLocalPlayer)
			{
				((TMP_Text)WaitingLabel).text = "Your turn!";
			}
			else
			{
				((TMP_Text)WaitingLabel).text = "Waiting for " + CurrentGame.PlayerTurn.PlayerName + "...";
			}
			((Component)WaitingContainer).gameObject.SetActive(true);
		}
		else if (CurrentGame.CurrentStage == BlackjackGameController.EStage.DealerTurn)
		{
			((TMP_Text)WaitingLabel).text = "Dealer's turn...";
			((Component)WaitingContainer).gameObject.SetActive(true);
		}
		else
		{
			((Component)WaitingContainer).gameObject.SetActive(false);
		}
	}

	public void Open(BlackjackGameController game)
	{
		CurrentGame = game;
		BlackjackGameController currentGame = CurrentGame;
		currentGame.onLocalPlayerBetChange = (Action)Delegate.Combine(currentGame.onLocalPlayerBetChange, new Action(RefreshDisplayedBet));
		BlackjackGameController currentGame2 = CurrentGame;
		currentGame2.onLocalPlayerExitRound = (Action)Delegate.Combine(currentGame2.onLocalPlayerExitRound, new Action(LocalPlayerExitRound));
		BlackjackGameController currentGame3 = CurrentGame;
		currentGame3.onInitialCardsDealt = (Action)Delegate.Combine(currentGame3.onInitialCardsDealt, new Action(ShowScores));
		BlackjackGameController currentGame4 = CurrentGame;
		currentGame4.onLocalPlayerReadyForInput = (Action)Delegate.Combine(currentGame4.onLocalPlayerReadyForInput, new Action(LocalPlayerReadyForInput));
		BlackjackGameController currentGame5 = CurrentGame;
		currentGame5.onLocalPlayerBust = (Action)Delegate.Combine(currentGame5.onLocalPlayerBust, new Action(OnLocalPlayerBust));
		BlackjackGameController currentGame6 = CurrentGame;
		currentGame6.onLocalPlayerRoundCompleted = (Action<BlackjackGameController.EPayoutType>)Delegate.Combine(currentGame6.onLocalPlayerRoundCompleted, new Action<BlackjackGameController.EPayoutType>(OnLocalPlayerRoundCompleted));
		PlayerDisplay.Bind(game.Players);
		Singleton<InputPromptsCanvas>.Instance.LoadModule("exitonly");
		((Behaviour)Canvas).enabled = true;
		BetSlider.SetValueWithoutNotify(0f);
		game.SetLocalPlayerBet(10f);
		RefreshDisplayedBet();
		RefreshDisplayedBet();
	}

	public void Close()
	{
		if ((Object)(object)CurrentGame != (Object)null)
		{
			BlackjackGameController currentGame = CurrentGame;
			currentGame.onLocalPlayerBetChange = (Action)Delegate.Remove(currentGame.onLocalPlayerBetChange, new Action(RefreshDisplayedBet));
			BlackjackGameController currentGame2 = CurrentGame;
			currentGame2.onLocalPlayerExitRound = (Action)Delegate.Remove(currentGame2.onLocalPlayerExitRound, new Action(LocalPlayerExitRound));
			BlackjackGameController currentGame3 = CurrentGame;
			currentGame3.onInitialCardsDealt = (Action)Delegate.Remove(currentGame3.onInitialCardsDealt, new Action(ShowScores));
			BlackjackGameController currentGame4 = CurrentGame;
			currentGame4.onLocalPlayerReadyForInput = (Action)Delegate.Remove(currentGame4.onLocalPlayerReadyForInput, new Action(LocalPlayerReadyForInput));
			BlackjackGameController currentGame5 = CurrentGame;
			currentGame5.onLocalPlayerBust = (Action)Delegate.Remove(currentGame5.onLocalPlayerBust, new Action(OnLocalPlayerBust));
			BlackjackGameController currentGame6 = CurrentGame;
			currentGame6.onLocalPlayerRoundCompleted = (Action<BlackjackGameController.EPayoutType>)Delegate.Remove(currentGame6.onLocalPlayerRoundCompleted, new Action<BlackjackGameController.EPayoutType>(OnLocalPlayerRoundCompleted));
		}
		CurrentGame = null;
		PlayerDisplay.Unbind();
		Singleton<InputPromptsCanvas>.Instance.UnloadModule();
		((Behaviour)Canvas).enabled = false;
	}

	private void BetSliderChanged(float newValue)
	{
		CurrentGame.SetLocalPlayerBet(GetBetFromSliderValue(newValue));
		RefreshDisplayedBet();
	}

	private float GetBetFromSliderValue(float sliderVal)
	{
		return Mathf.Lerp(10f, 1000f, Mathf.Pow(sliderVal, 2f));
	}

	private void RefreshDisplayedBet()
	{
		((TMP_Text)BetAmount).text = MoneyManager.FormatAmount(CurrentGame.LocalPlayerBet);
		BetSlider.SetValueWithoutNotify(Mathf.Sqrt(Mathf.InverseLerp(10f, 1000f, CurrentGame.LocalPlayerBet)));
	}

	private void RefreshReadyButton()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkSingleton<MoneyManager>.Instance.cashBalance >= CurrentGame.LocalPlayerBet)
		{
			((Selectable)ReadyButton).interactable = true;
			((Graphic)BetAmount).color = Color32.op_Implicit(new Color32((byte)84, (byte)231, (byte)23, byte.MaxValue));
		}
		else
		{
			((Selectable)ReadyButton).interactable = false;
			((Graphic)BetAmount).color = Color32.op_Implicit(new Color32((byte)231, (byte)52, (byte)23, byte.MaxValue));
		}
		if (CurrentGame.LocalPlayerData.GetData<bool>("Ready"))
		{
			((TMP_Text)ReadyLabel).text = "Cancel";
		}
		else
		{
			((TMP_Text)ReadyLabel).text = "Ready";
		}
	}

	private void LocalPlayerReadyForInput()
	{
		((Component)SelectionIndicator).gameObject.SetActive(false);
		InputContainerCanvasGroup.interactable = true;
		InputContainerAnimation.Play(((Object)InputContainerFadeIn).name);
	}

	private void ShowScores()
	{
		ScoresContainerAnimation.Play(((Object)InputContainerFadeIn).name);
	}

	private void HideScores()
	{
		ScoresContainerAnimation.Play(((Object)InputContainerFadeOut).name);
	}

	private void HitClicked()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		((Component)SelectionIndicator).transform.position = ((Component)HitButton).transform.position;
		((Component)SelectionIndicator).gameObject.SetActive(true);
		CurrentGame.LocalPlayerData.SetData("Action", 1f);
		InputContainerCanvasGroup.interactable = false;
		InputContainerAnimation.Play(((Object)InputContainerFadeOut).name);
	}

	private void StandClicked()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		((Component)SelectionIndicator).transform.position = ((Component)StandButton).transform.position;
		((Component)SelectionIndicator).gameObject.SetActive(true);
		CurrentGame.LocalPlayerData.SetData("Action", 2f);
		InputContainerCanvasGroup.interactable = false;
		InputContainerAnimation.Play(((Object)InputContainerFadeOut).name);
	}

	private void LocalPlayerExitRound()
	{
		HideScores();
		if (InputContainerCanvasGroup.alpha > 0f)
		{
			InputContainerCanvasGroup.interactable = false;
			InputContainerAnimation.Play(((Object)InputContainerFadeOut).name);
		}
	}

	private void ReadyButtonClicked()
	{
		CurrentGame.ToggleLocalPlayerReady();
	}

	private void OnLocalPlayerBust()
	{
		if (onBust != null)
		{
			onBust.Invoke();
		}
	}

	private void OnLocalPlayerRoundCompleted(BlackjackGameController.EPayoutType payout)
	{
		float payout2 = CurrentGame.GetPayout(CurrentGame.LocalPlayerBet, payout);
		((TMP_Text)PayoutLabel).text = MoneyManager.FormatAmount(payout2);
		switch (payout)
		{
		case BlackjackGameController.EPayoutType.None:
			if (!CurrentGame.IsLocalPlayerBust && onLose != null)
			{
				onLose.Invoke();
			}
			break;
		case BlackjackGameController.EPayoutType.Blackjack:
			((TMP_Text)PositiveOutcomeLabel).text = "Blackjack!";
			if (onBlackjack != null)
			{
				onBlackjack.Invoke();
			}
			break;
		case BlackjackGameController.EPayoutType.Win:
			((TMP_Text)PositiveOutcomeLabel).text = "Win!";
			if (onWin != null)
			{
				onWin.Invoke();
			}
			break;
		case BlackjackGameController.EPayoutType.Push:
			if (onPush != null)
			{
				onPush.Invoke();
			}
			break;
		}
	}
}
