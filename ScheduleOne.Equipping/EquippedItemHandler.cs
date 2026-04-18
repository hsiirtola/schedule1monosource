using System;
using FishNet.Component.Ownership;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Object.Synchronizing;
using FishNet.Object.Synchronizing.Internal;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Core.Equipping.Framework;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.Equipping.Framework;
using UnityEngine;

namespace ScheduleOne.Equipping;

[RequireComponent(typeof(PredictedSpawn))]
public class EquippedItemHandler : NetworkBehaviour, IEquippedItemHandler
{
	[SyncVar]
	public INetworkedEquippableUser _user;

	[SyncVar]
	[HideInInspector]
	public EquippableData _equippableData;

	public SyncVar<INetworkedEquippableUser> syncVar____user;

	public SyncVar<EquippableData> syncVar____equippableData;

	private bool NetworkInitialize___EarlyScheduleOne_002EEquipping_002EEquippedItemHandlerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EEquipping_002EEquippedItemHandlerAssembly_002DCSharp_002Edll_Excuted;

	public IEquippableUser User => (IEquippableUser)SyncAccessor__user;

	public EquippableData EquippableData => SyncAccessor__equippableData;

	public bool IsEquipped { get; private set; }

	public INetworkedEquippableUser SyncAccessor__user
	{
		get
		{
			return _user;
		}
		set
		{
			if (value || !((NetworkBehaviour)this).IsServerInitialized)
			{
				_user = value;
			}
			if (Application.isPlaying)
			{
				syncVar____user.SetValue(value, value);
			}
		}
	}

	public EquippableData SyncAccessor__equippableData
	{
		get
		{
			return _equippableData;
		}
		set
		{
			if (value || !((NetworkBehaviour)this).IsServerInitialized)
			{
				_equippableData = value;
			}
			if (Application.isPlaying)
			{
				syncVar____equippableData.SetValue(value, value);
			}
		}
	}

	public event Action OnUnequipped;

	public virtual void Equipped(IEquippableUser user, EquippableData data)
	{
		if (!(user is INetworkedEquippableUser))
		{
			Debug.LogError((object)$"EquippedItemHandler can only be used with users that implement INetworkedEquippableUser. User {user} does not implement this interface.");
			return;
		}
		if ((Object)(object)data == (Object)null)
		{
			Debug.LogError((object)"EquippedItemHandler.Equipped called with null EquippableData.");
			return;
		}
		IsEquipped = true;
		this.sync___set_value__user(user as INetworkedEquippableUser, true);
		this.sync___set_value__equippableData(data, true);
		if (!((NetworkBehaviour)this).IsNetworked)
		{
			SetupParent();
		}
		if (((IEquippableUser)SyncAccessor__user).IsLocalPlayer)
		{
			SetupFirstPerson();
		}
		SetupThirdPerson();
	}

	public virtual void EquippedWithItem(IEquippableUser user, EquippableData data, BaseItemInstance itemInstance)
	{
		Equipped(user, data);
	}

	public virtual void Unequipped()
	{
		if (IsEquipped)
		{
			IsEquipped = false;
			if (this.OnUnequipped != null)
			{
				this.OnUnequipped();
			}
			if (((NetworkBehaviour)this).IsSpawned && ((NetworkBehaviour)this).IsServer)
			{
				((NetworkBehaviour)this).Despawn(((Component)this).gameObject, (DespawnType?)(DespawnType)0);
			}
			else if (((NetworkBehaviour)this).IsSpawned)
			{
				((Component)this).gameObject.SetActive(false);
			}
			else
			{
				Object.Destroy((Object)(object)((Component)this).gameObject);
			}
		}
	}

	public override void OnStartClient()
	{
		((NetworkBehaviour)this).OnStartClient();
		if (!IsEquipped)
		{
			Equipped((IEquippableUser)SyncAccessor__user, SyncAccessor__equippableData);
		}
		SetupParent();
	}

	private void SetupParent()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.SetParent(SyncAccessor__user.ItemHandlerContainer, false);
		((Component)this).transform.localPosition = Vector3.zero;
		((Component)this).transform.localRotation = Quaternion.identity;
	}

	protected virtual void SetupThirdPerson()
	{
		if (!((Object)(object)SyncAccessor__equippableData.ThirdPersonEquippedItemPrefab == (Object)null))
		{
			((Component)Object.Instantiate<TPEquippedItem>(SyncAccessor__equippableData.ThirdPersonEquippedItemPrefab)).GetComponent<TPEquippedItem>().Equip((IEquippedItemHandler)(object)this);
		}
	}

	protected virtual void SetupFirstPerson()
	{
		if (!((IEquippableUser)SyncAccessor__user).IsLocalPlayer)
		{
			Debug.LogWarning((object)"SetupFirstPerson called on an item handler that doesn't belong to the local player. This should only be called on the local player's equipped items.");
		}
		else if (!(SyncAccessor__user is IEquippablePlayerUser))
		{
			Debug.LogWarning((object)"SetupFirstPerson called on an item handler whose user does not implement IEquippablePlayerUser.");
		}
		else if (!((Object)(object)SyncAccessor__equippableData.FirstPersonEquippedItemPrefab == (Object)null))
		{
			FPEquippedItem component = ((Component)Object.Instantiate<FPEquippedItem>(SyncAccessor__equippableData.FirstPersonEquippedItemPrefab)).GetComponent<FPEquippedItem>();
			INetworkedEquippableUser networkedEquippableUser = SyncAccessor__user;
			component.Equip((IEquippedItemHandler)(object)this, (IEquippablePlayerUser)((networkedEquippableUser is IEquippablePlayerUser) ? networkedEquippableUser : null));
		}
	}

	protected virtual void Update()
	{
		if (User.IsLocalPlayer)
		{
			UserUpdate();
		}
	}

	protected virtual void UserUpdate()
	{
	}

	public override void NetworkInitialize___Early()
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EEquipping_002EEquippedItemHandlerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EEquipping_002EEquippedItemHandlerAssembly_002DCSharp_002Edll_Excuted = true;
			syncVar____equippableData = new SyncVar<EquippableData>((NetworkBehaviour)(object)this, 1u, (WritePermission)0, (ReadPermission)0, -1f, (Channel)0, _equippableData);
			syncVar____user = new SyncVar<INetworkedEquippableUser>((NetworkBehaviour)(object)this, 0u, (WritePermission)0, (ReadPermission)0, -1f, (Channel)0, _user);
			((NetworkBehaviour)this).RegisterSyncVarRead(new SyncVarReadDelegate(ReadSyncVar___ScheduleOne_002EEquipping_002EEquippedItemHandler));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EEquipping_002EEquippedItemHandlerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EEquipping_002EEquippedItemHandlerAssembly_002DCSharp_002Edll_Excuted = true;
			((SyncBase)syncVar____equippableData).SetRegistered();
			((SyncBase)syncVar____user).SetRegistered();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	public override bool ReadSyncVar___ScheduleOne_002EEquipping_002EEquippedItemHandler(PooledReader PooledReader0, uint UInt321, bool Boolean2)
	{
		switch (UInt321)
		{
		case 1u:
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value__equippableData(syncVar____equippableData.GetValue(true), true);
				return true;
			}
			EquippableData value2 = ((Reader)(object)PooledReader0).ReadEquippableData();
			this.sync___set_value__equippableData(value2, Boolean2);
			return true;
		}
		case 0u:
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value__user(syncVar____user.GetValue(true), true);
				return true;
			}
			INetworkedEquippableUser value = ((Reader)(object)PooledReader0).ReadINetworkedEquippableUser();
			this.sync___set_value__user(value, Boolean2);
			return true;
		}
		default:
			return false;
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}
}
