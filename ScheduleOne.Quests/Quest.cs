using System;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Levelling;
using ScheduleOne.Map;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using ScheduleOne.UI.Compass;
using ScheduleOne.UI.Phone;
using ScheduleOne.UI.Phone.Map;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScheduleOne.Quests;

[Serializable]
public class Quest : MonoBehaviour, IGUIDRegisterable, ISaveable
{
	public const int MAX_HUD_ENTRY_LABELS = 10;

	public const int CriticalExpiryThreshold = 120;

	public static List<Quest> Quests = new List<Quest>();

	public static Quest HoveredQuest = null;

	public static List<Quest> ActiveQuests = new List<Quest>();

	[Header("Basic Settings")]
	[SerializeField]
	protected string title = string.Empty;

	public string Subtitle = string.Empty;

	public Action onSubtitleChanged;

	[TextArea(3, 10)]
	public string Description = string.Empty;

	public string StaticGUID = string.Empty;

	public bool TrackOnBegin;

	public EExpiryVisibility ExpiryVisibility;

	public bool AutoCompleteOnAllEntriesComplete;

	public bool PlayQuestCompleteSound = true;

	public int CompletionXP;

	[Header("Entries")]
	public bool AutoStartFirstEntry = true;

	public List<QuestEntry> Entries = new List<QuestEntry>();

	[Header("UI")]
	public RectTransform IconPrefab;

	[Header("PoI Settings")]
	public GameObject PoIPrefab;

	[Header("Events")]
	public UnityEvent onQuestBegin;

	public UnityEvent<EQuestState> onQuestEnd;

	public UnityEvent onActiveState;

	public UnityEvent<bool> onTrackChange;

	public UnityEvent onComplete;

	public UnityEvent onInitialComplete;

	[Header("Reminders")]
	public bool ShouldSendExpiryReminder = true;

	public bool ShouldSendExpiredNotification = true;

	protected RectTransform journalEntry;

	protected RectTransform entryTitleRect;

	protected RectTransform trackedRect;

	protected Text entryTimeLabel;

	protected Image criticalTimeBackground;

	protected RectTransform detailPanel;

	public Action onHudUICreated;

	private bool expiryReminderSent;

	private CompassManager.Element compassElement;

	protected bool autoInitialize = true;

	public EQuestState State { get; protected set; }

	public Guid GUID { get; protected set; }

	public bool IsTracked { get; protected set; }

	public int ActiveEntryCount => Entries.Count((QuestEntry x) => x.State == EQuestState.Active);

	public string Title => GetQuestTitle();

	public bool Expires { get; protected set; }

	public GameDateTime Expiry { get; protected set; }

	public bool hudUIExists => (Object)(object)hudUI != (Object)null;

	public QuestHUDUI hudUI { get; private set; }

	public string SaveFolderName => "Quest_" + GUID.ToString().Substring(0, 6);

	public string SaveFileName => "Quest_" + GUID.ToString().Substring(0, 6);

	public Loader Loader => null;

	public bool ShouldSaveUnderFolder => false;

	public List<string> LocalExtraFolders { get; set; } = new List<string>();

	public List<string> LocalExtraFiles { get; set; } = new List<string>();

	public bool HasChanged { get; set; }

	protected virtual void Awake()
	{
	}

	protected virtual void Start()
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		if (autoInitialize)
		{
			if ((Object)(object)Player.Local != (Object)null)
			{
				Initialize();
			}
			else
			{
				Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, new Action(Initialize));
			}
		}
		if (AutoCompleteOnAllEntriesComplete)
		{
			for (int i = 0; i < Entries.Count; i++)
			{
				Entries[i].onComplete.AddListener(new UnityAction(CheckAutoComplete));
			}
		}
		void Initialize()
		{
			Player.onLocalPlayerSpawned = (Action)Delegate.Remove(Player.onLocalPlayerSpawned, new Action(Initialize));
			if (!GUIDManager.IsGUIDValid(StaticGUID))
			{
				Console.LogWarning("Invalid GUID for quest: " + title + " Generating random GUID");
				StaticGUID = GUIDManager.GenerateUniqueGUID().ToString();
			}
			QuestEntryData[] entries = new QuestEntryData[0];
			InitializeQuest(title, Description, entries, StaticGUID);
		}
	}

	public virtual void InitializeQuest(string title, string description, QuestEntryData[] entries, string guid)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		if (guid == string.Empty)
		{
			guid = Guid.NewGuid().ToString();
		}
		if (entries.Length == 0 && Entries.Count == 0)
		{
			Console.LogWarning(title + " quest has no entries!");
		}
		((Object)((Component)this).gameObject).name = title;
		for (int i = 0; i < entries.Length; i++)
		{
			GameObject val = new GameObject(entries[i].Name);
			val.transform.SetParent(((Component)this).transform);
			QuestEntry questEntry = val.AddComponent<QuestEntry>();
			Entries.Add(questEntry);
			questEntry.SetData(entries[i]);
		}
		GUID = new Guid(guid);
		GUIDManager.RegisterObject(this);
		this.title = title;
		Description = description;
		HasChanged = true;
		Quests.Add(this);
		InitializeSaveable();
		SetupJournalEntry();
		SetupHUDUI();
		NetworkSingleton<TimeManager>.Instance.onMinutePass += new Action(OnMinPass);
		NetworkSingleton<TimeManager>.Instance.onUncappedMinutePass += new Action(OnUncappedMinPass);
	}

	public virtual void InitializeSaveable()
	{
		Singleton<SaveManager>.Instance.RegisterSaveable(this);
	}

	public void ConfigureExpiry(bool expires, GameDateTime expiry)
	{
		Expires = expires;
		Expiry = expiry;
	}

	public virtual void Begin(bool network = true)
	{
		if (State != EQuestState.Active)
		{
			SetQuestState(EQuestState.Active, network: false);
			if (AutoStartFirstEntry && Entries.Count > 0)
			{
				Entries[0].SetState(EQuestState.Active, network);
			}
			if (TrackOnBegin)
			{
				SetIsTracked(tracked: true);
			}
			UpdateHUDUI();
			if (network)
			{
				NetworkSingleton<QuestManager>.Instance.SendQuestAction(GUID.ToString(), QuestManager.EQuestAction.Begin);
			}
			if (onQuestBegin != null)
			{
				onQuestBegin.Invoke();
			}
		}
	}

	public virtual void Complete(bool network = true)
	{
		if (State != EQuestState.Completed)
		{
			if (CompletionXP > 0 && InstanceFinder.IsServer && !Singleton<LoadManager>.Instance.IsLoading)
			{
				Console.Log("Adding XP for quest: " + Title);
				NetworkSingleton<LevelManager>.Instance.AddXP(CompletionXP);
			}
			SetQuestState(EQuestState.Completed, network: false);
			if (PlayQuestCompleteSound)
			{
				NetworkSingleton<QuestManager>.Instance.PlayCompleteQuestSound();
			}
			End();
			if (network)
			{
				NetworkSingleton<QuestManager>.Instance.SendQuestAction(GUID.ToString(), QuestManager.EQuestAction.Success);
			}
			if (onComplete != null)
			{
				onComplete.Invoke();
			}
			if (2 != 2 && !Singleton<LoadManager>.Instance.IsLoading && onInitialComplete != null)
			{
				onInitialComplete.Invoke();
			}
		}
	}

	public virtual void Fail(bool network = true)
	{
		SetQuestState(EQuestState.Failed, network: false);
		End();
		if (network)
		{
			NetworkSingleton<QuestManager>.Instance.SendQuestAction(GUID.ToString(), QuestManager.EQuestAction.Fail);
		}
	}

	public virtual void Expire(bool network = true)
	{
		if (State != EQuestState.Expired)
		{
			SetQuestState(EQuestState.Expired, network: false);
			if (ShouldSendExpiredNotification)
			{
				SendExpiredNotification();
			}
			End();
			if (network)
			{
				NetworkSingleton<QuestManager>.Instance.SendQuestAction(GUID.ToString(), QuestManager.EQuestAction.Expire);
			}
		}
	}

	public virtual void Cancel(bool network = true)
	{
		SetQuestState(EQuestState.Cancelled, network: false);
		End();
		if (network)
		{
			NetworkSingleton<QuestManager>.Instance.SendQuestAction(GUID.ToString(), QuestManager.EQuestAction.Cancel);
		}
	}

	public virtual void End()
	{
		NetworkSingleton<TimeManager>.Instance.onMinutePass -= new Action(OnMinPass);
		NetworkSingleton<TimeManager>.Instance.onUncappedMinutePass -= new Action(OnUncappedMinPass);
		ActiveQuests.Remove(this);
		DestroyDetailDisplay();
		DestroyJournalEntry();
		DestroyHUDUI();
		if (onQuestEnd != null)
		{
			onQuestEnd.Invoke(State);
		}
	}

	public virtual void SetQuestState(EQuestState state, bool network = true)
	{
		State = state;
		HasChanged = true;
		StateMachine.ChangeState();
		if ((Object)(object)hudUI != (Object)null)
		{
			((Component)hudUI).gameObject.SetActive(IsTracked && (State == EQuestState.Active || State == EQuestState.Completed));
		}
		if ((Object)(object)journalEntry != (Object)null)
		{
			((Component)journalEntry).gameObject.SetActive(ShouldShowJournalEntry());
		}
		for (int i = 0; i < Entries.Count; i++)
		{
			Entries[i].UpdateCompassElement();
		}
		if (state == EQuestState.Active && onActiveState != null)
		{
			onActiveState.Invoke();
		}
		if (network)
		{
			NetworkSingleton<QuestManager>.Instance.SendQuestState(GUID.ToString(), state);
		}
	}

	protected virtual bool ShouldShowJournalEntry()
	{
		return State == EQuestState.Active;
	}

	public virtual void SetQuestEntryState(int entryIndex, EQuestState state, bool network = true)
	{
		if (entryIndex < 0 || entryIndex >= Entries.Count)
		{
			Console.LogWarning("Invalid entry index: " + entryIndex);
			return;
		}
		HasChanged = true;
		Entries[entryIndex].SetState(state, network);
		if (state == EQuestState.Completed)
		{
			BopHUDUI();
		}
	}

	protected virtual void OnMinPass()
	{
		if (Expires)
		{
			bool flag = GetMinsUntilExpiry() <= 120;
			if ((Object)(object)entryTimeLabel != (Object)null)
			{
				entryTimeLabel.text = GetExpiryText();
			}
			if ((Object)(object)criticalTimeBackground != (Object)null)
			{
				((Behaviour)criticalTimeBackground).enabled = flag;
			}
			UpdateHUDUI();
			CheckExpiry();
			if (ShouldSendExpiryReminder && flag && !expiryReminderSent && State == EQuestState.Active)
			{
				SendExpiryReminder();
				expiryReminderSent = true;
			}
		}
	}

	protected virtual void OnUncappedMinPass()
	{
	}

	protected virtual void CheckExpiry()
	{
		if (InstanceFinder.IsServer && Expires && GetMinsUntilExpiry() <= 0 && CanExpire())
		{
			Expire();
		}
	}

	private void CheckAutoComplete()
	{
		bool flag = true;
		for (int i = 0; i < Entries.Count; i++)
		{
			if (Entries[i].State != EQuestState.Completed)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			Complete();
		}
	}

	protected virtual bool CanExpire()
	{
		return true;
	}

	protected virtual void SendExpiryReminder()
	{
		Singleton<NotificationsManager>.Instance.SendNotification("<color=#FFB43C>Quest Expiring Soon</color>", title, PlayerSingleton<JournalApp>.Instance.AppIcon);
	}

	protected virtual void SendExpiredNotification()
	{
		Singleton<NotificationsManager>.Instance.SendNotification("<color=#FF6455>Quest Expired</color>", title, PlayerSingleton<JournalApp>.Instance.AppIcon);
	}

	public void SetGUID(Guid guid)
	{
		GUID = guid;
		GUIDManager.RegisterObject(this);
	}

	public void SetSubtitle(string subtitle)
	{
		Subtitle = subtitle;
		if (onSubtitleChanged != null)
		{
			onSubtitleChanged();
		}
	}

	public virtual void SetIsTracked(bool tracked)
	{
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		IsTracked = tracked;
		if ((Object)(object)hudUI != (Object)null)
		{
			((Component)hudUI).gameObject.SetActive(tracked && State == EQuestState.Active);
		}
		if ((Object)(object)journalEntry != (Object)null)
		{
			((Component)trackedRect).gameObject.SetActive(tracked);
			((Graphic)((Component)journalEntry).GetComponent<Image>()).color = Color32.op_Implicit(IsTracked ? new Color32((byte)75, (byte)75, (byte)75, byte.MaxValue) : new Color32((byte)150, (byte)150, (byte)150, byte.MaxValue));
		}
		HasChanged = true;
		for (int i = 0; i < Entries.Count; i++)
		{
			Entries[i].UpdateCompassElement();
		}
		if (onTrackChange != null)
		{
			onTrackChange.Invoke(tracked);
		}
	}

	public virtual void SetupJournalEntry()
	{
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Expected O, but got Unknown
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Expected O, but got Unknown
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		journalEntry = Object.Instantiate<GameObject>(PlayerSingleton<JournalApp>.Instance.GenericEntry, (Transform)(object)PlayerSingleton<JournalApp>.Instance.EntryContainer).GetComponent<RectTransform>();
		((Component)((Transform)journalEntry).Find("Title")).GetComponent<Text>().text = title;
		entryTitleRect = ((Component)((Transform)journalEntry).Find("Title")).GetComponent<RectTransform>();
		trackedRect = ((Component)((Transform)journalEntry).Find("Tracked")).GetComponent<RectTransform>();
		SetIsTracked(IsTracked);
		((Component)((Transform)journalEntry).Find("Expiry")).gameObject.SetActive(Expires);
		entryTimeLabel = ((Component)((Transform)journalEntry).Find("Expiry/Time")).GetComponent<Text>();
		criticalTimeBackground = ((Component)((Transform)journalEntry).Find("Expiry/Critical")).GetComponent<Image>();
		((UnityEvent)((Component)journalEntry).GetComponent<Button>().onClick).AddListener(new UnityAction(JournalEntryClicked));
		EventTrigger component = ((Component)journalEntry).GetComponent<EventTrigger>();
		Entry val = new Entry();
		val.eventID = (EventTriggerType)0;
		((UnityEvent<BaseEventData>)(object)val.callback).AddListener((UnityAction<BaseEventData>)delegate
		{
			JournalEntryHoverStart();
		});
		component.triggers.Add(val);
		((Component)Object.Instantiate<RectTransform>(IconPrefab, ((Transform)journalEntry).Find("IconContainer"))).GetComponent<RectTransform>().sizeDelta = new Vector2(25f, 25f);
		((Component)journalEntry).gameObject.SetActive(false);
		if (Expires)
		{
			entryTimeLabel.text = GetExpiryText();
		}
	}

	private void DestroyJournalEntry()
	{
		if (!((Object)(object)journalEntry == (Object)null))
		{
			Object.Destroy((Object)(object)((Component)journalEntry).gameObject);
			journalEntry = null;
		}
	}

	private void JournalEntryClicked()
	{
		SetIsTracked(!IsTracked);
	}

	private void JournalEntryHoverStart()
	{
		HoveredQuest = this;
	}

	public int GetMinsUntilExpiry()
	{
		int totalMinSum = NetworkSingleton<TimeManager>.Instance.GetTotalMinSum();
		int num = Expiry.GetMinSum() - totalMinSum;
		if (num > 0)
		{
			return num;
		}
		return 0;
	}

	public string GetExpiryText()
	{
		int minsUntilExpiry = GetMinsUntilExpiry();
		if (minsUntilExpiry >= 60)
		{
			return Mathf.RoundToInt((float)minsUntilExpiry / 60f) + " hrs";
		}
		return minsUntilExpiry + " min";
	}

	public virtual QuestHUDUI SetupHUDUI()
	{
		if ((Object)(object)hudUI != (Object)null)
		{
			return hudUI;
		}
		hudUI = ((Component)Object.Instantiate<QuestHUDUI>(PlayerSingleton<JournalApp>.Instance.QuestHUDUIPrefab, (Transform)(object)Singleton<HUD>.Instance.QuestEntryContainer)).GetComponent<QuestHUDUI>();
		hudUI.Initialize(this);
		if (onHudUICreated != null)
		{
			onHudUICreated();
		}
		((Component)hudUI).gameObject.SetActive(IsTracked && State == EQuestState.Active);
		return hudUI;
	}

	public void UpdateHUDUI()
	{
		hudUI?.UpdateUI();
	}

	public void DestroyHUDUI()
	{
		hudUI = null;
	}

	public void BopHUDUI()
	{
		if (!((Object)(object)hudUI == (Object)null))
		{
			hudUI.BopIcon();
		}
	}

	public virtual string GetQuestTitle()
	{
		return title;
	}

	public QuestEntry GetFirstActiveEntry()
	{
		for (int i = 0; i < Entries.Count; i++)
		{
			if (Entries[i].State == EQuestState.Active)
			{
				return Entries[i];
			}
		}
		return null;
	}

	public virtual RectTransform CreateDetailDisplay(RectTransform parent)
	{
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Expected O, but got Unknown
		if ((Object)(object)detailPanel != (Object)null)
		{
			Console.LogWarning("Detail panel already exists!");
			return null;
		}
		if (!PlayerSingleton<JournalApp>.InstanceExists)
		{
			Console.LogWarning("Journal app does not exist!");
			return null;
		}
		detailPanel = Object.Instantiate<GameObject>(PlayerSingleton<JournalApp>.Instance.GenericDetailsPanel, (Transform)(object)parent).GetComponent<RectTransform>();
		((Component)((Transform)detailPanel).Find("Title")).GetComponent<Text>().text = title;
		((Component)((Transform)detailPanel).Find("Description")).GetComponent<Text>().text = Description;
		float preferredHeight = ((Component)((Transform)detailPanel).Find("Description")).GetComponent<Text>().preferredHeight;
		((Component)((Transform)detailPanel).Find("OuterContainer")).GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -45f - preferredHeight);
		RectTransform component = ((Component)((Transform)detailPanel).Find("OuterContainer/Entries")).GetComponent<RectTransform>();
		int num = 0;
		for (int i = 0; i < Entries.Count; i++)
		{
			GameObject gameObject = Object.Instantiate<GameObject>(PlayerSingleton<JournalApp>.Instance.GenericQuestEntry, (Transform)(object)component).gameObject;
			((Component)gameObject.transform.Find("Title")).GetComponent<Text>().text = Entries[i].Title;
			((Component)gameObject.transform.Find("State")).GetComponent<Text>().text = Entries[i].State.ToString();
			((Graphic)((Component)gameObject.transform.Find("State")).GetComponent<Text>()).color = Color32.op_Implicit((Entries[i].State == EQuestState.Active) ? new Color32((byte)50, (byte)50, (byte)50, byte.MaxValue) : new Color32((byte)150, (byte)150, (byte)150, byte.MaxValue));
			gameObject.gameObject.SetActive(Entries[i].State != EQuestState.Inactive);
			if (gameObject.gameObject.activeSelf)
			{
				num++;
			}
		}
		((Component)((Transform)detailPanel).Find("OuterContainer/Contents")).GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -40f - (float)num * 35f);
		POI pOI = null;
		QuestEntry firstActiveEntry = GetFirstActiveEntry();
		if ((Object)(object)firstActiveEntry != (Object)null)
		{
			pOI = firstActiveEntry.PoI;
		}
		GameObject gameObject2 = ((Component)((Transform)detailPanel).Find("OuterContainer/Contents/ShowOnMap")).gameObject;
		gameObject2.SetActive((Object)(object)pOI != (Object)null && !GameManager.IS_TUTORIAL);
		((UnityEvent)gameObject2.GetComponent<Button>().onClick).AddListener(new UnityAction(ShowOnMap));
		return detailPanel;
		void ShowOnMap()
		{
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			POI pOI2 = null;
			QuestEntry firstActiveEntry2 = GetFirstActiveEntry();
			if ((Object)(object)firstActiveEntry2 != (Object)null)
			{
				pOI2 = firstActiveEntry2.PoI;
			}
			if ((Object)(object)pOI2 != (Object)null && (Object)(object)pOI2.UI != (Object)null && PlayerSingleton<MapApp>.InstanceExists && PlayerSingleton<JournalApp>.InstanceExists)
			{
				PlayerSingleton<MapApp>.Instance.FocusPosition(pOI2.UI.anchoredPosition);
				PlayerSingleton<JournalApp>.Instance.SetOpen(open: false);
				PlayerSingleton<MapApp>.Instance.SkipFocusPlayer = true;
				PlayerSingleton<MapApp>.Instance.SetOpen(open: true);
			}
		}
	}

	public void DestroyDetailDisplay()
	{
		if ((Object)(object)detailPanel != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)detailPanel).gameObject);
		}
		detailPanel = null;
	}

	public virtual bool ShouldSave()
	{
		return true;
	}

	public virtual SaveData GetSaveData()
	{
		List<QuestEntryData> list = new List<QuestEntryData>();
		for (int i = 0; i < Entries.Count; i++)
		{
			list.Add(Entries[i].GetSaveData());
		}
		return new QuestData(GUID.ToString(), State, IsTracked, title, Description, Expires, new GameDateTimeData(Expiry), list.ToArray());
	}

	public string GetSaveString()
	{
		return GetSaveData().GetJson();
	}

	public virtual void Load(QuestData data)
	{
		SetQuestState(data.State);
		if (data.IsTracked)
		{
			SetIsTracked(tracked: true);
		}
		for (int i = 0; i < data.Entries.Length; i++)
		{
			int num = i;
			float versionNumber = SaveManager.GetVersionNumber(data.GameVersion);
			if (SaveManager.GetVersionNumber(Application.version) > versionNumber)
			{
				int num2 = i;
				for (int j = 0; j < num2 && j < Entries.Count; j++)
				{
					if (SaveManager.GetVersionNumber(Entries[j].EntryAddedIn) > versionNumber)
					{
						Console.Log("Increasing index for quest entry: " + Entries[j].Title);
						num++;
						num2++;
					}
				}
			}
			SetQuestEntryState(num, data.Entries[i].State);
		}
	}

	public static Quest GetQuest(string questName)
	{
		return Quests.FirstOrDefault((Quest x) => x.title.ToLower() == questName.ToLower());
	}
}
