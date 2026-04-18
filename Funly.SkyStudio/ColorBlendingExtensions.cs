using UnityEngine;

namespace Funly.SkyStudio;

public static class ColorBlendingExtensions
{
	public static Color Clear(this Color color)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		return new Color(color.r, color.g, color.b, 0f);
	}
}
