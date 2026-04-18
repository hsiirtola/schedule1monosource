using ScheduleOne.FX;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ScheduleOne.Tools;

public class ResetRenderShit : MonoBehaviour
{
	public UniversalRendererData rendererData;

	private void Awake()
	{
		PsychedelicFullScreenFeature psychedelicFullScreenFeature = ((ScriptableRendererData)rendererData).rendererFeatures.Find((ScriptableRendererFeature x) => ((Object)x).name == "PsychedelicFullScreenFeature") as PsychedelicFullScreenFeature;
		if ((Object)(object)psychedelicFullScreenFeature != (Object)null)
		{
			((ScriptableRendererFeature)psychedelicFullScreenFeature).SetActive(false);
		}
	}
}
