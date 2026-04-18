using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.Money;
using ScheduleOne.Persistence;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.MainMenu;

public class SaveDisplay : MonoBehaviour
{
	[Header("References")]
	public RectTransform[] Slots;

	public void Awake()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		Singleton<LoadManager>.Instance.onSaveInfoLoaded.AddListener(new UnityAction(Refresh));
		Refresh();
	}

	public void Refresh()
	{
		for (int i = 0; i < LoadManager.SaveGames.Length; i++)
		{
			SetDisplayedSave(i, LoadManager.SaveGames[i]);
		}
	}

	public void SetDisplayedSave(int index, SaveInfo info)
	{
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		Transform val = ((Transform)Slots[index]).Find("Container");
		if (info == null)
		{
			((Component)val.Find("Info")).gameObject.SetActive(false);
			return;
		}
		((TMP_Text)((Component)val.Find("Info/Organisation")).GetComponent<TextMeshProUGUI>()).text = info.OrganisationName;
		((TMP_Text)((Component)val.Find("Info/Version")).GetComponent<TextMeshProUGUI>()).text = "v" + info.SaveVersion;
		float networth = info.Networth;
		string empty = string.Empty;
		Color color = Color32.op_Implicit(new Color32((byte)75, byte.MaxValue, (byte)10, byte.MaxValue));
		if (networth > 1000000f)
		{
			networth /= 1000000f;
			empty = "$" + RoundToDecimalPlaces(networth, 1) + "M";
			color = Color32.op_Implicit(new Color32(byte.MaxValue, (byte)225, (byte)10, byte.MaxValue));
		}
		else if (networth > 1000f)
		{
			networth /= 1000f;
			empty = "$" + RoundToDecimalPlaces(networth, 1) + "K";
		}
		else
		{
			empty = MoneyManager.FormatAmount(networth);
		}
		((TMP_Text)((Component)val.Find("Info/NetWorth/Text")).GetComponent<TextMeshProUGUI>()).text = empty;
		((Graphic)((Component)val.Find("Info/NetWorth/Text")).GetComponent<TextMeshProUGUI>()).color = color;
		int hours = Mathf.RoundToInt((float)(DateTime.Now - info.DateCreated).TotalHours);
		((TMP_Text)((Component)val.Find("Info/Created/Text")).GetComponent<TextMeshProUGUI>()).text = GetTimeLabel(hours);
		int hours2 = Mathf.RoundToInt((float)(DateTime.Now - info.DateLastPlayed).TotalHours);
		((TMP_Text)((Component)val.Find("Info/LastPlayed/Text")).GetComponent<TextMeshProUGUI>()).text = GetTimeLabel(hours2);
		((Component)val.Find("Info")).gameObject.SetActive(true);
	}

	private float RoundToDecimalPlaces(float value, int decimalPlaces)
	{
		return ToSingle(System.Math.Floor((double)value * System.Math.Pow(10.0, decimalPlaces)) / System.Math.Pow(10.0, decimalPlaces));
	}

	public static float ToSingle(double value)
	{
		return (float)value;
	}

	private string GetTimeLabel(int hours)
	{
		int num = hours / 24;
		if (num == 0)
		{
			return "Today";
		}
		if (num == 1)
		{
			return "Yesterday";
		}
		if (num > 365)
		{
			return "More than a year ago";
		}
		return num + " days ago";
	}
}
