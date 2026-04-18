using ScheduleOne.DevUtilities;
using ScheduleOne.Heatmap;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.Building;

public class BuildStop_Base : MonoBehaviour
{
	public virtual void Stop_Building()
	{
		if (PlayerSingleton<PlayerCamera>.Instance.activeUIElementCount == 0)
		{
			Singleton<HUD>.Instance.SetCrosshairVisible(vis: true);
		}
		((Component)this).GetComponent<BuildUpdate_Base>().Stop();
		Singleton<InputPromptsCanvas>.Instance.UnloadModule();
		if (Singleton<HeatmapManager>.InstanceExists)
		{
			Singleton<HeatmapManager>.Instance.SetAllHeatmapsActive(isActive: false);
		}
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}
}
