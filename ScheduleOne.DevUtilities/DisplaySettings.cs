using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScheduleOne.DevUtilities;

[Serializable]
public struct DisplaySettings
{
	public enum EDisplayMode
	{
		Windowed,
		FullscreenWindow,
		ExclusiveFullscreen
	}

	public int ResolutionIndex;

	public EDisplayMode DisplayMode;

	public bool VSync;

	public int TargetFPS;

	public float UIScale;

	public float CameraBobbing;

	public int ActiveDisplayIndex;

	public Settings.EUnitType UnitType;

	public static List<Resolution> GetResolutions()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		Resolution[] resolutions = Screen.resolutions;
		RefreshRate refreshRateRatio = ((Resolution)(ref resolutions[resolutions.Length - 1])).refreshRateRatio;
		float num = refreshRateRatio.numerator / GetDenominatorSafe(refreshRateRatio);
		List<Resolution> list = new List<Resolution>();
		int i;
		for (i = 0; i < resolutions.Length; i++)
		{
			if (!list.Exists((Resolution x) => ((Resolution)(ref x)).width == ((Resolution)(ref resolutions[i])).width && ((Resolution)(ref x)).height == ((Resolution)(ref resolutions[i])).height))
			{
				Resolution item = resolutions[i];
				if ((float)(((Resolution)(ref item)).refreshRateRatio.numerator / GetDenominatorSafe(((Resolution)(ref item)).refreshRateRatio)) >= num - 0.1f)
				{
					list.Add(item);
				}
			}
		}
		return list;
	}

	private static uint GetDenominatorSafe(RefreshRate refreshRate)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if (refreshRate.denominator == 0)
		{
			return 1u;
		}
		return refreshRate.denominator;
	}
}
