using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Audio;
using ScheduleOne.AvatarFramework.Equipping;
using ScheduleOne.DevUtilities;
using ScheduleOne.Law;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI.WorldspacePopup;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour;

public class CallPoliceBehaviour : Behaviour
{
	public const float CALL_POLICE_TIME = 4f;

	[Header("References")]
	public WorldspacePopup PhoneCallPopup;

	public AvatarEquippable PhonePrefab;

	public AudioSourceController CallSound;

	private float currentCallTime;

	public Player Target;

	public Crime ReportedCrime;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ECallPoliceBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ECallPoliceBehaviourAssembly_002DCSharp_002Edll_Excuted;

	public override void Activate()
	{
		base.Activate();
		if (!IsTargetValid())
		{
			Deactivate();
			Disable();
			return;
		}
		if (ReportedCrime == null)
		{
			Console.LogError("CallPoliceBehaviour doesn't have a crime set, disabling.");
			Disable();
			Deactivate();
			return;
		}
		Console.Log("CallPoliceBehaviour started on player " + Target.PlayerName);
		currentCallTime = 0f;
		RefreshIcon();
		if (((NetworkBehaviour)Target).Owner.IsLocalClient)
		{
			((Behaviour)PhoneCallPopup).enabled = true;
		}
		CallSound.Play();
		if (InstanceFinder.IsServer)
		{
			base.Npc.SetEquippable_Client(null, PhonePrefab.AssetPath);
		}
	}

	public void SetData(NetworkObject player, Crime crime)
	{
	}

	public override void Resume()
	{
		base.Resume();
		if (!IsTargetValid())
		{
			Deactivate();
			Disable();
			return;
		}
		currentCallTime = 0f;
		RefreshIcon();
		if (((NetworkBehaviour)Target).Owner.IsLocalClient)
		{
			((Behaviour)PhoneCallPopup).enabled = true;
		}
		CallSound.Play();
		if (InstanceFinder.IsServer)
		{
			base.Npc.SetEquippable_Client(null, PhonePrefab.AssetPath);
		}
	}

	public override void Deactivate()
	{
		base.Deactivate();
		currentCallTime = 0f;
		((Behaviour)PhoneCallPopup).enabled = false;
		CallSound.Stop();
		if (InstanceFinder.IsServer)
		{
			base.Npc.SetEquippable_Client(null, string.Empty);
		}
	}

	public override void Pause()
	{
		base.Pause();
		currentCallTime = 0f;
		((Behaviour)PhoneCallPopup).enabled = false;
		CallSound.Stop();
		if (InstanceFinder.IsServer)
		{
			base.Npc.SetEquippable_Client(null, string.Empty);
		}
	}

	public override void BehaviourUpdate()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		base.BehaviourUpdate();
		currentCallTime += Time.deltaTime;
		RefreshIcon();
		base.Npc.Avatar.LookController.OverrideLookTarget(Target.EyePosition, 1, rotateBody: true);
		if (currentCallTime >= 4f && InstanceFinder.IsServer)
		{
			FinalizeCall();
		}
	}

	private void RefreshIcon()
	{
		PhoneCallPopup.CurrentFillLevel = currentCallTime / 4f;
	}

	[ObserversRpc(RunLocally = true)]
	private void FinalizeCall()
	{
		RpcWriter___Observers_FinalizeCall_2166136261();
		RpcLogic___FinalizeCall_2166136261();
	}

	private bool IsTargetValid()
	{
		if ((Object)(object)Target == (Object)null)
		{
			return false;
		}
		if (!Target.Health.IsAlive)
		{
			return false;
		}
		if (Target.IsArrested)
		{
			return false;
		}
		return true;
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ECallPoliceBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ECallPoliceBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterObserversRpc(0u, new ClientRpcDelegate(RpcReader___Observers_FinalizeCall_2166136261));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ECallPoliceBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ECallPoliceBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_FinalizeCall_2166136261()
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

	private void RpcLogic___FinalizeCall_2166136261()
	{
		if (!base.Active)
		{
			return;
		}
		if (!IsTargetValid())
		{
			Deactivate();
			Disable();
			return;
		}
		Target.CrimeData.RecordLastKnownPosition(resetTimeSinceSighted: true);
		if (Target.CrimeData.CurrentPursuitLevel < PlayerCrimeData.EPursuitLevel.Investigating)
		{
			Target.CrimeData.SetPursuitLevel(PlayerCrimeData.EPursuitLevel.Investigating);
		}
		Target.CrimeData.AddCrime(ReportedCrime);
		if (InstanceFinder.IsServer)
		{
			Singleton<LawManager>.Instance.PoliceCalled(Target, ReportedCrime);
		}
		Deactivate();
		Disable();
	}

	private void RpcReader___Observers_FinalizeCall_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___FinalizeCall_2166136261();
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
