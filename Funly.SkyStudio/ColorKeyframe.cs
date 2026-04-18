using System;
using UnityEngine;

namespace Funly.SkyStudio;

[Serializable]
public class ColorKeyframe : BaseKeyframe
{
	public Color color = Color.white;

	public ColorKeyframe(Color c, float time)
		: base(time)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		color = c;
	}

	public ColorKeyframe(ColorKeyframe keyframe)
		: base(keyframe.time)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		color = keyframe.color;
		base.interpolationCurve = keyframe.interpolationCurve;
		base.interpolationDirection = keyframe.interpolationDirection;
	}
}
