using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Object.Synchronizing;
using FishNet.Object.Synchronizing.Internal;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.Interaction;
using ScheduleOne.Money;
using ScheduleOne.ObjectScripts.Cash;
using UnityEngine;

namespace ScheduleOne.ItemFramework;

public class CashPickup : NetworkedItemPickup
{
	[SyncVar(OnChange = "ValueChanged")]
	public float Value = 10f;

	public bool PlayCashPickupSound;

	[Header("References")]
	public CashStackVisuals CashStackVisuals;

	public SyncVar<float> syncVar___Value;

	private bool NetworkInitialize___EarlyScheduleOne_002EItemFramework_002ECashPickupAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EItemFramework_002ECashPickupAssembly_002DCSharp_002Edll_Excuted;

	public float SyncAccessor_Value
	{
		get
		{
			return Value;
		}
		set
		{
			if (value || !((NetworkBehaviour)this).IsServerInitialized)
			{
				Value = value;
			}
			if (Application.isPlaying)
			{
				syncVar___Value.SetValue(value, value);
			}
		}
	}

	private void Start()
	{
		UpdateCashStackVisuals();
	}

	protected override void Hovered()
	{
		IntObj.SetMessage("Pick up " + MoneyManager.FormatAmount(SyncAccessor_Value));
		IntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
	}

	protected override bool CanPickup()
	{
		return true;
	}

	protected override void Pickup()
	{
		NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(SyncAccessor_Value);
		if (PlayCashPickupSound)
		{
			NetworkSingleton<MoneyManager>.Instance.PlayCashSound();
		}
		base.Pickup();
	}

	private void ValueChanged(float oldValue, float newValue, bool asServer)
	{
		UpdateCashStackVisuals();
	}

	private void UpdateCashStackVisuals()
	{
		if ((Object)(object)CashStackVisuals != (Object)null)
		{
			CashStackVisuals.ShowAmount(SyncAccessor_Value);
		}
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EItemFramework_002ECashPickupAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EItemFramework_002ECashPickupAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			syncVar___Value = new SyncVar<float>((NetworkBehaviour)(object)this, 0u, (WritePermission)0, (ReadPermission)0, -1f, (Channel)0, Value);
			syncVar___Value.OnChange += ValueChanged;
			((NetworkBehaviour)this).RegisterSyncVarRead(new SyncVarReadDelegate(ReadSyncVar___ScheduleOne_002EItemFramework_002ECashPickup));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EItemFramework_002ECashPickupAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EItemFramework_002ECashPickupAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
			((SyncBase)syncVar___Value).SetRegistered();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	public override bool ReadSyncVar___ScheduleOne_002EItemFramework_002ECashPickup(PooledReader PooledReader0, uint UInt321, bool Boolean2)
	{
		if (UInt321 == 0)
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value_Value(syncVar___Value.GetValue(true), true);
				return true;
			}
			float value = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
			this.sync___set_value_Value(value, Boolean2);
			return true;
		}
		return false;
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
