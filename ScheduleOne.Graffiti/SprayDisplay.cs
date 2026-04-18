using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ScheduleOne.Graffiti;

[RequireComponent(typeof(SpraySurface))]
public class SprayDisplay : MonoBehaviour
{
	public SpraySurface SpraySurface;

	public DecalProjector Projector;

	private Material cachedMaterial;

	private void Awake()
	{
		SpraySurface spraySurface = SpraySurface;
		spraySurface.onDrawingChanged = (Action)Delegate.Combine(spraySurface.onDrawingChanged, new Action(Redraw));
		((Behaviour)Projector).enabled = false;
	}

	private void Redraw()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)cachedMaterial == (Object)null)
		{
			cachedMaterial = new Material(Projector.material);
			Projector.material = cachedMaterial;
		}
		cachedMaterial.SetTexture("_Base_Map", SpraySurface.DrawingOutputTexture);
		cachedMaterial.SetColor("_Color", ((Object)(object)SpraySurface.DrawingOutputTexture != (Object)null) ? Color.white : Color.clear);
		((Behaviour)Projector).enabled = SpraySurface.DrawingPaintedPixelCount > 0;
	}
}
