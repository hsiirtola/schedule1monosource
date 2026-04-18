using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Object.Synchronizing;
using FishNet.Object.Synchronizing.Internal;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using ScheduleOne.Variables;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.Money;

public class MoneyManager : NetworkSingleton<MoneyManager>, IBaseSaveable, ISaveable
{
	public class FloatContainer
	{
		public float value { get; private set; }

		public void ChangeValue(float value)
		{
			this.value += value;
		}
	}

	public const string MONEY_TEXT_COLOR = "#54E717";

	public const string MONEY_TEXT_COLOR_DARKER = "#46CB4F";

	public const string ONLINE_BALANCE_COLOR = "#4CBFFF";

	public List<Transaction> ledger = new List<Transaction>();

	[SyncVar(/*Could not decode attribute arguments.*/)]
	public float onlineBalance;

	[SyncVar(/*Could not decode attribute arguments.*/)]
	public float lifetimeEarnings;

	[SerializeField]
	protected AudioSourceController CashSound;

	[Header("Prefabs")]
	[SerializeField]
	protected GameObject moneyChangePrefab;

	[SerializeField]
	protected GameObject cashChangePrefab;

	public Sprite LaunderingNotificationIcon;

	public Action<FloatContainer> onNetworthCalculation;

	private MoneyLoader loader = new MoneyLoader();

	public SyncVar<float> syncVar___onlineBalance;

	public SyncVar<float> syncVar___lifetimeEarnings;

	private bool NetworkInitialize___EarlyScheduleOne_002EMoney_002EMoneyManagerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EMoney_002EMoneyManagerAssembly_002DCSharp_002Edll_Excuted;

	public float LifetimeEarnings => SyncAccessor_lifetimeEarnings;

	public float LastCalculatedNetworth { get; protected set; }

	public float cashBalance => cashInstance.Balance;

	protected CashInstance cashInstance => PlayerSingleton<PlayerInventory>.Instance.cashInstance;

	public string SaveFolderName => "Money";

	public string SaveFileName => "Money";

	public Loader Loader => loader;

	public bool ShouldSaveUnderFolder => false;

	public List<string> LocalExtraFiles { get; set; } = new List<string>();

	public List<string> LocalExtraFolders { get; set; } = new List<string>();

	public bool HasChanged { get; set; }

	public int LoadOrder { get; }

	public float SyncAccessor_onlineBalance
	{
		get
		{
			return onlineBalance;
		}
		set
		{
			if (value || !((NetworkBehaviour)this).IsServerInitialized)
			{
				onlineBalance = value;
			}
			if (Application.isPlaying)
			{
				syncVar___onlineBalance.SetValue(value, value);
			}
		}
	}

	public float SyncAccessor_lifetimeEarnings
	{
		get
		{
			return lifetimeEarnings;
		}
		set
		{
			if (value || !((NetworkBehaviour)this).IsServerInitialized)
			{
				lifetimeEarnings = value;
			}
			if (Application.isPlaying)
			{
				syncVar___lifetimeEarnings.SetValue(value, value);
			}
		}
	}

	public static string ApplyMoneyTextColor(string text)
	{
		return "<color=#54E717>" + text + "</color>";
	}

	public static string ApplyMoneyTextColorDarker(string text)
	{
		return "<color=#46CB4F>" + text + "</color>";
	}

	public static string ApplyOnlineBalanceColor(string text)
	{
		return "<color=#4CBFFF>" + text + "</color>";
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EMoney_002EMoneyManager_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public virtual void InitializeSaveable()
	{
		Singleton<SaveManager>.Instance.RegisterSaveable(this);
	}

	protected override void Start()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		base.Start();
		Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(Loaded));
		NetworkSingleton<TimeManager>.Instance.onMinutePass += new Action(MinPass);
		TimeManager timeManager = NetworkSingleton<TimeManager>.Instance;
		timeManager.onDayPass = (Action)Delegate.Combine(timeManager.onDayPass, new Action(CheckNetworthAchievements));
		Singleton<HUD>.Instance.OnlineBalanceDisplay.SetBalance(SyncAccessor_onlineBalance);
	}

	public override void OnStartServer()
	{
		((NetworkBehaviour)this).OnStartServer();
		if (NetworkSingleton<VariableDatabase>.InstanceExists)
		{
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("LifetimeEarnings", lifetimeEarnings.ToString());
		}
	}

	public override void OnStartClient()
	{
		((NetworkBehaviour)this).OnStartClient();
		Singleton<HUD>.Instance.OnlineBalanceDisplay.SetBalance(SyncAccessor_onlineBalance);
	}

	protected override void OnDestroy()
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected O, but got Unknown
		base.OnDestroy();
		if (NetworkSingleton<TimeManager>.InstanceExists)
		{
			NetworkSingleton<TimeManager>.Instance.onMinutePass -= new Action(MinPass);
			TimeManager timeManager = NetworkSingleton<TimeManager>.Instance;
			timeManager.onDayPass = (Action)Delegate.Remove(timeManager.onDayPass, new Action(CheckNetworthAchievements));
		}
		if (Singleton<LoadManager>.InstanceExists)
		{
			Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(new UnityAction(Loaded));
		}
	}

	private void Loaded()
	{
		GetNetWorth();
		Singleton<HUD>.Instance.OnlineBalanceDisplay.SetBalance(SyncAccessor_onlineBalance);
	}

	private void Update()
	{
		HasChanged = true;
	}

	private void MinPass()
	{
		if (NetworkSingleton<VariableDatabase>.InstanceExists)
		{
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Online_Balance", onlineBalance.ToString(), network: false);
			if (PlayerSingleton<PlayerInventory>.InstanceExists)
			{
				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Cash_Balance", cashBalance.ToString(), network: false);
				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Total_Money", (SyncAccessor_onlineBalance + cashBalance).ToString(), network: false);
			}
		}
	}

	public CashInstance GetCashInstance(float amount)
	{
		CashInstance obj = Registry.GetItem<CashDefinition>("cash").GetDefaultInstance() as CashInstance;
		obj.SetBalance(amount);
		return obj;
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void CreateOnlineTransaction(string _transaction_Name, float _unit_Amount, float _quantity, string _transaction_Note)
	{
		RpcWriter___Server_CreateOnlineTransaction_1419830531(_transaction_Name, _unit_Amount, _quantity, _transaction_Note);
		RpcLogic___CreateOnlineTransaction_1419830531(_transaction_Name, _unit_Amount, _quantity, _transaction_Note);
	}

	[ObserversRpc]
	private void ReceiveOnlineTransaction(string _transaction_Name, float _unit_Amount, float _quantity, string _transaction_Note)
	{
		RpcWriter___Observers_ReceiveOnlineTransaction_1419830531(_transaction_Name, _unit_Amount, _quantity, _transaction_Note);
	}

	protected IEnumerator ShowOnlineBalanceChange(RectTransform changeDisplay)
	{
		TextMeshProUGUI text = ((Component)changeDisplay).GetComponent<TextMeshProUGUI>();
		float startVert = changeDisplay.anchoredPosition.y;
		float lerpTime = 2.5f;
		float vertOffset = startVert + 60f;
		for (float i = 0f; i < lerpTime; i += Time.unscaledDeltaTime)
		{
			((Graphic)text).color = new Color(((Graphic)text).color.r, ((Graphic)text).color.g, ((Graphic)text).color.b, Mathf.Lerp(1f, 0f, i / lerpTime));
			changeDisplay.anchoredPosition = new Vector2(changeDisplay.anchoredPosition.x, Mathf.Lerp(startVert, vertOffset, i / lerpTime));
			yield return (object)new WaitForEndOfFrame();
		}
		Object.Destroy((Object)(object)((Component)changeDisplay).gameObject);
	}

	[ServerRpc(RequireOwnership = false)]
	public void ChangeLifetimeEarnings(float change)
	{
		RpcWriter___Server_ChangeLifetimeEarnings_431000436(change);
	}

	public void PlayCashSound()
	{
		if (!Singleton<LoadManager>.Instance.IsLoading)
		{
			CashSound.Play();
		}
	}

	public void ChangeCashBalance(float change, bool visualizeChange = true, bool playCashSound = false)
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Clamp(cashInstance.Balance + change, 0f, float.MaxValue) - cashInstance.Balance;
		cashInstance.ChangeBalance(change);
		if (playCashSound && num != 0f)
		{
			CashSound.Play();
		}
		if (visualizeChange && num != 0f)
		{
			RectTransform component = Object.Instantiate<GameObject>(cashChangePrefab, (Transform)(object)Singleton<HUD>.Instance.cashSlotContainer).GetComponent<RectTransform>();
			((Transform)component).position = new Vector3(((Transform)Singleton<HUD>.Instance.cashSlotUI).position.x, ((Transform)component).position.y);
			component.anchoredPosition = new Vector2(component.anchoredPosition.x, 10f);
			TextMeshProUGUI component2 = ((Component)component).GetComponent<TextMeshProUGUI>();
			if (num > 0f)
			{
				((TMP_Text)component2).text = "+ " + FormatAmount(num);
				((Graphic)component2).color = Color32.op_Implicit(new Color32((byte)25, (byte)240, (byte)30, byte.MaxValue));
			}
			else
			{
				((TMP_Text)component2).text = FormatAmount(num);
				((Graphic)component2).color = Color32.op_Implicit(new Color32((byte)176, (byte)63, (byte)59, byte.MaxValue));
			}
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(ShowCashChange(component));
		}
	}

	protected IEnumerator ShowCashChange(RectTransform changeDisplay)
	{
		TextMeshProUGUI text = ((Component)changeDisplay).GetComponent<TextMeshProUGUI>();
		float startVert = changeDisplay.anchoredPosition.y;
		float lerpTime = 2.5f;
		float vertOffset = startVert + 60f;
		for (float i = 0f; i < lerpTime; i += Time.unscaledDeltaTime)
		{
			((Graphic)text).color = new Color(((Graphic)text).color.r, ((Graphic)text).color.g, ((Graphic)text).color.b, Mathf.Lerp(1f, 0f, i / lerpTime));
			changeDisplay.anchoredPosition = new Vector2(changeDisplay.anchoredPosition.x, Mathf.Lerp(startVert, vertOffset, i / lerpTime));
			yield return (object)new WaitForEndOfFrame();
		}
		Object.Destroy((Object)(object)((Component)changeDisplay).gameObject);
	}

	public static string FormatAmount(float amount, bool showDecimals = false, bool includeColor = false)
	{
		string text = string.Empty;
		if (includeColor)
		{
			text += "<color=#54E717>";
		}
		if (amount < 0f)
		{
			text = "-";
		}
		text = ((!showDecimals) ? (text + string.Format(new CultureInfo("en-US"), "{0:C0}", Mathf.RoundToInt(Mathf.Abs(amount)))) : (text + string.Format(new CultureInfo("en-US"), "{0:C}", Mathf.Abs(amount))));
		if (includeColor)
		{
			text += "</color>";
		}
		return text;
	}

	public virtual string GetSaveString()
	{
		return new MoneyData(SyncAccessor_onlineBalance, GetNetWorth(), SyncAccessor_lifetimeEarnings, ATM.WeeklyDepositSum).GetJson();
	}

	public void Load(MoneyData data)
	{
		this.sync___set_value_onlineBalance(Mathf.Clamp(data.OnlineBalance, 0f, float.MaxValue), true);
		this.sync___set_value_lifetimeEarnings(Mathf.Clamp(data.LifetimeEarnings, 0f, float.MaxValue), true);
		Singleton<HUD>.Instance.OnlineBalanceDisplay.SetBalance(SyncAccessor_onlineBalance);
		ATM.WeeklyDepositSum = data.WeeklyDepositSum;
	}

	public void CheckNetworthAchievements()
	{
		float netWorth = GetNetWorth();
		if (netWorth >= 100000f)
		{
			AchievementManager.UnlockAchievement(AchievementManager.EAchievement.BUSINESSMAN);
		}
		if (netWorth >= 1000000f)
		{
			AchievementManager.UnlockAchievement(AchievementManager.EAchievement.BIGWIG);
		}
		if (netWorth >= 10000000f)
		{
			AchievementManager.UnlockAchievement(AchievementManager.EAchievement.MAGNATE);
		}
	}

	public float GetNetWorth()
	{
		float num = 0f;
		num += SyncAccessor_onlineBalance;
		if (onNetworthCalculation != null)
		{
			FloatContainer floatContainer = new FloatContainer();
			onNetworthCalculation(floatContainer);
			num += floatContainer.value;
		}
		LastCalculatedNetworth = num;
		return num;
	}

	public override void NetworkInitialize___Early()
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected O, but got Unknown
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected O, but got Unknown
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EMoney_002EMoneyManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EMoney_002EMoneyManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			syncVar___lifetimeEarnings = new SyncVar<float>((NetworkBehaviour)(object)this, 1u, (WritePermission)1, (ReadPermission)0, -1f, (Channel)0, lifetimeEarnings);
			syncVar___onlineBalance = new SyncVar<float>((NetworkBehaviour)(object)this, 0u, (WritePermission)1, (ReadPermission)0, -1f, (Channel)0, onlineBalance);
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_CreateOnlineTransaction_1419830531));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_ReceiveOnlineTransaction_1419830531));
			((NetworkBehaviour)this).RegisterServerRpc(2u, new ServerRpcDelegate(RpcReader___Server_ChangeLifetimeEarnings_431000436));
			((NetworkBehaviour)this).RegisterSyncVarRead(new SyncVarReadDelegate(ReadSyncVar___ScheduleOne_002EMoney_002EMoneyManager));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EMoney_002EMoneyManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EMoney_002EMoneyManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
			((SyncBase)syncVar___lifetimeEarnings).SetRegistered();
			((SyncBase)syncVar___onlineBalance).SetRegistered();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_CreateOnlineTransaction_1419830531(string _transaction_Name, float _unit_Amount, float _quantity, string _transaction_Note)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(_transaction_Name);
			((Writer)writer).WriteSingle(_unit_Amount, (AutoPackType)0);
			((Writer)writer).WriteSingle(_quantity, (AutoPackType)0);
			((Writer)writer).WriteString(_transaction_Note);
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___CreateOnlineTransaction_1419830531(string _transaction_Name, float _unit_Amount, float _quantity, string _transaction_Note)
	{
		ReceiveOnlineTransaction(_transaction_Name, _unit_Amount, _quantity, _transaction_Note);
	}

	private void RpcReader___Server_CreateOnlineTransaction_1419830531(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string transaction_Name = ((Reader)PooledReader0).ReadString();
		float unit_Amount = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		float quantity = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		string transaction_Note = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___CreateOnlineTransaction_1419830531(transaction_Name, unit_Amount, quantity, transaction_Note);
		}
	}

	private void RpcWriter___Observers_ReceiveOnlineTransaction_1419830531(string _transaction_Name, float _unit_Amount, float _quantity, string _transaction_Note)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(_transaction_Name);
			((Writer)writer).WriteSingle(_unit_Amount, (AutoPackType)0);
			((Writer)writer).WriteSingle(_quantity, (AutoPackType)0);
			((Writer)writer).WriteString(_transaction_Note);
			((NetworkBehaviour)this).SendObserversRpc(1u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___ReceiveOnlineTransaction_1419830531(string _transaction_Name, float _unit_Amount, float _quantity, string _transaction_Note)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		Transaction transaction = new Transaction(_transaction_Name, _unit_Amount, _quantity, _transaction_Note);
		ledger.Add(transaction);
		this.sync___set_value_onlineBalance(SyncAccessor_onlineBalance + transaction.total_Amount, true);
		Singleton<HUD>.Instance.OnlineBalanceDisplay.SetBalance(SyncAccessor_onlineBalance);
		RectTransform component = Object.Instantiate<GameObject>(moneyChangePrefab, (Transform)(object)Singleton<HUD>.Instance.cashSlotContainer).GetComponent<RectTransform>();
		((Transform)component).position = new Vector3(((Transform)Singleton<HUD>.Instance.onlineBalanceSlotUI).position.x, ((Transform)component).position.y);
		component.anchoredPosition = new Vector2(component.anchoredPosition.x, 10f);
		TextMeshProUGUI component2 = ((Component)component).GetComponent<TextMeshProUGUI>();
		if (transaction.total_Amount > 0f)
		{
			((TMP_Text)component2).text = "+ " + FormatAmount(transaction.total_Amount);
			((Graphic)component2).color = Color32.op_Implicit(new Color32((byte)25, (byte)190, (byte)240, byte.MaxValue));
		}
		else
		{
			((TMP_Text)component2).text = FormatAmount(transaction.total_Amount);
			((Graphic)component2).color = Color32.op_Implicit(new Color32((byte)176, (byte)63, (byte)59, byte.MaxValue));
		}
		((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(ShowOnlineBalanceChange(component));
		HasChanged = true;
	}

	private void RpcReader___Observers_ReceiveOnlineTransaction_1419830531(PooledReader PooledReader0, Channel channel)
	{
		string transaction_Name = ((Reader)PooledReader0).ReadString();
		float unit_Amount = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		float quantity = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		string transaction_Note = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ReceiveOnlineTransaction_1419830531(transaction_Name, unit_Amount, quantity, transaction_Note);
		}
	}

	private void RpcWriter___Server_ChangeLifetimeEarnings_431000436(float change)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteSingle(change, (AutoPackType)0);
			((NetworkBehaviour)this).SendServerRpc(2u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___ChangeLifetimeEarnings_431000436(float change)
	{
		this.sync___set_value_lifetimeEarnings(Mathf.Clamp(SyncAccessor_lifetimeEarnings + change, 0f, float.MaxValue), true);
		NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("LifetimeEarnings", lifetimeEarnings.ToString());
	}

	private void RpcReader___Server_ChangeLifetimeEarnings_431000436(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		float change = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___ChangeLifetimeEarnings_431000436(change);
		}
	}

	public override bool ReadSyncVar___ScheduleOne_002EMoney_002EMoneyManager(PooledReader PooledReader0, uint UInt321, bool Boolean2)
	{
		switch (UInt321)
		{
		case 1u:
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value_lifetimeEarnings(syncVar___lifetimeEarnings.GetValue(true), true);
				return true;
			}
			float value2 = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
			this.sync___set_value_lifetimeEarnings(value2, Boolean2);
			return true;
		}
		case 0u:
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value_onlineBalance(syncVar___onlineBalance.GetValue(true), true);
				return true;
			}
			float value = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
			this.sync___set_value_onlineBalance(value, Boolean2);
			return true;
		}
		default:
			return false;
		}
	}

	protected override void Awake_UserLogic_ScheduleOne_002EMoney_002EMoneyManager_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		InitializeSaveable();
	}
}
