using UnityEngine;

namespace ScheduleOne.Effects.MixMaps;

public class MixMapEffect : MonoBehaviour
{
	public Effect Property;

	[Range(0.05f, 3f)]
	public float Radius = 0.5f;

	public Vector2 Position => new Vector2(((Component)this).transform.position.x, ((Component)this).transform.position.z);

	public void OnValidate()
	{
		if (!((Object)(object)Property == (Object)null))
		{
			((Object)((Component)this).gameObject).name = Property.Name;
		}
	}
}
