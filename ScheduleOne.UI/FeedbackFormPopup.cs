using TMPro;
using UnityEngine;

namespace ScheduleOne.UI;

public class FeedbackFormPopup : MonoBehaviour
{
	public TextMeshProUGUI Label;

	public bool AutoClose = true;

	private float closeTime;

	public void Open(string text)
	{
		if ((Object)(object)Label != (Object)null)
		{
			((TMP_Text)Label).text = text;
		}
		((Component)this).gameObject.SetActive(true);
		closeTime = Time.unscaledTime + 4f;
	}

	public void Close()
	{
		((Component)this).gameObject.SetActive(false);
	}

	private void Update()
	{
		if (AutoClose && Time.unscaledTime > closeTime)
		{
			Close();
		}
	}
}
