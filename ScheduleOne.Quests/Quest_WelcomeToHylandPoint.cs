using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.NPCs.CharacterClasses;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Property;
using ScheduleOne.UI.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ScheduleOne.Quests;

public class Quest_WelcomeToHylandPoint : Quest
{
	public QuestEntry ReturnToRVQuest;

	public QuestEntry ReadMessagesQuest;

	public RV RV;

	public UncleNelson Nelson;

	[Header("Settings")]
	public float ExplosionMaxDist = 25f;

	public float ExplosionMinDist = 50f;

	private float cameraLookTime;

	protected override void OnUncappedMinPass()
	{
		base.OnUncappedMinPass();
		if (base.State == EQuestState.Active && ReadMessagesQuest.State == EQuestState.Active && Nelson.MSGConversation != null && Nelson.MSGConversation.Read)
		{
			ReadMessagesQuest.Complete();
		}
	}

	private void Update()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		if (base.State != EQuestState.Active || ReturnToRVQuest.State != EQuestState.Active || !InstanceFinder.IsServer)
		{
			return;
		}
		float distance;
		Player closestPlayer = Player.GetClosestPlayer(((Component)RV).transform.position, out distance);
		if (distance < ExplosionMinDist)
		{
			ReturnToRVQuest.Complete();
		}
		else
		{
			if (!(distance < ExplosionMaxDist))
			{
				return;
			}
			if (Vector3.Angle(closestPlayer.MimicCamera.forward, ((Component)RV).transform.position - closestPlayer.MimicCamera.position) < 60f)
			{
				cameraLookTime += Time.deltaTime;
				if (cameraLookTime > 0.4f)
				{
					ReturnToRVQuest.Complete();
				}
			}
			else
			{
				cameraLookTime = 0f;
			}
		}
	}

	public override void SetQuestState(EQuestState state, bool network = true)
	{
		base.SetQuestState(state, network);
		if (state == EQuestState.Active)
		{
			string text = default(string);
			string controlPath = default(string);
			InputActionRebindingExtensions.GetBindingDisplayString(Singleton<GameInput>.Instance.GetAction(GameInput.ButtonCode.TogglePhone), 0, ref text, ref controlPath, (DisplayStringOptions)0);
			string displayNameForControlPath = Singleton<InputPromptsManager>.Instance.GetDisplayNameForControlPath(controlPath);
			ReadMessagesQuest.SetEntryTitle("Open your phone (press " + displayNameForControlPath + ") and read your messages");
		}
	}

	public void BlowupRV()
	{
		(Singleton<PropertyManager>.Instance.GetProperty("rv") as RV).BlowUp();
	}

	public void SetRVDestroyed()
	{
		(Singleton<PropertyManager>.Instance.GetProperty("rv") as RV).SetDestroyed();
	}
}
