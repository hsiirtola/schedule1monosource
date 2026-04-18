using System;
using System.Collections;
using ScheduleOne.DevUtilities;
using ScheduleOne.Quests;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public class QuestHUDUI : MonoBehaviour
{
	public string CriticalTimeColor = "FF7A7A";

	[Header("References")]
	public RectTransform EntryContainer;

	public TextMeshProUGUI MainLabel;

	public VerticalLayoutGroup hudUILayout;

	public Animation Animation;

	public RectTransform Shade;

	public Action onUpdateUI;

	public Quest Quest { get; private set; }

	public void Initialize(Quest quest)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		Quest = quest;
		Quest quest2 = Quest;
		quest2.onSubtitleChanged = (Action)Delegate.Combine(quest2.onSubtitleChanged, new Action(UpdateMainLabel));
		((Component)Object.Instantiate<RectTransform>(Quest.IconPrefab, ((Component)this).transform.Find("Title/IconContainer"))).GetComponent<RectTransform>().sizeDelta = new Vector2(20f, 20f);
		UpdateUI();
		if (Quest.State == EQuestState.Active)
		{
			FadeIn();
		}
		else
		{
			Quest.onQuestBegin.AddListener(new UnityAction(FadeIn));
			((Component)this).gameObject.SetActive(false);
		}
		Quest.onQuestEnd.AddListener((UnityAction<EQuestState>)EntryEnded);
	}

	public void Destroy()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		Quest quest = Quest;
		quest.onSubtitleChanged = (Action)Delegate.Remove(quest.onSubtitleChanged, new Action(UpdateMainLabel));
		Quest.onQuestBegin.RemoveListener(new UnityAction(FadeIn));
		Quest.onQuestEnd.RemoveListener((UnityAction<EQuestState>)EntryEnded);
		QuestEntryHUDUI[] componentsInChildren = ((Component)this).GetComponentsInChildren<QuestEntryHUDUI>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Destroy();
		}
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	public void UpdateUI()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		UpdateMainLabel();
		if (onUpdateUI != null)
		{
			onUpdateUI();
		}
		((LayoutGroup)hudUILayout).CalculateLayoutInputVertical();
		((LayoutGroup)hudUILayout).SetLayoutVertical();
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)((Component)hudUILayout).transform);
		((Behaviour)hudUILayout).enabled = false;
		((Behaviour)hudUILayout).enabled = true;
		UpdateShade();
		((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(DelayFix());
		IEnumerator DelayFix()
		{
			yield return (object)new WaitForEndOfFrame();
			((LayoutGroup)hudUILayout).CalculateLayoutInputVertical();
			((LayoutGroup)hudUILayout).SetLayoutVertical();
			LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)((Component)hudUILayout).transform);
			((Behaviour)hudUILayout).enabled = false;
			((Behaviour)hudUILayout).enabled = true;
			UpdateShade();
		}
	}

	public void UpdateMainLabel()
	{
		((TMP_Text)MainLabel).text = Quest.GetQuestTitle() + Quest.Subtitle;
		((TMP_Text)MainLabel).ForceMeshUpdate(false, false);
	}

	public void UpdateShade()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		Shade.sizeDelta = new Vector2(550f, ((LayoutGroup)hudUILayout).preferredHeight + 120f);
	}

	public void BopIcon()
	{
		((Component)((Component)this).transform.Find("Title/IconContainer")).GetComponent<Animation>().Play();
	}

	private void FadeIn()
	{
		if (Quest.IsTracked)
		{
			((Component)this).gameObject.SetActive(true);
		}
		Animation.Play("Quest enter");
	}

	private void EntryEnded(EQuestState endState)
	{
		if (endState == EQuestState.Completed)
		{
			Complete();
		}
		else
		{
			FadeOut();
		}
	}

	private void FadeOut()
	{
		Animation.Play("Quest exit");
		((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return (object)new WaitForSeconds(0.5f);
			Destroy();
		}
	}

	private void Complete()
	{
		Animation.Play("Quest complete");
		((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return (object)new WaitForSeconds(3f);
			FadeOut();
		}
	}
}
