using System;
using System.Collections;
using System.Collections.Generic;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.Polling;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScheduleOne.UI.Polling;

public class PollPanel : MonoBehaviour
{
	public const float BUTTON_PRESS_TIME = 0.8f;

	public const string ResponseSubmittedMessage = "Your vote has been recorded.\n Thank you!";

	public GameObject ButtonPrefab;

	public Color TextColor_Green;

	public Color TextColor_Red;

	[Header("References")]
	public PollManager PollManager;

	public GameObject Container;

	public GameObject ActivePill;

	public GameObject ClosedPill;

	public TextMeshProUGUI QuestionLabel;

	public RectTransform ButtonContainer;

	public TextMeshProUGUI InstructionLabel;

	public TextMeshProUGUI ConfirmationMessageLabel;

	public AudioSourceController SubmissionStartSound;

	public AudioSourceController SubmissionSuccessSound;

	public AudioSourceController SubmissionFailSound;

	private List<Button> buttons = new List<Button>();

	private List<Image> buttonFills = new List<Image>();

	private int heldButton = -1;

	private int selectedButton = -1;

	private int lastHeldButton = -1;

	private float buttonPressTime;

	private void Awake()
	{
		PollManager pollManager = PollManager;
		pollManager.onActivePollReceived = (Action<PollData>)Delegate.Combine(pollManager.onActivePollReceived, new Action<PollData>(DisplayActivePoll));
		PollManager pollManager2 = PollManager;
		pollManager2.onConfirmedPollReceived = (Action<PollData>)Delegate.Combine(pollManager2.onConfirmedPollReceived, new Action<PollData>(DisplayConfirmedPoll));
		((Component)this).gameObject.SetActive(false);
	}

	private void Update()
	{
		if (GameInput.GetButton(GameInput.ButtonCode.PrimaryClick) && heldButton != -1)
		{
			buttonPressTime += Time.deltaTime;
			if (buttonPressTime >= 0.8f)
			{
				FinalizeButtonPress(heldButton);
			}
		}
		else
		{
			buttonPressTime = Mathf.Clamp(buttonPressTime - Time.deltaTime, 0f, 0.8f);
			heldButton = -1;
		}
		for (int i = 0; i < buttonFills.Count; i++)
		{
			if (selectedButton == i)
			{
				buttonFills[i].fillAmount = 1f;
			}
			else if (heldButton == i || lastHeldButton == i)
			{
				buttonFills[i].fillAmount = buttonPressTime / 0.8f;
			}
			else
			{
				buttonFills[i].fillAmount = 0f;
			}
		}
	}

	public void DisplayActivePoll(PollData poll)
	{
		Console.Log("Displaying active poll: " + poll.question);
		ActivePill.SetActive(true);
		ClosedPill.SetActive(false);
		((TMP_Text)QuestionLabel).text = poll.question;
		buttons = CreateButtons(poll);
		foreach (Button button in buttons)
		{
			buttonFills.Add(((Component)((Component)button).transform.Find("Fill")).GetComponent<Image>());
		}
		((Component)InstructionLabel).gameObject.SetActive(true);
		((Component)ConfirmationMessageLabel).gameObject.SetActive(false);
		((Component)this).gameObject.SetActive(true);
		if (PollManager.TryGetExistingPollResponse(poll.pollId, out var response))
		{
			DisplaySubmittedAnswer(response);
		}
		Rebuild();
	}

	public void DisplayConfirmedPoll(PollData poll)
	{
		Console.Log("Displaying confirmed poll: " + poll.question);
		ActivePill.SetActive(false);
		ClosedPill.SetActive(true);
		((TMP_Text)QuestionLabel).text = poll.question;
		buttons = CreateButtons(poll);
		foreach (Button button in buttons)
		{
			buttonFills.Add(((Component)((Component)button).transform.Find("Fill")).GetComponent<Image>());
			((Selectable)button).interactable = false;
		}
		if (poll.winnerIndex >= 0)
		{
			((Component)((Component)buttons[poll.winnerIndex]).transform.Find("Winner")).gameObject.SetActive(true);
			((Component)((Component)buttons[poll.winnerIndex]).transform.Find("Outline")).gameObject.SetActive(true);
		}
		((Component)InstructionLabel).gameObject.SetActive(false);
		((Component)ConfirmationMessageLabel).gameObject.SetActive(true);
		((TMP_Text)ConfirmationMessageLabel).text = poll.confirmationMessage;
		((Component)this).gameObject.SetActive(true);
		Rebuild();
	}

	private void DisplaySubmittedAnswer(int buttonIndex)
	{
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		if (buttonIndex < 0 || buttonIndex >= buttons.Count)
		{
			Console.LogError("Button index out of range: " + buttonIndex);
			return;
		}
		foreach (Button button in buttons)
		{
			((Selectable)((Component)button).GetComponent<Button>()).interactable = false;
		}
		selectedButton = buttonIndex;
		((Component)((Component)buttons[buttonIndex]).transform.Find("Outline")).gameObject.SetActive(true);
		((Component)((Component)buttons[buttonIndex]).transform.Find("Tick")).gameObject.SetActive(true);
		((Component)InstructionLabel).gameObject.SetActive(false);
		((TMP_Text)ConfirmationMessageLabel).text = "Your vote has been recorded.\n Thank you!";
		((Graphic)ConfirmationMessageLabel).color = TextColor_Green;
		((Component)ConfirmationMessageLabel).gameObject.SetActive(true);
	}

	private void Rebuild()
	{
		VerticalLayoutGroup layout = ((Component)((Component)this).transform.parent).GetComponent<VerticalLayoutGroup>();
		((Component)layout).gameObject.SetActive(false);
		((Behaviour)layout).enabled = false;
		LayoutRebuilder.ForceRebuildLayoutImmediate(((Component)layout).GetComponent<RectTransform>());
		((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Wait());
		IEnumerator Wait()
		{
			yield return (object)new WaitForEndOfFrame();
			((Component)layout).gameObject.SetActive(true);
			((Behaviour)layout).enabled = true;
			LayoutRebuilder.ForceRebuildLayoutImmediate(((Component)layout).GetComponent<RectTransform>());
			yield return (object)new WaitForEndOfFrame();
			((Component)layout).gameObject.SetActive(true);
			((Behaviour)layout).enabled = true;
			LayoutRebuilder.ForceRebuildLayoutImmediate(((Component)layout).GetComponent<RectTransform>());
		}
	}

	private List<Button> CreateButtons(PollData data)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Expected O, but got Unknown
		List<Button> list = new List<Button>();
		for (int i = 0; i < data.answers.Length; i++)
		{
			GameObject val = Object.Instantiate<GameObject>(ButtonPrefab, (Transform)(object)ButtonContainer);
			((TMP_Text)((Component)val.transform.Find("Text")).GetComponent<TextMeshProUGUI>()).text = data.answers[i];
			int buttonIndex = i;
			EventTrigger val2 = val.GetComponent<EventTrigger>();
			if ((Object)(object)val2 == (Object)null)
			{
				val2 = val.AddComponent<EventTrigger>();
			}
			Entry val3 = new Entry
			{
				eventID = (EventTriggerType)2
			};
			((UnityEvent<BaseEventData>)(object)val3.callback).AddListener((UnityAction<BaseEventData>)delegate
			{
				ButtonPressed(buttonIndex);
			});
			val2.triggers.Add(val3);
			list.Add(val.GetComponent<Button>());
		}
		return list;
	}

	private void ButtonPressed(int buttonIndex)
	{
		if (!((Selectable)((Component)buttons[buttonIndex]).GetComponent<Button>()).interactable)
		{
			Console.LogWarning("Button " + buttonIndex + " is not interactable, ignoring press.");
			return;
		}
		Console.Log("Button pressed: " + buttonIndex);
		if (lastHeldButton != buttonIndex)
		{
			buttonPressTime = 0.1f;
		}
		heldButton = buttonIndex;
		lastHeldButton = heldButton;
		PollManager.GenerateAppTicket();
	}

	private void FinalizeButtonPress(int buttonIndex)
	{
		selectedButton = buttonIndex;
		heldButton = -1;
		((Component)((Component)buttons[buttonIndex]).transform.Find("Outline")).gameObject.SetActive(true);
		foreach (Button button in buttons)
		{
			((Selectable)((Component)button).GetComponent<Button>()).interactable = false;
		}
		SubmissionStartSound.Play();
		((MonoBehaviour)this).StartCoroutine(Submit());
		IEnumerator Submit()
		{
			PollManager.SelectPollResponse(buttonIndex);
			((Component)InstructionLabel).gameObject.SetActive(false);
			((Component)ConfirmationMessageLabel).gameObject.SetActive(true);
			((TMP_Text)ConfirmationMessageLabel).text = "Submitting...";
			yield return (object)new WaitUntil((Func<bool>)(() => PollManager.SubmissionResult != PollManager.EPollSubmissionResult.InProgress));
			if (PollManager.SubmissionResult == PollManager.EPollSubmissionResult.Success)
			{
				DisplaySubmittedAnswer(buttonIndex);
				SubmissionSuccessSound.Play();
			}
			else
			{
				((TMP_Text)ConfirmationMessageLabel).text = "Error: " + PollManager.SubmisssionFailedMesssage;
				((Graphic)ConfirmationMessageLabel).color = TextColor_Red;
				SubmissionFailSound.Play();
			}
		}
	}
}
