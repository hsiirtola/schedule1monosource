using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ScheduleOne.FX;

public class ProximityCircle : MonoBehaviour
{
	[Header("References")]
	public DecalProjector Circle;

	private bool enabledThisFrame;

	private Material materialInstance;

	private void Awake()
	{
		materialInstance = Object.Instantiate<Material>(Circle.material);
		Circle.material = materialInstance;
	}

	private void LateUpdate()
	{
		if (!enabledThisFrame)
		{
			SetAlpha(0f);
			enabledThisFrame = false;
		}
		enabledThisFrame = false;
	}

	public void SetRadius(float rad)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		Circle.size = new Vector3(rad * 2f, rad * 2f, 3f);
	}

	public void SetAlpha(float alpha)
	{
		enabledThisFrame = true;
		Circle.fadeFactor = alpha;
		((Behaviour)Circle).enabled = alpha > 0f;
	}

	public void SetColor(Color col)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		materialInstance.color = col;
	}
}
