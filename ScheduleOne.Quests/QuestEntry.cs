using System;
using FishNet.Serializing.Helping;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Map;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.UI;
using ScheduleOne.UI.Compass;
using ScheduleOne.UI.Phone;
using ScheduleOne.Variables;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace ScheduleOne.Quests;

[Serializable]
public class QuestEntry : MonoBehaviour
{
	[Header("Naming")]
	[SerializeField]
	protected string EntryTitle = string.Empty;

	[SerializeField]
	protected EQuestState state;

	[Header("Settings")]
	public bool AutoComplete;

	public Conditions AutoCompleteConditions;

	public bool CompleteParentQuest;

	public string EntryAddedIn = "0.0.1";

	[Header("PoI Settings")]
	public bool AutoCreatePoI = true;

	public Transform PoILocation;

	public bool AutoUpdatePoILocation;

	public POI PoI;

	public UnityEvent onStart = new UnityEvent();

	public UnityEvent onEnd = new UnityEvent();

	public UnityEvent onComplete = new UnityEvent();

	public UnityEvent onInitialComplete = new UnityEvent();

	private CompassManager.Element compassElement;

	private QuestEntryHUDUI entryUI;

	private RectTransform PoIIcon;

	[CodegenExclude]
	[field: NonSerialized]
	public Quest ParentQuest { get; private set; }

	[CodegenExclude]
	public string Title => EntryTitle;

	[CodegenExclude]
	public EQuestState State => state;

	public int QuestEntryIndex => ParentQuest.Entries.IndexOf(this);

	protected virtual void Awake()
	{
		ParentQuest = ((Component)this).GetComponentInParent<Quest>();
		ParentQuest.onQuestEnd.AddListener((UnityAction<EQuestState>)delegate
		{
			DestroyPoI();
		});
		ParentQuest.onTrackChange.AddListener((UnityAction<bool>)delegate
		{
			UpdatePoI();
		});
		if (AutoComplete)
		{
			StateMachine.OnStateChange = (Action)Delegate.Combine(StateMachine.OnStateChange, new Action(EvaluateConditions));
		}
	}

	protected virtual void Start()
	{
		if (AutoCreatePoI && (Object)(object)PoI == (Object)null)
		{
			CreatePoI();
		}
		if (!ParentQuest.Entries.Contains(this))
		{
			Console.LogError("Parent quest '" + ParentQuest.GetQuestTitle() + "' does not contain entry '" + EntryTitle + "'.");
		}
		if (ParentQuest.hudUIExists)
		{
			CreateEntryUI();
		}
		else
		{
			Quest parentQuest = ParentQuest;
			parentQuest.onHudUICreated = (Action)Delegate.Combine(parentQuest.onHudUICreated, new Action(CreateEntryUI));
		}
		CreateCompassElement();
	}

	private void OnValidate()
	{
		if (((Behaviour)this).enabled && ((Component)this).gameObject.activeInHierarchy)
		{
			UpdateName();
			if (EntryAddedIn == null || EntryAddedIn == string.Empty)
			{
				EntryAddedIn = Application.version;
			}
		}
	}

	public virtual void MinPass()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (AutoUpdatePoILocation && (Object)(object)PoI != (Object)null)
		{
			((Component)PoI).transform.position = PoILocation.position;
			PoI.UpdatePosition();
		}
	}

	public void SetData(QuestEntryData data)
	{
		EntryTitle = data.Name;
		SetState(data.State, network: false);
	}

	public void Begin()
	{
		SetState(EQuestState.Active);
	}

	public void Complete()
	{
		SetState(EQuestState.Completed);
	}

	public void SetActive(bool network = true)
	{
		SetState(EQuestState.Active, network);
	}

	public virtual void SetState(EQuestState newState, bool network = true)
	{
		EQuestState eQuestState = state;
		NetworkSingleton<TimeManager>.Instance.onMinutePass -= new Action(MinPass);
		state = newState;
		if (newState == EQuestState.Active && eQuestState != EQuestState.Active)
		{
			if (onStart != null)
			{
				onStart.Invoke();
			}
			NetworkSingleton<TimeManager>.Instance.onMinutePass += new Action(MinPass);
		}
		if (newState != EQuestState.Active && eQuestState == EQuestState.Active && onEnd != null)
		{
			onEnd.Invoke();
		}
		if (newState == EQuestState.Completed && eQuestState != EQuestState.Completed)
		{
			if (onComplete != null)
			{
				onComplete.Invoke();
			}
			if (!Singleton<LoadManager>.Instance.IsLoading && eQuestState == EQuestState.Active)
			{
				if (onInitialComplete != null)
				{
					onInitialComplete.Invoke();
				}
				NetworkSingleton<QuestManager>.Instance.PlayCompleteQuestEntrySound();
			}
			if (CompleteParentQuest)
			{
				ParentQuest.Complete(network);
			}
		}
		if ((Object)(object)PoI != (Object)null)
		{
			((Component)PoI).gameObject.SetActive(ShouldShowPoI());
		}
		ParentQuest.UpdateHUDUI();
		UpdateCompassElement();
		if (network)
		{
			NetworkSingleton<QuestManager>.Instance.SendQuestEntryState(ParentQuest.GUID.ToString(), QuestEntryIndex, newState);
		}
		UpdateName();
		StateMachine.ChangeState();
	}

	protected virtual bool ShouldShowPoI()
	{
		if (State == EQuestState.Active)
		{
			return ParentQuest.IsTracked;
		}
		return false;
	}

	protected virtual void UpdatePoI()
	{
		if ((Object)(object)PoI != (Object)null)
		{
			((Component)PoI).gameObject.SetActive(ShouldShowPoI());
		}
	}

	public virtual void SetPoIColor(string componentName, string colourName)
	{
		if ((Object)(object)PoI != (Object)null)
		{
			PoI.FontSetter?.SetColour(componentName, colourName);
		}
	}

	public void SetPoILocation(Vector3 location)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		PoILocation.position = location;
		if ((Object)(object)PoI != (Object)null)
		{
			((Component)PoI).transform.position = location;
			PoI.UpdatePosition();
		}
	}

	public void CreatePoI()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Expected O, but got Unknown
		if ((Object)(object)PoI != (Object)null)
		{
			Console.LogWarning("PoI already exists for quest entry " + EntryTitle);
			return;
		}
		if ((Object)(object)ParentQuest == (Object)null)
		{
			Console.LogWarning("Parent quest is null for quest entry " + EntryTitle);
			return;
		}
		if ((Object)(object)PoILocation == (Object)null)
		{
			Console.LogWarning("PoI location is null for quest entry " + EntryTitle);
			return;
		}
		PoI = Object.Instantiate<GameObject>(ParentQuest.PoIPrefab, ((Component)this).transform).GetComponent<POI>();
		((Component)PoI).transform.position = PoILocation.position;
		PoI.SetMainText(Title);
		PoI.UpdatePosition();
		((Component)PoI).gameObject.SetActive(ShouldShowPoI());
		if ((Object)(object)PoI.IconContainer != (Object)null)
		{
			CreateUI();
		}
		else
		{
			PoI.onUICreated.AddListener(new UnityAction(CreateUI));
		}
		void CreateUI()
		{
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)PoIIcon != (Object)null)
			{
				Console.LogWarning("PoI icon already exists");
			}
			else if ((Object)(object)ParentQuest == (Object)null)
			{
				Console.LogWarning("Parent quest is null for quest entry " + EntryTitle);
			}
			else
			{
				PoIIcon = Object.Instantiate<GameObject>(((Component)ParentQuest.IconPrefab).gameObject, (Transform)(object)PoI.IconContainer).GetComponent<RectTransform>();
				PoIIcon.sizeDelta = new Vector2(20f, 20f);
				UpdatePoI();
			}
		}
	}

	public void DestroyPoI()
	{
		if ((Object)(object)PoI != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)PoI).gameObject);
			PoI = null;
		}
	}

	public void CreateCompassElement()
	{
		if (compassElement != null)
		{
			Console.LogWarning("Compass element already exists for quest: " + Title);
			return;
		}
		compassElement = Singleton<CompassManager>.Instance.AddElement(PoILocation, ParentQuest.IconPrefab, state == EQuestState.Active);
		UpdateCompassElement();
	}

	public void UpdateCompassElement()
	{
		if (compassElement != null)
		{
			compassElement.Transform = PoILocation;
			compassElement.Visible = ParentQuest.State == EQuestState.Active && ParentQuest.IsTracked && state == EQuestState.Active && (Object)(object)PoILocation != (Object)null;
		}
	}

	public QuestEntryData GetSaveData()
	{
		return new QuestEntryData(EntryTitle, state);
	}

	private void UpdateName()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		Scene scene = ((Component)this).gameObject.scene;
		if (((Scene)(ref scene)).isLoaded)
		{
			((Object)this).name = "(" + ((Component)this).GetComponentInParent<Quest>().Entries.IndexOf(this) + ") " + EntryTitle + " (" + state.ToString() + ")";
		}
	}

	private void EvaluateConditions()
	{
		if (State == EQuestState.Active && AutoCompleteConditions.Evaluate())
		{
			SetState(EQuestState.Completed);
		}
	}

	public void SetEntryTitle(string newTitle)
	{
		EntryTitle = newTitle;
		ParentQuest.UpdateHUDUI();
		if ((Object)(object)PoI != (Object)null)
		{
			PoI.SetMainText(Title);
		}
	}

	protected virtual void CreateEntryUI()
	{
		if (!ParentQuest.hudUIExists)
		{
			Console.LogWarning("Quest HUD UI does not exist for quest " + ParentQuest.GetQuestTitle());
			return;
		}
		entryUI = ((Component)Object.Instantiate<QuestEntryHUDUI>(PlayerSingleton<JournalApp>.Instance.QuestEntryHUDUIPrefab, (Transform)(object)ParentQuest.hudUI.EntryContainer)).GetComponent<QuestEntryHUDUI>();
		entryUI.Initialize(this);
		UpdateEntryUI();
	}

	public virtual void UpdateEntryUI()
	{
		entryUI.UpdateUI();
	}
}
