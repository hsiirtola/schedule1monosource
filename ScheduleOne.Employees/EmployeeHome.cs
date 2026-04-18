using ScheduleOne.EntityFramework;
using ScheduleOne.ItemFramework;
using ScheduleOne.Money;
using ScheduleOne.Storage;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Employees;

public class EmployeeHome : MonoBehaviour
{
	public string HomeType = "Briefcase";

	[Header("References")]
	public GameObject Clipboard;

	public SpriteRenderer MugshotSprite;

	public TextMeshPro NameLabel;

	public StorageEntity Storage;

	public MeshRenderer[] EmployeeSpecificMeshes;

	public Material SpecificMat_Default;

	public Material SpecificMat_Botanist;

	public Material SpecificMat_Chemist;

	public Material SpecificMat_Packager;

	public Material SpecificMat_Cleaner;

	public UnityEvent onAssignedEmployeeChanged;

	public Employee AssignedEmployee { get; protected set; }

	private void Awake()
	{
		if ((Object)(object)Clipboard != (Object)null)
		{
			Clipboard.gameObject.SetActive(false);
		}
	}

	private void Start()
	{
		UpdateStorageText();
	}

	public void SetAssignedEmployee(Employee employee)
	{
		AssignedEmployee = employee;
		if ((Object)(object)AssignedEmployee != (Object)null)
		{
			MugshotSprite.sprite = AssignedEmployee.MugshotSprite;
			((TMP_Text)NameLabel).text = AssignedEmployee.FirstName + "\n" + AssignedEmployee.LastName;
			Clipboard.gameObject.SetActive(true);
		}
		else
		{
			Clipboard.gameObject.SetActive(false);
		}
		if (onAssignedEmployeeChanged != null)
		{
			onAssignedEmployeeChanged.Invoke();
		}
		UpdateStorageText();
		UpdateMaterial();
	}

	private void UpdateStorageText()
	{
		if ((Object)(object)AssignedEmployee != (Object)null)
		{
			Storage.StorageEntityName = AssignedEmployee.FirstName + "'s " + HomeType;
			string text = "<color=#54E717>" + MoneyManager.FormatAmount(AssignedEmployee.DailyWage) + "</color>";
			Storage.StorageEntitySubtitle = AssignedEmployee.fullName + " will draw " + (AssignedEmployee.IsMale ? "his" : "her") + " daily wage of " + text + " from this " + HomeType.ToLower();
		}
		else
		{
			Storage.StorageEntityName = HomeType;
			Storage.StorageEntitySubtitle = string.Empty;
		}
	}

	private void UpdateMaterial()
	{
		MeshRenderer[] employeeSpecificMeshes = EmployeeSpecificMeshes;
		foreach (MeshRenderer val in employeeSpecificMeshes)
		{
			if ((Object)(object)AssignedEmployee != (Object)null)
			{
				switch (AssignedEmployee.EmployeeType)
				{
				case EEmployeeType.Botanist:
					((Renderer)val).material = SpecificMat_Botanist;
					break;
				case EEmployeeType.Chemist:
					((Renderer)val).material = SpecificMat_Chemist;
					break;
				case EEmployeeType.Cleaner:
					((Renderer)val).material = SpecificMat_Cleaner;
					break;
				case EEmployeeType.Handler:
					((Renderer)val).material = SpecificMat_Packager;
					break;
				}
			}
			else
			{
				((Renderer)val).material = SpecificMat_Default;
			}
		}
	}

	public float GetCashSum()
	{
		float num = 0f;
		foreach (ItemSlot itemSlot in Storage.ItemSlots)
		{
			if (itemSlot.ItemInstance != null && itemSlot.ItemInstance is CashInstance)
			{
				num += (itemSlot.ItemInstance as CashInstance).Balance;
			}
		}
		return num;
	}

	public void RemoveCash(float amount)
	{
		foreach (ItemSlot itemSlot in Storage.ItemSlots)
		{
			if (amount <= 0f)
			{
				break;
			}
			if (itemSlot.ItemInstance != null && itemSlot.ItemInstance is CashInstance)
			{
				CashInstance cashInstance = itemSlot.ItemInstance as CashInstance;
				float num = Mathf.Min(amount, cashInstance.Balance);
				cashInstance.ChangeBalance(0f - num);
				itemSlot.ReplicateStoredInstance();
				amount -= num;
			}
		}
	}

	public static bool IsBuildableEntityAValidEmployeeHome(BuildableItem obj, out string reason)
	{
		reason = string.Empty;
		if ((Object)(object)((Component)obj).GetComponent<EmployeeHome>() != (Object)null)
		{
			EmployeeHome component = ((Component)obj).GetComponent<EmployeeHome>();
			if ((Object)(object)component.AssignedEmployee != (Object)null)
			{
				reason = "Already assigned to " + component.AssignedEmployee.fullName;
				return false;
			}
			return true;
		}
		return false;
	}
}
