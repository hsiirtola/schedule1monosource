using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.ItemFramework;
using ScheduleOne.Money;
using ScheduleOne.NPCs;
using ScheduleOne.NPCs.Relation;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using ScheduleOne.UI;
using ScheduleOne.UI.Phone;
using ScheduleOne.UI.Phone.Messages;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.Messaging;

[Serializable]
public class MSGConversation : ISaveable
{
	public const int MAX_MESSAGE_HISTORY = 10;

	public string contactName = string.Empty;

	public NPC sender;

	public List<Message> messageHistory = new List<Message>();

	public List<MessageChain> messageChainHistory = new List<MessageChain>();

	public List<MessageBubble> bubbles = new List<MessageBubble>();

	public List<SendableMessage> Sendables = new List<SendableMessage>();

	public List<EConversationCategory> Categories = new List<EConversationCategory>();

	public RectTransform entry;

	protected RectTransform container;

	protected RectTransform bubbleContainer;

	protected RectTransform scrollRectContainer;

	protected ScrollRect scrollRect;

	protected Text entryPreviewText;

	protected RectTransform unreadDot;

	protected Slider slider;

	protected Image sliderFill;

	protected RectTransform responseContainer;

	protected MessageSenderInterface senderInterface;

	protected UISelectable uiSelectable;

	protected UIPanel dialogueScreenUIPanel;

	private bool uiCreated;

	public Action onMessageRendered;

	public Action onLoaded;

	public Action onResponsesShown;

	public Action onConversationOpened;

	public List<Response> currentResponses = new List<Response>();

	private List<RectTransform> responseRects = new List<RectTransform>();

	public bool IsSenderKnown { get; protected set; } = true;

	public bool Read { get; private set; } = true;

	public int index { get; protected set; }

	public bool isOpen { get; protected set; }

	public bool rollingOut { get; protected set; }

	public bool EntryVisible { get; protected set; } = true;

	public UISelectable UISelectable => uiSelectable;

	public bool AreResponsesActive => currentResponses.Count > 0;

	public string SaveFolderName => "MessageConversation";

	public string SaveFileName => "MessageConversation";

	public Loader Loader => null;

	public bool ShouldSaveUnderFolder => false;

	public List<string> LocalExtraFiles { get; set; } = new List<string>();

	public List<string> LocalExtraFolders { get; set; } = new List<string>();

	public bool HasChanged { get; set; }

	public MSGConversation(NPC _npc, string _contactName)
	{
		contactName = _contactName;
		sender = _npc;
		MessagesApp.Conversations.Insert(0, this);
		index = 0;
		NetworkSingleton<MessagingManager>.Instance.Register(_npc, this);
		InitializeSaveable();
	}

	public virtual void InitializeSaveable()
	{
		Singleton<SaveManager>.Instance.RegisterSaveable(this);
	}

	public void SetCategories(List<EConversationCategory> cat)
	{
		Categories = cat;
	}

	public void MoveToTop()
	{
		MessagesApp.ActiveConversations.Remove(this);
		MessagesApp.ActiveConversations.Insert(0, this);
		index = 0;
		PlayerSingleton<MessagesApp>.Instance.RepositionEntries();
	}

	public bool ShouldReplicate()
	{
		if (messageHistory.Count <= 0)
		{
			return messageChainHistory.Count > 0;
		}
		return true;
	}

	public int GetReplicationByteSize()
	{
		int num = 32;
		return 0 + messageHistory.Count * num + messageChainHistory.Count * num * 2;
	}

	protected void CreateUI()
	{
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Expected O, but got Unknown
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Expected O, but got Unknown
		if (uiCreated)
		{
			return;
		}
		uiCreated = true;
		PlayerSingleton<MessagesApp>.Instance.CreateConversationUI(this, out entry, out container);
		MessagesApp.ActiveConversations.Add(this);
		entryPreviewText = ((Component)((Transform)entry).Find("Preview")).GetComponent<Text>();
		unreadDot = ((Component)((Transform)entry).Find("UnreadDot")).GetComponent<RectTransform>();
		slider = ((Component)((Transform)entry).Find("Slider")).GetComponent<Slider>();
		sliderFill = ((Component)slider.fillRect).GetComponent<Image>();
		((UnityEvent)((Component)((Transform)entry).Find("Button")).GetComponent<Button>().onClick).AddListener(new UnityAction(EntryClicked));
		uiSelectable = ((Component)((Transform)entry).Find("Button")).GetComponent<UISelectable>();
		PlayerSingleton<MessagesApp>.Instance.mainMessagesUIPanel.AddSelectable(uiSelectable);
		dialogueScreenUIPanel = ((Component)container).GetComponent<UIPanel>();
		Button component = ((Component)((Transform)entry).Find("Hide")).GetComponent<Button>();
		if (sender.ConversationCanBeHidden)
		{
			((Component)component).gameObject.SetActive(true);
			((UnityEvent)component.onClick).AddListener((UnityAction)delegate
			{
				SetEntryVisibility(v: false);
			});
		}
		else
		{
			((Component)component).gameObject.SetActive(false);
		}
		scrollRectContainer = ((Component)((Transform)container).Find("ScrollContainer")).GetComponent<RectTransform>();
		scrollRect = ((Component)((Transform)scrollRectContainer).Find("ScrollRect")).GetComponent<ScrollRect>();
		bubbleContainer = ((Component)((Component)scrollRect).transform.Find("Viewport/Content")).GetComponent<RectTransform>();
		entryPreviewText.text = string.Empty;
		((Component)unreadDot).gameObject.SetActive(!Read && messageHistory.Count > 0);
		responseContainer = ((Component)((Transform)container).Find("Responses")).GetComponent<RectTransform>();
		senderInterface = ((Component)((Transform)container).Find("SenderInterface")).GetComponent<MessageSenderInterface>();
		senderInterface.dialogueScreenUIPanel = dialogueScreenUIPanel;
		for (int num = 0; num < Sendables.Count; num++)
		{
			senderInterface.AddSendable(Sendables[num]);
		}
		RepositionEntry();
		SetResponseContainerVisible(v: false);
		SetOpen(open: false);
	}

	public void EnsureUIExists()
	{
		if (!uiCreated)
		{
			CreateUI();
		}
	}

	protected void RefreshPreviewText()
	{
		if (bubbles.Count == 0)
		{
			entryPreviewText.text = string.Empty;
		}
		else
		{
			entryPreviewText.text = bubbles[bubbles.Count - 1].text;
		}
	}

	public void RepositionEntry()
	{
		if (!((Object)(object)entry == (Object)null))
		{
			((Transform)entry).SetSiblingIndex(MessagesApp.ActiveConversations.IndexOf(this));
		}
	}

	public void SetIsKnown(bool known)
	{
		IsSenderKnown = known;
		if ((Object)(object)entry != (Object)null)
		{
			((Component)((Transform)entry).Find("Name")).GetComponent<Text>().text = (IsSenderKnown ? contactName : "Unknown");
			((Component)((Transform)entry).Find("IconMask/Icon")).GetComponent<Image>().sprite = (IsSenderKnown ? sender.GetMessagingIcon() : PlayerSingleton<MessagesApp>.Instance.BlankAvatarSprite);
		}
	}

	public void EntryClicked()
	{
		SetOpen(open: true);
	}

	public void SetOpen(bool open)
	{
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		isOpen = open;
		PlayerSingleton<MessagesApp>.Instance.homePage.gameObject.SetActive(!open);
		PlayerSingleton<MessagesApp>.Instance.dialoguePage.gameObject.SetActive(open);
		if (open)
		{
			PlayerSingleton<MessagesApp>.Instance.SetCurrentConversation(this);
			((Component)PlayerSingleton<MessagesApp>.Instance.relationshipContainer).gameObject.SetActive(false);
			((Component)PlayerSingleton<MessagesApp>.Instance.standardsContainer).gameObject.SetActive(false);
			((Component)PlayerSingleton<MessagesApp>.Instance.debtContainer).gameObject.SetActive(false);
			if (sender.ShowRelationshipInfo)
			{
				DisplayRelationshipInfo();
			}
			PlayerSingleton<MessagesApp>.Instance.dialoguePageNameText.text = (IsSenderKnown ? contactName : "Unknown");
			((Graphic)PlayerSingleton<MessagesApp>.Instance.dialoguePageNameText).rectTransform.anchoredPosition = new Vector2((0f - PlayerSingleton<MessagesApp>.Instance.dialoguePageNameText.preferredWidth) / 2f + 30f, sender.ShowRelationshipInfo ? 20f : 0f);
			PlayerSingleton<MessagesApp>.Instance.iconContainerRect.anchoredPosition = new Vector2((0f - PlayerSingleton<MessagesApp>.Instance.dialoguePageNameText.preferredWidth) / 2f - 30f, PlayerSingleton<MessagesApp>.Instance.iconContainerRect.anchoredPosition.y);
			PlayerSingleton<MessagesApp>.Instance.iconImage.sprite = (IsSenderKnown ? sender.GetMessagingIcon() : PlayerSingleton<MessagesApp>.Instance.BlankAvatarSprite);
			SetRead(r: true);
			CheckSendLoop();
			for (int i = 0; i < responseRects.Count; i++)
			{
				((Component)responseRects[i]).gameObject.GetComponent<MessageBubble>().RefreshDisplayedText();
			}
			for (int j = 0; j < bubbles.Count; j++)
			{
				bubbles[j].autosetPosition = false;
				bubbles[j].RefreshDisplayedText();
			}
			if (onConversationOpened != null)
			{
				onConversationOpened();
			}
			Singleton<UIScreenManager>.Instance.AddScreen(PlayerSingleton<MessagesApp>.Instance.dialogueMainUIScreen);
		}
		else
		{
			PlayerSingleton<MessagesApp>.Instance.SetCurrentConversation(null);
			Singleton<UIScreenManager>.Instance.RemoveScreen(PlayerSingleton<MessagesApp>.Instance.dialogueMainUIScreen);
		}
		((Component)container).gameObject.SetActive(open);
		SetResponseContainerVisible(AreResponsesActive);
		if (open)
		{
			PlayerSingleton<MessagesApp>.Instance.dialogueMainUIScreen.AddPanel(dialogueScreenUIPanel);
			PlayerSingleton<MessagesApp>.Instance.SelectDialogueUIPanel(dialogueScreenUIPanel);
			PlayerSingleton<MessagesApp>.Instance.dialogueMainUIScreen.ChangeActiveScrollRect(scrollRect);
		}
		else
		{
			PlayerSingleton<MessagesApp>.Instance.dialogueMainUIScreen.RemovePanel(dialogueScreenUIPanel);
		}
	}

	public void DisplayRelationshipInfo()
	{
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		if (isOpen)
		{
			PlayerSingleton<MessagesApp>.Instance.relationshipScrollbar.value = sender.RelationData.NormalizedRelationDelta;
			PlayerSingleton<MessagesApp>.Instance.relationshipTooltip.text = RelationshipCategory.GetCategory(sender.RelationData.RelationDelta).ToString();
			((Component)PlayerSingleton<MessagesApp>.Instance.relationshipContainer).gameObject.SetActive(true);
			Customer customer = default(Customer);
			Supplier supplier = default(Supplier);
			if (((Component)sender).TryGetComponent<Customer>(ref customer))
			{
				((Graphic)PlayerSingleton<MessagesApp>.Instance.standardsStar).color = ItemQuality.GetColor(customer.CustomerData.Standards.GetCorrespondingQuality());
				PlayerSingleton<MessagesApp>.Instance.standardsTooltip.text = customer.CustomerData.Standards.GetName() + " standards.";
				((Component)PlayerSingleton<MessagesApp>.Instance.standardsContainer).gameObject.SetActive(true);
			}
			else if (((Component)sender).TryGetComponent<Supplier>(ref supplier))
			{
				((Component)PlayerSingleton<MessagesApp>.Instance.debtContainer).gameObject.SetActive(true);
				PlayerSingleton<MessagesApp>.Instance.debtLabel.text = MoneyManager.FormatAmount(supplier.Debt);
			}
		}
	}

	protected virtual void RenderMessage(Message m)
	{
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		MessageBubble component = Object.Instantiate<GameObject>(PlayerSingleton<MessagesApp>.Instance.messageBubblePrefab, (Transform)(object)bubbleContainer).GetComponent<MessageBubble>();
		component.SetupBubble(m.text, (m.sender == Message.ESenderType.Other) ? MessageBubble.Alignment.Left : MessageBubble.Alignment.Right);
		float num = 0f;
		for (int i = 0; i < bubbles.Count; i++)
		{
			num += bubbles[i].height;
			num += bubbles[i].spacingAbove;
		}
		bool flag = false;
		if (messageHistory.IndexOf(m) > 0 && messageHistory[messageHistory.IndexOf(m) - 1].sender == m.sender)
		{
			flag = true;
		}
		float num2 = MessageBubble.baseBubbleSpacing;
		if (!flag)
		{
			num2 *= 10f;
		}
		if (flag && messageHistory[messageHistory.IndexOf(m) - 1].endOfGroup)
		{
			num2 *= 20f;
		}
		component.container.anchoredPosition = new Vector2(component.container.anchoredPosition.x, 0f - num - num2 - component.height / 2f);
		component.spacingAbove = num2;
		component.showTriangle = true;
		if (flag && !messageHistory[messageHistory.IndexOf(m) - 1].endOfGroup)
		{
			bubbles[bubbles.Count - 1].showTriangle = false;
		}
		bubbleContainer.sizeDelta = new Vector2(bubbleContainer.sizeDelta.x, num + component.height + num2 + MessageBubble.baseBubbleSpacing * 10f);
		scrollRect.verticalNormalizedPosition = 0f;
		bubbles.Add(component);
		if (m.sender == Message.ESenderType.Player && PlayerSingleton<MessagesApp>.Instance.isOpen && PlayerSingleton<Phone>.Instance.IsOpen)
		{
			PlayerSingleton<MessagesApp>.Instance.MessageSentSound.Play();
		}
		else if (PlayerSingleton<Phone>.Instance.IsOpen && PlayerSingleton<MessagesApp>.Instance.isOpen && (isOpen || PlayerSingleton<MessagesApp>.Instance.currentConversation == null))
		{
			PlayerSingleton<MessagesApp>.Instance.MessageReceivedSound.Play();
		}
		if (onMessageRendered != null)
		{
			onMessageRendered();
		}
	}

	public void SetEntryVisibility(bool v)
	{
		if (v || sender.ConversationCanBeHidden)
		{
			EntryVisible = v;
			((Component)entry).gameObject.SetActive(v);
			if (v)
			{
				PlayerSingleton<MessagesApp>.Instance.mainMessagesUIPanel.AddSelectable(uiSelectable);
			}
			else
			{
				PlayerSingleton<MessagesApp>.Instance.mainMessagesUIPanel.RemoveSelectable(uiSelectable);
			}
			if (!v)
			{
				SetRead(r: true);
			}
			HasChanged = true;
		}
	}

	public void SetRead(bool r)
	{
		Read = r;
		if (Read)
		{
			if (PlayerSingleton<MessagesApp>.Instance.unreadConversations.Contains(this))
			{
				PlayerSingleton<MessagesApp>.Instance.unreadConversations.Remove(this);
				PlayerSingleton<MessagesApp>.Instance.RefreshNotifications();
			}
		}
		else if (!PlayerSingleton<MessagesApp>.Instance.unreadConversations.Contains(this))
		{
			PlayerSingleton<MessagesApp>.Instance.unreadConversations.Add(this);
			PlayerSingleton<MessagesApp>.Instance.RefreshNotifications();
		}
		if ((Object)(object)unreadDot != (Object)null)
		{
			((Component)unreadDot).gameObject.SetActive(!Read);
		}
		HasChanged = true;
	}

	public void SendMessage(Message message, bool notify = true, bool network = true)
	{
		EnsureUIExists();
		if (message.messageId == -1)
		{
			message.messageId = Random.Range(int.MinValue, int.MaxValue);
		}
		if (messageHistory.Find((Message x) => x.messageId == message.messageId) != null)
		{
			return;
		}
		if (network)
		{
			NetworkSingleton<MessagingManager>.Instance.SendMessage(message, notify, sender.ID);
			return;
		}
		messageHistory.Add(message);
		if (messageHistory.Count > 10)
		{
			messageHistory.RemoveAt(0);
		}
		if (message.sender == Message.ESenderType.Other && notify)
		{
			SetEntryVisibility(v: true);
			if (!isOpen)
			{
				SetRead(r: false);
			}
			if (!isOpen || !PlayerSingleton<MessagesApp>.Instance.isOpen || !PlayerSingleton<Phone>.Instance.IsOpen)
			{
				Singleton<NotificationsManager>.Instance.SendNotification(IsSenderKnown ? contactName : "Unknown", message.text, PlayerSingleton<MessagesApp>.Instance.AppIcon);
			}
		}
		RenderMessage(message);
		RefreshPreviewText();
		MoveToTop();
		HasChanged = true;
	}

	public void SendMessageChain(MessageChain messages, float initialDelay = 0f, bool notify = true, bool network = true)
	{
		EnsureUIExists();
		if (messages.id == -1)
		{
			messages.id = Random.Range(int.MinValue, int.MaxValue);
		}
		if (messageChainHistory.Find((MessageChain x) => x.id == messages.id) == null)
		{
			if (network)
			{
				NetworkSingleton<MessagingManager>.Instance.SendMessageChain(messages, sender.ID, initialDelay, notify);
				return;
			}
			messageChainHistory.Add(messages);
			HasChanged = true;
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Routine(messages, initialDelay));
		}
		IEnumerator Routine(MessageChain messageChain, float num)
		{
			rollingOut = true;
			List<Message> messageClasses = new List<Message>();
			for (int i = 0; i < messageChain.Messages.Count; i++)
			{
				Message item = new Message(messageChain.Messages[i], Message.ESenderType.Other, i == messageChain.Messages.Count - 1);
				messageHistory.Add(item);
				if (messageHistory.Count > 10)
				{
					messageHistory.RemoveAt(0);
				}
				messageClasses.Add(item);
			}
			yield return (object)new WaitForSeconds(num);
			if (notify && (!isOpen || !PlayerSingleton<MessagesApp>.Instance.isOpen || !PlayerSingleton<Phone>.Instance.IsOpen))
			{
				Singleton<NotificationsManager>.Instance.SendNotification(IsSenderKnown ? contactName : "Unknown", messageChain.Messages[0], PlayerSingleton<MessagesApp>.Instance.AppIcon);
			}
			for (int j = 0; j < messageClasses.Count; j++)
			{
				RenderMessage(messageClasses[j]);
				RefreshPreviewText();
				MoveToTop();
				if (!isOpen && notify)
				{
					SetEntryVisibility(v: true);
					SetRead(r: false);
				}
				if (j + 1 < messageClasses.Count)
				{
					yield return (object)new WaitForSeconds(1f);
				}
			}
			rollingOut = false;
		}
	}

	public MSGConversationData GetSaveData()
	{
		List<TextMessageData> list = new List<TextMessageData>();
		for (int i = 0; i < messageHistory.Count; i++)
		{
			list.Add(messageHistory[i].GetSaveData());
		}
		List<TextResponseData> list2 = new List<TextResponseData>();
		for (int j = 0; j < currentResponses.Count; j++)
		{
			list2.Add(new TextResponseData(currentResponses[j].text, currentResponses[j].label));
		}
		return new MSGConversationData(MessagesApp.ActiveConversations.IndexOf(this), Read, list.ToArray(), list2.ToArray(), !EntryVisible);
	}

	public virtual string GetSaveString()
	{
		return GetSaveData().GetJson();
	}

	public virtual void Load(MSGConversationData data)
	{
		EnsureUIExists();
		ResetConversation();
		index = data.ConversationIndex;
		SetRead(data.Read);
		if (data.MessageHistory != null)
		{
			for (int i = 0; i < data.MessageHistory.Length; i++)
			{
				Message message = new Message(data.MessageHistory[i]);
				messageHistory.Add(message);
				if (messageHistory.Count > 10)
				{
					messageHistory.RemoveAt(0);
				}
				RenderMessage(message);
			}
		}
		else
		{
			Console.LogWarning("Message history null!");
		}
		if (data.ActiveResponses != null)
		{
			List<Response> list = new List<Response>();
			for (int j = 0; j < data.ActiveResponses.Length; j++)
			{
				list.Add(new Response(data.ActiveResponses[j].Text, data.ActiveResponses[j].Label));
			}
			if (list.Count > 0)
			{
				ShowResponses(list);
			}
		}
		else
		{
			Console.LogWarning("Message reponses null!");
		}
		RefreshPreviewText();
		HasChanged = false;
		_ = data.IsHidden;
		if (data.IsHidden)
		{
			SetEntryVisibility(v: false);
		}
		if (onLoaded != null)
		{
			onLoaded();
		}
	}

	public void ResetConversation()
	{
		messageHistory.Clear();
		messageChainHistory.Clear();
		ClearResponses();
		for (int i = 0; i < bubbles.Count; i++)
		{
			Object.Destroy((Object)(object)((Component)bubbles[i]).gameObject);
		}
		bubbles.Clear();
		HasChanged = true;
	}

	public void SetSliderValue(float value, Color color)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)slider == (Object)null))
		{
			slider.value = value;
			((Graphic)sliderFill).color = color;
			((Component)slider).gameObject.SetActive(value > 0f);
		}
	}

	public Response GetResponse(string label)
	{
		return currentResponses.Find((Response x) => x.label == label);
	}

	public void ShowResponses(List<Response> _responses, float showResponseDelay = 0f, bool network = true)
	{
		if (network)
		{
			NetworkSingleton<MessagingManager>.Instance.ShowResponses(sender.ID, _responses, showResponseDelay);
			return;
		}
		EnsureUIExists();
		currentResponses = _responses;
		ClearResponseUI();
		for (int i = 0; i < _responses.Count; i++)
		{
			CreateResponseUI(_responses[i]);
		}
		if (showResponseDelay == 0f)
		{
			SetResponseContainerVisible(v: true);
		}
		else
		{
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Routine());
		}
		HasChanged = true;
		if (onResponsesShown != null)
		{
			onResponsesShown();
		}
		IEnumerator Routine()
		{
			yield return (object)new WaitForSeconds(showResponseDelay);
			SetResponseContainerVisible(v: true);
		}
	}

	protected void CreateResponseUI(Response r)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Expected O, but got Unknown
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		EnsureUIExists();
		MessageBubble component = Object.Instantiate<GameObject>(PlayerSingleton<MessagesApp>.Instance.messageBubblePrefab, (Transform)(object)responseContainer).GetComponent<MessageBubble>();
		float num = 5f;
		float num2 = 25f;
		Rect rect = responseContainer.rect;
		component.bubble_MinWidth = ((Rect)(ref rect)).width - num2 * 2f;
		rect = responseContainer.rect;
		component.bubble_MaxWidth = ((Rect)(ref rect)).width - num2 * 2f;
		component.autosetPosition = false;
		component.SetupBubble(r.text, MessageBubble.Alignment.Center, alignCenter: true);
		float num3 = num2;
		for (int i = 0; i < responseRects.Count; i++)
		{
			num3 += ((Component)responseRects[i]).gameObject.GetComponent<MessageBubble>().height;
			num3 += num;
		}
		component.container.anchoredPosition = new Vector2(0f, 0f - num3 - 35f);
		responseRects.Add(component.container);
		((Selectable)component.button).interactable = true;
		bool network = !r.disableDefaultResponseBehaviour;
		((UnityEvent)component.button.onClick).AddListener((UnityAction)delegate
		{
			ResponseChosen(r, network);
		});
		responseContainer.sizeDelta = new Vector2(responseContainer.sizeDelta.x, num3 + component.height + num2);
		responseContainer.anchoredPosition = new Vector2(0f, responseContainer.sizeDelta.y / 2f);
	}

	protected void ClearResponseUI()
	{
		dialogueScreenUIPanel.ClearAllSelectables();
		for (int i = 0; i < responseRects.Count; i++)
		{
			Object.Destroy((Object)(object)((Component)responseRects[i]).gameObject);
		}
		responseRects.Clear();
	}

	public void SetResponseContainerVisible(bool v)
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		if (v)
		{
			scrollRectContainer.offsetMin = new Vector2(0f, responseContainer.sizeDelta.y);
			dialogueScreenUIPanel.ClearAllSelectables();
			foreach (RectTransform responseRect in responseRects)
			{
				dialogueScreenUIPanel.AddSelectable(((Component)responseRect).GetComponentInChildren<UISelectable>());
			}
			dialogueScreenUIPanel.SelectSelectable(0);
		}
		else
		{
			scrollRectContainer.offsetMin = new Vector2(0f, 0f);
		}
		((Component)responseContainer).gameObject.SetActive(v);
		bubbleContainer.anchoredPosition = new Vector2(bubbleContainer.anchoredPosition.x, Mathf.Clamp(bubbleContainer.anchoredPosition.y, 1100f, float.MaxValue));
	}

	public void ResponseChosen(Response r, bool network)
	{
		if (!AreResponsesActive)
		{
			return;
		}
		if (r.disableDefaultResponseBehaviour)
		{
			if (r.callback != null)
			{
				r.callback();
			}
			return;
		}
		if (network)
		{
			NetworkSingleton<MessagingManager>.Instance.SendResponse(currentResponses.IndexOf(r), sender.ID);
			return;
		}
		ClearResponses();
		RenderMessage(new Message(r.text, Message.ESenderType.Player, _endOfGroup: true));
		HasChanged = true;
		MoveToTop();
		if (r.callback != null)
		{
			r.callback();
		}
	}

	public void ClearResponses(bool network = false)
	{
		ClearResponseUI();
		SetResponseContainerVisible(v: false);
		currentResponses.Clear();
		if (network)
		{
			NetworkSingleton<MessagingManager>.Instance.ClearResponses(sender.ID);
		}
	}

	public SendableMessage CreateSendableMessage(string text)
	{
		SendableMessage sendableMessage = new SendableMessage(text, this);
		Sendables.Add(sendableMessage);
		if (uiCreated)
		{
			senderInterface.AddSendable(sendableMessage);
		}
		return sendableMessage;
	}

	public void SendPlayerMessage(int sendableIndex, int sentIndex, bool network)
	{
		if (network)
		{
			NetworkSingleton<MessagingManager>.Instance.SendPlayerMessage(sendableIndex, sentIndex, sender.ID);
		}
		else
		{
			Sendables[sendableIndex].Send(network: false, sentIndex);
		}
	}

	public void RenderPlayerMessage(SendableMessage sendable)
	{
		Message m = new Message(sendable.Text, Message.ESenderType.Player, _endOfGroup: true);
		RenderMessage(m);
	}

	private void CheckSendLoop()
	{
		CanSendNewMessage();
		((MonoBehaviour)PlayerSingleton<MessagesApp>.Instance).StartCoroutine(Loop());
		IEnumerator Loop()
		{
			while (isOpen)
			{
				if (CanSendNewMessage())
				{
					if (senderInterface.Visibility == MessageSenderInterface.EVisibility.Hidden)
					{
						senderInterface.SetVisibility(MessageSenderInterface.EVisibility.Docked);
					}
				}
				else if (senderInterface.Visibility != MessageSenderInterface.EVisibility.Hidden)
				{
					senderInterface.SetVisibility(MessageSenderInterface.EVisibility.Hidden);
				}
				((Component)scrollRect).GetComponent<RectTransform>().offsetMin = new Vector2(0f, (senderInterface.Visibility == MessageSenderInterface.EVisibility.Docked) ? 200f : 0f);
				yield return (object)new WaitForEndOfFrame();
			}
			senderInterface.SetVisibility(MessageSenderInterface.EVisibility.Hidden);
			((Component)scrollRect).GetComponent<RectTransform>().offsetMin = new Vector2(0f, 0f);
		}
	}

	private bool CanSendNewMessage()
	{
		if (rollingOut)
		{
			return false;
		}
		if (AreResponsesActive)
		{
			return false;
		}
		if (Sendables.FirstOrDefault((SendableMessage x) => x.ShouldShow()) == null)
		{
			return false;
		}
		return true;
	}
}
