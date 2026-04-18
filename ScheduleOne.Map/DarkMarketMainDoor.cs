using System;
using System.Collections;
using ScheduleOne.DevUtilities;
using ScheduleOne.Dialogue;
using ScheduleOne.Doors;
using ScheduleOne.Interaction;
using ScheduleOne.Levelling;
using ScheduleOne.NPCs.CharacterClasses;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.Map;

public class DarkMarketMainDoor : MonoBehaviour
{
	public AudioSource KnockSound;

	public InteractableObject InteractableObject;

	public Peephole Peephole;

	public Igor Igor;

	public DialogueContainer FailDialogue;

	public DialogueContainer SuccessDialogue;

	public DialogueContainer SuccessDialogueNotOpen;

	private Coroutine knockRoutine;

	public bool KnockingEnabled { get; private set; } = true;

	private void Start()
	{
		((Component)Igor).gameObject.SetActive(false);
	}

	public void SetKnockingEnabled(bool enabled)
	{
		((Component)InteractableObject).gameObject.SetActive(enabled);
		KnockingEnabled = enabled;
	}

	public void Hovered()
	{
		if (KnockingEnabled && knockRoutine == null && Player.Local.CrimeData.CurrentPursuitLevel == PlayerCrimeData.EPursuitLevel.None)
		{
			InteractableObject.SetMessage("Knock");
			InteractableObject.SetInteractableState(InteractableObject.EInteractableState.Default);
		}
		else
		{
			InteractableObject.SetInteractableState(InteractableObject.EInteractableState.Disabled);
		}
	}

	public void Interacted()
	{
		Knocked();
	}

	private void Knocked()
	{
		knockRoutine = ((MonoBehaviour)this).StartCoroutine(Knock());
		IEnumerator Knock()
		{
			KnockSound.Play();
			((Component)Igor).gameObject.SetActive(true);
			Igor.Avatar.LookController.ForceLookTarget = ((Component)PlayerSingleton<PlayerCamera>.Instance).transform;
			yield return (object)new WaitForSeconds(0.75f);
			((Component)Igor).gameObject.SetActive(true);
			Peephole.Open();
			yield return (object)new WaitForSeconds(0.3f);
			bool shouldUnlock = false;
			if (Vector3.Distance(((Component)Player.Local).transform.position, ((Component)this).transform.position) < 3f)
			{
				shouldUnlock = NetworkSingleton<LevelManager>.Instance.GetFullRank() >= NetworkSingleton<DarkMarket>.Instance.UnlockRank;
				DialogueContainer container = ((!shouldUnlock) ? FailDialogue : (NetworkSingleton<DarkMarket>.Instance.IsOpen ? SuccessDialogue : SuccessDialogueNotOpen));
				Igor.DialogueHandler.InitializeDialogue(container);
				yield return (object)new WaitUntil((Func<bool>)(() => !Igor.DialogueHandler.IsDialogueInProgress));
			}
			else
			{
				yield return (object)new WaitForSeconds(1f);
			}
			yield return (object)new WaitForSeconds(0.2f);
			Peephole.Close();
			yield return (object)new WaitForSeconds(0.2f);
			if (shouldUnlock)
			{
				NetworkSingleton<DarkMarket>.Instance.SendUnlocked();
			}
			else
			{
				HintDisplay instance = Singleton<HintDisplay>.Instance;
				FullRank unlockRank = NetworkSingleton<DarkMarket>.Instance.UnlockRank;
				instance.ShowHint("Reach the rank of <h1>" + unlockRank.ToString() + "</h> to access this area.", 15f);
			}
			yield return (object)new WaitForSeconds(0.5f);
			((Component)Igor).gameObject.SetActive(false);
			knockRoutine = null;
		}
	}
}
