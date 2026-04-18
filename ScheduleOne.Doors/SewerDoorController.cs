using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Map;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Doors;

public class SewerDoorController : DoorController
{
	private bool NetworkInitialize___EarlyScheduleOne_002EDoors_002ESewerDoorControllerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EDoors_002ESewerDoorControllerAssembly_002DCSharp_002Edll_Excuted;

	protected override bool CanPlayerAccess(EDoorSide side, out string reason)
	{
		reason = string.Empty;
		if (side == EDoorSide.Exterior && !NetworkSingleton<SewerManager>.Instance.IsSewerUnlocked && !base.IsOpen)
		{
			if (PlayerSingleton<PlayerInventory>.Instance.GetAmountOfItem(((BaseItemDefinition)NetworkSingleton<SewerManager>.Instance.SewerKeyItem).ID) != 0)
			{
				return true;
			}
			reason = ((BaseItemDefinition)NetworkSingleton<SewerManager>.Instance.SewerKeyItem).Name + " required";
			return false;
		}
		return base.CanPlayerAccess(side, out reason);
	}

	public override void ExteriorHandleInteracted()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		base.ExteriorHandleInteracted();
		if (CanPlayerAccess(EDoorSide.Exterior) && !NetworkSingleton<SewerManager>.Instance.IsSewerUnlocked)
		{
			((Component)NetworkSingleton<SewerManager>.Instance.SewerUnlockSound).transform.position = ((Component)ExteriorIntObjs[0]).transform.position;
			NetworkSingleton<SewerManager>.Instance.SewerUnlockSound.Play();
			NetworkSingleton<SewerManager>.Instance.SetSewerUnlocked_Server();
			PlayerSingleton<PlayerInventory>.Instance.RemoveAmountOfItem(((BaseItemDefinition)NetworkSingleton<SewerManager>.Instance.SewerKeyItem).ID);
		}
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002EDoors_002ESewerDoorControllerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EDoors_002ESewerDoorControllerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EDoors_002ESewerDoorControllerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EDoors_002ESewerDoorControllerAssembly_002DCSharp_002Edll_Excuted = true;
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
