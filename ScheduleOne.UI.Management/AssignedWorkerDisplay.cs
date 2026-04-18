using ScheduleOne.NPCs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Management;

public class AssignedWorkerDisplay : MonoBehaviour
{
	public Image Icon;

	public TextMeshProUGUI NameLabel;

	public void Set(NPC npc)
	{
		if ((Object)(object)npc != (Object)null)
		{
			Icon.sprite = npc.MugshotSprite;
			((TMP_Text)NameLabel).text = npc.FirstName;
		}
		((Component)this).gameObject.SetActive((Object)(object)npc != (Object)null);
	}
}
