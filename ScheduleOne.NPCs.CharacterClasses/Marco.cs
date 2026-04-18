using ScheduleOne.DevUtilities;
using ScheduleOne.Dialogue;
using ScheduleOne.Persistence;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using ScheduleOne.Variables;
using ScheduleOne.Vehicles;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.NPCs.CharacterClasses;

public class Marco : NPC
{
	public Transform VehicleRecoveryPoint;

	public VehicleDetector VehicleDetector;

	public DialogueContainer RecoveryConversation;

	public DialogueContainer GreetingDialogue;

	public string GreetedVariable = "MarcoGreeted";

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002EMarcoAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002EMarcoAssembly_002DCSharp_002Edll_Excuted;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ENPCs_002ECharacterClasses_002EMarco_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	protected override void Start()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		base.Start();
		Singleton<VehicleModMenu>.Instance.onPaintPurchased.AddListener((UnityAction)delegate
		{
			DialogueHandler.ShowWorldspaceDialogue_5s("Thanks buddy");
		});
		Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(Loaded));
	}

	private bool ShouldShowRecoverVehicle(bool enabled)
	{
		return (Object)(object)Player.Local.LastDrivenVehicle != (Object)null;
	}

	private bool RecoverVehicleValid(out string reason)
	{
		if ((Object)(object)Player.Local.LastDrivenVehicle == (Object)null)
		{
			reason = "You have no vehicle to recover";
			return false;
		}
		if (Player.Local.LastDrivenVehicle.IsOccupied)
		{
			reason = "Someone is in the vehicle";
			return false;
		}
		reason = string.Empty;
		return true;
	}

	private bool RepaintVehicleValid(out string reason)
	{
		if ((Object)(object)VehicleDetector.closestVehicle == (Object)null)
		{
			reason = "Vehicle must be parked inside the shop";
			return false;
		}
		if (VehicleDetector.closestVehicle.IsOccupied)
		{
			reason = "Someone is in the vehicle";
			return false;
		}
		reason = string.Empty;
		return true;
	}

	private void RecoverVehicle()
	{
		LandVehicle lastDrivenVehicle = Player.Local.LastDrivenVehicle;
		if (!((Object)(object)lastDrivenVehicle == (Object)null))
		{
			lastDrivenVehicle.AlignTo(VehicleRecoveryPoint, EParkingAlignment.RearToKerb, network: true);
		}
	}

	private void Loaded()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(new UnityAction(Loaded));
		if (!NetworkSingleton<VariableDatabase>.Instance.GetValue<bool>(GreetedVariable))
		{
			EnableGreeting();
		}
	}

	private void EnableGreeting()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		((Component)DialogueHandler).GetComponent<DialogueController>().OverrideContainer = GreetingDialogue;
		DialogueHandler.onConversationStart.AddListener(new UnityAction(SetGreeted));
	}

	private void SetGreeted()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		DialogueHandler.onConversationStart.RemoveListener(new UnityAction(SetGreeted));
		NetworkSingleton<VariableDatabase>.Instance.SetVariableValue(GreetedVariable, true.ToString());
		((Component)DialogueHandler).GetComponent<DialogueController>().OverrideContainer = null;
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002EMarcoAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002EMarcoAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002EMarcoAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002EMarcoAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	protected override void Awake_UserLogic_ScheduleOne_002ENPCs_002ECharacterClasses_002EMarco_Assembly_002DCSharp_002Edll()
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Expected O, but got Unknown
		base.Awake();
		DialogueController.DialogueChoice dialogueChoice = new DialogueController.DialogueChoice();
		dialogueChoice.ChoiceText = "My vehicle is stuck";
		dialogueChoice.Enabled = true;
		dialogueChoice.shouldShowCheck = ShouldShowRecoverVehicle;
		dialogueChoice.isValidCheck = RecoverVehicleValid;
		dialogueChoice.Conversation = RecoveryConversation;
		dialogueChoice.onChoosen.AddListener(new UnityAction(RecoverVehicle));
		DialogueController.DialogueChoice dialogueChoice2 = new DialogueController.DialogueChoice();
		dialogueChoice2.ChoiceText = "I'd like to repaint my vehicle";
		dialogueChoice2.Enabled = true;
		dialogueChoice2.isValidCheck = RepaintVehicleValid;
		dialogueChoice2.onChoosen.AddListener((UnityAction)delegate
		{
			Singleton<VehicleModMenu>.Instance.Open(VehicleDetector.closestVehicle);
		});
		((Component)DialogueHandler).GetComponent<DialogueController>().Choices.Add(dialogueChoice2);
		((Component)DialogueHandler).GetComponent<DialogueController>().Choices.Add(dialogueChoice);
	}
}
