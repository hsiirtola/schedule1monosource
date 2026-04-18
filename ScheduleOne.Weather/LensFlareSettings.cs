using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace ScheduleOne.Weather;

[CreateAssetMenu(fileName = "LensFlareSettings", menuName = "ScriptableObjects/Weather/Lens Flare Settings")]
public class LensFlareSettings : ScriptableObject
{
	[Serializable]
	public class LensFlareSettingsGroup
	{
		public LensFlareDataSRP LensFlare;

		public float Intensity;
	}

	[SerializeField]
	private LensFlareSettingsGroup[] lensFlareGroups;

	public bool TryGetLensFlareSettings(LensFlareDataSRP lensFlare, out LensFlareSettingsGroup group)
	{
		LensFlareSettingsGroup[] array = lensFlareGroups;
		foreach (LensFlareSettingsGroup lensFlareSettingsGroup in array)
		{
			if ((Object)(object)lensFlareSettingsGroup.LensFlare == (Object)(object)lensFlare)
			{
				group = lensFlareSettingsGroup;
				return true;
			}
		}
		group = null;
		return false;
	}

	public LensFlareSettingsGroup[] GetLensFlareGroups()
	{
		return lensFlareGroups;
	}
}
