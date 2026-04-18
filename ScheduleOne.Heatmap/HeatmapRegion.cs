using ScheduleOne.Tiles;
using UnityEngine;

namespace ScheduleOne.Heatmap;

public class HeatmapRegion : MonoBehaviour
{
	public int _textureIndex;

	private MeshRenderer _renderer;

	public void Create(Grid grid, int textureIndex, Material heatmapMat)
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Expected O, but got Unknown
		GameObject val = GameObject.CreatePrimitive((PrimitiveType)5);
		((Object)val).name = "Quad";
		Collider component = val.GetComponent<Collider>();
		if ((Object)(object)component != (Object)null)
		{
			Object.Destroy((Object)(object)component);
		}
		val.transform.parent = ((Component)this).transform;
		_renderer = val.GetComponent<MeshRenderer>();
		((Renderer)_renderer).material = heatmapMat;
		_textureIndex = textureIndex;
		val.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
		val.transform.localScale = Vector3.one * 128f / 2f;
		val.transform.localPosition = new Vector3((val.transform.localScale.x - 1.5f) / 2f, 0.001f, (val.transform.localScale.z - 1.5f) / 2f);
		MaterialPropertyBlock val2 = new MaterialPropertyBlock();
		((Renderer)_renderer).GetPropertyBlock(val2);
		val2.SetFloat("_TextureIndex", (float)_textureIndex);
		((Renderer)_renderer).SetPropertyBlock(val2);
	}
}
