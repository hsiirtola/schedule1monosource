using UnityEngine;

namespace VLB;

public class PlatformHelper
{
	public static string GetCurrentPlatformSuffix()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return GetPlatformSuffix(Application.platform);
	}

	private unsafe static string GetPlatformSuffix(RuntimePlatform platform)
	{
		return ((object)(*(RuntimePlatform*)(&platform))/*cast due to .constrained prefix*/).ToString();
	}
}
