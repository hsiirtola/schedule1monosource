using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.Messaging;
using ScheduleOne.Persistence;
using ScheduleOne.UI.Tooltips;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone.Messages;

public class MessagesApp : App<MessagesApp>
{
	[Serializable]
	public class CategoryInfo
	{
		public EConversationCategory Category;

		public string Name;

		public Color Color;
	}

	public static List<MSGConversation> Conversations = new List<MSGConversation>();

	public static List<MSGConversation> ActiveConversations = new List<MSGConversation>();

	public List<CategoryInfo> categoryInfos;

	[Header("References")]
	[SerializeField]
	protected RectTransform conversationEntryContainer;

	[SerializeField]
	protected RectTransform conversationContainer;

	public GameObject homePage;

	public GameObject dialoguePage;

	public Text dialoguePageNameText;

	public RectTransform relationshipContainer;

	public Scrollbar relationshipScrollbar;

	public Tooltip relationshipTooltip;

	public RectTransform debtContainer;

	public Text debtLabel;

	public RectTransform standardsContainer;

	public Image standardsStar;

	public Tooltip standardsTooltip;

	public RectTransform iconContainerRect;

	public Image iconImage;

	public Sprite BlankAvatarSprite;

	public DealWindowSelector DealWindowSelector;

	public PhoneShopInterface PhoneShopInterface;

	public CounterofferInterface CounterofferInterface;

	public RectTransform ClearFilterButton;

	public Button[] CategoryButtons;

	public AudioSourceController MessageReceivedSound;

	public AudioSourceController MessageSentSound;

	public ConfirmationPopup ConfirmationPopup;

	[Header("Prefabs")]
	[SerializeField]
	protected GameObject conversationEntryPrefab;

	[SerializeField]
	protected GameObject conversationContainerPrefab;

	public GameObject messageBubblePrefab;

	public List<MSGConversation> unreadConversations = new List<MSGConversation>();

	[Header("Custom UI")]
	public UIScreen mainMessagesUIScreen;

	public UIPanel mainMessagesUIPanel;

	public UIScreen dialogueMainUIScreen;

	public MSGConversation currentConversation { get; private set; }

	protected override void Start()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Expected O, but got Unknown
		base.Start();
		Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(new UnityAction(Loaded));
		Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(Loaded));
		Singleton<LoadManager>.Instance.onPreSceneChange.RemoveListener(new UnityAction(Clean));
		Singleton<LoadManager>.Instance.onPreSceneChange.AddListener(new UnityAction(Clean));
		dialoguePage.gameObject.SetActive(false);
		mainMessagesUIScreen.SetCurrentSelectedPanel(mainMessagesUIPanel);
	}

	protected override void Update()
	{
		base.Update();
	}

	private void Loaded()
	{
		ActiveConversations = ActiveConversations.OrderBy((MSGConversation x) => x.index).ToList();
		RepositionEntries();
	}

	private void Clean()
	{
		Conversations.Clear();
		ActiveConversations.Clear();
	}

	public void CreateConversationUI(MSGConversation c, out RectTransform entry, out RectTransform container)
	{
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		entry = Object.Instantiate<GameObject>(conversationEntryPrefab, (Transform)(object)conversationEntryContainer).GetComponent<RectTransform>();
		((Component)((Transform)entry).Find("Name")).GetComponent<Text>().text = (c.IsSenderKnown ? c.contactName : "Unknown");
		((Component)((Transform)entry).Find("IconMask/Icon")).GetComponent<Image>().sprite = (c.IsSenderKnown ? c.sender.GetMessagingIcon() : BlankAvatarSprite);
		((Transform)entry).SetAsLastSibling();
		if (c.Categories != null && c.Categories.Count > 0)
		{
			CategoryInfo categoryInfo = GetCategoryInfo(c.Categories[0]);
			RectTransform component = ((Component)((Transform)entry).Find("Category")).GetComponent<RectTransform>();
			Text component2 = ((Component)((Transform)component).Find("Label")).GetComponent<Text>();
			component2.text = categoryInfo.Name[0].ToString();
			LayoutRebuilder.ForceRebuildLayoutImmediate(((Graphic)component2).rectTransform);
			((Graphic)((Component)component).GetComponent<Image>()).color = categoryInfo.Color;
			component.anchoredPosition = new Vector2(225f + ((Component)((Transform)entry).Find("Name")).GetComponent<Text>().preferredWidth, component.anchoredPosition.y);
			((Component)component).gameObject.SetActive(true);
		}
		else
		{
			((Component)((Transform)entry).Find("Category")).gameObject.SetActive(false);
		}
		container = Object.Instantiate<GameObject>(conversationContainerPrefab, (Transform)(object)conversationContainer).GetComponent<RectTransform>();
		RepositionEntries();
	}

	public void RepositionEntries()
	{
		for (int i = 0; i < ActiveConversations.Count; i++)
		{
			ActiveConversations[i].RepositionEntry();
		}
		for (int j = 0; j < ActiveConversations.Count; j++)
		{
			ActiveConversations[j].RepositionEntry();
		}
	}

	public void ReturnButtonClicked()
	{
		if (currentConversation != null)
		{
			currentConversation.SetOpen(open: false);
		}
	}

	public void RefreshNotifications()
	{
		SetNotificationCount(unreadConversations.Count);
		((Component)Singleton<HUD>.Instance.UnreadMessagesPrompt).gameObject.SetActive(unreadConversations.Count > 0);
	}

	public override void Exit(ExitAction exit)
	{
		if (!base.isOpen || exit.Used || !PlayerSingleton<Phone>.Instance.IsOpen)
		{
			base.Exit(exit);
			return;
		}
		if (currentConversation != null)
		{
			currentConversation.SetOpen(open: false);
			exit.Used = true;
		}
		base.Exit(exit);
	}

	public void SetCurrentConversation(MSGConversation conversation)
	{
		if (conversation != currentConversation)
		{
			MSGConversation mSGConversation = currentConversation;
			currentConversation = conversation;
			mSGConversation?.SetOpen(open: false);
		}
	}

	public CategoryInfo GetCategoryInfo(EConversationCategory category)
	{
		return categoryInfos.Find((CategoryInfo x) => x.Category == category);
	}

	public void FilterByCategory(int category)
	{
		for (int i = 0; i < CategoryButtons.Length; i++)
		{
			((Selectable)CategoryButtons[i]).interactable = true;
		}
		for (int j = 0; j < ActiveConversations.Count; j++)
		{
			((Component)ActiveConversations[j].entry).gameObject.SetActive(ActiveConversations[j].Categories.Contains((EConversationCategory)category));
		}
		((Component)ClearFilterButton).gameObject.SetActive(true);
	}

	public void ClearFilter()
	{
		for (int i = 0; i < ActiveConversations.Count; i++)
		{
			((Component)ActiveConversations[i].entry).gameObject.SetActive(true);
		}
		for (int j = 0; j < CategoryButtons.Length; j++)
		{
			((Selectable)CategoryButtons[j]).interactable = true;
		}
		((Component)ClearFilterButton).gameObject.SetActive(false);
	}

	public override void SetOpen(bool open)
	{
		base.SetOpen(open);
		SelectMessageSelectable();
	}

	protected override void OnPhoneOpened()
	{
		base.OnPhoneOpened();
		SelectMessageSelectable();
	}

	private void SelectMessageSelectable()
	{
		if (base.isOpen)
		{
			if ((Object)(object)mainMessagesUIPanel.CurrentSelectedSelectable == (Object)null)
			{
				((MonoBehaviour)this).StartCoroutine(DelaySelect());
			}
			else
			{
				((MonoBehaviour)this).StartCoroutine(DelaySelectCurrentSelectedSelectable());
			}
		}
	}

	private IEnumerator DelaySelectCurrentSelectedSelectable()
	{
		yield return null;
		mainMessagesUIPanel.SelectSelectable(mainMessagesUIPanel.CurrentSelectedSelectable);
	}

	private IEnumerator DelaySelect()
	{
		yield return null;
		if (mainMessagesUIPanel.Selectables.Count == 1)
		{
			mainMessagesUIPanel.SelectSelectable(0);
		}
		else
		{
			if (mainMessagesUIPanel.Selectables.Count <= 1)
			{
				yield break;
			}
			MSGConversation mSGConversation = null;
			int num = int.MaxValue;
			foreach (MSGConversation activeConversation in ActiveConversations)
			{
				if (mainMessagesUIPanel.Selectables.Contains(activeConversation.UISelectable))
				{
					int siblingIndex = ((Transform)activeConversation.entry).GetSiblingIndex();
					if (siblingIndex < num)
					{
						num = siblingIndex;
						mSGConversation = activeConversation;
					}
				}
			}
			if (mSGConversation != null)
			{
				mainMessagesUIPanel.SelectSelectable(mSGConversation.UISelectable);
			}
		}
	}

	public void SelectDialogueUIPanel(UIPanel uIPanel)
	{
		((MonoBehaviour)this).StartCoroutine(DelaySelectDialogueUIPanel(uIPanel));
	}

	private IEnumerator DelaySelectDialogueUIPanel(UIPanel uIPanel)
	{
		yield return null;
		dialogueMainUIScreen.SetCurrentSelectedPanel(uIPanel);
		uIPanel.SelectSelectable(returnFirstFound: true);
	}
}
