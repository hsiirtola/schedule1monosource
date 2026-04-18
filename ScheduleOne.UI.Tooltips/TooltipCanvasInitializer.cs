using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.UI.Tooltips;

[RequireComponent(typeof(Canvas))]
public class TooltipCanvasInitializer : MonoBehaviour
{
	private void Start()
	{
		Singleton<TooltipManager>.Instance.AddCanvas(((Component)this).GetComponent<Canvas>());
	}
}
