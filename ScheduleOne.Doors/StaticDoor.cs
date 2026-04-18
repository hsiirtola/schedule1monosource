using System.Collections;
using System.Collections.Generic;
using FluffyUnderware.DevTools.Extensions;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.Interaction;
using ScheduleOne.Map;
using ScheduleOne.NPCs;
using ScheduleOne.UI;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Doors;

public class StaticDoor : MonoBehaviour
{
	public const float KNOCK_COOLDOWN = 2f;

	public const float SUMMON_DURATION = 8f;

	[Header("References")]
	public Transform AccessPoint;

	public InteractableObject IntObj;

	public AudioSourceController KnockSound;

	public AudioSourceController EnterSound;

	public AudioSourceController ExitSound;

	public NPCEnterableBuilding Building;

	[Header("Settings")]
	public bool Usable = true;

	public bool CanKnock = true;

	private float timeSinceLastKnock;

	private int doorIndex = -1;

	protected virtual void Awake()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		IntObj.onHovered.AddListener(new UnityAction(Hovered));
		IntObj.onInteractStart.AddListener(new UnityAction(Interacted));
		if ((Object)(object)Building == (Object)null)
		{
			Building = ((Component)this).GetComponentInParent<NPCEnterableBuilding>();
			if ((Object)(object)Building == (Object)null && (Usable || CanKnock))
			{
				Console.LogWarning("StaticDoor " + ((Object)this).name + " has no NPCEnterableBuilding!");
				Usable = false;
				CanKnock = false;
			}
		}
		if ((Object)(object)Building != (Object)null && CanKnock)
		{
			doorIndex = ArrayExt.IndexOf<StaticDoor>(Building.Doors, this);
			if (doorIndex < 0)
			{
				Console.LogWarning("StaticDoor " + ((Object)this).name + " is not in the Building's Doors list!", (Object)(object)this);
			}
		}
		timeSinceLastKnock = Time.time - 2f;
	}

	protected virtual void OnValidate()
	{
		if ((Object)(object)Building == (Object)null)
		{
			Building = ((Component)this).GetComponentInParent<NPCEnterableBuilding>();
		}
		if ((Object)(object)Building != (Object)null && !((Component)this).transform.IsChildOf(((Component)Building).transform))
		{
			Console.LogWarning("StaticDoor " + ((Object)this).name + " is not a child of " + Building.BuildingName);
		}
	}

	protected virtual void Hovered()
	{
		if (CanKnockNow())
		{
			if (IsKnockValid(out var message))
			{
				IntObj.SetMessage("Knock");
				IntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
			}
			else
			{
				IntObj.SetMessage(message);
				IntObj.SetInteractableState(InteractableObject.EInteractableState.Invalid);
			}
		}
		else
		{
			IntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
		}
	}

	protected virtual bool CanKnockNow()
	{
		if (CanKnock && Time.time - timeSinceLastKnock >= 2f)
		{
			return (Object)(object)Building != (Object)null;
		}
		return false;
	}

	protected virtual bool IsKnockValid(out string message)
	{
		message = string.Empty;
		return true;
	}

	protected virtual void Interacted()
	{
		Knock();
	}

	protected virtual void Knock()
	{
		timeSinceLastKnock = Time.time;
		if ((Object)(object)KnockSound != (Object)null)
		{
			KnockSound.Play();
		}
		((MonoBehaviour)this).StartCoroutine(knockRoutine());
		IEnumerator knockRoutine()
		{
			yield return (object)new WaitForSeconds(0.7f);
			List<NPC> summonableNPCs = Building.GetSummonableNPCs();
			if (summonableNPCs.Count > 1)
			{
				Singleton<NPCSummonMenu>.Instance.Open(summonableNPCs, NPCSelected);
			}
			else if (summonableNPCs.Count == 1)
			{
				NPCSelected(summonableNPCs[0]);
			}
			else
			{
				Console.Log("Building is empty!");
			}
		}
	}

	protected virtual void NPCSelected(NPC npc)
	{
		npc.Behaviour.Summon(Building.GUID.ToString(), ArrayExt.IndexOf<StaticDoor>(Building.Doors, this), 8f);
	}
}
