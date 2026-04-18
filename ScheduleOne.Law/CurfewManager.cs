using System;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Law;

public class CurfewManager : NetworkSingleton<CurfewManager>
{
	private const string NORMAL_MESSAGE = "CURFEW TONIGHT\n9PM - 5AM";

	private const string CURFEW_MESSAGE = "CURFEW ACTIVE\n UNTIL 5AM";

	private const string WARNING_MESSAGE = "CURFEW SOON\n{0} MINS";

	public const int HOUR_BEFORE_CURFEW = 2000;

	public const int WARNING_TIME = 2030;

	public const int CURFEW_START_TIME = 2100;

	public const int HARD_CURFEW_START_TIME = 2115;

	public const int CURFEW_END_TIME = 500;

	[Header("References")]
	public VMSBoard[] VMSBoards;

	public AudioSourceController CurfewWarningSound;

	public AudioSourceController CurfewAlarmSound;

	public UnityEvent onCurfewEnabled;

	public UnityEvent onCurfewDisabled;

	public UnityEvent onCurfewHint;

	public UnityEvent onCurfewWarning;

	public UnityEvent onCurfewStart;

	public UnityEvent onCurfewHardStart;

	public UnityEvent onCurfewEnd;

	private bool NetworkInitialize___EarlyScheduleOne_002ELaw_002ECurfewManagerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ELaw_002ECurfewManagerAssembly_002DCSharp_002Edll_Excuted;

	public bool IsEnabled { get; protected set; }

	public bool IsCurrentlyActive { get; protected set; }

	public bool IsHardCurfewActive { get; protected set; }

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ELaw_002ECurfewManager_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	protected override void Start()
	{
		base.Start();
		NetworkSingleton<TimeManager>.Instance.onUncappedMinutePass -= new Action(OnUncappedMinPass);
		NetworkSingleton<TimeManager>.Instance.onUncappedMinutePass += new Action(OnUncappedMinPass);
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		((NetworkBehaviour)this).OnSpawnServer(connection);
		if (!connection.IsHost && IsEnabled)
		{
			Enable(connection);
		}
	}

	[ObserversRpc]
	[TargetRpc]
	public void Enable(NetworkConnection conn)
	{
		if (conn == null)
		{
			RpcWriter___Observers_Enable_328543758(conn);
		}
		else
		{
			RpcWriter___Target_Enable_328543758(conn);
		}
	}

	[ObserversRpc]
	public void Disable()
	{
		RpcWriter___Observers_Disable_2166136261();
	}

	private void OnUncappedMinPass()
	{
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		if (IsEnabled)
		{
			if (NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(2000, 2100))
			{
				int num = TimeManager.GetMinSumFrom24HourTime(2100) - TimeManager.GetMinSumFrom24HourTime(NetworkSingleton<TimeManager>.Instance.CurrentTime);
				VMSBoard[] vMSBoards = VMSBoards;
				for (int i = 0; i < vMSBoards.Length; i++)
				{
					vMSBoards[i].SetText($"CURFEW SOON\n{num} MINS");
				}
			}
			if (NetworkSingleton<TimeManager>.Instance.CurrentTime == 2030)
			{
				if (onCurfewWarning != null)
				{
					onCurfewWarning.Invoke();
				}
				if (NetworkSingleton<TimeManager>.Instance.ElapsedDays == 0 && onCurfewHint != null)
				{
					onCurfewHint.Invoke();
				}
				CurfewWarningSound.Play();
			}
			if (!IsCurrentlyActive && NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(2100, 500))
			{
				IsCurrentlyActive = true;
				onCurfewStart.Invoke();
				if (!NetworkSingleton<TimeManager>.Instance.IsSleepInProgress && Singleton<LoadManager>.Instance.TimeSinceGameLoaded > 3f && !CurfewAlarmSound.IsPlaying)
				{
					CurfewAlarmSound.Play();
				}
				VMSBoard[] vMSBoards = VMSBoards;
				for (int i = 0; i < vMSBoards.Length; i++)
				{
					vMSBoards[i].SetText("CURFEW ACTIVE\n UNTIL 5AM", Color32.op_Implicit(new Color32(byte.MaxValue, (byte)85, (byte)60, byte.MaxValue)));
				}
			}
			if (!IsHardCurfewActive && NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(2115, 500))
			{
				IsHardCurfewActive = true;
				if (onCurfewHardStart != null)
				{
					onCurfewHardStart.Invoke();
				}
			}
			if (IsCurrentlyActive && NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(500, 2100))
			{
				IsCurrentlyActive = false;
				IsHardCurfewActive = false;
				if (onCurfewEnd != null)
				{
					onCurfewEnd.Invoke();
				}
				VMSBoard[] vMSBoards = VMSBoards;
				for (int i = 0; i < vMSBoards.Length; i++)
				{
					vMSBoards[i].SetText("CURFEW TONIGHT\n9PM - 5AM");
				}
			}
		}
		else
		{
			IsCurrentlyActive = false;
			IsHardCurfewActive = false;
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
		if (!NetworkInitialize___EarlyScheduleOne_002ELaw_002ECurfewManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ELaw_002ECurfewManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterObserversRpc(0u, new ClientRpcDelegate(RpcReader___Observers_Enable_328543758));
			((NetworkBehaviour)this).RegisterTargetRpc(1u, new ClientRpcDelegate(RpcReader___Target_Enable_328543758));
			((NetworkBehaviour)this).RegisterObserversRpc(2u, new ClientRpcDelegate(RpcReader___Observers_Disable_2166136261));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ELaw_002ECurfewManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ELaw_002ECurfewManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_Enable_328543758(NetworkConnection conn)
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
			((NetworkBehaviour)this).SendObserversRpc(0u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___Enable_328543758(NetworkConnection conn)
	{
		IsEnabled = true;
		if (onCurfewEnabled != null)
		{
			onCurfewEnabled.Invoke();
		}
		VMSBoard[] vMSBoards = VMSBoards;
		foreach (VMSBoard obj in vMSBoards)
		{
			((Component)obj).gameObject.SetActive(true);
			obj.SetText("CURFEW TONIGHT\n9PM - 5AM");
		}
	}

	private void RpcReader___Observers_Enable_328543758(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___Enable_328543758(null);
		}
	}

	private void RpcWriter___Target_Enable_328543758(NetworkConnection conn)
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
			((NetworkBehaviour)this).SendTargetRpc(1u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_Enable_328543758(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___Enable_328543758(((NetworkBehaviour)this).LocalConnection);
		}
	}

	private void RpcWriter___Observers_Disable_2166136261()
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

	public void RpcLogic___Disable_2166136261()
	{
		IsEnabled = false;
		if (onCurfewDisabled != null)
		{
			onCurfewDisabled.Invoke();
		}
		VMSBoard[] vMSBoards = VMSBoards;
		for (int i = 0; i < vMSBoards.Length; i++)
		{
			((Component)vMSBoards[i]).gameObject.SetActive(false);
		}
	}

	private void RpcReader___Observers_Disable_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___Disable_2166136261();
		}
	}

	protected override void Awake_UserLogic_ScheduleOne_002ELaw_002ECurfewManager_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		IsEnabled = false;
		VMSBoard[] vMSBoards = VMSBoards;
		for (int i = 0; i < vMSBoards.Length; i++)
		{
			((Component)vMSBoards[i]).gameObject.SetActive(false);
		}
	}
}
