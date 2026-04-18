using FishNet.Object;
using UnityEngine;

namespace ScheduleOne.DevUtilities;

public abstract class NetworkSingleton<T> : NetworkBehaviour where T : NetworkSingleton<T>
{
	private static T instance;

	protected bool Destroyed;

	private bool NetworkInitialize___EarlyScheduleOne_002EDevUtilities_002ENetworkSingleton_00601Assembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EDevUtilities_002ENetworkSingleton_00601Assembly_002DCSharp_002Edll_Excuted;

	public static bool InstanceExists => (Object)(object)instance != (Object)null;

	public static T Instance
	{
		get
		{
			return instance;
		}
		protected set
		{
			instance = value;
		}
	}

	protected virtual void Start()
	{
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EDevUtilities_002ENetworkSingleton_00601_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	protected virtual void OnDestroy()
	{
		if ((Object)(object)instance == (Object)(object)this)
		{
			instance = null;
		}
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002EDevUtilities_002ENetworkSingleton_00601Assembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EDevUtilities_002ENetworkSingleton_00601Assembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EDevUtilities_002ENetworkSingleton_00601Assembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EDevUtilities_002ENetworkSingleton_00601Assembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	protected virtual void Awake_UserLogic_ScheduleOne_002EDevUtilities_002ENetworkSingleton_00601_Assembly_002DCSharp_002Edll()
	{
		if ((Object)(object)instance != (Object)null)
		{
			Console.LogWarning("Multiple instances of " + ((Object)this).name + " exist. Keeping prior instance reference.");
		}
		else
		{
			instance = (T)this;
		}
	}
}
