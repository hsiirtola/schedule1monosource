using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Audio;
using ScheduleOne.Core;
using UnityEngine;

namespace ScheduleOne.Map;

public class Gate : NetworkBehaviour
{
	public Transform Gate1;

	public Vector3 Gate1Open;

	public Vector3 Gate1Closed;

	public Transform Gate2;

	public Vector3 Gate2Open;

	public Vector3 Gate2Closed;

	public float OpenSpeed;

	public float Acceleration = 2f;

	[Header("Sound")]
	public AudioSourceController[] StartSounds;

	public AudioSourceController[] LoopSounds;

	public AudioSourceController[] StopSounds;

	private float Momentum;

	private float openDelta;

	private bool NetworkInitialize___EarlyScheduleOne_002EMap_002EGateAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EMap_002EGateAssembly_002DCSharp_002Edll_Excuted;

	public bool IsOpen { get; protected set; }

	private void Update()
	{
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		Momentum = Mathf.MoveTowards(Momentum, 1f, Time.deltaTime * Acceleration);
		if (IsOpen)
		{
			openDelta += Time.deltaTime * OpenSpeed * Momentum;
		}
		else
		{
			openDelta -= Time.deltaTime * OpenSpeed * Momentum;
		}
		openDelta = Mathf.Clamp01(openDelta);
		if (openDelta <= 0.01f || openDelta >= 0.99f)
		{
			if (LoopSounds[0].IsPlaying)
			{
				AudioSourceController[] loopSounds = LoopSounds;
				for (int i = 0; i < loopSounds.Length; i++)
				{
					loopSounds[i].Stop();
				}
				loopSounds = StopSounds;
				for (int i = 0; i < loopSounds.Length; i++)
				{
					loopSounds[i].Play();
				}
			}
		}
		else if (!LoopSounds[0].IsPlaying && StartSounds[0].Time >= StartSounds[0].Clip.length * 0.5f)
		{
			AudioSourceController[] loopSounds = LoopSounds;
			for (int i = 0; i < loopSounds.Length; i++)
			{
				loopSounds[i].Play();
			}
		}
		Gate1.localPosition = Vector3.Lerp(Gate1Closed, Gate1Open, openDelta);
		Gate2.localPosition = Vector3.Lerp(Gate2Closed, Gate2Open, openDelta);
	}

	[Button]
	[ObserversRpc(RunLocally = true)]
	public void Open()
	{
		RpcWriter___Observers_Open_2166136261();
		RpcLogic___Open_2166136261();
	}

	[Button]
	[ObserversRpc]
	public void Close()
	{
		RpcWriter___Observers_Close_2166136261();
	}

	public override void NetworkInitialize___Early()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EMap_002EGateAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EMap_002EGateAssembly_002DCSharp_002Edll_Excuted = true;
			((NetworkBehaviour)this).RegisterObserversRpc(0u, new ClientRpcDelegate(RpcReader___Observers_Open_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_Close_2166136261));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EMap_002EGateAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EMap_002EGateAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_Open_2166136261()
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

	public void RpcLogic___Open_2166136261()
	{
		if (!IsOpen)
		{
			Momentum *= -1f;
			if (openDelta == 0f)
			{
				Momentum = 0f;
			}
			AudioSourceController[] startSounds = StartSounds;
			for (int i = 0; i < startSounds.Length; i++)
			{
				startSounds[i].Play();
			}
			IsOpen = true;
		}
	}

	private void RpcReader___Observers_Open_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___Open_2166136261();
		}
	}

	private void RpcWriter___Observers_Close_2166136261()
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

	public void RpcLogic___Close_2166136261()
	{
		if (IsOpen)
		{
			Momentum *= -1f;
			if (openDelta == 1f)
			{
				Momentum = 0f;
			}
			AudioSourceController[] startSounds = StartSounds;
			for (int i = 0; i < startSounds.Length; i++)
			{
				startSounds[i].Play();
			}
			IsOpen = false;
		}
	}

	private void RpcReader___Observers_Close_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___Close_2166136261();
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}
}
