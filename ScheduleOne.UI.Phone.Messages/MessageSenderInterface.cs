using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Messaging;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone.Messages;

public class MessageSenderInterface : MonoBehaviour
{
	public enum EVisibility
	{
		Hidden,
		Docked,
		Expanded
	}

	public EVisibility Visibility;

	[Header("Settings")]
	public float DockedMenuYPos;

	public float ExpandedMenuYPos;

	[Header("References")]
	public RectTransform Menu;

	public RectTransform SendablesContainer;

	public RectTransform[] DockedUIElements;

	public RectTransform[] ExpandedUIElements;

	public Button ComposeButton;

	public Button[] CancelButtons;

	private List<MessageBubble> sendableBubbles = new List<MessageBubble>();

	private Dictionary<MessageBubble, SendableMessage> sendableMap = new Dictionary<MessageBubble, SendableMessage>();

	private List<UISelectable> bubbleUISelectables = new List<UISelectable>();

	public UIPanel dialogueScreenUIPanel { get; set; }

	public void Awake()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		((UnityEvent)ComposeButton.onClick).AddListener((UnityAction)delegate
		{
			SetVisibility(EVisibility.Expanded);
		});
		Button[] cancelButtons = CancelButtons;
		for (int num = 0; num < cancelButtons.Length; num++)
		{
			((UnityEvent)cancelButtons[num].onClick).AddListener((UnityAction)delegate
			{
				SetVisibility(EVisibility.Docked);
			});
		}
	}

	public void Start()
	{
		GameInput.RegisterExitListener(Exit, 15);
		SetVisibility(EVisibility.Hidden);
	}

	private void Exit(ExitAction exit)
	{
		if (!exit.Used && Visibility == EVisibility.Expanded)
		{
			SetVisibility(EVisibility.Docked);
			exit.Used = true;
		}
	}

	public void SetVisibility(EVisibility visibility)
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		Visibility = visibility;
		RectTransform[] dockedUIElements = DockedUIElements;
		for (int i = 0; i < dockedUIElements.Length; i++)
		{
			((Component)dockedUIElements[i]).gameObject.SetActive(visibility == EVisibility.Docked);
		}
		dockedUIElements = ExpandedUIElements;
		for (int i = 0; i < dockedUIElements.Length; i++)
		{
			((Component)dockedUIElements[i]).gameObject.SetActive(visibility == EVisibility.Expanded);
		}
		if (visibility == EVisibility.Expanded)
		{
			UpdateSendables();
		}
		((Component)SendablesContainer).gameObject.SetActive(visibility == EVisibility.Expanded);
		Menu.anchoredPosition = new Vector2(0f, (Visibility == EVisibility.Expanded) ? ExpandedMenuYPos : DockedMenuYPos);
		((Component)this).gameObject.SetActive(visibility != EVisibility.Hidden);
		for (int j = 0; j < sendableBubbles.Count; j++)
		{
			sendableBubbles[j].RefreshDisplayedText();
		}
		dialogueScreenUIPanel.ClearAllSelectables();
		switch (visibility)
		{
		case EVisibility.Expanded:
			foreach (UISelectable bubbleUISelectable in bubbleUISelectables)
			{
				dialogueScreenUIPanel.AddSelectable(bubbleUISelectable);
			}
			dialogueScreenUIPanel.SelectSelectable(returnFirstFound: true);
			break;
		case EVisibility.Docked:
			dialogueScreenUIPanel.AddSelectable(((Component)ComposeButton).GetComponent<UISelectable>());
			dialogueScreenUIPanel.SelectSelectable(returnFirstFound: true);
			break;
		}
	}

	public void UpdateSendables()
	{
		for (int i = 0; i < sendableBubbles.Count; i++)
		{
			SendableMessage sendableMessage = sendableMap[sendableBubbles[i]];
			string invalidReason;
			if (!sendableMessage.ShouldShow())
			{
				((Component)sendableBubbles[i]).gameObject.SetActive(false);
			}
			else if (sendableMessage.IsValid(out invalidReason))
			{
				((Selectable)sendableBubbles[i].button).interactable = true;
				((Component)sendableBubbles[i]).gameObject.SetActive(true);
			}
			else
			{
				((Selectable)sendableBubbles[i].button).interactable = false;
				((Component)sendableBubbles[i]).gameObject.SetActive(false);
			}
		}
	}

	public void AddSendable(SendableMessage sendable)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		MessageBubble component = Object.Instantiate<GameObject>(PlayerSingleton<MessagesApp>.Instance.messageBubblePrefab, (Transform)(object)SendablesContainer).GetComponent<MessageBubble>();
		component.SetupBubble(sendable.Text, MessageBubble.Alignment.Center, alignCenter: true);
		((UnityEvent)component.button.onClick).AddListener((UnityAction)delegate
		{
			SendableSelected(sendable);
		});
		sendableBubbles.Add(component);
		sendableMap.Add(component, sendable);
		UpdateSendables();
		bubbleUISelectables.Add(((Component)component).GetComponentInChildren<UISelectable>());
	}

	protected virtual void SendableSelected(SendableMessage sendable)
	{
		sendable.Send(network: true);
		SetVisibility(EVisibility.Hidden);
	}
}
