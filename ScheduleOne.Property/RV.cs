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
using ScheduleOne.Combat;
using ScheduleOne.Core;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.GameTime;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Variables;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Property;

public class RV : Property
{
	public Transform ModelContainer;

	public Transform FXContainer;

	public UnityEvent onExplode;

	public UnityEvent onDestroyedState;

	private bool _exploded;

	private bool NetworkInitialize___EarlyScheduleOne_002EProperty_002ERVAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EProperty_002ERVAssembly_002DCSharp_002Edll_Excuted;

	public bool IsDestroyed { get; private set; }

	protected override void Start()
	{
		base.Start();
		((MonoBehaviour)this).InvokeRepeating("UpdateVariables", 0f, 0.5f);
		TimeManager instance = NetworkSingleton<TimeManager>.Instance;
		instance.onSleepStart = (Action)Delegate.Combine(instance.onSleepStart, new Action(OnSleep));
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		base.OnSpawnServer(connection);
		if (!connection.IsHost && IsDestroyed)
		{
			SetDestroyed_Client(connection);
		}
	}

	private void UpdateVariables()
	{
		if (!InstanceFinder.IsServer || IsDestroyed)
		{
			return;
		}
		Pot[] array = (from x in BuildableItems
			where x is Pot
			select x as Pot).ToArray();
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		for (int num5 = 0; num5 < array.Length; num5++)
		{
			if (array[num5].IsFullyFilledWithSoil)
			{
				num++;
			}
			if (array[num5].NormalizedMoistureAmount > 0.9f)
			{
				num2++;
			}
			if ((Object)(object)array[num5].Plant != (Object)null)
			{
				num3++;
			}
			if (Object.op_Implicit((Object)(object)array[num5].AppliedAdditives.Find((AdditiveDefinition x) => ((BaseItemDefinition)x).ID == "speedgrow")))
			{
				num4++;
			}
		}
		NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("RV_Soil_Pots", num.ToString());
		NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("RV_Watered_Pots", num2.ToString());
		NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("RV_Seed_Pots", num3.ToString());
		NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("RV_SpeedGrow_Pots", num4.ToString());
	}

	public override bool ShouldSave()
	{
		if (IsDestroyed)
		{
			return false;
		}
		return base.ShouldSave();
	}

	[Button]
	public void BlowUp()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (!_exploded)
		{
			_exploded = true;
			Console.Log("RV exploding!");
			if (onExplode != null)
			{
				onExplode.Invoke();
			}
			if (InstanceFinder.IsServer)
			{
				ExplosionData data = new ExplosionData(20f, 200f, 1000f, checkLoS: false);
				NetworkSingleton<CombatManager>.Instance.CreateExplosion(((Component)this).transform.position, data);
			}
			((MonoBehaviour)this).StartCoroutine(Shake());
			SetDestroyed();
		}
		static IEnumerator Shake()
		{
			yield return (object)new WaitForSeconds(0.35f);
			PlayerSingleton<PlayerCamera>.Instance.StartCameraShake(2f, 1f);
		}
	}

	[TargetRpc]
	private void SetDestroyed_Client(NetworkConnection conn)
	{
		RpcWriter___Target_SetDestroyed_Client_328543758(conn);
	}

	public void SetDestroyed()
	{
		IsDestroyed = true;
		if (onDestroyedState != null)
		{
			onDestroyedState.Invoke();
		}
	}

	private void OnSleep()
	{
		if ((Object)(object)FXContainer != (Object)null)
		{
			((Component)FXContainer).gameObject.SetActive(false);
		}
	}

	public override bool CanDeliverToProperty()
	{
		return false;
	}

	public override bool CanRespawnInsideProperty()
	{
		return !IsDestroyed;
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EProperty_002ERVAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EProperty_002ERVAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterTargetRpc(5u, new ClientRpcDelegate(RpcReader___Target_SetDestroyed_Client_328543758));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EProperty_002ERVAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EProperty_002ERVAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Target_SetDestroyed_Client_328543758(NetworkConnection conn)
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
			((NetworkBehaviour)this).SendTargetRpc(5u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcLogic___SetDestroyed_Client_328543758(NetworkConnection conn)
	{
		SetDestroyed();
	}

	private void RpcReader___Target_SetDestroyed_Client_328543758(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetDestroyed_Client_328543758(((NetworkBehaviour)this).LocalConnection);
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
