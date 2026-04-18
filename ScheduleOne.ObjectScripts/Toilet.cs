using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.EntityFramework;
using ScheduleOne.Interaction;
using ScheduleOne.Trash;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.ObjectScripts;

public class Toilet : GridItem
{
	public float InitialDelay = 0.5f;

	public float FlushTime = 5f;

	public InteractableObject IntObj;

	public LayerMask ItemLayerMask;

	public SphereCollider ItemDetectionCollider;

	public UnityEvent OnFlush;

	private Coroutine _flushCoroutine;

	private bool isFlushing;

	private bool NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EToiletAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EObjectScripts_002EToiletAssembly_002DCSharp_002Edll_Excuted;

	public void Hovered()
	{
		if (!isFlushing)
		{
			IntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
			IntObj.SetMessage("Flush");
		}
		else
		{
			IntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
		}
	}

	public void Interacted()
	{
		isFlushing = true;
		SendFlush();
	}

	[ServerRpc(RequireOwnership = false)]
	private void SendFlush()
	{
		RpcWriter___Server_SendFlush_2166136261();
	}

	[ObserversRpc]
	private void Flush()
	{
		RpcWriter___Observers_Flush_2166136261();
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EToiletAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EToiletAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterServerRpc(5u, new ServerRpcDelegate(RpcReader___Server_SendFlush_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(6u, new ClientRpcDelegate(RpcReader___Observers_Flush_2166136261));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EObjectScripts_002EToiletAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EObjectScripts_002EToiletAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_SendFlush_2166136261()
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
			((NetworkBehaviour)this).SendServerRpc(5u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___SendFlush_2166136261()
	{
		Flush();
	}

	private void RpcReader___Server_SendFlush_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SendFlush_2166136261();
		}
	}

	private void RpcWriter___Observers_Flush_2166136261()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkBehaviour)this).IsServerInitialized)
		{
			NetworkManager networkManager = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager == null)
			{
				networkManager = InstanceFinder.NetworkManager;
			}
			if (networkManager != null)
			{
				networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((NetworkBehaviour)this).SendObserversRpc(6u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___Flush_2166136261()
	{
		isFlushing = true;
		_flushCoroutine = ((MonoBehaviour)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			if (OnFlush != null)
			{
				OnFlush.Invoke();
			}
			yield return (object)new WaitForSeconds(InitialDelay);
			float checkRate = 0.5f;
			int reps = (int)(FlushTime / checkRate);
			for (int i = 0; i < reps; i++)
			{
				if (InstanceFinder.IsServer)
				{
					Collider[] array = Physics.OverlapSphere(((Component)ItemDetectionCollider).transform.position, ItemDetectionCollider.radius, LayerMask.op_Implicit(ItemLayerMask));
					List<TrashItem> list = new List<TrashItem>();
					Collider[] array2 = array;
					for (int j = 0; j < array2.Length; j++)
					{
						TrashItem componentInParent = ((Component)array2[j]).GetComponentInParent<TrashItem>();
						if ((Object)(object)componentInParent != (Object)null && !list.Contains(componentInParent))
						{
							list.Add(componentInParent);
						}
					}
					if (list.Count > 0)
					{
						foreach (TrashItem item in list)
						{
							item.DestroyTrash();
						}
					}
				}
				yield return (object)new WaitForSeconds(checkRate);
			}
			_flushCoroutine = null;
			isFlushing = false;
		}
	}

	private void RpcReader___Observers_Flush_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___Flush_2166136261();
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
