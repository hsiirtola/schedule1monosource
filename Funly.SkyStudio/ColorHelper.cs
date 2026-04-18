using UnityEngine;

namespace Funly.SkyStudio;

public abstract class ColorHelper
{
	public static Color ColorWithHex(uint hex)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		return ColorWithHexAlpha((hex << 8) | 0xFF);
	}

	public static Color ColorWithHexAlpha(uint hex)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)((hex >> 24) & 0xFF) / 255f;
		float num2 = (float)((hex >> 16) & 0xFF) / 255f;
		float num3 = (float)((hex >> 8) & 0xFF) / 255f;
		float num4 = (float)(hex & 0xFF) / 255f;
		return new Color(num, num2, num3, num4);
	}
}
