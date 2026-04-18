using System;
using System.Collections;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence;
using ScheduleOne.Quests;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public class QuestEntryHUDUI : MonoBehaviour
{
	[Header("References")]
	public TextMeshProUGUI MainLabel;

	public Animation Animation;

	public QuestEntry QuestEntry { get; private set; }

	public void Initialize(QuestEntry entry)
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Expected O, but got Unknown
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Expected O, but got Unknown
		QuestEntry = entry;
		((TMP_Text)MainLabel).text = entry.Title;
		QuestHUDUI hudUI = QuestEntry.ParentQuest.hudUI;
		hudUI.onUpdateUI = (Action)Delegate.Combine(hudUI.onUpdateUI, new Action(UpdateUI));
		if (QuestEntry.State == EQuestState.Active)
		{
			FadeIn();
		}
		else
		{
			QuestEntry.onStart.AddListener(new UnityAction(FadeIn));
		}
		QuestEntry.onEnd.AddListener(new UnityAction(EntryEnded));
	}

	public void Destroy()
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Expected O, but got Unknown
		if ((Object)(object)QuestEntry.ParentQuest.hudUI != (Object)null)
		{
			QuestHUDUI hudUI = QuestEntry.ParentQuest.hudUI;
			hudUI.onUpdateUI = (Action)Delegate.Remove(hudUI.onUpdateUI, new Action(UpdateUI));
		}
		QuestEntry.onStart.RemoveListener(new UnityAction(FadeIn));
		QuestEntry.onEnd.RemoveListener(new UnityAction(EntryEnded));
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	public virtual void UpdateUI()
	{
		if ((Object)(object)this == (Object)null || (Object)(object)((Component)this).gameObject == (Object)null)
		{
			return;
		}
		if (QuestEntry.State != EQuestState.Active)
		{
			if (!Animation.isPlaying)
			{
				((Component)this).gameObject.SetActive(false);
			}
			return;
		}
		if (QuestEntry.ParentQuest.ActiveEntryCount > 1)
		{
			((TMP_Text)MainLabel).text = "• " + QuestEntry.Title;
		}
		else
		{
			((TMP_Text)MainLabel).text = QuestEntry.Title;
		}
		((Component)this).gameObject.SetActive(true);
		((TMP_Text)MainLabel).ForceMeshUpdate(false, false);
	}

	private void FadeIn()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (!Singleton<LoadManager>.InstanceExists || !Singleton<LoadManager>.Instance.IsLoading)
		{
			if (Animation.isPlaying)
			{
				Animation.Stop();
			}
			((Graphic)MainLabel).color = Color.white;
			QuestEntry.UpdateEntryUI();
			((Component)this).transform.SetAsLastSibling();
			Animation.Play("Quest entry enter");
		}
	}

	private void EntryEnded()
	{
		if (QuestEntry.State == EQuestState.Completed)
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
		Animation.Play("Quest entry exit");
		((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return (object)new WaitForSeconds(Animation.GetClip("Quest entry exit").length);
			if ((Object)(object)this != (Object)null && (Object)(object)((Component)this).gameObject != (Object)null)
			{
				((Component)this).gameObject.SetActive(false);
				QuestEntry.UpdateEntryUI();
			}
		}
	}

	private void Complete()
	{
		if (!((Component)this).gameObject.activeSelf)
		{
			((Component)this).gameObject.SetActive(false);
			return;
		}
		Animation.Play("Quest entry complete");
		((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return (object)new WaitForSeconds(3f);
			FadeOut();
		}
	}
}
