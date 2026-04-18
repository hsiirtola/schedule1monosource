using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.ObjectScripts;

public class VMSBoard : MonoBehaviour
{
	[Header("References")]
	public TextMeshProUGUI Label;

	public void SetText(string text, Color col)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		((TMP_Text)Label).text = text;
		((Graphic)Label).color = col;
	}

	public void SetText(string text)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		SetText(text, Color32.op_Implicit(new Color32(byte.MaxValue, (byte)215, (byte)50, byte.MaxValue)));
	}
}
