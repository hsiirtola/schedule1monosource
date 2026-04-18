using System;
using System.Collections;
using ScheduleOne.DevUtilities;
using ScheduleOne.Money;
using ScheduleOne.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.Casino.UI;

public class RTBInterface : Singleton<RTBInterface>
{
	[Header("References")]
	public Canvas Canvas;

	public CasinoGamePlayerDisplay PlayerDisplay;

	public TextMeshProUGUI StatusLabel;

	public RectTransform BetContainer;

	public TextMeshProUGUI BetTitleLabel;

	public Slider BetSlider;

	public TextMeshProUGUI BetAmount;

	public Button ReadyButton;

	public TextMeshProUGUI ReadyLabel;

	public TextMeshProUGUI WinningsMultiplierLabel;

	[Header("Question and answers")]
	public RectTransform QuestionContainer;

	public TextMeshProUGUI QuestionLabel;

	public Slider TimerSlider;

	public Button[] AnswerButtons;

	public TextMeshProUGUI[] AnswerLabels;

	public Button ForfeitButton;

	public TextMeshProUGUI ForfeitLabel;

	public Animation QuestionContainerAnimation;

	public AnimationClip QuestionContainerFadeIn;

	public AnimationClip QuestionContainerFadeOut;

	public CanvasGroup QuestionCanvasGroup;

	public RectTransform SelectionIndicator;

	public UnityEvent onCorrect;

	public UnityEvent onFinalCorrect;

	public UnityEvent onIncorrect;

	public RTBGameController CurrentGame { get; private set; }

	protected override void Awake()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Expected O, but got Unknown
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		base.Awake();
		((UnityEvent<float>)(object)BetSlider.onValueChanged).AddListener((UnityAction<float>)BetSliderChanged);
		((UnityEvent)ReadyButton.onClick).AddListener(new UnityAction(ReadyButtonClicked));
		for (int i = 0; i < AnswerButtons.Length; i++)
		{
			int index = i;
			((UnityEvent)AnswerButtons[i].onClick).AddListener((UnityAction)delegate
			{
				AnswerButtonClicked(index);
			});
		}
		((UnityEvent)ForfeitButton.onClick).AddListener(new UnityAction(ForfeitClicked));
		QuestionCanvasGroup.alpha = 0f;
		QuestionCanvasGroup.interactable = false;
		((Behaviour)Canvas).enabled = false;
	}

	private void FixedUpdate()
	{
		if (!((Object)(object)CurrentGame == (Object)null))
		{
			((TMP_Text)StatusLabel).text = GetStatusText();
			bool data = CurrentGame.LocalPlayerData.GetData<bool>("Ready");
			((Selectable)BetSlider).interactable = CurrentGame.CurrentStage == RTBGameController.EStage.WaitingForPlayers && !data;
			if (data)
			{
				((TMP_Text)BetTitleLabel).text = "Waiting for other players...";
			}
			else
			{
				((TMP_Text)BetTitleLabel).text = "Place your bet and press 'ready'";
			}
			if (CurrentGame.CurrentStage == RTBGameController.EStage.WaitingForPlayers)
			{
				((Component)BetContainer).gameObject.SetActive(true);
				RefreshReadyButton();
			}
			else
			{
				((Component)BetContainer).gameObject.SetActive(false);
			}
		}
	}

	private string GetStatusText()
	{
		return CurrentGame.CurrentStage switch
		{
			RTBGameController.EStage.WaitingForPlayers => "Waiting for players... (" + CurrentGame.GetPlayersReadyCount() + "/" + CurrentGame.Players.CurrentPlayerCount + ")", 
			RTBGameController.EStage.RedOrBlack => "Round 1\nPredict if the next card will be red or black.\nYou can also forfeit and cash out.", 
			RTBGameController.EStage.HigherOrLower => "Round 2\nPredict if the next card will be higher or lower than the previous card.\nYou can also forfeit and cash out.", 
			RTBGameController.EStage.InsideOrOutside => "Round 3\nPredict if the next card will be inside or outside the previous two cards (Ace counts as 11).\nYou can also forfeit and cash out.", 
			RTBGameController.EStage.Suit => "Round 4\nPredict the suit of the next card.\nYou can also forfeit and cash out.", 
			_ => "Unknown", 
		};
	}

	public void Open(RTBGameController game)
	{
		CurrentGame = game;
		RTBGameController currentGame = CurrentGame;
		currentGame.onQuestionReady = (Action<string, string[]>)Delegate.Combine(currentGame.onQuestionReady, new Action<string, string[]>(QuestionReady));
		RTBGameController currentGame2 = CurrentGame;
		currentGame2.onQuestionDone = (Action)Delegate.Combine(currentGame2.onQuestionDone, new Action(QuestionDone));
		RTBGameController currentGame3 = CurrentGame;
		currentGame3.onLocalPlayerCorrect = (Action)Delegate.Combine(currentGame3.onLocalPlayerCorrect, new Action(Correct));
		RTBGameController currentGame4 = CurrentGame;
		currentGame4.onLocalPlayerIncorrect = (Action)Delegate.Combine(currentGame4.onLocalPlayerIncorrect, new Action(Incorrect));
		RTBGameController currentGame5 = CurrentGame;
		currentGame5.onLocalPlayerBetChange = (Action)Delegate.Combine(currentGame5.onLocalPlayerBetChange, new Action(RefreshDisplayedBet));
		RTBGameController currentGame6 = CurrentGame;
		currentGame6.onLocalPlayerExitRound = (Action)Delegate.Combine(currentGame6.onLocalPlayerExitRound, new Action(LocalPlayerExitRound));
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
			RTBGameController currentGame = CurrentGame;
			currentGame.onQuestionReady = (Action<string, string[]>)Delegate.Remove(currentGame.onQuestionReady, new Action<string, string[]>(QuestionReady));
			RTBGameController currentGame2 = CurrentGame;
			currentGame2.onQuestionDone = (Action)Delegate.Remove(currentGame2.onQuestionDone, new Action(QuestionDone));
			RTBGameController currentGame3 = CurrentGame;
			currentGame3.onLocalPlayerCorrect = (Action)Delegate.Remove(currentGame3.onLocalPlayerCorrect, new Action(Correct));
			RTBGameController currentGame4 = CurrentGame;
			currentGame4.onLocalPlayerIncorrect = (Action)Delegate.Remove(currentGame4.onLocalPlayerIncorrect, new Action(Incorrect));
			RTBGameController currentGame5 = CurrentGame;
			currentGame5.onLocalPlayerBetChange = (Action)Delegate.Remove(currentGame5.onLocalPlayerBetChange, new Action(RefreshDisplayedBet));
			RTBGameController currentGame6 = CurrentGame;
			currentGame6.onLocalPlayerExitRound = (Action)Delegate.Remove(currentGame6.onLocalPlayerExitRound, new Action(LocalPlayerExitRound));
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
		return Mathf.Lerp(10f, 500f, Mathf.Pow(sliderVal, 2f));
	}

	private void RefreshDisplayedBet()
	{
		((TMP_Text)BetAmount).text = MoneyManager.FormatAmount(CurrentGame.LocalPlayerBet);
		BetSlider.SetValueWithoutNotify(Mathf.Sqrt(Mathf.InverseLerp(10f, 500f, CurrentGame.LocalPlayerBet)));
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

	private void QuestionReady(string question, string[] answers)
	{
		((TMP_Text)QuestionLabel).text = question;
		((Component)SelectionIndicator).gameObject.SetActive(false);
		((TMP_Text)ForfeitLabel).text = "Forfeit and collect " + MoneyManager.FormatAmount(CurrentGame.MultipliedLocalPlayerBet, showDecimals: false, includeColor: true);
		QuestionCanvasGroup.interactable = true;
		for (int i = 0; i < AnswerButtons.Length; i++)
		{
			if (answers.Length > i)
			{
				((TMP_Text)AnswerLabels[i]).text = answers[i];
				((Component)AnswerButtons[i]).gameObject.SetActive(true);
			}
			else
			{
				((Component)AnswerButtons[i]).gameObject.SetActive(false);
			}
		}
		QuestionContainerAnimation.Play(((Object)QuestionContainerFadeIn).name);
		TimerSlider.value = 1f;
		((MonoBehaviour)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			while ((Object)(object)CurrentGame != (Object)null && CurrentGame.IsQuestionActive)
			{
				TimerSlider.value = CurrentGame.RemainingAnswerTime / 6f;
				yield return (object)new WaitForEndOfFrame();
			}
		}
	}

	private void AnswerButtonClicked(int index)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		((Component)SelectionIndicator).transform.position = ((Component)AnswerButtons[index]).transform.position;
		((Component)SelectionIndicator).gameObject.SetActive(true);
		CurrentGame.SetLocalPlayerAnswer((float)index + 1f);
	}

	private void ForfeitClicked()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		((Component)SelectionIndicator).transform.position = ((Component)ForfeitButton).transform.position;
		((Component)SelectionIndicator).gameObject.SetActive(true);
		CurrentGame.RemoveLocalPlayerFromGame(payout: true);
		QuestionDone();
	}

	private void QuestionDone()
	{
		QuestionCanvasGroup.interactable = false;
		QuestionContainerAnimation.Play(((Object)QuestionContainerFadeOut).name);
	}

	private void LocalPlayerExitRound()
	{
		QuestionCanvasGroup.interactable = false;
		if (QuestionCanvasGroup.alpha > 0f)
		{
			QuestionContainerAnimation.Stop();
			QuestionCanvasGroup.alpha = 0f;
		}
	}

	private void Correct()
	{
		((TMP_Text)WinningsMultiplierLabel).text = Mathf.RoundToInt(CurrentGame.LocalPlayerBetMultiplier) + "x";
		if (CurrentGame.CurrentStage == RTBGameController.EStage.Suit)
		{
			if (onFinalCorrect != null)
			{
				onFinalCorrect.Invoke();
			}
		}
		else if (onCorrect != null)
		{
			onCorrect.Invoke();
		}
	}

	private void Incorrect()
	{
		if (onIncorrect != null)
		{
			onIncorrect.Invoke();
		}
	}

	private void ReadyButtonClicked()
	{
		CurrentGame.ToggleLocalPlayerReady();
	}
}
