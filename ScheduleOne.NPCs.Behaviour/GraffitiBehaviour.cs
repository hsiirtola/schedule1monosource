using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using GameKit.Utilities;
using ScheduleOne.Cartel;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Graffiti;
using ScheduleOne.Levelling;
using ScheduleOne.NPCs.Other;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour;

public class GraffitiBehaviour : Behaviour
{
	public const int InterruptionXP = 50;

	public const float InterruptionCartelInfluenceChange = -0.1f;

	[Header("Graffiti: Components")]
	[SerializeField]
	private SprayPaint _sprayPaint;

	[Header("Graffiti: Settings")]
	[SerializeField]
	private Vector2Int _graffitiDurationInMinutes = new Vector2Int(120, 180);

	[SerializeField]
	private Vector2 _minMaxEffectLoopDuration = new Vector2(0.5f, 3f);

	[SerializeField]
	private Vector2 _minMaxEffectPauseDuration = new Vector2(0.5f, 1.5f);

	[SerializeField]
	private Gradient _effectColorGradient;

	[Header("Graffiti: Drawings")]
	[SerializeField]
	private List<SerializedGraffitiDrawing> _drawinglist;

	[Header("Graffiti: Interruptions")]
	[SerializeField]
	private List<Behaviour> _interruptingBehaviours;

	[Header("Debugging & Development")]
	[SerializeField]
	private bool _debugMode;

	private int _duration;

	private Coroutine _effectCoroutine;

	private WorldSpraySurface _spraySurface;

	private bool _graffitiCompleted;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EGraffitiBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EGraffitiBehaviourAssembly_002DCSharp_002Edll_Excuted;

	public override void OnSpawnServer(NetworkConnection connection)
	{
		((NetworkBehaviour)this).OnSpawnServer(connection);
		if (!connection.IsHost && base.Enabled && (Object)(object)_spraySurface != (Object)null)
		{
			SetSpraySurface_Client(null, ((NetworkBehaviour)_spraySurface).NetworkObject);
		}
	}

	public override void Enable()
	{
		base.Enable();
		_duration = Random.Range(((Vector2Int)(ref _graffitiDurationInMinutes)).x, ((Vector2Int)(ref _graffitiDurationInMinutes)).y);
		SetupEvents();
		_graffitiCompleted = false;
		if (_debugMode)
		{
			Debug.Log((object)$"[NPC Behaviour][Graffiti] Graffiti behaviour enabled for {_duration} minutes");
		}
	}

	public override void Disable()
	{
		CleanUp();
		if (!_graffitiCompleted && InstanceFinder.IsServer && (Object)(object)_spraySurface != (Object)null && NetworkSingleton<ScheduleOne.Cartel.Cartel>.InstanceExists && NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance.Status == ECartelStatus.Hostile)
		{
			Debug.Log((object)$"[NPC Behaviour][Graffiti] Graffiti behaviour interrupted, awarding {50} XP and changing cartel influence by {-0.1f} in region {_spraySurface.Region.ToString()}");
			NetworkSingleton<LevelManager>.Instance.AddXP(50);
			NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance.Influence.ChangeInfluence(_spraySurface.Region, -0.1f);
		}
		base.Disable();
	}

	public override void Activate()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		base.Activate();
		if ((Object)(object)_spraySurface == (Object)null)
		{
			Debug.LogWarning((object)"[NPC Behaviour][Graffiti] Graffiti behaviour activation failed - no spray surface assigned!");
			return;
		}
		_sprayPaint.Begin();
		StopEffectRoutine();
		_effectCoroutine = ((MonoBehaviour)this).StartCoroutine(DoEffectRoutine());
		base.Npc.Movement.FacePoint(_spraySurface.CenterPoint);
		if (InstanceFinder.IsServer)
		{
			_spraySurface.SetCurrentEditor_Server(((NetworkBehaviour)base.Npc).NetworkObject);
		}
		if (_debugMode)
		{
			Debug.Log((object)"[NPC Behaviour][Graffiti] Graffiti behaviour activated");
		}
	}

	public override void Pause()
	{
		base.Pause();
		_sprayPaint.End();
		StopEffectRoutine();
		if (InstanceFinder.IsServer)
		{
			_spraySurface.SetCurrentEditor_Server(null);
		}
	}

	public override void Deactivate()
	{
		base.Deactivate();
		_sprayPaint.End();
		StopEffectRoutine();
		if (InstanceFinder.IsServer)
		{
			_spraySurface.SetCurrentEditor_Server(null);
		}
		if (_debugMode)
		{
			Debug.Log((object)"[NPC Behaviour][Graffiti] Graffiti behaviour deactivated");
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void Complete_Server()
	{
		RpcWriter___Server_Complete_Server_2166136261();
	}

	private void CheckForInterruptions()
	{
		if (!InstanceFinder.IsServer)
		{
			return;
		}
		foreach (Behaviour interruptingBehaviour in _interruptingBehaviours)
		{
			if (interruptingBehaviour.Enabled)
			{
				Disable_Server();
				break;
			}
		}
	}

	private void SetupEvents()
	{
		CleanUp();
		NetworkSingleton<TimeManager>.Instance.onTick += new Action(CheckForInterruptions);
		NetworkSingleton<TimeManager>.Instance.onMinutePass += new Action(OnMinPass);
		TimeManager instance = NetworkSingleton<TimeManager>.Instance;
		instance.onTimeSkip = (Action<int>)Delegate.Combine(instance.onTimeSkip, new Action<int>(OnTimePass));
	}

	private void CleanUp()
	{
		NetworkSingleton<TimeManager>.Instance.onTick -= new Action(CheckForInterruptions);
		NetworkSingleton<TimeManager>.Instance.onMinutePass -= new Action(OnMinPass);
		TimeManager instance = NetworkSingleton<TimeManager>.Instance;
		instance.onTimeSkip = (Action<int>)Delegate.Remove(instance.onTimeSkip, new Action<int>(OnTimePass));
	}

	private void OnMinPass()
	{
		OnTimePass(1);
	}

	private void OnTimePass(int minutes)
	{
		if (InstanceFinder.IsServer && _duration > 0)
		{
			_duration = Mathf.Max(0, _duration - minutes);
			if (_duration <= 0)
			{
				Complete_Server();
			}
		}
	}

	private void StopEffectRoutine()
	{
		if (_effectCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(_effectCoroutine);
			_effectCoroutine = null;
		}
	}

	private IEnumerator DoEffectRoutine()
	{
		int safetyCounter = 0;
		do
		{
			float num = Random.Range(_minMaxEffectLoopDuration.x, _minMaxEffectLoopDuration.y);
			Color colour = _effectColorGradient.Evaluate(Random.Range(0f, 1f));
			_sprayPaint.SetEffect(value: true, colour);
			yield return (object)new WaitForSeconds(num);
			_sprayPaint.SetEffect(value: false);
			float num2 = Random.Range(_minMaxEffectLoopDuration.x, _minMaxEffectLoopDuration.y);
			yield return (object)new WaitForSeconds(num2);
			safetyCounter++;
		}
		while (safetyCounter < 80);
	}

	[ObserversRpc(RunLocally = true)]
	public void SetSpraySurface_Client(NetworkConnection conn, NetworkObject surface)
	{
		RpcWriter___Observers_SetSpraySurface_Client_1824087381(conn, surface);
		RpcLogic___SetSpraySurface_Client_1824087381(conn, surface);
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EGraffitiBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EGraffitiBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_Complete_Server_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_SetSpraySurface_Client_1824087381));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EGraffitiBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EGraffitiBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_Complete_Server_2166136261()
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

	private void RpcLogic___Complete_Server_2166136261()
	{
		if (_graffitiCompleted)
		{
			return;
		}
		Arrays.Shuffle<SerializedGraffitiDrawing>(_drawinglist);
		foreach (SerializedGraffitiDrawing item in _drawinglist)
		{
			if (_spraySurface.WillDrawingFit(item.Width, item.Height))
			{
				Debug.Log((object)("[NPC Behaviour][Graffiti] Graffiti behaviour completed, applying drawing " + item.DrawingName + " to surface " + ((Object)_spraySurface).name));
				_spraySurface.LoadSerializedDrawing(item, isCartelGraffiti: true);
				break;
			}
		}
		_graffitiCompleted = true;
		Disable_Server();
	}

	private void RpcReader___Server_Complete_Server_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___Complete_Server_2166136261();
		}
	}

	private void RpcWriter___Observers_SetSpraySurface_Client_1824087381(NetworkConnection conn, NetworkObject surface)
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
			((Writer)writer).WriteNetworkConnection(conn);
			((Writer)writer).WriteNetworkObject(surface);
			((NetworkBehaviour)this).SendObserversRpc(1u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___SetSpraySurface_Client_1824087381(NetworkConnection conn, NetworkObject surface)
	{
		_spraySurface = ((Component)surface).GetComponent<WorldSpraySurface>();
	}

	private void RpcReader___Observers_SetSpraySurface_Client_1824087381(PooledReader PooledReader0, Channel channel)
	{
		NetworkConnection conn = ((Reader)PooledReader0).ReadNetworkConnection();
		NetworkObject surface = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetSpraySurface_Client_1824087381(conn, surface);
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
