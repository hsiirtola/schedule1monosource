using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.MainMenu;

public class ConfirmExitScreen : MainMenuScreen
{
	public TextMeshProUGUI TimeSinceSaveLabel;

	private void Update()
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		if (!base.IsOpen)
		{
			return;
		}
		float secondsSinceLastSave = Singleton<SaveManager>.Instance.SecondsSinceLastSave;
		if (InstanceFinder.IsServer)
		{
			if (secondsSinceLastSave <= 60f)
			{
				((TMP_Text)TimeSinceSaveLabel).text = "Last save was " + Mathf.RoundToInt(secondsSinceLastSave) + " seconds ago";
				((Graphic)TimeSinceSaveLabel).color = Color.white;
			}
			else
			{
				int num = Mathf.FloorToInt(secondsSinceLastSave / 60f);
				((TMP_Text)TimeSinceSaveLabel).text = "Last save was " + num + " minute" + ((num > 1) ? "s" : "") + " ago";
				((Graphic)TimeSinceSaveLabel).color = ((num > 1) ? Color32.op_Implicit(new Color32(byte.MaxValue, (byte)100, (byte)100, byte.MaxValue)) : Color.white);
			}
			((Behaviour)TimeSinceSaveLabel).enabled = true;
		}
		else
		{
			((Behaviour)TimeSinceSaveLabel).enabled = false;
		}
	}

	public void ConfirmExit()
	{
		Singleton<LoadManager>.Instance.ExitToMenu();
		Close(openPrevious: true);
	}
}
