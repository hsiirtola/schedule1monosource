using TMPro;
using UnityEngine;

namespace ScheduleOne.UI;

public class VersionText : MonoBehaviour
{
	private void Awake()
	{
		((TMP_Text)((Component)this).GetComponent<TextMeshProUGUI>()).text = "v" + Application.version;
	}
}
