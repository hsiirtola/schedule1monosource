using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.Networking;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Variables;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.GameTime;

public class TimeManager : NetworkSingleton<TimeManager>, IBaseSaveable, ISaveable
{
	private const float DefaultCycleDuration = 24f;

	public const float TickDuration = 0.5f;

	public const int EndOfDay = 400;

	public const int WakeTime = 700;

	private static float CycleDuration = 24f;

	[SerializeField]
	private EDay _defaultDay;

	private float _lastMinWaitExcess;

	private bool _stopMinPassWait;

	private float _secondsOnCurrentMinute;

	public ActionList onMinutePass = new ActionList();

	public ActionList onUncappedMinutePass = new ActionList();

	public ActionList onTick = new ActionList(shuffleCallbackList: true);

	public Action onTimeChanged;

	public Action<int> onTimeSkip;

	public Action onTimeSet;

	public Action onHourPass;

	public Action onDayPass;

	public Action onWeekPass;

	public Action onUpdate;

	public Action onFixedUpdate;

	public Action onSleepStart;

	public Action onSleepEnd;

	private TimeLoader loader = new TimeLoader();

	private bool NetworkInitialize___EarlyScheduleOne_002EGameTime_002ETimeManagerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EGameTime_002ETimeManagerAssembly_002DCSharp_002Edll_Excuted;

	public static float MinuteDuration => CycleDuration / 24f;

	[field: SerializeField]
	public int DefaultTime { get; private set; } = 900;

	public int CurrentTime { get; private set; }

	public EDay CurrentDay => (EDay)DayIndex;

	public int ElapsedDays { get; private set; }

	public bool IsEndOfDay => CurrentTime == 400;

	public bool IsNight
	{
		get
		{
			if (CurrentTime >= 600)
			{
				return CurrentTime >= 1800;
			}
			return true;
		}
	}

	public float NormalizedTimeOfDay => (Mathf.Clamp01(_secondsOnCurrentMinute / MinuteDuration) + (float)DailyMinSum) / 1440f;

	public int DayIndex => ElapsedDays % 7;

	public bool IsSleepInProgress { get; private set; }

	public float Playtime { get; private set; }

	public bool HostSleepDone { get; private set; }

	public float TimeSpeedMultiplier { get; private set; } = 1f;

	public int DailyMinSum { get; private set; }

	private float _minuteStaggerTime => MinuteDuration / (Time.timeScale * 0.9f);

	private float _tickStaggerTime => 0.45f;

	public string SaveFolderName => "Time";

	public string SaveFileName => "Time";

	public Loader Loader => loader;

	public bool ShouldSaveUnderFolder => false;

	public List<string> LocalExtraFiles { get; set; } = new List<string>();

	public List<string> LocalExtraFolders { get; set; } = new List<string>();

	public bool HasChanged { get; set; }

	public int LoadOrder { get; }

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EGameTime_002ETimeManager_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	protected override void Start()
	{
		base.Start();
		if (!Singleton<Lobby>.InstanceExists || !Singleton<Lobby>.Instance.IsInLobby || Singleton<Lobby>.Instance.IsHost || GameManager.IS_TUTORIAL)
		{
			SetTime(DefaultTime);
			ElapsedDays = (int)_defaultDay;
		}
	}

	public virtual void InitializeSaveable()
	{
		Singleton<SaveManager>.Instance.RegisterSaveable(this);
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		((NetworkBehaviour)this).OnSpawnServer(connection);
		if (!connection.IsHost)
		{
			SetTimeData_Client(connection, ElapsedDays, CurrentTime, ((NetworkBehaviour)this).TimeManager.Tick);
		}
	}

	public override void OnStartServer()
	{
		((NetworkBehaviour)this).OnStartServer();
		((MonoBehaviour)this).StartCoroutine(TimeLoop());
	}

	public override void OnStartClient()
	{
		((NetworkBehaviour)this).OnStartClient();
		((MonoBehaviour)this).StartCoroutine(TickLoop());
	}

	private void Clean()
	{
		onSleepStart = null;
		onSleepEnd = null;
		onMinutePass.Clear();
		onMinutePass = null;
		onHourPass = null;
		onDayPass = null;
		onTick.Clear();
		onTick = null;
		onTimeSet = null;
		CycleDuration = 24f;
	}

	[ObserversRpc(RunLocally = true, ExcludeServer = true)]
	[TargetRpc]
	private void SetTimeData_Client(NetworkConnection conn, int elapsedDays, int time, uint serverTick)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetTimeData_Client_1794730778(conn, elapsedDays, time, serverTick);
			RpcLogic___SetTimeData_Client_1794730778(conn, elapsedDays, time, serverTick);
		}
		else
		{
			RpcWriter___Target_SetTimeData_Client_1794730778(conn, elapsedDays, time, serverTick);
		}
	}

	protected virtual void Update()
	{
		if (CurrentTime != 400)
		{
			_secondsOnCurrentMinute += Time.unscaledDeltaTime * TimeSpeedMultiplier;
		}
		Playtime += Time.unscaledDeltaTime;
		CheckSleepStart();
		if (onUpdate != null)
		{
			onUpdate();
		}
	}

	protected virtual void FixedUpdate()
	{
		if (onFixedUpdate != null)
		{
			onFixedUpdate();
		}
	}

	private IEnumerator TickLoop()
	{
		float lastWaitExcess = 0f;
		while ((Object)(object)((Component)this).gameObject != (Object)null)
		{
			if (Time.timeScale == 0f)
			{
				yield return (object)new WaitUntil((Func<bool>)(() => Time.timeScale > 0f));
			}
			float timeToWait = 0.5f - lastWaitExcess;
			if (timeToWait > 0f)
			{
				float timeOnWaitStart = Time.realtimeSinceStartup;
				yield return (object)new WaitForSecondsRealtime(timeToWait);
				float num = Time.realtimeSinceStartup - timeOnWaitStart;
				lastWaitExcess = Mathf.Max(num - timeToWait, 0f);
			}
			else
			{
				lastWaitExcess -= 0.5f;
			}
			onTick.InvokeAllStaggered(_tickStaggerTime);
			yield return (object)new WaitForEndOfFrame();
		}
	}

	private IEnumerator TimeLoop()
	{
		while ((Object)(object)((Component)this).gameObject != (Object)null)
		{
			if (Singleton<LoadManager>.Instance.IsLoading)
			{
				Console.Log("TimeLoop waiting for host load to finish...");
				yield return (object)new WaitUntil((Func<bool>)(() => !Singleton<LoadManager>.Instance.IsLoading));
			}
			if (TimeSpeedMultiplier <= 0f)
			{
				yield return (object)new WaitUntil((Func<bool>)(() => TimeSpeedMultiplier > 0f));
			}
			if (Time.timeScale == 0f)
			{
				yield return (object)new WaitUntil((Func<bool>)(() => Time.timeScale > 0f));
			}
			float timeToWait = MinuteDuration / (TimeSpeedMultiplier * Time.timeScale) - _lastMinWaitExcess;
			if (timeToWait > 0f)
			{
				_stopMinPassWait = false;
				float timeOnWaitStart = Time.realtimeSinceStartup;
				for (float i = 0f; i < timeToWait; i += Time.unscaledDeltaTime)
				{
					if (_stopMinPassWait)
					{
						break;
					}
					yield return (object)new WaitForEndOfFrame();
				}
				float num = Time.realtimeSinceStartup - timeOnWaitStart;
				_lastMinWaitExcess = Mathf.Max(num - timeToWait, 0f);
			}
			else
			{
				_lastMinWaitExcess -= MinuteDuration / TimeSpeedMultiplier;
			}
			PassMinute();
			yield return (object)new WaitForEndOfFrame();
		}
	}

	private bool ShouldMinutePass()
	{
		if (CurrentTime == 400 || (IsCurrentTimeWithinRange(400, 600) && !GameManager.IS_TUTORIAL))
		{
			return false;
		}
		return true;
	}

	private void PassMinute()
	{
		PassMinute_Client(CurrentTime);
	}

	[ObserversRpc(RunLocally = true, ExcludeServer = true)]
	private void PassMinute_Client(int oldTime)
	{
		RpcWriter___Observers_PassMinute_Client_3316948804(oldTime);
		RpcLogic___PassMinute_Client_3316948804(oldTime);
	}

	public void SetTimeAndSync(int time)
	{
		if (!InstanceFinder.IsHost)
		{
			Console.LogWarning("SetTime can only be called by host");
			return;
		}
		SetTime(time);
		SetTimeData_Client(null, ElapsedDays, CurrentTime, ((NetworkBehaviour)this).TimeManager.Tick);
	}

	private void SetTime(int time)
	{
		Console.Log("Setting time to: " + time);
		CurrentTime = time;
		_secondsOnCurrentMinute = 0f;
		DailyMinSum = GetMinSumFrom24HourTime(CurrentTime);
		if (onTimeChanged != null)
		{
			onTimeChanged();
		}
		try
		{
			if (onTimeSet != null)
			{
				onTimeSet();
			}
		}
		catch (Exception ex)
		{
			Console.LogError("Error invoking onTimeChanged: " + ex.Message + "\nSite:" + ex.StackTrace);
		}
	}

	public bool IsCurrentTimeWithinRange(int min, int max)
	{
		return IsGivenTimeWithinRange(CurrentTime, min, max);
	}

	public bool IsCurrentDateWithinRange(GameDateTime start, GameDateTime end)
	{
		int totalMinSum = GetTotalMinSum();
		if (totalMinSum >= start.GetMinSum())
		{
			return totalMinSum <= end.GetMinSum();
		}
		return false;
	}

	public GameDateTime GetDateTime()
	{
		return new GameDateTime(ElapsedDays, CurrentTime);
	}

	public int GetTotalMinSum()
	{
		return ElapsedDays * 1440 + DailyMinSum;
	}

	public void SetTimeSpeedMultiplier(float multiplier)
	{
		if (!InstanceFinder.IsServer)
		{
			Console.LogWarning("SetTimeProgressionMultiplier can only be called by the server.");
		}
		else
		{
			TimeSpeedMultiplier = Mathf.Max(multiplier, 0f);
		}
	}

	public void SetCycleDuration(float time)
	{
		if (!InstanceFinder.IsServer)
		{
			Console.LogWarning("SetCycleDuration can only be called by the server.");
			return;
		}
		CycleDuration = Mathf.Clamp(time, 0.1f, 1440f);
		_secondsOnCurrentMinute = 0f;
		_lastMinWaitExcess = 0f;
		_stopMinPassWait = true;
		Console.Log("Setting 24-hour cycle duration to: " + CycleDuration + " minutes.");
	}

	private void CheckSleepStart()
	{
		if (InstanceFinder.IsServer && !IsSleepInProgress && Player.AreAllPlayersReadyToSleep())
		{
			StartSleep();
		}
	}

	[ObserversRpc(RunLocally = true)]
	public void StartSleep()
	{
		RpcWriter___Observers_StartSleep_2166136261();
		RpcLogic___StartSleep_2166136261();
	}

	[ObserversRpc(RunLocally = true)]
	public void SetHostSleepDone(bool done)
	{
		RpcWriter___Observers_SetHostSleepDone_1140765316(done);
		RpcLogic___SetHostSleepDone_1140765316(done);
	}

	private void SkipForwardToTime(int newTime)
	{
		if (!InstanceFinder.IsServer)
		{
			Console.LogWarning("SkipForwardToTime can only be called by the server.");
			return;
		}
		int currentTime = CurrentTime;
		OnTimeSkip_Client(currentTime, 700);
		SetTimeAndSync(newTime);
	}

	[ObserversRpc(RunLocally = true)]
	private void OnTimeSkip_Client(int oldTime, int newTime)
	{
		RpcWriter___Observers_OnTimeSkip_Client_1692629761(oldTime, newTime);
		RpcLogic___OnTimeSkip_Client_1692629761(oldTime, newTime);
	}

	public static bool IsGivenTimeWithinRange(int givenTime, int min, int max)
	{
		if (max > min)
		{
			if (givenTime >= min)
			{
				return givenTime <= max;
			}
			return false;
		}
		if (givenTime < min)
		{
			return givenTime <= max;
		}
		return true;
	}

	public static bool IsValid24HourTime(string input)
	{
		string pattern = "^([01]?[0-9]|2[0-3])[0-5][0-9]$";
		return Regex.IsMatch(input, pattern);
	}

	public static string Get12HourTime(float _time, bool appendDesignator = true)
	{
		string text = _time.ToString();
		while (text.Length < 4)
		{
			text = "0" + text;
		}
		int num = Convert.ToInt32(text.Substring(0, 2));
		int num2 = Convert.ToInt32(text.Substring(2, 2));
		string text2 = "AM";
		if (num == 0)
		{
			num = 12;
		}
		else if (num == 12)
		{
			text2 = "PM";
		}
		else if (num == 24)
		{
			num = 12;
			text2 = "AM";
		}
		else if (num > 12)
		{
			num -= 12;
			text2 = "PM";
		}
		string text3 = $"{num}:{num2:00}";
		if (appendDesignator)
		{
			text3 = text3 + " " + text2;
		}
		return text3;
	}

	public static int Get24HourTimeFromMinSum(int minSum)
	{
		if (minSum < 0)
		{
			minSum = 1440 - minSum;
		}
		minSum %= 1440;
		int num = (int)((float)minSum / 60f);
		int num2 = minSum - 60 * num;
		return num * 100 + num2;
	}

	public static int GetMinSumFrom24HourTime(int _time)
	{
		int num = (int)((float)_time / 100f);
		int num2 = _time - num * 100;
		return num * 60 + num2;
	}

	public static string GetMinutesToDisplayTime(int minutes)
	{
		int num = minutes / 60;
		int num2 = minutes % 60;
		return num + "h " + num2 + "m";
	}

	public static int AddMinutesTo24HourTime(int time, int minsToAdd)
	{
		int num = GetMinSumFrom24HourTime(time) + minsToAdd;
		if (num < 0)
		{
			num = 1440 + num;
		}
		return Get24HourTimeFromMinSum(num);
	}

	public virtual string GetSaveString()
	{
		return new TimeData(CurrentTime, ElapsedDays, Mathf.RoundToInt(Playtime)).GetJson();
	}

	public void Load(TimeData timeData)
	{
		if (timeData != null)
		{
			SetTime(timeData.TimeOfDay);
			ElapsedDays = timeData.ElapsedDays;
			Playtime = timeData.Playtime;
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
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EGameTime_002ETimeManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EGameTime_002ETimeManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterObserversRpc(0u, new ClientRpcDelegate(RpcReader___Observers_SetTimeData_Client_1794730778));
			((NetworkBehaviour)this).RegisterTargetRpc(1u, new ClientRpcDelegate(RpcReader___Target_SetTimeData_Client_1794730778));
			((NetworkBehaviour)this).RegisterObserversRpc(2u, new ClientRpcDelegate(RpcReader___Observers_PassMinute_Client_3316948804));
			((NetworkBehaviour)this).RegisterObserversRpc(3u, new ClientRpcDelegate(RpcReader___Observers_StartSleep_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(4u, new ClientRpcDelegate(RpcReader___Observers_SetHostSleepDone_1140765316));
			((NetworkBehaviour)this).RegisterObserversRpc(5u, new ClientRpcDelegate(RpcReader___Observers_OnTimeSkip_Client_1692629761));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EGameTime_002ETimeManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EGameTime_002ETimeManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_SetTimeData_Client_1794730778(NetworkConnection conn, int elapsedDays, int time, uint serverTick)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(elapsedDays, (AutoPackType)1);
			((Writer)writer).WriteInt32(time, (AutoPackType)1);
			((Writer)writer).WriteUInt32(serverTick, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(0u, writer, val, (DataOrderType)0, false, true, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetTimeData_Client_1794730778(NetworkConnection conn, int elapsedDays, int time, uint serverTick)
	{
		uint tick = ((NetworkBehaviour)this).TimeManager.Tick;
		double num = ((tick > serverTick) ? ((NetworkBehaviour)this).TimeManager.TicksToTime(tick - serverTick) : 0.0);
		Console.Log("Client received time data. Server Tick: " + serverTick + " Current Tick: " + tick + " Delay: " + num + " seconds.");
		ElapsedDays = elapsedDays;
		SetTime(time);
		Console.Log("Setting time data: Days: " + ElapsedDays + " Current Time: " + CurrentTime);
	}

	private void RpcReader___Observers_SetTimeData_Client_1794730778(PooledReader PooledReader0, Channel channel)
	{
		int elapsedDays = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		int time = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		uint serverTick = ((Reader)PooledReader0).ReadUInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetTimeData_Client_1794730778(null, elapsedDays, time, serverTick);
		}
	}

	private void RpcWriter___Target_SetTimeData_Client_1794730778(NetworkConnection conn, int elapsedDays, int time, uint serverTick)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(elapsedDays, (AutoPackType)1);
			((Writer)writer).WriteInt32(time, (AutoPackType)1);
			((Writer)writer).WriteUInt32(serverTick, (AutoPackType)1);
			((NetworkBehaviour)this).SendTargetRpc(1u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetTimeData_Client_1794730778(PooledReader PooledReader0, Channel channel)
	{
		int elapsedDays = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		int time = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		uint serverTick = ((Reader)PooledReader0).ReadUInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetTimeData_Client_1794730778(((NetworkBehaviour)this).LocalConnection, elapsedDays, time, serverTick);
		}
	}

	private void RpcWriter___Observers_PassMinute_Client_3316948804(int oldTime)
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
			((Writer)writer).WriteInt32(oldTime, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(2u, writer, val, (DataOrderType)0, false, true, false);
			writer.Store();
		}
	}

	private void RpcLogic___PassMinute_Client_3316948804(int oldTime)
	{
		if (!ShouldMinutePass())
		{
			onUncappedMinutePass.InvokeAllStaggered(_minuteStaggerTime);
			return;
		}
		CurrentTime = oldTime;
		if (CurrentTime == 2359)
		{
			ElapsedDays++;
			CurrentTime = 0;
			DailyMinSum = 0;
			if (onHourPass != null)
			{
				onHourPass();
			}
			if (onDayPass != null)
			{
				onDayPass();
			}
			if (CurrentDay == EDay.Monday && onWeekPass != null)
			{
				onWeekPass();
			}
		}
		else if (CurrentTime % 100 >= 59)
		{
			CurrentTime += 41;
			if (onHourPass != null)
			{
				onHourPass();
			}
		}
		else
		{
			CurrentTime++;
		}
		DailyMinSum = GetMinSumFrom24HourTime(CurrentTime);
		_secondsOnCurrentMinute = 0f;
		onMinutePass.InvokeAllStaggered(_minuteStaggerTime);
		onUncappedMinutePass.InvokeAllStaggered(_minuteStaggerTime);
		if (onTimeChanged != null)
		{
			onTimeChanged();
		}
	}

	private void RpcReader___Observers_PassMinute_Client_3316948804(PooledReader PooledReader0, Channel channel)
	{
		int oldTime = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___PassMinute_Client_3316948804(oldTime);
		}
	}

	private void RpcWriter___Observers_StartSleep_2166136261()
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

	public void RpcLogic___StartSleep_2166136261()
	{
		if (IsSleepInProgress)
		{
			return;
		}
		IsSleepInProgress = true;
		try
		{
			if (onSleepStart != null)
			{
				onSleepStart();
			}
		}
		catch (Exception ex)
		{
			Console.LogError("Error invoking onSleepStart: " + ex.Message + "\nSite:" + ex.StackTrace);
		}
		SetHostSleepDone(done: false);
		((MonoBehaviour)this).StartCoroutine(WaitForSleepEnd());
		IEnumerator WaitForSleepEnd()
		{
			yield return (object)new WaitUntil((Func<bool>)(() => HostSleepDone));
			if (InstanceFinder.IsServer)
			{
				SkipForwardToTime(GameManager.IS_TUTORIAL ? 300 : 700);
				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Sleep_Count", (NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("Sleep_Count") + 1f).ToString());
			}
			IsSleepInProgress = false;
			try
			{
				if (onSleepEnd != null)
				{
					onSleepEnd();
				}
			}
			catch (Exception ex2)
			{
				Console.LogError("Error invoking onSleepEnd: " + ex2.Message + "\nSite:" + ex2.StackTrace);
			}
			Singleton<SaveManager>.Instance.Save();
		}
	}

	private void RpcReader___Observers_StartSleep_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___StartSleep_2166136261();
		}
	}

	private void RpcWriter___Observers_SetHostSleepDone_1140765316(bool done)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteBoolean(done);
			((NetworkBehaviour)this).SendObserversRpc(4u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___SetHostSleepDone_1140765316(bool done)
	{
		Console.Log("Host sleep done: " + done);
		HostSleepDone = done;
	}

	private void RpcReader___Observers_SetHostSleepDone_1140765316(PooledReader PooledReader0, Channel channel)
	{
		bool done = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetHostSleepDone_1140765316(done);
		}
	}

	private void RpcWriter___Observers_OnTimeSkip_Client_1692629761(int oldTime, int newTime)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(oldTime, (AutoPackType)1);
			((Writer)writer).WriteInt32(newTime, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(5u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___OnTimeSkip_Client_1692629761(int oldTime, int newTime)
	{
		if (newTime == CurrentTime)
		{
			return;
		}
		Console.Log("Skipping from " + oldTime + " to " + newTime);
		int minSumFrom24HourTime = GetMinSumFrom24HourTime(oldTime);
		int minSumFrom24HourTime2 = GetMinSumFrom24HourTime(newTime);
		int num = Mathf.Abs(minSumFrom24HourTime2 - minSumFrom24HourTime);
		SetTime(newTime);
		int num2 = num / 60;
		for (int i = 0; i < num2; i++)
		{
			if (onHourPass != null)
			{
				onHourPass();
			}
		}
		if (minSumFrom24HourTime2 < minSumFrom24HourTime)
		{
			ElapsedDays++;
			if (onDayPass != null)
			{
				onDayPass();
			}
			if (CurrentDay == EDay.Monday && onWeekPass != null)
			{
				onWeekPass();
			}
		}
		onMinutePass.InvokeAllStaggered(_minuteStaggerTime);
		onUncappedMinutePass.InvokeAllStaggered(_minuteStaggerTime);
		try
		{
			if (onTimeSkip != null)
			{
				onTimeSkip(num);
			}
		}
		catch (Exception ex)
		{
			Console.LogError("Error invoking onTimeSkip: " + ex.Message + "\nSite:" + ex.StackTrace);
		}
	}

	private void RpcReader___Observers_OnTimeSkip_Client_1692629761(PooledReader PooledReader0, Channel channel)
	{
		int oldTime = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		int newTime = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___OnTimeSkip_Client_1692629761(oldTime, newTime);
		}
	}

	protected override void Awake_UserLogic_ScheduleOne_002EGameTime_002ETimeManager_Assembly_002DCSharp_002Edll()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		base.Awake();
		InitializeSaveable();
		Singleton<LoadManager>.Instance.onPreSceneChange.AddListener(new UnityAction(Clean));
	}
}
