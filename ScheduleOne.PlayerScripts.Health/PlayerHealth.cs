using System;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.UI;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.PlayerScripts.Health;

public class PlayerHealth : NetworkBehaviour
{
	public const float MAX_HEALTH = 100f;

	public const float HEALTH_RECOVERY_PER_MINUTE = 0.5f;

	[Header("References")]
	public Player Player;

	public UnityEvent<float> onHealthChanged;

	public UnityEvent onDie;

	public UnityEvent onRevive;

	private bool AfflictedWithLethalEffect;

	private bool NetworkInitialize___EarlyScheduleOne_002EPlayerScripts_002EHealth_002EPlayerHealthAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EPlayerScripts_002EHealth_002EPlayerHealthAssembly_002DCSharp_002Edll_Excuted;

	public bool IsAlive { get; protected set; } = true;

	public float CurrentHealth { get; protected set; } = 100f;

	public float TimeSinceLastDamage { get; protected set; }

	public bool CanTakeDamage
	{
		get
		{
			if (IsAlive && !Player.Local.IsArrested)
			{
				return !Player.Local.IsUnconscious;
			}
			return false;
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EPlayerScripts_002EHealth_002EPlayerHealth_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	private void Start()
	{
		NetworkSingleton<TimeManager>.Instance.onUncappedMinutePass -= new Action(MinPass);
		NetworkSingleton<TimeManager>.Instance.onUncappedMinutePass += new Action(MinPass);
	}

	[ObserversRpc]
	public void TakeDamage(float damage, bool flinch = true, bool playBloodMist = true)
	{
		RpcWriter___Observers_TakeDamage_3505310624(damage, flinch, playBloodMist);
	}

	private void Update()
	{
		TimeSinceLastDamage += Time.deltaTime;
		if (IsAlive && AfflictedWithLethalEffect)
		{
			TakeDamage(15f * Time.deltaTime, flinch: false, playBloodMist: false);
		}
	}

	private void MinPass()
	{
		if (IsAlive && CurrentHealth < 100f && TimeSinceLastDamage > 30f)
		{
			RecoverHealth(0.5f);
		}
	}

	public void SetAfflictedWithLethalEffect(bool value)
	{
		AfflictedWithLethalEffect = value;
	}

	public void RecoverHealth(float recovery)
	{
		if (CurrentHealth == 0f)
		{
			Console.LogWarning("RecoverHealth called on dead player. Use Revive() instead.");
			return;
		}
		CurrentHealth = Mathf.Clamp(CurrentHealth + recovery, 0f, 100f);
		if (onHealthChanged != null)
		{
			onHealthChanged.Invoke(CurrentHealth);
		}
	}

	public void SetHealth(float health)
	{
		CurrentHealth = Mathf.Clamp(health, 0f, 100f);
		if (onHealthChanged != null)
		{
			onHealthChanged.Invoke(CurrentHealth);
		}
		if (CurrentHealth <= 0f)
		{
			SendDie();
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SendDie()
	{
		RpcWriter___Server_SendDie_2166136261();
		RpcLogic___SendDie_2166136261();
	}

	[ObserversRpc(RunLocally = true)]
	public void Die()
	{
		RpcWriter___Observers_Die_2166136261();
		RpcLogic___Die_2166136261();
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SendRevive(Vector3 position, Quaternion rotation)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		RpcWriter___Server_SendRevive_3848837105(position, rotation);
		RpcLogic___SendRevive_3848837105(position, rotation);
	}

	[ObserversRpc(RunLocally = true, ExcludeOwner = true)]
	public void Revive(Vector3 position, Quaternion rotation)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		RpcWriter___Observers_Revive_3848837105(position, rotation);
		RpcLogic___Revive_3848837105(position, rotation);
	}

	[ObserversRpc]
	public void PlayBloodMist()
	{
		RpcWriter___Observers_PlayBloodMist_2166136261();
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
		if (!NetworkInitialize___EarlyScheduleOne_002EPlayerScripts_002EHealth_002EPlayerHealthAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EPlayerScripts_002EHealth_002EPlayerHealthAssembly_002DCSharp_002Edll_Excuted = true;
			((NetworkBehaviour)this).RegisterObserversRpc(0u, new ClientRpcDelegate(RpcReader___Observers_TakeDamage_3505310624));
			((NetworkBehaviour)this).RegisterServerRpc(1u, new ServerRpcDelegate(RpcReader___Server_SendDie_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(2u, new ClientRpcDelegate(RpcReader___Observers_Die_2166136261));
			((NetworkBehaviour)this).RegisterServerRpc(3u, new ServerRpcDelegate(RpcReader___Server_SendRevive_3848837105));
			((NetworkBehaviour)this).RegisterObserversRpc(4u, new ClientRpcDelegate(RpcReader___Observers_Revive_3848837105));
			((NetworkBehaviour)this).RegisterObserversRpc(5u, new ClientRpcDelegate(RpcReader___Observers_PlayBloodMist_2166136261));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EPlayerScripts_002EHealth_002EPlayerHealthAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EPlayerScripts_002EHealth_002EPlayerHealthAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_TakeDamage_3505310624(float damage, bool flinch = true, bool playBloodMist = true)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteSingle(damage, (AutoPackType)0);
			((Writer)writer).WriteBoolean(flinch);
			((Writer)writer).WriteBoolean(playBloodMist);
			((NetworkBehaviour)this).SendObserversRpc(0u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___TakeDamage_3505310624(float damage, bool flinch = true, bool playBloodMist = true)
	{
		if (!IsAlive)
		{
			return;
		}
		if (!CanTakeDamage)
		{
			Console.LogWarning("Player cannot take damage right now.");
			return;
		}
		CurrentHealth = Mathf.Clamp(CurrentHealth - damage, 0f, 100f);
		TimeSinceLastDamage = 0f;
		if (onHealthChanged != null)
		{
			onHealthChanged.Invoke(CurrentHealth);
		}
		if (((NetworkBehaviour)Player).IsOwner)
		{
			if (flinch && PlayerSingleton<PlayerCamera>.InstanceExists)
			{
				PlayerSingleton<PlayerCamera>.Instance.JoltCamera();
			}
			if (CurrentHealth <= 0f)
			{
				SendDie();
			}
		}
		if (playBloodMist)
		{
			PlayBloodMist();
		}
	}

	private void RpcReader___Observers_TakeDamage_3505310624(PooledReader PooledReader0, Channel channel)
	{
		float damage = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		bool flinch = ((Reader)PooledReader0).ReadBoolean();
		bool playBloodMist = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___TakeDamage_3505310624(damage, flinch, playBloodMist);
		}
	}

	private void RpcWriter___Server_SendDie_2166136261()
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
			((NetworkBehaviour)this).SendServerRpc(1u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendDie_2166136261()
	{
		Die();
	}

	private void RpcReader___Server_SendDie_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendDie_2166136261();
		}
	}

	private void RpcWriter___Observers_Die_2166136261()
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

	public void RpcLogic___Die_2166136261()
	{
		if (!IsAlive)
		{
			Console.LogWarning("Already dead!");
			return;
		}
		IsAlive = false;
		Debug.Log((object)(((object)Player)?.ToString() + " died."));
		if (onDie != null)
		{
			onDie.Invoke();
		}
	}

	private void RpcReader___Observers_Die_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___Die_2166136261();
		}
	}

	private void RpcWriter___Server_SendRevive_3848837105(Vector3 position, Quaternion rotation)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteVector3(position);
			((Writer)writer).WriteQuaternion(rotation, (AutoPackType)1);
			((NetworkBehaviour)this).SendServerRpc(3u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendRevive_3848837105(Vector3 position, Quaternion rotation)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		Revive(position, rotation);
	}

	private void RpcReader___Server_SendRevive_3848837105(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Reader)PooledReader0).ReadVector3();
		Quaternion rotation = ((Reader)PooledReader0).ReadQuaternion((AutoPackType)1);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendRevive_3848837105(position, rotation);
		}
	}

	private void RpcWriter___Observers_Revive_3848837105(Vector3 position, Quaternion rotation)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteVector3(position);
			((Writer)writer).WriteQuaternion(rotation, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(4u, writer, val, (DataOrderType)0, false, false, true);
			writer.Store();
		}
	}

	public void RpcLogic___Revive_3848837105(Vector3 position, Quaternion rotation)
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		if (IsAlive)
		{
			Console.LogWarning("Revive called on living player. Use RecoverHealth() instead.");
			return;
		}
		CurrentHealth = 100f;
		IsAlive = true;
		if (onHealthChanged != null)
		{
			onHealthChanged.Invoke(CurrentHealth);
		}
		if (onRevive != null)
		{
			onRevive.Invoke();
		}
		if (((NetworkBehaviour)this).IsOwner)
		{
			((Behaviour)Singleton<HUD>.Instance.canvas).enabled = true;
			Player.Local.Energy.RestoreEnergy();
			PlayerSingleton<PlayerMovement>.Instance.Teleport(position);
			((Component)Player.Local).transform.rotation = rotation;
			PlayerSingleton<PlayerCamera>.Instance.ResetRotation();
		}
	}

	private void RpcReader___Observers_Revive_3848837105(PooledReader PooledReader0, Channel channel)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Reader)PooledReader0).ReadVector3();
		Quaternion rotation = ((Reader)PooledReader0).ReadQuaternion((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___Revive_3848837105(position, rotation);
		}
	}

	private void RpcWriter___Observers_PlayBloodMist_2166136261()
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
			((NetworkBehaviour)this).SendObserversRpc(5u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___PlayBloodMist_2166136261()
	{
		LayerUtility.SetLayerRecursively(((Component)Player.Avatar.BloodParticles).gameObject, LayerMask.NameToLayer("Default"));
		Player.Avatar.BloodParticles.Play();
	}

	private void RpcReader___Observers_PlayBloodMist_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___PlayBloodMist_2166136261();
		}
	}

	private void Awake_UserLogic_ScheduleOne_002EPlayerScripts_002EHealth_002EPlayerHealth_Assembly_002DCSharp_002Edll()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		Singleton<SleepCanvas>.Instance.onSleepFullyFaded.AddListener((UnityAction)delegate
		{
			SetHealth(100f);
		});
	}
}
