using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Object.Synchronizing;
using FishNet.Object.Synchronizing.Internal;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Dialogue;
using ScheduleOne.GameTime;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.NPCs;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Product;
using ScheduleOne.Property;
using ScheduleOne.Tools;
using ScheduleOne.UI;
using ScheduleOne.Variables;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Employees;

public class Employee : NPC
{
	public class NoWorkReason
	{
		public string Reason;

		public string Fix;

		public int Priority;

		public NoWorkReason(string reason, string fix, int priority)
		{
			Reason = reason;
			Fix = fix;
			Priority = priority;
		}
	}

	public const int MAX_CONSECUTIVE_PATHING_FAILURES = 5;

	public bool DEBUG;

	[CompilerGenerated]
	[SyncVar]
	public bool _003CPaidForToday_003Ek__BackingField;

	[SerializeField]
	protected EEmployeeType Type;

	public FloatStack WorkSpeedController = new FloatStack(1f);

	[Header("Payment")]
	public float SigningFee = 500f;

	public float DailyWage = 100f;

	[Header("References")]
	public IdleBehaviour WaitOutside;

	public MoveItemBehaviour MoveItemBehaviour;

	public DialogueContainer BedNotAssignedDialogue;

	public DialogueContainer NotPaidDialogue;

	public DialogueContainer WorkIssueDialogueTemplate;

	public DialogueContainer FireDialogue;

	public DialogueContainer TransferDialogue;

	private List<NoWorkReason> WorkIssues = new List<NoWorkReason>();

	protected bool initialized;

	protected int consecutivePathingFailures;

	private float timeOnLastPathingFailure;

	private Transform cachedNPCSpawnPoint;

	public SyncVar<bool> syncVar____003CPaidForToday_003Ek__BackingField;

	private bool NetworkInitialize___EarlyScheduleOne_002EEmployees_002EEmployeeAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EEmployees_002EEmployeeAssembly_002DCSharp_002Edll_Excuted;

	public ScheduleOne.Property.Property AssignedProperty { get; protected set; }

	public int EmployeeIndex { get; protected set; }

	public bool PaidForToday
	{
		[CompilerGenerated]
		get
		{
			return SyncAccessor__003CPaidForToday_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			this.sync___set_value__003CPaidForToday_003Ek__BackingField(value, true);
		}
	}

	public bool Fired { get; private set; }

	public bool IsWaitingOutside => WaitOutside.Active;

	public bool IsMale { get; private set; } = true;

	protected int AppearanceIndex { get; private set; }

	public EEmployeeType EmployeeType => Type;

	public float CurrentWorkSpeed => WorkSpeedController.Value;

	public int TicksSinceLastWork { get; private set; }

	public bool SyncAccessor__003CPaidForToday_003Ek__BackingField
	{
		get
		{
			return PaidForToday;
		}
		set
		{
			if (value || !((NetworkBehaviour)this).IsServerInitialized)
			{
				PaidForToday = value;
			}
			if (Application.isPlaying)
			{
				syncVar____003CPaidForToday_003Ek__BackingField.SetValue(value, value);
			}
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EEmployees_002EEmployee_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	protected override void Start()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		base.Start();
		DialogueController.DialogueChoice dialogueChoice = new DialogueController.DialogueChoice();
		dialogueChoice.ChoiceText = "I need to trade some items";
		dialogueChoice.Enabled = true;
		dialogueChoice.onChoosen.AddListener(new UnityAction(TradeItems));
		((Component)DialogueHandler).GetComponent<DialogueController>().AddDialogueChoice(dialogueChoice, 3);
		DialogueController.DialogueChoice dialogueChoice2 = new DialogueController.DialogueChoice();
		dialogueChoice2.ChoiceText = "Why aren't you working?";
		dialogueChoice2.Enabled = true;
		dialogueChoice2.shouldShowCheck = ShouldShowNoWorkDialogue;
		dialogueChoice2.onChoosen.AddListener(new UnityAction(OnNotWorkingDialogue));
		((Component)DialogueHandler).GetComponent<DialogueController>().AddDialogueChoice(dialogueChoice2, 2);
		DialogueController.DialogueChoice dialogueChoice3 = new DialogueController.DialogueChoice();
		dialogueChoice3.ChoiceText = "I need to transfer you to another property";
		dialogueChoice3.Enabled = true;
		dialogueChoice3.Conversation = TransferDialogue;
		((Component)DialogueHandler).GetComponent<DialogueController>().AddDialogueChoice(dialogueChoice3, 1);
		DialogueController.DialogueChoice dialogueChoice4 = new DialogueController.DialogueChoice();
		dialogueChoice4.ChoiceText = "Your services are no longer required.";
		dialogueChoice4.Enabled = true;
		dialogueChoice4.shouldShowCheck = ShouldShowFireDialogue;
		dialogueChoice4.Conversation = FireDialogue;
		((Component)DialogueHandler).GetComponent<DialogueController>().AddDialogueChoice(dialogueChoice4, -1);
		DialogueHandler.onDialogueChoiceChosen.AddListener((UnityAction<string>)CheckDialogueChoice);
	}

	public override void OnStartServer()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		base.OnStartServer();
		Health.onDie.AddListener(new UnityAction(SendFire));
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		base.OnSpawnServer(connection);
		if (!connection.IsLocalClient)
		{
			Initialize(connection, FirstName, LastName, ID, base.GUID.ToString(), AssignedProperty.PropertyCode, IsMale, AppearanceIndex);
		}
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public virtual void Initialize(NetworkConnection conn, string firstName, string lastName, string id, string guid, string propertyID, bool male, int appearanceIndex)
	{
		if (conn == null)
		{
			RpcWriter___Observers_Initialize_2260823878(conn, firstName, lastName, id, guid, propertyID, male, appearanceIndex);
			RpcLogic___Initialize_2260823878(conn, firstName, lastName, id, guid, propertyID, male, appearanceIndex);
		}
		else
		{
			RpcWriter___Target_Initialize_2260823878(conn, firstName, lastName, id, guid, propertyID, male, appearanceIndex);
		}
	}

	protected virtual void AssignProperty(ScheduleOne.Property.Property prop, bool warp)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		AssignedProperty = prop;
		EmployeeIndex = AssignedProperty.RegisterEmployee(this);
		if (warp)
		{
			Movement.Warp(prop.NPCSpawnPoint.position);
		}
		WaitOutside.IdlePoint = prop.EmployeeIdlePoints[EmployeeIndex];
		cachedNPCSpawnPoint = prop.NPCSpawnPoint;
	}

	protected virtual void UnassignProperty()
	{
		if (!((Object)(object)AssignedProperty == (Object)null))
		{
			ResetConfiguration();
			AssignedProperty.DeregisterEmployee(this);
			AssignedProperty = null;
		}
	}

	[ServerRpc(RequireOwnership = false)]
	public void SendTransfer(string propertyCode)
	{
		RpcWriter___Server_SendTransfer_3615296227(propertyCode);
	}

	[ObserversRpc]
	private void TransferToProperty(string code)
	{
		RpcWriter___Observers_TransferToProperty_3615296227(code);
	}

	protected virtual void TransferToProperty(ScheduleOne.Property.Property prop)
	{
		if (!((Object)(object)AssignedProperty == (Object)(object)prop))
		{
			Console.Log("Transferring employee " + FirstName + " " + LastName + " to property " + prop.PropertyName);
			UnassignProperty();
			AssignProperty(prop, warp: false);
		}
	}

	protected virtual void InitializeInfo(string firstName, string lastName, string id)
	{
		FirstName = firstName;
		LastName = lastName;
		ID = id;
		((Object)((Component)this).gameObject).name = base.fullName + $" ({Type})";
		NetworkSingleton<EmployeeManager>.Instance.RegisterName(firstName + " " + lastName);
	}

	protected virtual void InitializeAppearance(bool male, int index)
	{
		IsMale = male;
		AppearanceIndex = index;
		EmployeeManager.EmployeeAppearance appearance = NetworkSingleton<EmployeeManager>.Instance.GetAppearance(male, index);
		appearance.Settings.BodyLayerSettings.Clear();
		Avatar.LoadNakedSettings(appearance.Settings, 100);
		MugshotSprite = appearance.Mugshot;
		VoiceOverEmitter.SetDatabase(NetworkSingleton<EmployeeManager>.Instance.GetVoice(male, index));
		int num = (FirstName + LastName).GetHashCode() / 1000;
		VoiceOverEmitter.PitchMultiplier = 0.9f + (float)(num % 10) / 10f * 0.2f;
		NetworkSingleton<EmployeeManager>.Instance.RegisterAppearance(male, index);
		float num2 = (male ? 0.8f : 1.3f);
		float num3 = 0.2f;
		float num4 = (0f - num3) / 2f + Mathf.Clamp01((float)(FirstName.GetHashCode() % 10) / 10f) * num3;
		num2 += num4;
		VoiceOverEmitter.PitchMultiplier = num2;
	}

	protected virtual void CheckDialogueChoice(string choiceLabel)
	{
		if (choiceLabel == "CONFIRM_FIRE")
		{
			SendFire();
		}
	}

	[ServerRpc(RequireOwnership = false)]
	public void SendFire()
	{
		RpcWriter___Server_SendFire_2166136261();
	}

	[ObserversRpc]
	private void ReceiveFire()
	{
		RpcWriter___Observers_ReceiveFire_2166136261();
	}

	protected virtual void ResetConfiguration()
	{
	}

	protected virtual void Fire()
	{
		Console.Log("Firing employee " + FirstName + " " + LastName);
		ResetConfiguration();
		UnassignProperty();
		Avatar.EmotionManager.AddEmotionOverride("Concerned", "fired");
		SetWaitOutside(wait: false);
		Fired = true;
	}

	protected bool CanWork()
	{
		if ((Object)(object)GetHome() != (Object)null && SyncAccessor__003CPaidForToday_003Ek__BackingField && !NetworkSingleton<TimeManager>.Instance.IsEndOfDay)
		{
			return !Behaviour.ConsumeProductBehaviour.Enabled;
		}
		return false;
	}

	protected virtual bool CanConsumeProduct()
	{
		if (Fired)
		{
			return false;
		}
		if (!SyncAccessor__003CPaidForToday_003Ek__BackingField)
		{
			return false;
		}
		if (NetworkSingleton<TimeManager>.Instance.IsEndOfDay)
		{
			return false;
		}
		if (Behaviour.ConsumeProductBehaviour.Enabled)
		{
			return false;
		}
		if (Behaviour.ConsumeProductBehaviour.ConsumedProduct != null)
		{
			return false;
		}
		if (IsAnyWorkInProgress())
		{
			return false;
		}
		if ((Object)(object)GetHome() == (Object)null)
		{
			return false;
		}
		if (Behaviour.GenericDialogueBehaviour.Enabled)
		{
			return false;
		}
		if (Behaviour.FaceTargetBehaviour.Enabled)
		{
			return false;
		}
		return true;
	}

	protected ItemSlot GetFirstInventorySlotContainingProduct()
	{
		return Inventory.GetSlots((ItemSlot x) => x.ItemInstance != null && x.ItemInstance is ProductItemInstance && ((ProductItemInstance)x.ItemInstance).PackagingID == "baggie").FirstOrDefault();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (InstanceFinder.IsServer)
		{
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onSleepEnd = (Action)Delegate.Remove(instance.onSleepEnd, new Action(OnSleepEnd));
		}
		if (NetworkSingleton<EmployeeManager>.InstanceExists)
		{
			NetworkSingleton<EmployeeManager>.Instance.AllEmployees.Remove(this);
		}
	}

	protected virtual void UpdateBehaviour()
	{
		if (Fired)
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		if ((Object)(object)GetHome() == (Object)null)
		{
			flag = true;
			SubmitNoWorkReason("I haven't been assigned a locker", "You can use your management clipboard to assign me a locker.");
		}
		else if (NetworkSingleton<TimeManager>.Instance.IsEndOfDay)
		{
			flag = true;
			SubmitNoWorkReason("Sorry boss, my shift ends at 4AM.", string.Empty);
		}
		else if (!SyncAccessor__003CPaidForToday_003Ek__BackingField)
		{
			if (IsPayAvailable())
			{
				flag2 = true;
			}
			else
			{
				flag = true;
				SubmitNoWorkReason("I haven't been paid yet", "You can place cash in my locker.");
			}
		}
		if (InstanceFinder.IsServer)
		{
			UpdateConsumeProduct();
		}
		if (InstanceFinder.IsServer && ((Object)(object)Behaviour.activeBehaviour == (Object)null || (Object)(object)Behaviour.activeBehaviour == (Object)(object)WaitOutside))
		{
			if (flag)
			{
				SetWaitOutside(wait: true);
			}
			if (flag2 && IsPayAvailable())
			{
				RemoveDailyWage();
				SetIsPaid();
			}
		}
	}

	private void UpdateConsumeProduct()
	{
		if (CanConsumeProduct())
		{
			ItemSlot firstInventorySlotContainingProduct = GetFirstInventorySlotContainingProduct();
			if (firstInventorySlotContainingProduct != null)
			{
				Debug.Log((object)("Employee " + base.fullName + " will consume a product: " + ((BaseItemInstance)firstInventorySlotContainingProduct.ItemInstance).Name));
				Behaviour.ConsumeProduct(firstInventorySlotContainingProduct.ItemInstance as ProductItemInstance, removeFromInventory: true);
			}
		}
	}

	protected void MarkIsWorking()
	{
		TicksSinceLastWork = 0;
	}

	protected virtual bool IsAnyWorkInProgress()
	{
		return false;
	}

	private void SetWaitOutside(bool wait)
	{
		if (wait)
		{
			if (!WaitOutside.Enabled)
			{
				WaitOutside.Enable_Networked();
			}
		}
		else if (WaitOutside.Enabled || WaitOutside.Active)
		{
			WaitOutside.Disable_Networked(null);
			WaitOutside.Deactivate_Networked(null);
		}
	}

	protected virtual bool ShouldIdle()
	{
		return false;
	}

	protected override void OnTick()
	{
		base.OnTick();
		TicksSinceLastWork++;
		WorkIssues.Clear();
		UpdateBehaviour();
	}

	private void OnSleepEnd()
	{
		PaidForToday = false;
	}

	public void SetIsPaid()
	{
		PaidForToday = true;
	}

	public override bool ShouldSave()
	{
		return false;
	}

	public override NPCData GetNPCData()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		return new EmployeeData(ID, AssignedProperty.PropertyCode, FirstName, LastName, IsMale, AppearanceIndex, ((Component)this).transform.position, ((Component)this).transform.rotation, base.GUID, SyncAccessor__003CPaidForToday_003Ek__BackingField);
	}

	public virtual EmployeeHome GetHome()
	{
		Console.LogError("GETBED NOT IMPLEMENTED");
		return null;
	}

	public bool IsPayAvailable()
	{
		EmployeeHome home = GetHome();
		if ((Object)(object)home == (Object)null)
		{
			return false;
		}
		return home.GetCashSum() >= DailyWage;
	}

	public void RemoveDailyWage()
	{
		if (InstanceFinder.IsServer)
		{
			EmployeeHome home = GetHome();
			if (!((Object)(object)home == (Object)null) && home.GetCashSum() >= DailyWage)
			{
				home.RemoveCash(DailyWage);
			}
		}
	}

	public virtual bool GetWorkIssue(out DialogueContainer notWorkingReason)
	{
		if ((Object)(object)GetHome() == (Object)null)
		{
			notWorkingReason = BedNotAssignedDialogue;
			return true;
		}
		if (!SyncAccessor__003CPaidForToday_003Ek__BackingField)
		{
			notWorkingReason = NotPaidDialogue;
			return true;
		}
		if (TicksSinceLastWork >= 5 && WorkIssues.Count > 0)
		{
			notWorkingReason = Object.Instantiate<DialogueContainer>(WorkIssueDialogueTemplate);
			notWorkingReason.GetDialogueNodeByLabel("ENTRY").DialogueText = WorkIssues[0].Reason;
			if (!string.IsNullOrEmpty(WorkIssues[0].Fix))
			{
				notWorkingReason.GetDialogueNodeByLabel("FIX").DialogueText = WorkIssues[0].Fix;
			}
			else
			{
				notWorkingReason.GetDialogueNodeByLabel("ENTRY").choices = new DialogueChoiceData[0];
			}
			return true;
		}
		notWorkingReason = null;
		return false;
	}

	public virtual void SetIdle(bool idle)
	{
		SetWaitOutside(idle);
	}

	protected void LeavePropertyAndDespawn()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (!Movement.IsMoving && InstanceFinder.IsServer)
		{
			if (Movement.IsAsCloseAsPossible(cachedNPCSpawnPoint.position, 1f))
			{
				((NetworkBehaviour)this).Despawn(((NetworkBehaviour)this).NetworkObject, (DespawnType?)null);
			}
			else
			{
				SetDestination(cachedNPCSpawnPoint.position);
			}
		}
	}

	[ObserversRpc(RunLocally = true)]
	public void SubmitNoWorkReason(string reason, string fix, int priority = 0)
	{
		RpcWriter___Observers_SubmitNoWorkReason_15643032(reason, fix, priority);
		RpcLogic___SubmitNoWorkReason_15643032(reason, fix, priority);
	}

	private bool ShouldShowNoWorkDialogue(bool enabled)
	{
		if (Fired)
		{
			return false;
		}
		DialogueContainer notWorkingReason;
		if (WaitOutside.Active)
		{
			return GetWorkIssue(out notWorkingReason);
		}
		return false;
	}

	private void OnNotWorkingDialogue()
	{
		if (GetWorkIssue(out var notWorkingReason))
		{
			DialogueHandler.InitializeDialogue(notWorkingReason);
		}
	}

	private bool ShouldShowFireDialogue(bool enabled)
	{
		if (Fired)
		{
			return false;
		}
		return true;
	}

	private void TradeItems()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		DialogueHandler.SkipNextDialogueBehaviourEnd();
		Singleton<StorageMenu>.Instance.Open(Inventory, base.fullName + "'s Inventory", string.Empty);
		Singleton<StorageMenu>.Instance.onClosed.AddListener(new UnityAction(TradeItemsDone));
	}

	private void TradeItemsDone()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		Singleton<StorageMenu>.Instance.onClosed.RemoveListener(new UnityAction(TradeItemsDone));
		Behaviour.GenericDialogueBehaviour.Disable_Server();
	}

	protected void SetDestination(ITransitEntity transitEntity, bool teleportIfFail = true)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		SetDestination(NavMeshUtility.GetReachableAccessPoint(transitEntity, this).position, teleportIfFail);
	}

	protected unsafe void SetDestination(Vector3 position, bool teleportIfFail = true)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (InstanceFinder.IsServer)
		{
			if (teleportIfFail && consecutivePathingFailures >= 5 && !Movement.CanGetTo(position))
			{
				Console.LogWarning(base.fullName + " too many pathing failures. Warping to " + ((object)(*(Vector3*)(&position))/*cast due to .constrained prefix*/).ToString());
				Movement.Warp(position);
				WalkCallback(NPCMovement.WalkResult.Success);
			}
			Movement.SetDestination(position, WalkCallback, 1f, 0.1f);
		}
	}

	protected virtual void WalkCallback(NPCMovement.WalkResult result)
	{
		if (result == NPCMovement.WalkResult.Failed)
		{
			if (Time.timeSinceLevelLoad - timeOnLastPathingFailure > 0.2f)
			{
				timeOnLastPathingFailure = Time.timeSinceLevelLoad;
				consecutivePathingFailures++;
			}
		}
		else
		{
			consecutivePathingFailures = 0;
		}
	}

	public override void NetworkInitialize___Early()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Expected O, but got Unknown
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Expected O, but got Unknown
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Expected O, but got Unknown
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Expected O, but got Unknown
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EEmployees_002EEmployeeAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EEmployees_002EEmployeeAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			syncVar____003CPaidForToday_003Ek__BackingField = new SyncVar<bool>((NetworkBehaviour)(object)this, 1u, (WritePermission)0, (ReadPermission)0, -1f, (Channel)0, PaidForToday);
			((NetworkBehaviour)this).RegisterObserversRpc(39u, new ClientRpcDelegate(RpcReader___Observers_Initialize_2260823878));
			((NetworkBehaviour)this).RegisterTargetRpc(40u, new ClientRpcDelegate(RpcReader___Target_Initialize_2260823878));
			((NetworkBehaviour)this).RegisterServerRpc(41u, new ServerRpcDelegate(RpcReader___Server_SendTransfer_3615296227));
			((NetworkBehaviour)this).RegisterObserversRpc(42u, new ClientRpcDelegate(RpcReader___Observers_TransferToProperty_3615296227));
			((NetworkBehaviour)this).RegisterServerRpc(43u, new ServerRpcDelegate(RpcReader___Server_SendFire_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(44u, new ClientRpcDelegate(RpcReader___Observers_ReceiveFire_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(45u, new ClientRpcDelegate(RpcReader___Observers_SubmitNoWorkReason_15643032));
			((NetworkBehaviour)this).RegisterSyncVarRead(new SyncVarReadDelegate(ReadSyncVar___ScheduleOne_002EEmployees_002EEmployee));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EEmployees_002EEmployeeAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EEmployees_002EEmployeeAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
			((SyncBase)syncVar____003CPaidForToday_003Ek__BackingField).SetRegistered();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_Initialize_2260823878(NetworkConnection conn, string firstName, string lastName, string id, string guid, string propertyID, bool male, int appearanceIndex)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(firstName);
			((Writer)writer).WriteString(lastName);
			((Writer)writer).WriteString(id);
			((Writer)writer).WriteString(guid);
			((Writer)writer).WriteString(propertyID);
			((Writer)writer).WriteBoolean(male);
			((Writer)writer).WriteInt32(appearanceIndex, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(39u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public virtual void RpcLogic___Initialize_2260823878(NetworkConnection conn, string firstName, string lastName, string id, string guid, string propertyID, bool male, int appearanceIndex)
	{
		if (initialized)
		{
			return;
		}
		NetworkSingleton<EmployeeManager>.Instance.AllEmployees.Add(this);
		initialized = true;
		SetGUID(new Guid(guid));
		InitializeInfo(firstName, lastName, id);
		InitializeAppearance(male, appearanceIndex);
		AssignProperty(Singleton<PropertyManager>.Instance.GetProperty(propertyID), InstanceFinder.IsServer);
		Movement.Agent.avoidancePriority = 10 + appearanceIndex;
		if (InstanceFinder.IsServer)
		{
			if (!NetworkSingleton<VariableDatabase>.Instance.GetValue<bool>("ClipboardAcquired"))
			{
				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("ClipboardAcquired", true.ToString());
			}
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onSleepEnd = (Action)Delegate.Combine(instance.onSleepEnd, new Action(OnSleepEnd));
		}
	}

	private void RpcReader___Observers_Initialize_2260823878(PooledReader PooledReader0, Channel channel)
	{
		string firstName = ((Reader)PooledReader0).ReadString();
		string lastName = ((Reader)PooledReader0).ReadString();
		string id = ((Reader)PooledReader0).ReadString();
		string guid = ((Reader)PooledReader0).ReadString();
		string propertyID = ((Reader)PooledReader0).ReadString();
		bool male = ((Reader)PooledReader0).ReadBoolean();
		int appearanceIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___Initialize_2260823878(null, firstName, lastName, id, guid, propertyID, male, appearanceIndex);
		}
	}

	private void RpcWriter___Target_Initialize_2260823878(NetworkConnection conn, string firstName, string lastName, string id, string guid, string propertyID, bool male, int appearanceIndex)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(firstName);
			((Writer)writer).WriteString(lastName);
			((Writer)writer).WriteString(id);
			((Writer)writer).WriteString(guid);
			((Writer)writer).WriteString(propertyID);
			((Writer)writer).WriteBoolean(male);
			((Writer)writer).WriteInt32(appearanceIndex, (AutoPackType)1);
			((NetworkBehaviour)this).SendTargetRpc(40u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_Initialize_2260823878(PooledReader PooledReader0, Channel channel)
	{
		string firstName = ((Reader)PooledReader0).ReadString();
		string lastName = ((Reader)PooledReader0).ReadString();
		string id = ((Reader)PooledReader0).ReadString();
		string guid = ((Reader)PooledReader0).ReadString();
		string propertyID = ((Reader)PooledReader0).ReadString();
		bool male = ((Reader)PooledReader0).ReadBoolean();
		int appearanceIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___Initialize_2260823878(((NetworkBehaviour)this).LocalConnection, firstName, lastName, id, guid, propertyID, male, appearanceIndex);
		}
	}

	private void RpcWriter___Server_SendTransfer_3615296227(string propertyCode)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(propertyCode);
			((NetworkBehaviour)this).SendServerRpc(41u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendTransfer_3615296227(string propertyCode)
	{
		TransferToProperty(propertyCode);
	}

	private void RpcReader___Server_SendTransfer_3615296227(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string propertyCode = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SendTransfer_3615296227(propertyCode);
		}
	}

	private void RpcWriter___Observers_TransferToProperty_3615296227(string code)
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
			((Writer)writer).WriteString(code);
			((NetworkBehaviour)this).SendObserversRpc(42u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___TransferToProperty_3615296227(string code)
	{
		ScheduleOne.Property.Property property = Singleton<PropertyManager>.Instance.GetProperty(code);
		if ((Object)(object)property == (Object)null)
		{
			Console.LogError("Property not found: " + code);
		}
		else
		{
			TransferToProperty(property);
		}
	}

	private void RpcReader___Observers_TransferToProperty_3615296227(PooledReader PooledReader0, Channel channel)
	{
		string code = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___TransferToProperty_3615296227(code);
		}
	}

	private void RpcWriter___Server_SendFire_2166136261()
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
			((NetworkBehaviour)this).SendServerRpc(43u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendFire_2166136261()
	{
		ReceiveFire();
	}

	private void RpcReader___Server_SendFire_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SendFire_2166136261();
		}
	}

	private void RpcWriter___Observers_ReceiveFire_2166136261()
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
			((NetworkBehaviour)this).SendObserversRpc(44u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___ReceiveFire_2166136261()
	{
		Fire();
	}

	private void RpcReader___Observers_ReceiveFire_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ReceiveFire_2166136261();
		}
	}

	private void RpcWriter___Observers_SubmitNoWorkReason_15643032(string reason, string fix, int priority = 0)
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
			((Writer)writer).WriteString(reason);
			((Writer)writer).WriteString(fix);
			((Writer)writer).WriteInt32(priority, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(45u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___SubmitNoWorkReason_15643032(string reason, string fix, int priority = 0)
	{
		NoWorkReason noWorkReason = new NoWorkReason(reason, fix, priority);
		for (int i = 0; i < WorkIssues.Count; i++)
		{
			if (WorkIssues[i].Priority < noWorkReason.Priority)
			{
				WorkIssues.Insert(i, noWorkReason);
				return;
			}
		}
		WorkIssues.Add(noWorkReason);
	}

	private void RpcReader___Observers_SubmitNoWorkReason_15643032(PooledReader PooledReader0, Channel channel)
	{
		string reason = ((Reader)PooledReader0).ReadString();
		string fix = ((Reader)PooledReader0).ReadString();
		int priority = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SubmitNoWorkReason_15643032(reason, fix, priority);
		}
	}

	public override bool ReadSyncVar___ScheduleOne_002EEmployees_002EEmployee(PooledReader PooledReader0, uint UInt321, bool Boolean2)
	{
		if (UInt321 == 1)
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value__003CPaidForToday_003Ek__BackingField(syncVar____003CPaidForToday_003Ek__BackingField.GetValue(true), true);
				return true;
			}
			bool value = ((Reader)PooledReader0).ReadBoolean();
			this.sync___set_value__003CPaidForToday_003Ek__BackingField(value, Boolean2);
			return true;
		}
		return false;
	}

	protected override void Awake_UserLogic_ScheduleOne_002EEmployees_002EEmployee_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		WorkSpeedController.OnValueChanged += delegate(float newValue)
		{
			Avatar.Animation.animator.SetFloat("WorkSpeedMultiplier", newValue);
		};
	}
}
