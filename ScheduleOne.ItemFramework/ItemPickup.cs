using System;
using System.Collections;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Interaction;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Variables;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.ItemFramework;

[RequireComponent(typeof(InteractableObject))]
public class ItemPickup : MonoBehaviour
{
	public ItemDefinition ItemToGive;

	public bool DestroyOnPickup = true;

	public bool ConditionallyActive;

	public Condition ActiveCondition;

	[Header("References")]
	public InteractableObject IntObj;

	public UnityEvent onPickup;

	protected virtual void Awake()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		if ((Object)(object)ItemToGive != (Object)null)
		{
			IntObj.SetMessage("Pick up " + ((BaseItemDefinition)ItemToGive).Name);
		}
		IntObj.onHovered.AddListener(new UnityAction(Hovered));
		IntObj.onInteractStart.AddListener(new UnityAction(Interacted));
	}

	private void Start()
	{
		if ((Object)(object)Player.Local != (Object)null)
		{
			Init();
		}
		else
		{
			Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, new Action(Init));
		}
	}

	private void Init()
	{
		Player.onLocalPlayerSpawned = (Action)Delegate.Remove(Player.onLocalPlayerSpawned, new Action(Init));
		((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Wait());
		IEnumerator Wait()
		{
			yield return (object)new WaitUntil((Func<bool>)(() => Player.Local.playerDataRetrieveReturned));
			if (ConditionallyActive && ActiveCondition != null)
			{
				((Component)this).gameObject.SetActive(ActiveCondition.Evaluate());
			}
		}
	}

	protected virtual void Hovered()
	{
		if (CanPickup())
		{
			IntObj.SetMessage("Pick up " + ((BaseItemDefinition)ItemToGive).Name);
			IntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
		}
		else
		{
			IntObj.SetMessage("Inventory Full");
			IntObj.SetInteractableState(InteractableObject.EInteractableState.Invalid);
		}
	}

	private void Interacted()
	{
		if (CanPickup())
		{
			Pickup();
		}
	}

	protected virtual bool CanPickup()
	{
		if ((Object)(object)ItemToGive != (Object)null)
		{
			return PlayerSingleton<PlayerInventory>.Instance.CanItemFitInInventory(ItemToGive.GetDefaultInstance());
		}
		return false;
	}

	protected virtual void Pickup()
	{
		if ((Object)(object)ItemToGive != (Object)null)
		{
			PlayerSingleton<PlayerInventory>.Instance.AddItemToInventory(ItemToGive.GetDefaultInstance());
		}
		if (onPickup != null)
		{
			onPickup.Invoke();
		}
		if (DestroyOnPickup)
		{
			((Component)this).gameObject.SetActive(false);
		}
	}

	public void Destroy()
	{
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}
}
