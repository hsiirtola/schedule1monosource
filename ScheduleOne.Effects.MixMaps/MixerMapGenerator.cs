using System.Linq;
using ScheduleOne.Core;
using UnityEngine;

namespace ScheduleOne.Effects.MixMaps;

public class MixerMapGenerator : MonoBehaviour
{
	public float MapRadius = 5f;

	public string MapName = "New Map";

	public Transform BasePlateMesh;

	public MixMapEffect EffectPrefab;

	private void OnValidate()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		BasePlateMesh.localScale = Vector3.one * MapRadius * 2f * 0.01f;
		((Object)((Component)this).gameObject).name = MapName;
	}

	[Button]
	public void CreateEffectPrefabs()
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		Effect[] array = Resources.LoadAll<Effect>("Effects");
		foreach (Effect effect in array)
		{
			if ((Object)(object)GetEffect(effect) == (Object)null)
			{
				MixMapEffect mixMapEffect = Object.Instantiate<MixMapEffect>(EffectPrefab, ((Component)this).transform);
				mixMapEffect.Property = effect;
				mixMapEffect.Radius = 0.5f;
				((Component)mixMapEffect).transform.position = new Vector3(Random.Range(0f - MapRadius, MapRadius), 0.1f, Random.Range(0f - MapRadius, MapRadius));
			}
		}
	}

	[Button]
	public MixMapEffect GetEffect(Effect effect)
	{
		return ((Component)this).GetComponentsInChildren<MixMapEffect>().FirstOrDefault((MixMapEffect mixMapEffect) => (Object)(object)mixMapEffect.Property == (Object)(object)mixMapEffect);
	}
}
