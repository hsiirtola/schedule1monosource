using FishNet.Object;
using ScheduleOne.DevUtilities;
using ScheduleOne.Effects;
using ScheduleOne.Employees;
using ScheduleOne.GameTime;
using ScheduleOne.Interaction;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Product;
using ScheduleOne.Tools;
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.ObjectScripts;

public class Bed : NetworkBehaviour
{
	public const int MIN_SLEEP_TIME = 1800;

	[Header("References")]
	[SerializeField]
	protected InteractableObject intObj;

	public EmployeeHome EmployeeStationThing;

	public MeshRenderer BlanketMesh;

	[Header("Materials")]
	public Material DefaultBlanket;

	public Material BotanistBlanket;

	public Material ChemistBlanket;

	public Material PackagerBlanket;

	public Material CleanerBlanket;

	private bool NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EBedAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EObjectScripts_002EBedAssembly_002DCSharp_002Edll_Excuted;

	public Employee AssignedEmployee
	{
		get
		{
			if (!((Object)(object)EmployeeStationThing != (Object)null))
			{
				return null;
			}
			return EmployeeStationThing.AssignedEmployee;
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EObjectScripts_002EBed_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public void Hovered()
	{
		string noSleepReason;
		if (Singleton<ManagementClipboard>.Instance.IsEquipped)
		{
			intObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
		}
		else if ((Object)(object)AssignedEmployee != (Object)null)
		{
			intObj.SetInteractableState(InteractableObject.EInteractableState.Invalid);
			intObj.SetMessage("Assigned to " + AssignedEmployee.fullName);
		}
		else if (CanSleep(out noSleepReason))
		{
			intObj.SetInteractableState(InteractableObject.EInteractableState.Default);
			intObj.SetMessage("Sleep");
		}
		else
		{
			intObj.SetInteractableState(InteractableObject.EInteractableState.Invalid);
			intObj.SetMessage(noSleepReason);
		}
	}

	public void Interacted()
	{
		Player.Local.CurrentBed = ((NetworkBehaviour)this).NetworkObject;
		Singleton<SleepCanvas>.Instance.SetIsOpen(open: true);
	}

	private bool CanSleep(out string noSleepReason)
	{
		noSleepReason = string.Empty;
		if (GameManager.IS_TUTORIAL)
		{
			return true;
		}
		if (!NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(1800, 400))
		{
			noSleepReason = "Can't sleep before " + TimeManager.Get12HourTime(1800f);
			return false;
		}
		if (Player.Local.ConsumedProduct != null && (Player.Local.ConsumedProduct.Definition as ProductDefinition).Properties.Exists((Effect x) => ((object)x).GetType() == typeof(Energizing)))
		{
			noSleepReason = "Can't sleep while energized!";
			return false;
		}
		if (Player.Local.ConsumedProduct != null && (Player.Local.ConsumedProduct.Definition as ProductDefinition).Properties.Exists((Effect x) => ((object)x).GetType() == typeof(Athletic)))
		{
			noSleepReason = "Can't sleep while athletic!";
			return false;
		}
		return true;
	}

	public void UpdateMaterial()
	{
		if ((Object)(object)BlanketMesh == (Object)null)
		{
			return;
		}
		Material material = DefaultBlanket;
		if ((Object)(object)AssignedEmployee != (Object)null)
		{
			switch (AssignedEmployee.EmployeeType)
			{
			case EEmployeeType.Botanist:
				material = BotanistBlanket;
				break;
			case EEmployeeType.Chemist:
				material = ChemistBlanket;
				break;
			case EEmployeeType.Handler:
				material = PackagerBlanket;
				break;
			case EEmployeeType.Cleaner:
				material = CleanerBlanket;
				break;
			}
		}
		((Renderer)BlanketMesh).material = material;
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EBedAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EBedAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EObjectScripts_002EBedAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EObjectScripts_002EBedAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void Awake_UserLogic_ScheduleOne_002EObjectScripts_002EBed_Assembly_002DCSharp_002Edll()
	{
		UpdateMaterial();
	}
}
