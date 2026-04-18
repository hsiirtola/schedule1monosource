using UnityEngine;

namespace VLB;

public static class UtilsBeamProps
{
	public static bool CanChangeDuringPlaytime(VolumetricLightBeamAbstractBase self)
	{
		VolumetricLightBeamSD volumetricLightBeamSD = self as VolumetricLightBeamSD;
		if (Object.op_Implicit((Object)(object)volumetricLightBeamSD))
		{
			return volumetricLightBeamSD.trackChangesDuringPlaytime;
		}
		return true;
	}

	public static Quaternion GetInternalLocalRotation(VolumetricLightBeamAbstractBase self)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		VolumetricLightBeamSD volumetricLightBeamSD = self as VolumetricLightBeamSD;
		if (Object.op_Implicit((Object)(object)volumetricLightBeamSD))
		{
			return volumetricLightBeamSD.beamInternalLocalRotation;
		}
		VolumetricLightBeamHD volumetricLightBeamHD = self as VolumetricLightBeamHD;
		if (Object.op_Implicit((Object)(object)volumetricLightBeamHD))
		{
			return volumetricLightBeamHD.beamInternalLocalRotation;
		}
		return Quaternion.identity;
	}

	public static float GetThickness(VolumetricLightBeamAbstractBase self)
	{
		VolumetricLightBeamSD volumetricLightBeamSD = self as VolumetricLightBeamSD;
		if (Object.op_Implicit((Object)(object)volumetricLightBeamSD))
		{
			return Mathf.Clamp01(1f - volumetricLightBeamSD.fresnelPow / 10f);
		}
		VolumetricLightBeamHD volumetricLightBeamHD = self as VolumetricLightBeamHD;
		if (Object.op_Implicit((Object)(object)volumetricLightBeamHD))
		{
			return Mathf.Clamp01(1f - volumetricLightBeamHD.sideSoftness / 10f);
		}
		return 0f;
	}

	public static float GetFallOffEnd(VolumetricLightBeamAbstractBase self)
	{
		VolumetricLightBeamSD volumetricLightBeamSD = self as VolumetricLightBeamSD;
		if (Object.op_Implicit((Object)(object)volumetricLightBeamSD))
		{
			return volumetricLightBeamSD.fallOffEnd;
		}
		VolumetricLightBeamHD volumetricLightBeamHD = self as VolumetricLightBeamHD;
		if (Object.op_Implicit((Object)(object)volumetricLightBeamHD))
		{
			return volumetricLightBeamHD.fallOffEnd;
		}
		return 0f;
	}

	public static ColorMode GetColorMode(VolumetricLightBeamAbstractBase self)
	{
		VolumetricLightBeamSD volumetricLightBeamSD = self as VolumetricLightBeamSD;
		if (Object.op_Implicit((Object)(object)volumetricLightBeamSD))
		{
			return volumetricLightBeamSD.usedColorMode;
		}
		VolumetricLightBeamHD volumetricLightBeamHD = self as VolumetricLightBeamHD;
		if (Object.op_Implicit((Object)(object)volumetricLightBeamHD))
		{
			return volumetricLightBeamHD.colorMode;
		}
		return ColorMode.Flat;
	}

	public static Color GetColorFlat(VolumetricLightBeamAbstractBase self)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		VolumetricLightBeamSD volumetricLightBeamSD = self as VolumetricLightBeamSD;
		if (Object.op_Implicit((Object)(object)volumetricLightBeamSD))
		{
			return volumetricLightBeamSD.color;
		}
		VolumetricLightBeamHD volumetricLightBeamHD = self as VolumetricLightBeamHD;
		if (Object.op_Implicit((Object)(object)volumetricLightBeamHD))
		{
			return volumetricLightBeamHD.colorFlat;
		}
		return Color.white;
	}

	public static Gradient GetColorGradient(VolumetricLightBeamAbstractBase self)
	{
		VolumetricLightBeamSD volumetricLightBeamSD = self as VolumetricLightBeamSD;
		if (Object.op_Implicit((Object)(object)volumetricLightBeamSD))
		{
			return volumetricLightBeamSD.colorGradient;
		}
		VolumetricLightBeamHD volumetricLightBeamHD = self as VolumetricLightBeamHD;
		if (Object.op_Implicit((Object)(object)volumetricLightBeamHD))
		{
			return volumetricLightBeamHD.colorGradient;
		}
		return null;
	}

	public static float GetConeAngle(VolumetricLightBeamAbstractBase self)
	{
		VolumetricLightBeamSD volumetricLightBeamSD = self as VolumetricLightBeamSD;
		if (Object.op_Implicit((Object)(object)volumetricLightBeamSD))
		{
			return volumetricLightBeamSD.coneAngle;
		}
		VolumetricLightBeamHD volumetricLightBeamHD = self as VolumetricLightBeamHD;
		if (Object.op_Implicit((Object)(object)volumetricLightBeamHD))
		{
			return volumetricLightBeamHD.coneAngle;
		}
		return 0f;
	}

	public static float GetConeRadiusStart(VolumetricLightBeamAbstractBase self)
	{
		VolumetricLightBeamSD volumetricLightBeamSD = self as VolumetricLightBeamSD;
		if (Object.op_Implicit((Object)(object)volumetricLightBeamSD))
		{
			return volumetricLightBeamSD.coneRadiusStart;
		}
		VolumetricLightBeamHD volumetricLightBeamHD = self as VolumetricLightBeamHD;
		if (Object.op_Implicit((Object)(object)volumetricLightBeamHD))
		{
			return volumetricLightBeamHD.coneRadiusStart;
		}
		return 0f;
	}

	public static float GetConeRadiusEnd(VolumetricLightBeamAbstractBase self)
	{
		VolumetricLightBeamSD volumetricLightBeamSD = self as VolumetricLightBeamSD;
		if (Object.op_Implicit((Object)(object)volumetricLightBeamSD))
		{
			return volumetricLightBeamSD.coneRadiusEnd;
		}
		VolumetricLightBeamHD volumetricLightBeamHD = self as VolumetricLightBeamHD;
		if (Object.op_Implicit((Object)(object)volumetricLightBeamHD))
		{
			return volumetricLightBeamHD.coneRadiusEnd;
		}
		return 0f;
	}

	public static int GetSortingLayerID(VolumetricLightBeamAbstractBase self)
	{
		VolumetricLightBeamSD volumetricLightBeamSD = self as VolumetricLightBeamSD;
		if (Object.op_Implicit((Object)(object)volumetricLightBeamSD))
		{
			return volumetricLightBeamSD.sortingLayerID;
		}
		VolumetricLightBeamHD volumetricLightBeamHD = self as VolumetricLightBeamHD;
		if (Object.op_Implicit((Object)(object)volumetricLightBeamHD))
		{
			return volumetricLightBeamHD.GetSortingLayerID();
		}
		return 0;
	}

	public static int GetSortingOrder(VolumetricLightBeamAbstractBase self)
	{
		VolumetricLightBeamSD volumetricLightBeamSD = self as VolumetricLightBeamSD;
		if (Object.op_Implicit((Object)(object)volumetricLightBeamSD))
		{
			return volumetricLightBeamSD.sortingOrder;
		}
		VolumetricLightBeamHD volumetricLightBeamHD = self as VolumetricLightBeamHD;
		if (Object.op_Implicit((Object)(object)volumetricLightBeamHD))
		{
			return volumetricLightBeamHD.GetSortingOrder();
		}
		return 0;
	}

	public static bool GetFadeOutEnabled(VolumetricLightBeamAbstractBase self)
	{
		VolumetricLightBeamSD volumetricLightBeamSD = self as VolumetricLightBeamSD;
		if (Object.op_Implicit((Object)(object)volumetricLightBeamSD))
		{
			return volumetricLightBeamSD.isFadeOutEnabled;
		}
		return false;
	}

	public static float GetFadeOutEnd(VolumetricLightBeamAbstractBase self)
	{
		VolumetricLightBeamSD volumetricLightBeamSD = self as VolumetricLightBeamSD;
		if (Object.op_Implicit((Object)(object)volumetricLightBeamSD))
		{
			return volumetricLightBeamSD.fadeOutEnd;
		}
		return 0f;
	}

	public static Dimensions GetDimensions(VolumetricLightBeamAbstractBase self)
	{
		VolumetricLightBeamSD volumetricLightBeamSD = self as VolumetricLightBeamSD;
		if (Object.op_Implicit((Object)(object)volumetricLightBeamSD))
		{
			return volumetricLightBeamSD.dimensions;
		}
		VolumetricLightBeamHD volumetricLightBeamHD = self as VolumetricLightBeamHD;
		if (Object.op_Implicit((Object)(object)volumetricLightBeamHD))
		{
			return volumetricLightBeamHD.GetDimensions();
		}
		return Dimensions.Dim3D;
	}

	public static int GetGeomSides(VolumetricLightBeamAbstractBase self)
	{
		VolumetricLightBeamSD volumetricLightBeamSD = self as VolumetricLightBeamSD;
		if (Object.op_Implicit((Object)(object)volumetricLightBeamSD))
		{
			return volumetricLightBeamSD.geomSides;
		}
		return Config.Instance.sharedMeshSides;
	}
}
