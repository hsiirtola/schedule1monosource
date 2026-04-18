using ScheduleOne.EntityFramework;
using ScheduleOne.Money;
using ScheduleOne.Storage;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.ObjectScripts;

public class BedItem : PlaceableStorageEntity
{
	public Bed Bed;

	public StorageEntity Storage;

	public GameObject Briefcase;

	private bool NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EBedItemAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EObjectScripts_002EBedItemAssembly_002DCSharp_002Edll_Excuted;

	protected override void Start()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		base.Start();
		Bed.EmployeeStationThing.onAssignedEmployeeChanged.AddListener(new UnityAction(UpdateBriefcase));
		UpdateBriefcase();
	}

	public static bool IsBedValid(BuildableItem obj, out string reason)
	{
		reason = string.Empty;
		if (obj is BedItem)
		{
			BedItem bedItem = obj as BedItem;
			if ((Object)(object)bedItem.Bed.AssignedEmployee != (Object)null)
			{
				reason = "Already assigned to " + bedItem.Bed.AssignedEmployee.fullName;
				return false;
			}
			return true;
		}
		return false;
	}

	private void UpdateBriefcase()
	{
		Briefcase.gameObject.SetActive((Object)(object)Bed.AssignedEmployee != (Object)null || Storage.ItemCount > 0);
		if ((Object)(object)Bed.AssignedEmployee != (Object)null)
		{
			Storage.StorageEntityName = Bed.AssignedEmployee.FirstName + "'s Briefcase";
			string text = "<color=#54E717>" + MoneyManager.FormatAmount(Bed.AssignedEmployee.DailyWage) + "</color>";
			Storage.StorageEntitySubtitle = Bed.AssignedEmployee.fullName + " will draw " + (Bed.AssignedEmployee.IsMale ? "his" : "her") + " daily wage of " + text + " from this briefcase.";
		}
		else
		{
			Storage.StorageEntityName = "Briefcase";
			Storage.StorageEntitySubtitle = string.Empty;
		}
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EBedItemAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EBedItemAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EObjectScripts_002EBedItemAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EObjectScripts_002EBedItemAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
