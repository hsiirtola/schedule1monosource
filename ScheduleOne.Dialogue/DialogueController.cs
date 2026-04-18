using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Interaction;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Tools;
using ScheduleOne.VoiceOver;
using ScheduleOne.Weather;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Dialogue;

public class DialogueController : MonoBehaviour
{
	[Serializable]
	public class DialogueChoice
	{
		public delegate bool ShouldShowCheck(bool enabled);

		public delegate bool IsChoiceValid(out string invalidReason);

		public bool Enabled = true;

		public string ChoiceText;

		public bool ShowWorldspaceDialogue = true;

		public DialogueContainer Conversation;

		public UnityEvent onChoosen = new UnityEvent();

		public ShouldShowCheck shouldShowCheck;

		public IsChoiceValid isValidCheck;

		public int Priority;

		public bool ShouldShow()
		{
			if (shouldShowCheck != null)
			{
				return shouldShowCheck(Enabled);
			}
			return Enabled;
		}

		public bool IsValid(out string invalidReason)
		{
			if (isValidCheck != null)
			{
				return isValidCheck(out invalidReason);
			}
			invalidReason = string.Empty;
			return true;
		}
	}

	[Serializable]
	public class GreetingOverride
	{
		public string Greeting;

		public bool ShouldShow;

		public bool PlayVO;

		public EVOLineType VOType;
	}

	private const float GreetingCooldown = 5f;

	private const float RainyGreetingThreshold = 0.4f;

	private const float RainyGreetingChance = 0.4f;

	[Header("References")]
	public InteractableObject IntObj;

	public DialogueContainer GenericDialogue;

	[Header("Settings")]
	public bool DialogueEnabled = true;

	public bool UseDialogueBehaviour = true;

	public List<DialogueChoice> Choices = new List<DialogueChoice>();

	public List<GreetingOverride> GreetingOverrides = new List<GreetingOverride>();

	public DialogueContainer OverrideContainer;

	protected NPC npc;

	protected DialogueHandler handler;

	private float lastGreetingTime;

	private List<DialogueChoice> shownChoices = new List<DialogueChoice>();

	private bool dialogueQueued;

	private string cachedGreeting = string.Empty;

	protected virtual void Start()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		handler = ((Component)this).GetComponent<DialogueHandler>();
		npc = handler.NPC;
		IntObj.onHovered.AddListener(new UnityAction(Hovered));
		IntObj.onInteractStart.AddListener(new UnityAction(Interacted));
		lastGreetingTime = Time.time - 20f;
	}

	private void Hovered()
	{
		if (CanStartDialogue() && (((GetActiveChoices().Count > 0 || Time.time - lastGreetingTime > 5f) && DialogueEnabled) || (Object)(object)OverrideContainer != (Object)null))
		{
			IntObj.SetMessage("Talk to " + npc.GetNameAddress());
			IntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
		}
		else
		{
			IntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
		}
	}

	public void StartGenericDialogue(bool allowExit = true)
	{
		Interacted();
		GenericDialogue.SetAllowExit(allowExit);
	}

	private void Interacted()
	{
		GenericDialogue.SetAllowExit(allowed: true);
		dialogueQueued = true;
		((MonoBehaviour)this).Invoke("Unqueue", 1f);
		if ((Object)(object)OverrideContainer != (Object)null)
		{
			handler.InitializeDialogue(OverrideContainer, UseDialogueBehaviour);
		}
		else if (GetActiveChoices().Count > 0)
		{
			shownChoices = GetActiveChoices();
			cachedGreeting = GetActiveGreeting(out var playVO, out var voLineType);
			handler.InitializeDialogue(GenericDialogue, UseDialogueBehaviour);
			if (playVO && voLineType != EVOLineType.None)
			{
				npc.PlayVO(voLineType);
			}
		}
		else
		{
			handler.ShowWorldspaceDialogue(GetActiveGreeting(out var playVO2, out var voLineType2), 5f);
			lastGreetingTime = Time.time;
			if (playVO2)
			{
				npc.PlayVO(voLineType2);
			}
		}
	}

	private void Unqueue()
	{
		dialogueQueued = false;
	}

	private string GetActiveGreeting(out bool playVO, out EVOLineType voLineType)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		playVO = false;
		if (GetCustomGreeting(out var greeting, out playVO, out voLineType))
		{
			return greeting;
		}
		playVO = true;
		voLineType = EVOLineType.Greeting;
		if (NetworkSingleton<EnvironmentManager>.InstanceExists && NetworkSingleton<EnvironmentManager>.Instance.GetActiveWeatherConditionsFromPosition(npc.CenterPoint).Rainy > 0.4f && Random.Range(0f, 1f) < 0.4f && handler.Database.HasLine(EDialogueModule.Greetings, "rainy_greeting"))
		{
			return handler.Database.GetLine(EDialogueModule.Greetings, "rainy_greeting");
		}
		if (NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(400, 1200))
		{
			return handler.Database.GetLine(EDialogueModule.Greetings, "morning_greeting");
		}
		if (NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(1200, 1800))
		{
			return handler.Database.GetLine(EDialogueModule.Greetings, "afternoon_greeting");
		}
		return handler.Database.GetLine(EDialogueModule.Greetings, "night_greeting");
	}

	private List<DialogueChoice> GetActiveChoices()
	{
		List<DialogueChoice> list = new List<DialogueChoice>();
		foreach (DialogueChoice choice in Choices)
		{
			if (choice.ShouldShow())
			{
				list.Add(choice);
			}
		}
		list.Sort((DialogueChoice a, DialogueChoice b) => b.Priority.CompareTo(a.Priority));
		return list;
	}

	protected virtual bool GetCustomGreeting(out string greeting, out bool playVO, out EVOLineType voLineType)
	{
		greeting = string.Empty;
		playVO = false;
		voLineType = EVOLineType.Greeting;
		for (int i = 0; i < GreetingOverrides.Count; i++)
		{
			if (GreetingOverrides[i].ShouldShow)
			{
				greeting = GreetingOverrides[i].Greeting;
				playVO = GreetingOverrides[i].PlayVO;
				voLineType = GreetingOverrides[i].VOType;
				return true;
			}
		}
		return false;
	}

	public virtual int AddDialogueChoice(DialogueChoice data, int priority = 0)
	{
		data.Priority = priority;
		Choices.Add(data);
		return Choices.Count - 1;
	}

	public virtual int AddGreetingOverride(GreetingOverride data)
	{
		GreetingOverrides.Add(data);
		return Choices.Count - 1;
	}

	public virtual bool CanStartDialogue()
	{
		if (Player.Local.CrimeData.CurrentPursuitLevel != PlayerCrimeData.EPursuitLevel.None && Player.Local.CrimeData.TimeSinceSighted < 5f)
		{
			return false;
		}
		if (Singleton<ManagementClipboard>.Instance.IsEquipped)
		{
			return false;
		}
		if (npc.Behaviour.CallPoliceBehaviour.Active)
		{
			return false;
		}
		if (npc.Behaviour.CombatBehaviour.Active)
		{
			return false;
		}
		if (npc.Behaviour.CoweringBehaviour.Active)
		{
			return false;
		}
		if (npc.Behaviour.RagdollBehaviour.Active)
		{
			return false;
		}
		if (npc.Behaviour.HeavyFlinchBehaviour.Active)
		{
			return false;
		}
		if (npc.Behaviour.ConsumeProductBehaviour.Active)
		{
			return false;
		}
		if (npc.Behaviour.FleeBehaviour.Active)
		{
			return false;
		}
		if (npc.Behaviour.GenericDialogueBehaviour.Active)
		{
			return false;
		}
		if (!npc.IsConscious)
		{
			return false;
		}
		if (dialogueQueued)
		{
			return false;
		}
		return true;
	}

	public virtual string ModifyDialogueText(string dialogueLabel, string dialogueText)
	{
		if ((Object)(object)DialogueHandler.activeDialogue == (Object)(object)GenericDialogue && dialogueLabel == "ENTRY")
		{
			return cachedGreeting;
		}
		return dialogueText;
	}

	public virtual string ModifyChoiceText(string choiceLabel, string choiceText)
	{
		return choiceText;
	}

	public virtual void ModifyChoiceList(string dialogueLabel, ref List<DialogueChoiceData> existingChoices)
	{
		if ((Object)(object)DialogueHandler.activeDialogue == (Object)(object)GenericDialogue && dialogueLabel == "ENTRY")
		{
			List<DialogueChoice> list = shownChoices;
			for (int i = 0; i < list.Count; i++)
			{
				DialogueChoiceData dialogueChoiceData = new DialogueChoiceData();
				dialogueChoiceData.ChoiceText = list[i].ChoiceText;
				dialogueChoiceData.ChoiceLabel = "GENERIC_CHOICE_" + i;
				dialogueChoiceData.ShowWorldspaceDialogue = list[i].ShowWorldspaceDialogue;
				existingChoices.Add(dialogueChoiceData);
			}
		}
	}

	public virtual void ChoiceCallback(string choiceLabel)
	{
		if (!((Object)(object)DialogueHandler.activeDialogue == (Object)(object)GenericDialogue) || !choiceLabel.Contains("GENERIC_CHOICE_"))
		{
			return;
		}
		int num = int.Parse(choiceLabel.Substring("GENERIC_CHOICE_".Length));
		List<DialogueChoice> list = shownChoices;
		if (num >= 0 && num < list.Count)
		{
			DialogueChoice dialogueChoice = list[num];
			if (dialogueChoice.onChoosen != null)
			{
				dialogueChoice.onChoosen.Invoke();
			}
			if ((Object)(object)dialogueChoice.Conversation != (Object)null)
			{
				handler.InitializeDialogue(dialogueChoice.Conversation);
			}
		}
	}

	public virtual bool CheckChoice(string choiceLabel, out string invalidReason)
	{
		if ((Object)(object)DialogueHandler.activeDialogue == (Object)(object)GenericDialogue && choiceLabel.Contains("GENERIC_CHOICE_"))
		{
			int num = int.Parse(choiceLabel.Substring("GENERIC_CHOICE_".Length));
			List<DialogueChoice> list = shownChoices;
			if (num >= 0 && num < list.Count)
			{
				return list[num].IsValid(out invalidReason);
			}
		}
		invalidReason = string.Empty;
		return true;
	}

	public void SetOverrideContainer(DialogueContainer container)
	{
		OverrideContainer = container;
	}

	public void ClearOverrideContainer()
	{
		OverrideContainer = null;
	}

	public virtual bool DecideBranch(string branchLabel, out int index)
	{
		index = 0;
		return false;
	}

	public void SetDialogueEnabled(bool enabled)
	{
		DialogueEnabled = enabled;
	}
}
