using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Dialogue;
using ScheduleOne.Law;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Police;
using ScheduleOne.Product;
using ScheduleOne.Product.Packaging;
using ScheduleOne.UI;
using ScheduleOne.Vehicles;
using ScheduleOne.VoiceOver;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.NPCs.Behaviour;

public class BodySearchBehaviour : Behaviour
{
	public const EStealthLevel MAX_STEALTH_LEVEL = EStealthLevel.None;

	public const float BODY_SEARCH_RANGE = 2f;

	public const float MAX_SEARCH_TIME = 15f;

	public const float MAX_TIME_OUTSIDE_RANGE = 4f;

	public const float RANGE_TO_ESCALATE = 15f;

	public const float MOVE_SPEED = 0.15f;

	public const float BODY_SEARCH_COOLDOWN = 30f;

	[Header("Settings")]
	public float ArrestCircle_MaxVisibleDistance = 5f;

	public float ArrestCircle_MaxOpacity = 0.25f;

	public bool ShowPostSearchDialogue = true;

	[Header("Item of interest settings")]
	public EStealthLevel MaxStealthLevel;

	private PoliceOfficer officer;

	private float targetDistanceOnStart;

	private float searchTime;

	private bool hasBeenInRange;

	private float timeOutsideRange;

	private float timeWithinSearchRange;

	private float timeSinceCantReach;

	[Header("Events")]
	public UnityEvent onSearchComplete_Clear;

	public UnityEvent onSearchComplete_ItemsFound;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EBodySearchBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EBodySearchBehaviourAssembly_002DCSharp_002Edll_Excuted;

	public static float BODY_SEARCH_TIME
	{
		get
		{
			if (!NetworkSingleton<GameManager>.Instance.IsTutorial)
			{
				return 2.5f;
			}
			return 4f;
		}
	}

	public Player TargetPlayer { get; protected set; }

	private DialogueDatabase dialogueDatabase => officer.DialogueHandler.Database;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EBodySearchBehaviour_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public override void Activate()
	{
		base.Activate();
		base.Npc.DialogueHandler.ShowWorldspaceDialogue(dialogueDatabase.GetLine(EDialogueModule.Police, "bodysearch_begin"), NetworkSingleton<GameManager>.Instance.IsTutorial ? 4f : 5f);
		base.Npc.Movement.SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("bodysearching", 40, 0.15f));
		base.Npc.Movement.SetAgentType(NPCMovement.EAgentType.IgnoreCosts);
		base.Npc.PlayVO(EVOLineType.Command);
		if (((NetworkBehaviour)TargetPlayer).IsOwner)
		{
			PlayerSingleton<PlayerCamera>.Instance.FocusCameraOnTarget(((Component)base.Npc.Avatar.MiddleSpineRB).transform);
		}
		TargetPlayer.CrimeData.ResetBodysearchCooldown();
	}

	public override void Resume()
	{
		base.Resume();
		base.Npc.DialogueHandler.ShowWorldspaceDialogue(dialogueDatabase.GetLine(EDialogueModule.Police, "bodysearch_begin"), 5f);
		base.Npc.Movement.SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("bodysearching", 40, 0.15f));
		base.Npc.Movement.SetAgentType(NPCMovement.EAgentType.IgnoreCosts);
		TargetPlayer.CrimeData.ResetBodysearchCooldown();
	}

	public override void Deactivate()
	{
		base.Deactivate();
		if ((Object)(object)TargetPlayer != (Object)null)
		{
			TargetPlayer.CrimeData.BodySearchPending = false;
		}
		Disable();
		base.Npc.Avatar.Animation.SetBool("PatDown", value: false);
		base.Npc.Movement.SetAgentType(NPCMovement.EAgentType.Humanoid);
		ClearSpeedControls();
	}

	public override void Pause()
	{
		base.Pause();
		base.Npc.Avatar.Animation.SetBool("PatDown", value: false);
		base.Npc.Movement.SetAgentType(NPCMovement.EAgentType.Humanoid);
		ClearSpeedControls();
	}

	public override void BehaviourUpdate()
	{
		base.BehaviourUpdate();
		searchTime += Time.deltaTime;
		UpdateSearch();
		UpdateCircle();
		UpdateLookAt();
		if (InstanceFinder.IsServer)
		{
			if (!IsTargetValid(TargetPlayer))
			{
				Disable_Networked(null);
				Deactivate_Networked(null);
			}
			else
			{
				UpdateMovement();
				UpdateEscalation();
			}
		}
	}

	private void UpdateSearch()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		if (!((Object)(object)TargetPlayer == (Object)null) && ((NetworkBehaviour)TargetPlayer).IsOwner && Vector3.Distance(((Component)this).transform.position, TargetPlayer.Avatar.CenterPoint) < 2f && !Singleton<BodySearchScreen>.Instance.IsOpen)
		{
			base.Npc.DialogueHandler.HideWorldspaceDialogue();
			Singleton<BodySearchScreen>.Instance.onSearchClear.AddListener(new UnityAction(SearchClean));
			if (!GameManager.IS_TUTORIAL)
			{
				Singleton<BodySearchScreen>.Instance.onSearchFail.AddListener(new UnityAction(SearchFail));
			}
			float num = 1f;
			base.Npc.Movement.Stop();
			Singleton<BodySearchScreen>.Instance.Open(officer, officer.BodySearchDuration * num);
			PlayerSingleton<PlayerCamera>.Instance.StopFocus();
		}
	}

	protected virtual void UpdateMovement()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		if (!InstanceFinder.IsServer || !(Vector3.Distance(((Component)this).transform.position, TargetPlayer.Avatar.CenterPoint) >= 2f))
		{
			return;
		}
		bool flag = false;
		if (!base.Npc.Movement.IsMoving)
		{
			flag = true;
		}
		if (Vector3.Distance(TargetPlayer.Avatar.CenterPoint, base.Npc.Movement.CurrentDestination) > 2f)
		{
			flag = true;
		}
		if (!flag)
		{
			return;
		}
		Vector3 newDestination = GetNewDestination();
		if (base.Npc.Movement.CanGetTo(newDestination, 2f))
		{
			timeSinceCantReach = 0f;
			base.Npc.Movement.SetDestination(GetNewDestination());
			return;
		}
		timeSinceCantReach += Time.deltaTime;
		if (timeSinceCantReach >= 1f)
		{
			Escalate();
		}
	}

	private void SearchClean()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		Singleton<BodySearchScreen>.Instance.onSearchClear.RemoveListener(new UnityAction(SearchClean));
		Singleton<BodySearchScreen>.Instance.onSearchFail.RemoveListener(new UnityAction(SearchFail));
		ConcludeSearch(clear: true);
	}

	private void SearchFail()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		Singleton<BodySearchScreen>.Instance.onSearchClear.RemoveListener(new UnityAction(SearchClean));
		Singleton<BodySearchScreen>.Instance.onSearchFail.RemoveListener(new UnityAction(SearchFail));
		ConcludeSearch(clear: false);
	}

	private void UpdateEscalation()
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkSingleton<GameManager>.Instance.IsTutorial)
		{
			if (searchTime >= 15f && ((NetworkBehaviour)TargetPlayer).IsOwner && !Singleton<BodySearchScreen>.Instance.IsOpen)
			{
				Escalate();
			}
			if (timeOutsideRange >= 4f)
			{
				Escalate();
			}
			if ((Object)(object)TargetPlayer.CurrentVehicle != (Object)null)
			{
				Escalate();
			}
			if (Vector3.Distance(((Component)this).transform.position, TargetPlayer.Avatar.CenterPoint) > Mathf.Max(15f, targetDistanceOnStart + 5f))
			{
				Escalate();
			}
		}
	}

	protected virtual void UpdateLookAt()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)TargetPlayer != (Object)null)
		{
			base.Npc.Avatar.LookController.OverrideLookTarget(TargetPlayer.MimicCamera.position, 10, rotateBody: true);
		}
	}

	protected virtual void UpdateCircle()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)TargetPlayer == (Object)null || (Object)(object)TargetPlayer != (Object)(object)Player.Local)
		{
			SetArrestCircleAlpha(0f);
			return;
		}
		float num = Vector3.Distance(TargetPlayer.Avatar.CenterPoint, ((Component)this).transform.position);
		if (num < 2f)
		{
			SetArrestCircleAlpha(ArrestCircle_MaxOpacity);
			SetArrestCircleColor(Color32.op_Implicit(new Color32((byte)75, (byte)165, byte.MaxValue, byte.MaxValue)));
		}
		else if (num < ArrestCircle_MaxVisibleDistance)
		{
			float arrestCircleAlpha = Mathf.Lerp(ArrestCircle_MaxOpacity, 0f, (num - 2f) / (ArrestCircle_MaxVisibleDistance - 2f));
			SetArrestCircleAlpha(arrestCircleAlpha);
			SetArrestCircleColor(Color.white);
		}
		else
		{
			SetArrestCircleAlpha(0f);
		}
	}

	private void SetArrestCircleAlpha(float alpha)
	{
		officer.ProxCircle.SetAlpha(alpha);
	}

	private void SetArrestCircleColor(Color col)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		officer.ProxCircle.SetColor(col);
	}

	private Vector3 GetNewDestination()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		Vector3 centerPoint = TargetPlayer.Avatar.CenterPoint;
		Vector3 val = ((Component)this).transform.position - TargetPlayer.Avatar.CenterPoint;
		return centerPoint + ((Vector3)(ref val)).normalized * 1.2f;
	}

	private void ClearSpeedControls()
	{
		if (base.Npc.Movement.SpeedController.DoesSpeedControlExist("bodysearching"))
		{
			base.Npc.Movement.SpeedController.RemoveSpeedControl("bodysearching");
		}
	}

	private bool IsTargetValid(Player player)
	{
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		if (player.IsArrested)
		{
			return false;
		}
		if (player.IsSleeping)
		{
			return false;
		}
		if (player.IsUnconscious)
		{
			return false;
		}
		if (!player.Health.IsAlive)
		{
			return false;
		}
		if (player.CrimeData.CurrentPursuitLevel != PlayerCrimeData.EPursuitLevel.None)
		{
			return false;
		}
		return true;
	}

	[ObserversRpc(RunLocally = true)]
	public virtual void AssignTarget(NetworkConnection conn, NetworkObject target)
	{
		RpcWriter___Observers_AssignTarget_1824087381(conn, target);
		RpcLogic___AssignTarget_1824087381(conn, target);
	}

	public virtual bool DoesPlayerContainItemsOfInterest()
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		foreach (HotbarSlot hotbarSlot in PlayerSingleton<PlayerInventory>.Instance.hotbarSlots)
		{
			if (hotbarSlot.ItemInstance == null)
			{
				continue;
			}
			if (hotbarSlot.ItemInstance is ProductItemInstance)
			{
				ProductItemInstance productItemInstance = hotbarSlot.ItemInstance as ProductItemInstance;
				if ((Object)(object)productItemInstance.AppliedPackaging == (Object)null || productItemInstance.AppliedPackaging.StealthLevel <= MaxStealthLevel)
				{
					return true;
				}
			}
			else if ((int)((BaseItemDefinition)hotbarSlot.ItemInstance.Definition).legalStatus != 0)
			{
				return true;
			}
		}
		return false;
	}

	public virtual void ConcludeSearch(bool clear)
	{
		if (!clear)
		{
			if (ShowPostSearchDialogue)
			{
				base.Npc.DialogueHandler.ShowWorldspaceDialogue(dialogueDatabase.GetLine(EDialogueModule.Police, "bodysearch_escalate"), 2f);
			}
			base.Npc.PlayVO(EVOLineType.Angry);
			TargetPlayer.CrimeData.SetPursuitLevel(PlayerCrimeData.EPursuitLevel.Arresting);
			officer.BeginFootPursuit_Networked(TargetPlayer.PlayerCode);
			if (onSearchComplete_ItemsFound != null)
			{
				onSearchComplete_ItemsFound.Invoke();
			}
		}
		else
		{
			NoItemsOfInterestFound();
			if (!NetworkSingleton<GameManager>.Instance.IsTutorial)
			{
				base.Npc.PlayVO(EVOLineType.Thanks);
			}
			if (onSearchComplete_Clear != null)
			{
				onSearchComplete_Clear.Invoke();
			}
			if (officer.CheckpointBehaviour.Enabled)
			{
				LandVehicle lastDrivenVehicle = TargetPlayer.LastDrivenVehicle;
				CheckpointBehaviour checkpointBehaviour = officer.CheckpointBehaviour;
				if ((Object)(object)lastDrivenVehicle != (Object)null && (checkpointBehaviour.Checkpoint.SearchArea1.vehicles.Contains(lastDrivenVehicle) || checkpointBehaviour.Checkpoint.SearchArea2.vehicles.Contains(lastDrivenVehicle)))
				{
					officer.DialogueHandler.ShowWorldspaceDialogue("Thanks. I'll now check your vehicle.", 5f);
					checkpointBehaviour.StartSearch(((NetworkBehaviour)lastDrivenVehicle).NetworkObject, ((NetworkBehaviour)TargetPlayer).NetworkObject);
				}
			}
		}
		Deactivate_Server();
	}

	public virtual void Escalate()
	{
		if (!GameManager.IS_TUTORIAL)
		{
			Debug.Log((object)"Escalating!");
			base.Npc.PlayVO(EVOLineType.Angry);
			base.Npc.DialogueHandler.ShowWorldspaceDialogue(dialogueDatabase.GetLine(EDialogueModule.Police, "bodysearch_escalate"), 2f);
			TargetPlayer.CrimeData.AddCrime(new FailureToComply());
			TargetPlayer.CrimeData.SetPursuitLevel(PlayerCrimeData.EPursuitLevel.Arresting);
			officer.BeginFootPursuit_Networked(TargetPlayer.PlayerCode);
		}
	}

	public virtual void NoItemsOfInterestFound()
	{
		if (ShowPostSearchDialogue)
		{
			base.Npc.DialogueHandler.ShowWorldspaceDialogue(dialogueDatabase.GetLine(EDialogueModule.Police, "bodysearch_all_clear"), 3f);
		}
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EBodySearchBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EBodySearchBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterObserversRpc(0u, new ClientRpcDelegate(RpcReader___Observers_AssignTarget_1824087381));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EBodySearchBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EBodySearchBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_AssignTarget_1824087381(NetworkConnection conn, NetworkObject target)
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
			((Writer)writer).WriteNetworkObject(target);
			((NetworkBehaviour)this).SendObserversRpc(0u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public virtual void RpcLogic___AssignTarget_1824087381(NetworkConnection conn, NetworkObject target)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		TargetPlayer = ((Component)target).GetComponent<Player>();
		TargetPlayer.CrimeData.BodySearchPending = true;
		searchTime = 0f;
		timeWithinSearchRange = 0f;
		timeOutsideRange = 0f;
		hasBeenInRange = false;
		timeSinceCantReach = 0f;
		targetDistanceOnStart = Vector3.Distance(TargetPlayer.Avatar.CenterPoint, ((Component)this).transform.position);
	}

	private void RpcReader___Observers_AssignTarget_1824087381(PooledReader PooledReader0, Channel channel)
	{
		NetworkConnection conn = ((Reader)PooledReader0).ReadNetworkConnection();
		NetworkObject target = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___AssignTarget_1824087381(conn, target);
		}
	}

	protected override void Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EBodySearchBehaviour_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		officer = base.Npc as PoliceOfficer;
	}
}
