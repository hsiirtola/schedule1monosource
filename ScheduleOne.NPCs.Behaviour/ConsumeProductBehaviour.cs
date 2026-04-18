using System;
using System.Collections;
using System.Linq;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Audio;
using ScheduleOne.AvatarFramework.Equipping;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.ItemFramework;
using ScheduleOne.Product;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.NPCs.Behaviour;

public class ConsumeProductBehaviour : Behaviour
{
	public AvatarEquippable JointPrefab;

	public AvatarEquippable PipePrefab;

	public AvatarEquippable ShroomPrefab;

	private ProductItemInstance product;

	private Coroutine consumeRoutine;

	public AudioSourceController WeedConsumeSound;

	public AudioSourceController MethConsumeSound;

	public AudioSourceController SnortSound;

	public AudioSourceController EatSound;

	public ParticleSystem SmokeExhaleParticles;

	public UnityEvent onConsumeDone;

	private TimedCallback _effectsCooldownTimer;

	private bool _removeFromInventoryOnConsume;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EConsumeProductBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EConsumeProductBehaviourAssembly_002DCSharp_002Edll_Excuted;

	public ProductItemInstance ConsumedProduct { get; private set; }

	protected virtual void Start()
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Expected O, but got Unknown
		TimeManager instance = NetworkSingleton<TimeManager>.Instance;
		instance.onSleepEnd = (Action)Delegate.Remove(instance.onSleepEnd, new Action(DayPass));
		TimeManager instance2 = NetworkSingleton<TimeManager>.Instance;
		instance2.onSleepEnd = (Action)Delegate.Combine(instance2.onSleepEnd, new Action(DayPass));
		onConsumeDone.RemoveListener(new UnityAction(ConsumeDone));
		onConsumeDone.AddListener(new UnityAction(ConsumeDone));
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SendProduct(ProductItemInstance _product, bool removeFromInventory)
	{
		RpcWriter___Server_SendProduct_3964170259(_product, removeFromInventory);
		RpcLogic___SendProduct_3964170259(_product, removeFromInventory);
	}

	[ObserversRpc(RunLocally = true)]
	private void SetProduct(ProductItemInstance _product, bool removeFromInventory)
	{
		RpcWriter___Observers_SetProduct_3964170259(_product, removeFromInventory);
		RpcLogic___SetProduct_3964170259(_product, removeFromInventory);
	}

	[ObserversRpc(RunLocally = true)]
	public void ClearEffects()
	{
		RpcWriter___Observers_ClearEffects_2166136261();
		RpcLogic___ClearEffects_2166136261();
	}

	public override void Activate()
	{
		base.Activate();
		TryConsume();
	}

	public override void Resume()
	{
		base.Resume();
		TryConsume();
	}

	private void TryConsume()
	{
		if (product == null)
		{
			Console.LogError("No product to consume");
			Disable();
			return;
		}
		switch ((product.Definition as ProductDefinition).DrugType)
		{
		case EDrugType.Marijuana:
			ConsumeWeed();
			break;
		case EDrugType.Methamphetamine:
			ConsumeMeth();
			break;
		case EDrugType.Cocaine:
			ConsumeCocaine();
			break;
		case EDrugType.Shrooms:
			ConsumeShrooms();
			break;
		default:
			Console.LogError("Trying to consume unsupported drug type: " + (product.Definition as ProductDefinition).DrugType);
			Disable();
			break;
		}
	}

	public override void Disable()
	{
		base.Disable();
		Clear();
		Deactivate();
	}

	public override void Deactivate()
	{
		base.Deactivate();
		if (consumeRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(consumeRoutine);
			consumeRoutine = null;
		}
		base.Npc.SetEquippable_Return(string.Empty);
	}

	private void ConsumeWeed()
	{
		consumeRoutine = ((MonoBehaviour)this).StartCoroutine(ConsumeWeedRoutine());
		IEnumerator ConsumeWeedRoutine()
		{
			base.Npc.SetEquippable_Return(JointPrefab.AssetPath);
			base.Npc.Avatar.Animation.SetBool("Smoking", value: true);
			WeedConsumeSound.Play();
			yield return (object)new WaitForSeconds(3f);
			SmokeExhaleParticles.Play();
			yield return (object)new WaitForSeconds(1.5f);
			base.Npc.Avatar.Animation.SetBool("Smoking", value: false);
			if (InstanceFinder.IsServer)
			{
				ApplyEffects();
				Disable_Networked(null);
			}
			if (onConsumeDone != null)
			{
				onConsumeDone.Invoke();
			}
		}
	}

	private void ConsumeMeth()
	{
		consumeRoutine = ((MonoBehaviour)this).StartCoroutine(ConsumeWeedRoutine());
		IEnumerator ConsumeWeedRoutine()
		{
			base.Npc.SetEquippable_Return(PipePrefab.AssetPath);
			base.Npc.Avatar.Animation.SetBool("Smoking", value: true);
			MethConsumeSound.Play();
			yield return (object)new WaitForSeconds(3f);
			SmokeExhaleParticles.Play();
			yield return (object)new WaitForSeconds(1.5f);
			base.Npc.Avatar.Animation.SetBool("Smoking", value: false);
			if (InstanceFinder.IsServer)
			{
				ApplyEffects();
				Disable_Networked(null);
			}
			if (onConsumeDone != null)
			{
				onConsumeDone.Invoke();
			}
		}
	}

	private void ConsumeCocaine()
	{
		consumeRoutine = ((MonoBehaviour)this).StartCoroutine(ConsumeWeedRoutine());
		IEnumerator ConsumeWeedRoutine()
		{
			base.Npc.Avatar.Animation.SetTrigger("Snort");
			yield return (object)new WaitForSeconds(0.8f);
			SnortSound.Play();
			yield return (object)new WaitForSeconds(1f);
			if (InstanceFinder.IsServer)
			{
				ApplyEffects();
				Disable_Networked(null);
			}
			if (onConsumeDone != null)
			{
				onConsumeDone.Invoke();
			}
		}
	}

	private void ConsumeShrooms()
	{
		consumeRoutine = ((MonoBehaviour)this).StartCoroutine(ConsumeRoutine());
		IEnumerator ConsumeRoutine()
		{
			base.Npc.SetEquippable_Return(ShroomPrefab.AssetPath);
			base.Npc.Avatar.Animation.SetTrigger("Eat");
			yield return (object)new WaitForSeconds(1f);
			base.Npc.SetEquippable_Return(string.Empty);
			EatSound.Play();
			if (InstanceFinder.IsServer)
			{
				ApplyEffects();
				Disable_Networked(null);
			}
			if (onConsumeDone != null)
			{
				onConsumeDone.Invoke();
			}
		}
	}

	[ObserversRpc(RunLocally = true)]
	private void ApplyEffects()
	{
		RpcWriter___Observers_ApplyEffects_2166136261();
		RpcLogic___ApplyEffects_2166136261();
	}

	private void Clear()
	{
		base.Npc.Avatar.Animation.SetBool("Smoking", value: false);
	}

	private void DayPass()
	{
		if (InstanceFinder.IsServer && ConsumedProduct != null)
		{
			ClearEffects();
		}
	}

	private void ConsumeDone()
	{
		if (InstanceFinder.IsServer && _removeFromInventoryOnConsume && ConsumedProduct != null)
		{
			base.Npc.Inventory.GetSlots((ItemSlot x) => x.ItemInstance != null && x.ItemInstance.CanStackWith(ConsumedProduct, checkQuantities: false)).FirstOrDefault()?.ChangeQuantity(-1);
		}
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EConsumeProductBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EConsumeProductBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_SendProduct_3964170259));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_SetProduct_3964170259));
			((NetworkBehaviour)this).RegisterObserversRpc(2u, new ClientRpcDelegate(RpcReader___Observers_ClearEffects_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(3u, new ClientRpcDelegate(RpcReader___Observers_ApplyEffects_2166136261));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EConsumeProductBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EConsumeProductBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_SendProduct_3964170259(ProductItemInstance _product, bool removeFromInventory)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)(object)writer).WriteProductItemInstance(_product);
			((Writer)writer).WriteBoolean(removeFromInventory);
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendProduct_3964170259(ProductItemInstance _product, bool removeFromInventory)
	{
		SetProduct(_product, removeFromInventory);
	}

	private void RpcReader___Server_SendProduct_3964170259(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		ProductItemInstance productItemInstance = ((Reader)(object)PooledReader0).ReadProductItemInstance();
		bool removeFromInventory = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendProduct_3964170259(productItemInstance, removeFromInventory);
		}
	}

	private void RpcWriter___Observers_SetProduct_3964170259(ProductItemInstance _product, bool removeFromInventory)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)(object)writer).WriteProductItemInstance(_product);
			((Writer)writer).WriteBoolean(removeFromInventory);
			((NetworkBehaviour)this).SendObserversRpc(1u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetProduct_3964170259(ProductItemInstance _product, bool removeFromInventory)
	{
		product = _product;
		_removeFromInventoryOnConsume = removeFromInventory;
	}

	private void RpcReader___Observers_SetProduct_3964170259(PooledReader PooledReader0, Channel channel)
	{
		ProductItemInstance productItemInstance = ((Reader)(object)PooledReader0).ReadProductItemInstance();
		bool removeFromInventory = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetProduct_3964170259(productItemInstance, removeFromInventory);
		}
	}

	private void RpcWriter___Observers_ClearEffects_2166136261()
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
			((NetworkBehaviour)this).SendObserversRpc(2u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___ClearEffects_2166136261()
	{
		if (ConsumedProduct != null)
		{
			Debug.Log((object)("Clearing effects of product: " + ((BaseItemDefinition)ConsumedProduct.Definition).Name));
			ConsumedProduct.ClearEffectsFromNPC(base.Npc);
			ConsumedProduct = null;
			if (_effectsCooldownTimer != null)
			{
				_effectsCooldownTimer.Cancel();
				_effectsCooldownTimer = null;
			}
		}
	}

	private void RpcReader___Observers_ClearEffects_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___ClearEffects_2166136261();
		}
	}

	private void RpcWriter___Observers_ApplyEffects_2166136261()
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
			((NetworkBehaviour)this).SendObserversRpc(3u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___ApplyEffects_2166136261()
	{
		if (ConsumedProduct == product)
		{
			if (InstanceFinder.IsServer && _effectsCooldownTimer != null)
			{
				_effectsCooldownTimer.Reset();
			}
			return;
		}
		if (ConsumedProduct != null)
		{
			ClearEffects();
		}
		ConsumedProduct = product;
		if (product != null)
		{
			product.ApplyEffectsToNPC(base.Npc);
		}
		if (InstanceFinder.IsServer)
		{
			_effectsCooldownTimer = new TimedCallback(ClearEffects, (product.Definition as ProductDefinition).NPCEffectDuration);
		}
	}

	private void RpcReader___Observers_ApplyEffects_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___ApplyEffects_2166136261();
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
