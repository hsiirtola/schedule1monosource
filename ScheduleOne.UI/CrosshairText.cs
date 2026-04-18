using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public class CrosshairText : MonoBehaviour
{
	public TextMeshProUGUI Label;

	private bool setThisFrame;

	private void Awake()
	{
		Hide();
	}

	private void LateUpdate()
	{
		if (!setThisFrame)
		{
			((Behaviour)Label).enabled = false;
		}
		setThisFrame = false;
	}

	public void Show(string text, Color col = default(Color))
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		setThisFrame = true;
		((Graphic)Label).color = ((col != default(Color)) ? col : Color.white);
		((TMP_Text)Label).text = text;
		((Behaviour)Label).enabled = true;
	}

	public void Hide()
	{
		((Behaviour)Label).enabled = false;
	}
}
