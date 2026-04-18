using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Interaction;
using ScheduleOne.Misc;
using ScheduleOne.Money;
using ScheduleOne.Trash;
using ScheduleOne.Variables;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.ObjectScripts;

public class Recycler : NetworkBehaviour
{
	public enum EState
	{
		HatchClosed,
		HatchOpen,
		Processing
	}

	public LayerMask DetectionMask;

	[Header("References")]
	public InteractableObject HandleIntObj;

	public InteractableObject ButtonIntObj;

	public InteractableObject CashIntObj;

	public ToggleableLight ButtonLight;

	public Animation ButtonAnim;

	public Animation HatchAnim;

	public Animation CashAnim;

	public RectTransform OpenHatchInstruction;

	public RectTransform InsertTrashInstruction;

	public RectTransform PressBeginInstruction;

	public RectTransform ProcessingScreen;

	public TextMeshProUGUI ProcessingLabel;

	public TextMeshProUGUI ValueLabel;

	public BoxCollider CheckCollider;

	public Transform Cash;

	public GameObject BankNote;

	[Header("Sound")]
	public AudioSourceController OpenSound;

	public AudioSourceController CloseSound;

	public AudioSourceController PressSound;

	public AudioSourceController DoneSound;

	public AudioSourceController CashEjectSound;

	private float cashValue;

	public UnityEvent onStart;

	public UnityEvent onStop;

	private bool NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002ERecyclerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EObjectScripts_002ERecyclerAssembly_002DCSharp_002Edll_Excuted;

	public EState State { get; protected set; }

	public bool IsHatchOpen { get; private set; }

	public void Start()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		HandleIntObj.onInteractStart.AddListener(new UnityAction(HandleInteracted));
		ButtonIntObj.onInteractStart.AddListener(new UnityAction(ButtonInteracted));
		CashIntObj.onInteractStart.AddListener(new UnityAction(CashInteracted));
		NetworkSingleton<TimeManager>.Instance.onTick += new Action(OnTick);
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		((NetworkBehaviour)this).OnSpawnServer(connection);
		SetState(connection, State, force: true);
	}

	private void OnDestroy()
	{
		if (NetworkSingleton<TimeManager>.InstanceExists)
		{
			NetworkSingleton<TimeManager>.Instance.onTick -= new Action(OnTick);
		}
	}

	private void OnTick()
	{
		if (State == EState.HatchOpen)
		{
			((Component)OpenHatchInstruction).gameObject.SetActive(false);
			((Component)InsertTrashInstruction).gameObject.SetActive(false);
			((Component)PressBeginInstruction).gameObject.SetActive(false);
			((Component)ProcessingScreen).gameObject.SetActive(false);
			if (GetTrash().Length != 0)
			{
				ButtonIntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
				ButtonLight.isOn = true;
				((Component)PressBeginInstruction).gameObject.SetActive(true);
			}
			else
			{
				ButtonIntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
				ButtonLight.isOn = false;
				((Component)InsertTrashInstruction).gameObject.SetActive(true);
			}
		}
	}

	public void HandleInteracted()
	{
		SendState(EState.HatchOpen);
	}

	public void ButtonInteracted()
	{
		((TMP_Text)ProcessingLabel).text = "Processing...";
		((TMP_Text)ValueLabel).text = MoneyManager.FormatAmount(0f);
		PressSound.Play();
		SendState(EState.Processing);
		((MonoBehaviour)this).StartCoroutine(Process(startedByLocalPlayer: true));
	}

	public void CashInteracted()
	{
		NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(cashValue);
		NetworkSingleton<MoneyManager>.Instance.ChangeLifetimeEarnings(cashValue);
		SendState(EState.HatchClosed);
		BankNote.gameObject.SetActive(false);
		cashValue = 0f;
		SendCashCollected();
	}

	[ServerRpc(RequireOwnership = false)]
	private void SendCashCollected()
	{
		RpcWriter___Server_SendCashCollected_2166136261();
	}

	[ObserversRpc(RunLocally = true)]
	private void CashCollected()
	{
		RpcWriter___Observers_CashCollected_2166136261();
		RpcLogic___CashCollected_2166136261();
	}

	[ObserversRpc(RunLocally = true)]
	private void EnableCash()
	{
		RpcWriter___Observers_EnableCash_2166136261();
		RpcLogic___EnableCash_2166136261();
	}

	[ObserversRpc(RunLocally = true)]
	private void SetCashValue(float amount)
	{
		RpcWriter___Observers_SetCashValue_431000436(amount);
		RpcLogic___SetCashValue_431000436(amount);
	}

	private IEnumerator Process(bool startedByLocalPlayer)
	{
		yield return (object)new WaitForSeconds(0.5f);
		if (onStart != null)
		{
			onStart.Invoke();
		}
		TrashItem[] trash = GetTrash();
		if (startedByLocalPlayer)
		{
			int num = trash.Length;
			float num2 = NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("TrashRecycled") + (float)num;
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("TrashRecycled", num2.ToString());
			if (num2 >= 500f)
			{
				AchievementManager.UnlockAchievement(AchievementManager.EAchievement.UPSTANDING_CITIZEN);
			}
		}
		float value = 0f;
		TrashItem[] array = trash;
		foreach (TrashItem trashItem in array)
		{
			if (trashItem is TrashBag)
			{
				foreach (TrashContent.Entry entry in ((TrashBag)trashItem).Content.Entries)
				{
					value += (float)(entry.UnitValue * entry.Quantity);
				}
			}
			else
			{
				value += (float)trashItem.SellValue;
			}
			if (InstanceFinder.IsServer)
			{
				trashItem.DestroyTrash();
			}
		}
		if (cashValue <= 0f)
		{
			SetCashValue(value);
		}
		float lerpTime = 1.5f;
		for (float i2 = 0f; i2 < lerpTime; i2 += Time.deltaTime)
		{
			float num3 = i2 / lerpTime;
			float amount = Mathf.Lerp(0f, cashValue, num3);
			((TMP_Text)ValueLabel).text = MoneyManager.FormatAmount(amount, showDecimals: true);
			yield return (object)new WaitForEndOfFrame();
		}
		if (onStop != null)
		{
			onStop.Invoke();
		}
		((TMP_Text)ProcessingLabel).text = "Thank you";
		((TMP_Text)ValueLabel).text = MoneyManager.FormatAmount(value);
		DoneSound.Play();
		yield return (object)new WaitForSeconds(0.3f);
		CashEjectSound.Play();
		CashAnim.Play();
		yield return (object)new WaitForSeconds(0.25f);
		if (InstanceFinder.IsServer)
		{
			EnableCash();
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SendState(EState state)
	{
		RpcWriter___Server_SendState_3569965459(state);
		RpcLogic___SendState_3569965459(state);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void SetState(NetworkConnection conn, EState state, bool force = false)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetState_3790170803(conn, state, force);
			RpcLogic___SetState_3790170803(conn, state, force);
		}
		else
		{
			RpcWriter___Target_SetState_3790170803(conn, state, force);
		}
	}

	private void SetHatchOpen(bool open)
	{
		if (open != IsHatchOpen)
		{
			IsHatchOpen = open;
			if (IsHatchOpen)
			{
				OpenSound.Play();
				HatchAnim.Play("Recycler open");
			}
			else
			{
				CloseSound.Play();
				HatchAnim.Play("Recycler close");
			}
		}
	}

	private TrashItem[] GetTrash()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		List<TrashItem> list = new List<TrashItem>();
		Vector3 val = ((Component)CheckCollider).transform.TransformPoint(CheckCollider.center);
		Vector3 val2 = Vector3.Scale(CheckCollider.size, ((Component)CheckCollider).transform.lossyScale) * 0.5f;
		Collider[] array = Physics.OverlapBox(val, val2, ((Component)CheckCollider).transform.rotation, LayerMask.op_Implicit(DetectionMask), (QueryTriggerInteraction)2);
		for (int i = 0; i < array.Length; i++)
		{
			TrashItem componentInParent = ((Component)array[i]).GetComponentInParent<TrashItem>();
			if ((Object)(object)componentInParent != (Object)null && !list.Contains(componentInParent))
			{
				list.Add(componentInParent);
			}
		}
		return list.ToArray();
	}

	private void OnDrawGizmos()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)CheckCollider).transform.TransformPoint(CheckCollider.center);
		Vector3 val2 = Vector3.Scale(CheckCollider.size, ((Component)CheckCollider).transform.lossyScale) * 0.5f;
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(val, val2 * 2f);
	}

	public override void NetworkInitialize___Early()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected O, but got Unknown
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002ERecyclerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002ERecyclerAssembly_002DCSharp_002Edll_Excuted = true;
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_SendCashCollected_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_CashCollected_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(2u, new ClientRpcDelegate(RpcReader___Observers_EnableCash_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(3u, new ClientRpcDelegate(RpcReader___Observers_SetCashValue_431000436));
			((NetworkBehaviour)this).RegisterServerRpc(4u, new ServerRpcDelegate(RpcReader___Server_SendState_3569965459));
			((NetworkBehaviour)this).RegisterObserversRpc(5u, new ClientRpcDelegate(RpcReader___Observers_SetState_3790170803));
			((NetworkBehaviour)this).RegisterTargetRpc(6u, new ClientRpcDelegate(RpcReader___Target_SetState_3790170803));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EObjectScripts_002ERecyclerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EObjectScripts_002ERecyclerAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_SendCashCollected_2166136261()
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

	private void RpcLogic___SendCashCollected_2166136261()
	{
		CashCollected();
	}

	private void RpcReader___Server_SendCashCollected_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SendCashCollected_2166136261();
		}
	}

	private void RpcWriter___Observers_CashCollected_2166136261()
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
			((NetworkBehaviour)this).SendObserversRpc(1u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___CashCollected_2166136261()
	{
		SendState(EState.HatchClosed);
		BankNote.gameObject.SetActive(false);
		cashValue = 0f;
	}

	private void RpcReader___Observers_CashCollected_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___CashCollected_2166136261();
		}
	}

	private void RpcWriter___Observers_EnableCash_2166136261()
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

	private void RpcLogic___EnableCash_2166136261()
	{
		CashIntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
	}

	private void RpcReader___Observers_EnableCash_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___EnableCash_2166136261();
		}
	}

	private void RpcWriter___Observers_SetCashValue_431000436(float amount)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteSingle(amount, (AutoPackType)0);
			((NetworkBehaviour)this).SendObserversRpc(3u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetCashValue_431000436(float amount)
	{
		cashValue = amount;
	}

	private void RpcReader___Observers_SetCashValue_431000436(PooledReader PooledReader0, Channel channel)
	{
		float amount = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetCashValue_431000436(amount);
		}
	}

	private void RpcWriter___Server_SendState_3569965459(EState state)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EObjectScripts_002ERecycler_002FEStateFishNet_002ESerializing_002EGenerated((Writer)(object)writer, state);
			((NetworkBehaviour)this).SendServerRpc(4u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendState_3569965459(EState state)
	{
		SetState(null, state);
	}

	private void RpcReader___Server_SendState_3569965459(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		EState state = GeneratedReaders___Internal.Read___ScheduleOne_002EObjectScripts_002ERecycler_002FEStateFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendState_3569965459(state);
		}
	}

	private void RpcWriter___Observers_SetState_3790170803(NetworkConnection conn, EState state, bool force = false)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EObjectScripts_002ERecycler_002FEStateFishNet_002ESerializing_002EGenerated((Writer)(object)writer, state);
			((Writer)writer).WriteBoolean(force);
			((NetworkBehaviour)this).SendObserversRpc(5u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetState_3790170803(NetworkConnection conn, EState state, bool force = false)
	{
		if (State == state && !force)
		{
			return;
		}
		State = state;
		HandleIntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
		ButtonIntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
		CashIntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
		((Component)OpenHatchInstruction).gameObject.SetActive(false);
		((Component)InsertTrashInstruction).gameObject.SetActive(false);
		((Component)PressBeginInstruction).gameObject.SetActive(false);
		((Component)ProcessingScreen).gameObject.SetActive(false);
		ButtonLight.isOn = false;
		((Component)Cash).gameObject.SetActive(false);
		switch (State)
		{
		case EState.HatchClosed:
			HandleIntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
			((Component)OpenHatchInstruction).gameObject.SetActive(true);
			break;
		case EState.HatchOpen:
			if (GetTrash().Length != 0)
			{
				ButtonIntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
				ButtonLight.isOn = true;
				((Component)PressBeginInstruction).gameObject.SetActive(true);
			}
			else
			{
				((Component)InsertTrashInstruction).gameObject.SetActive(true);
			}
			SetHatchOpen(open: true);
			break;
		case EState.Processing:
			((MonoBehaviour)this).StartCoroutine(Process(startedByLocalPlayer: false));
			((Component)ProcessingScreen).gameObject.SetActive(true);
			ButtonAnim.Play();
			SetHatchOpen(open: false);
			break;
		}
	}

	private void RpcReader___Observers_SetState_3790170803(PooledReader PooledReader0, Channel channel)
	{
		EState state = GeneratedReaders___Internal.Read___ScheduleOne_002EObjectScripts_002ERecycler_002FEStateFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		bool force = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetState_3790170803(null, state, force);
		}
	}

	private void RpcWriter___Target_SetState_3790170803(NetworkConnection conn, EState state, bool force = false)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EObjectScripts_002ERecycler_002FEStateFishNet_002ESerializing_002EGenerated((Writer)(object)writer, state);
			((Writer)writer).WriteBoolean(force);
			((NetworkBehaviour)this).SendTargetRpc(6u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetState_3790170803(PooledReader PooledReader0, Channel channel)
	{
		EState state = GeneratedReaders___Internal.Read___ScheduleOne_002EObjectScripts_002ERecycler_002FEStateFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		bool force = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetState_3790170803(((NetworkBehaviour)this).LocalConnection, state, force);
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}
}
