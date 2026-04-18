using ScheduleOne.Core.Items.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.ObjectScripts;

public class MixingStationMk2 : MixingStation
{
	public Animation Animation;

	[Header("Screen")]
	public Canvas ScreenCanvas;

	public Image OutputIcon;

	public RectTransform QuestionMark;

	public TextMeshProUGUI QuantityLabel;

	public TextMeshProUGUI ProgressLabel;

	private bool NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EMixingStationMk2Assembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EObjectScripts_002EMixingStationMk2Assembly_002DCSharp_002Edll_Excuted;

	protected override void OnTimePass(int minutes)
	{
		base.OnTimePass(minutes);
		UpdateScreen();
	}

	public override void MixingStart()
	{
		base.MixingStart();
		Animation.Play("Mixing station start");
		EnableScreen();
	}

	public override void MixingDone()
	{
		base.MixingDone();
		Animation.Play("Mixing station end");
		DisableScreen();
	}

	private void EnableScreen()
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (base.CurrentMixOperation != null)
		{
			((TMP_Text)QuantityLabel).text = base.CurrentMixOperation.Quantity + "x";
			if (base.CurrentMixOperation.IsOutputKnown(out var knownProduct))
			{
				OutputIcon.sprite = ((BaseItemDefinition)knownProduct).Icon;
				((Graphic)OutputIcon).color = Color.white;
				((Component)QuestionMark).gameObject.SetActive(false);
			}
			else
			{
				OutputIcon.sprite = ((BaseItemDefinition)Registry.GetItem(base.CurrentMixOperation.ProductID)).Icon;
				((Graphic)OutputIcon).color = Color.black;
				((Component)QuestionMark).gameObject.SetActive(true);
			}
			UpdateScreen();
			((Behaviour)ScreenCanvas).enabled = true;
		}
	}

	private void UpdateScreen()
	{
		if (base.CurrentMixOperation != null)
		{
			((TMP_Text)ProgressLabel).text = GetMixTimeForCurrentOperation() - base.CurrentMixTime + " mins remaining";
		}
	}

	private void DisableScreen()
	{
		((Behaviour)ScreenCanvas).enabled = false;
	}

	protected override void SetMixerToLowered()
	{
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EMixingStationMk2Assembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EMixingStationMk2Assembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EObjectScripts_002EMixingStationMk2Assembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EObjectScripts_002EMixingStationMk2Assembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
