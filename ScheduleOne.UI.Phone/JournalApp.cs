using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Quests;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone;

public class JournalApp : App<JournalApp>
{
	[Header("References")]
	public RectTransform EntryContainer;

	public Text NoTasksLabel;

	public Text NoDetailsLabel;

	public RectTransform DetailsPanelContainer;

	[Header("Entry prefabs")]
	public GameObject GenericEntry;

	[Header("Details panel prefabs")]
	public GameObject GenericDetailsPanel;

	[Header("Quest Entry prefab")]
	public GameObject GenericQuestEntry;

	[Header("HUD entry prefabs")]
	public QuestHUDUI QuestHUDUIPrefab;

	public QuestEntryHUDUI QuestEntryHUDUIPrefab;

	protected Quest currentDetailsPanelQuest;

	protected RectTransform currentDetailsPanel;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Start()
	{
		base.Start();
		NetworkSingleton<TimeManager>.Instance.onMinutePass += new Action(MinPass);
	}

	public override void SetOpen(bool open)
	{
		base.SetOpen(open);
		if (!open && (Object)(object)currentDetailsPanel != (Object)null)
		{
			currentDetailsPanelQuest.DestroyDetailDisplay();
			currentDetailsPanel = null;
			currentDetailsPanelQuest = null;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (base.isOpen)
		{
			RefreshDetailsPanel();
			((Behaviour)NoTasksLabel).enabled = Quest.ActiveQuests.Count == 0;
			((Behaviour)NoDetailsLabel).enabled = (Object)(object)currentDetailsPanel == (Object)null;
		}
	}

	private void RefreshDetailsPanel()
	{
		if ((Object)(object)Quest.HoveredQuest != (Object)null)
		{
			if ((Object)(object)currentDetailsPanelQuest != (Object)(object)Quest.HoveredQuest)
			{
				if ((Object)(object)currentDetailsPanel != (Object)null)
				{
					currentDetailsPanelQuest.DestroyDetailDisplay();
					currentDetailsPanel = null;
					currentDetailsPanelQuest = null;
				}
				currentDetailsPanel = Quest.HoveredQuest.CreateDetailDisplay(DetailsPanelContainer);
				currentDetailsPanelQuest = Quest.HoveredQuest;
			}
		}
		else if ((Object)(object)currentDetailsPanel != (Object)null)
		{
			currentDetailsPanelQuest.DestroyDetailDisplay();
			currentDetailsPanel = null;
			currentDetailsPanelQuest = null;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (NetworkSingleton<TimeManager>.InstanceExists)
		{
			NetworkSingleton<TimeManager>.Instance.onMinutePass -= new Action(MinPass);
		}
	}

	protected virtual void MinPass()
	{
		_ = base.isOpen;
	}
}
