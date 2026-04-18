using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.ItemFramework;
using ScheduleOne.Money;
using ScheduleOne.PlayerScripts;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public class DailySummary : NetworkSingleton<DailySummary>
{
	[Header("References")]
	public Canvas Canvas;

	public RectTransform Container;

	public UIScreen UIScreen;

	public Animation Anim;

	public TextMeshProUGUI TitleLabel;

	public RectTransform[] ProductEntries;

	public TextMeshProUGUI PlayerEarningsLabel;

	public TextMeshProUGUI DealerEarningsLabel;

	public TextMeshProUGUI XPGainedLabel;

	public UnityEvent onClosed;

	private Dictionary<string, int> itemsSoldByPlayer = new Dictionary<string, int>();

	private float moneyEarnedByPlayer;

	private float moneyEarnedByDealers;

	private bool NetworkInitialize___EarlyScheduleOne_002EUI_002EDailySummaryAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EUI_002EDailySummaryAssembly_002DCSharp_002Edll_Excuted;

	public bool IsOpen { get; private set; }

	public int xpGained { get; private set; }

	protected override void Start()
	{
		base.Start();
		IsOpen = false;
		((Behaviour)Canvas).enabled = false;
		((Component)Container).gameObject.SetActive(false);
		TimeManager timeManager = NetworkSingleton<TimeManager>.Instance;
		timeManager.onSleepEnd = (Action)Delegate.Combine(timeManager.onSleepEnd, new Action(SleepEnd));
	}

	public void Open()
	{
		IsOpen = true;
		((TMP_Text)TitleLabel).text = NetworkSingleton<TimeManager>.Instance.CurrentDay.ToString() + ", Day " + (NetworkSingleton<TimeManager>.Instance.ElapsedDays + 1);
		string[] items = itemsSoldByPlayer.Keys.ToArray();
		for (int i = 0; i < ProductEntries.Length; i++)
		{
			if (i < items.Length)
			{
				ItemDefinition item = Registry.GetItem(items[i]);
				((TMP_Text)((Component)((Transform)ProductEntries[i]).Find("Quantity")).GetComponent<TextMeshProUGUI>()).text = itemsSoldByPlayer[items[i]] + "x";
				((Component)((Transform)ProductEntries[i]).Find("Image")).GetComponent<Image>().sprite = ((BaseItemDefinition)item).Icon;
				((TMP_Text)((Component)((Transform)ProductEntries[i]).Find("Name")).GetComponent<TextMeshProUGUI>()).text = ((BaseItemDefinition)item).Name;
				((Component)ProductEntries[i]).gameObject.SetActive(true);
			}
			else
			{
				((Component)ProductEntries[i]).gameObject.SetActive(false);
			}
		}
		((TMP_Text)PlayerEarningsLabel).text = MoneyManager.FormatAmount(moneyEarnedByPlayer);
		((TMP_Text)DealerEarningsLabel).text = MoneyManager.FormatAmount(moneyEarnedByDealers);
		((TMP_Text)XPGainedLabel).text = xpGained + " XP";
		Anim.Play("Daily summary 1");
		((Behaviour)Canvas).enabled = true;
		((Component)Container).gameObject.SetActive(true);
		PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(((Object)this).name);
		Singleton<UIScreenManager>.Instance.AddScreen(UIScreen);
		((MonoBehaviour)this).StartCoroutine(Wait());
		IEnumerator Wait()
		{
			yield return (object)new WaitForSeconds(0.1f * (float)items.Length + 0.5f);
			if (IsOpen)
			{
				Anim.Play("Daily summary 2");
			}
		}
	}

	public void Close()
	{
		if (IsOpen)
		{
			IsOpen = false;
			Anim.Stop();
			Anim.Play("Daily summary close");
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
			Singleton<UIScreenManager>.Instance.RemoveScreen(UIScreen);
		}
	}

	private void SleepEnd()
	{
		ClearStats();
	}

	[ObserversRpc]
	public void AddSoldItem(string id, int amount)
	{
		RpcWriter___Observers_AddSoldItem_3643459082(id, amount);
	}

	[ObserversRpc]
	public void AddPlayerMoney(float amount)
	{
		RpcWriter___Observers_AddPlayerMoney_431000436(amount);
	}

	[ObserversRpc]
	public void AddDealerMoney(float amount)
	{
		RpcWriter___Observers_AddDealerMoney_431000436(amount);
	}

	[ObserversRpc]
	public void AddXP(int xp)
	{
		RpcWriter___Observers_AddXP_3316948804(xp);
	}

	private void ClearStats()
	{
		itemsSoldByPlayer.Clear();
		moneyEarnedByPlayer = 0f;
		moneyEarnedByDealers = 0f;
		xpGained = 0;
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
		if (!NetworkInitialize___EarlyScheduleOne_002EUI_002EDailySummaryAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EUI_002EDailySummaryAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterObserversRpc(0u, new ClientRpcDelegate(RpcReader___Observers_AddSoldItem_3643459082));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_AddPlayerMoney_431000436));
			((NetworkBehaviour)this).RegisterObserversRpc(2u, new ClientRpcDelegate(RpcReader___Observers_AddDealerMoney_431000436));
			((NetworkBehaviour)this).RegisterObserversRpc(3u, new ClientRpcDelegate(RpcReader___Observers_AddXP_3316948804));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EUI_002EDailySummaryAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EUI_002EDailySummaryAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_AddSoldItem_3643459082(string id, int amount)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(id);
			((Writer)writer).WriteInt32(amount, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(0u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___AddSoldItem_3643459082(string id, int amount)
	{
		if (itemsSoldByPlayer.ContainsKey(id))
		{
			itemsSoldByPlayer[id] += amount;
		}
		else
		{
			itemsSoldByPlayer.Add(id, amount);
		}
	}

	private void RpcReader___Observers_AddSoldItem_3643459082(PooledReader PooledReader0, Channel channel)
	{
		string id = ((Reader)PooledReader0).ReadString();
		int amount = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___AddSoldItem_3643459082(id, amount);
		}
	}

	private void RpcWriter___Observers_AddPlayerMoney_431000436(float amount)
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
			((NetworkBehaviour)this).SendObserversRpc(1u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___AddPlayerMoney_431000436(float amount)
	{
		moneyEarnedByPlayer += amount;
	}

	private void RpcReader___Observers_AddPlayerMoney_431000436(PooledReader PooledReader0, Channel channel)
	{
		float amount = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___AddPlayerMoney_431000436(amount);
		}
	}

	private void RpcWriter___Observers_AddDealerMoney_431000436(float amount)
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
			((NetworkBehaviour)this).SendObserversRpc(2u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___AddDealerMoney_431000436(float amount)
	{
		moneyEarnedByDealers += amount;
	}

	private void RpcReader___Observers_AddDealerMoney_431000436(PooledReader PooledReader0, Channel channel)
	{
		float amount = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___AddDealerMoney_431000436(amount);
		}
	}

	private void RpcWriter___Observers_AddXP_3316948804(int xp)
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
			((Writer)writer).WriteInt32(xp, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(3u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___AddXP_3316948804(int xp)
	{
		xpGained += xp;
	}

	private void RpcReader___Observers_AddXP_3316948804(PooledReader PooledReader0, Channel channel)
	{
		int xp = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___AddXP_3316948804(xp);
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
