using System;
using System.Collections;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Interaction;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Variables;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.ItemFramework;

[RequireComponent(typeof(InteractableObject))]
public class NetworkedItemPickup : NetworkBehaviour
{
	public ItemDefinition ItemToGive;

	public bool DestroyOnPickup = true;

	public bool ConditionallyActive;

	public Condition ActiveCondition;

	public bool Networked = true;

	[Header("References")]
	public InteractableObject IntObj;

	public UnityEvent onPickup;

	private bool NetworkInitialize___EarlyScheduleOne_002EItemFramework_002ENetworkedItemPickupAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EItemFramework_002ENetworkedItemPickupAssembly_002DCSharp_002Edll_Excuted;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EItemFramework_002ENetworkedItemPickup_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
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
			Destroy();
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void Destroy()
	{
		RpcWriter___Server_Destroy_2166136261();
		RpcLogic___Destroy_2166136261();
	}

	public override void NetworkInitialize___Early()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EItemFramework_002ENetworkedItemPickupAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EItemFramework_002ENetworkedItemPickupAssembly_002DCSharp_002Edll_Excuted = true;
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_Destroy_2166136261));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EItemFramework_002ENetworkedItemPickupAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EItemFramework_002ENetworkedItemPickupAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_Destroy_2166136261()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkBehaviour)this).IsClientInitialized)
		{
			NetworkManager networkManager = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager == null)
			{
				networkManager = InstanceFinder.NetworkManager;
			}
			if (networkManager != null)
			{
				networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___Destroy_2166136261()
	{
		if (((NetworkBehaviour)this).IsServer)
		{
			((NetworkBehaviour)this).NetworkObject.Despawn((DespawnType?)null);
		}
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	private void RpcReader___Server_Destroy_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___Destroy_2166136261();
		}
	}

	protected virtual void Awake_UserLogic_ScheduleOne_002EItemFramework_002ENetworkedItemPickup_Assembly_002DCSharp_002Edll()
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
}
