using FishNet.Object;
using ScheduleOne.Property;
using UnityEngine;

namespace ScheduleOne.DevUtilities;

[RequireComponent(typeof(ScheduleOne.Property.Property))]
public class PropertyTestTool : NetworkBehaviour
{
	public ScheduleOne.Property.Property Property;

	public TextAsset PropertyDataToLoad;

	private bool NetworkInitialize___EarlyScheduleOne_002EDevUtilities_002EPropertyTestToolAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EDevUtilities_002EPropertyTestToolAssembly_002DCSharp_002Edll_Excuted;

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002EDevUtilities_002EPropertyTestToolAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EDevUtilities_002EPropertyTestToolAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EDevUtilities_002EPropertyTestToolAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EDevUtilities_002EPropertyTestToolAssembly_002DCSharp_002Edll_Excuted = true;
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
		NetworkInitialize__Late();
	}
}
