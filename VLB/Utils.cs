using System;
using UnityEngine;

namespace VLB;

public static class Utils
{
	public enum FloatPackingPrecision
	{
		High = 64,
		Low = 8,
		Undef = 0
	}

	private const float kEpsilon = 1E-05f;

	private static FloatPackingPrecision ms_FloatPackingPrecision;

	private const int kFloatPackingHighMinShaderLevel = 35;

	public static float ComputeConeRadiusEnd(float fallOffEnd, float spotAngle)
	{
		return fallOffEnd * Mathf.Tan(spotAngle * ((float)Math.PI / 180f) * 0.5f);
	}

	public static float ComputeSpotAngle(float fallOffEnd, float coneRadiusEnd)
	{
		return Mathf.Atan2(coneRadiusEnd, fallOffEnd) * 57.29578f * 2f;
	}

	public static void Swap<T>(ref T a, ref T b)
	{
		T val = a;
		a = b;
		b = val;
	}

	public static string GetPath(Transform current)
	{
		if ((Object)(object)current.parent == (Object)null)
		{
			return "/" + ((Object)current).name;
		}
		return GetPath(current.parent) + "/" + ((Object)current).name;
	}

	public static T NewWithComponent<T>(string name) where T : Component
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		return new GameObject(name, new Type[1] { typeof(T) }).GetComponent<T>();
	}

	public static T GetOrAddComponent<T>(this GameObject self) where T : Component
	{
		T val = self.GetComponent<T>();
		if ((Object)(object)val == (Object)null)
		{
			val = self.AddComponent<T>();
		}
		return val;
	}

	public static T GetOrAddComponent<T>(this MonoBehaviour self) where T : Component
	{
		return ((Component)self).gameObject.GetOrAddComponent<T>();
	}

	public static void ForeachComponentsInAnyChildrenOnly<T>(this GameObject self, Action<T> lambda, bool includeInactive = false) where T : Component
	{
		T[] componentsInChildren = self.GetComponentsInChildren<T>(includeInactive);
		foreach (T val in componentsInChildren)
		{
			if ((Object)(object)((Component)val).gameObject != (Object)(object)self)
			{
				lambda(val);
			}
		}
	}

	public static void ForeachComponentsInDirectChildrenOnly<T>(this GameObject self, Action<T> lambda, bool includeInactive = false) where T : Component
	{
		T[] componentsInChildren = self.GetComponentsInChildren<T>(includeInactive);
		foreach (T val in componentsInChildren)
		{
			if ((Object)(object)((Component)val).transform.parent == (Object)(object)self.transform)
			{
				lambda(val);
			}
		}
	}

	public static void SetupDepthCamera(Camera depthCamera, float coneApexOffsetZ, float maxGeometryDistance, float coneRadiusStart, float coneRadiusEnd, Vector3 beamLocalForward, Vector3 lossyScale, bool isScalable, Quaternion beamInternalLocalRotation, bool shouldScaleMinNearClipPlane)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		if (!isScalable)
		{
			lossyScale.x = (lossyScale.y = 1f);
		}
		float num = coneApexOffsetZ;
		bool flag = num >= 0f;
		num = Mathf.Max(num, 0f);
		depthCamera.orthographic = !flag;
		((Component)depthCamera).transform.localPosition = beamLocalForward * (0f - num);
		Quaternion val = beamInternalLocalRotation;
		if (Mathf.Sign(lossyScale.z) < 0f)
		{
			val *= Quaternion.Euler(0f, 180f, 0f);
		}
		((Component)depthCamera).transform.localRotation = val;
		if (!Mathf.Approximately(lossyScale.y * lossyScale.z, 0f))
		{
			float num2 = (flag ? 0.1f : 0f);
			float num3 = Mathf.Abs(lossyScale.z);
			depthCamera.nearClipPlane = Mathf.Max(num * num3, num2 * (shouldScaleMinNearClipPlane ? num3 : 1f));
			depthCamera.farClipPlane = (maxGeometryDistance + num * (isScalable ? 1f : num3)) * (isScalable ? num3 : 1f);
			depthCamera.aspect = Mathf.Abs(lossyScale.x / lossyScale.y);
			if (flag)
			{
				float fieldOfView = Mathf.Atan2(coneRadiusEnd * Mathf.Abs(lossyScale.y), depthCamera.farClipPlane) * 57.29578f * 2f;
				depthCamera.fieldOfView = fieldOfView;
			}
			else
			{
				depthCamera.orthographicSize = coneRadiusStart * lossyScale.y;
			}
		}
	}

	public static bool HasFlag(this Enum mask, Enum flags)
	{
		return ((int)(object)mask & (int)(object)flags) == (int)(object)flags;
	}

	public static Vector3 Divide(this Vector3 aVector, Vector3 scale)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (Mathf.Approximately(scale.x * scale.y * scale.z, 0f))
		{
			return Vector3.zero;
		}
		return new Vector3(aVector.x / scale.x, aVector.y / scale.y, aVector.z / scale.z);
	}

	public static Vector2 xy(this Vector3 aVector)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(aVector.x, aVector.y);
	}

	public static Vector2 xz(this Vector3 aVector)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(aVector.x, aVector.z);
	}

	public static Vector2 yz(this Vector3 aVector)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(aVector.y, aVector.z);
	}

	public static Vector2 yx(this Vector3 aVector)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(aVector.y, aVector.x);
	}

	public static Vector2 zx(this Vector3 aVector)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(aVector.z, aVector.x);
	}

	public static Vector2 zy(this Vector3 aVector)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(aVector.z, aVector.y);
	}

	public static bool Approximately(this float a, float b, float epsilon = 1E-05f)
	{
		return Mathf.Abs(a - b) < epsilon;
	}

	public static bool Approximately(this Vector2 a, Vector2 b, float epsilon = 1E-05f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return Vector2.SqrMagnitude(a - b) < epsilon;
	}

	public static bool Approximately(this Vector3 a, Vector3 b, float epsilon = 1E-05f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.SqrMagnitude(a - b) < epsilon;
	}

	public static bool Approximately(this Vector4 a, Vector4 b, float epsilon = 1E-05f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return Vector4.SqrMagnitude(a - b) < epsilon;
	}

	public static Vector4 AsVector4(this Vector3 vec3, float w)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		return new Vector4(vec3.x, vec3.y, vec3.z, w);
	}

	public static Vector4 PlaneEquation(Vector3 normalizedNormal, Vector3 pt)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		return normalizedNormal.AsVector4(0f - Vector3.Dot(normalizedNormal, pt));
	}

	public static float GetVolumeCubic(this Bounds self)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		return ((Bounds)(ref self)).size.x * ((Bounds)(ref self)).size.y * ((Bounds)(ref self)).size.z;
	}

	public static float GetMaxArea2D(this Bounds self)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		return Mathf.Max(Mathf.Max(((Bounds)(ref self)).size.x * ((Bounds)(ref self)).size.y, ((Bounds)(ref self)).size.y * ((Bounds)(ref self)).size.z), ((Bounds)(ref self)).size.x * ((Bounds)(ref self)).size.z);
	}

	public static Color Opaque(this Color self)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		return new Color(self.r, self.g, self.b, 1f);
	}

	public static Color ComputeComplementaryColor(this Color self, bool blackAndWhite)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if (blackAndWhite)
		{
			if (!((double)self.r * 0.299 + (double)self.g * 0.587 + (double)self.b * 0.114 > 0.729411780834198))
			{
				return Color.white;
			}
			return Color.black;
		}
		return new Color(1f - self.r, 1f - self.g, 1f - self.b);
	}

	public static Plane TranslateCustom(this Plane plane, Vector3 translation)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		((Plane)(ref plane)).distance = ((Plane)(ref plane)).distance + Vector3.Dot(((Vector3)(ref translation)).normalized, ((Plane)(ref plane)).normal) * ((Vector3)(ref translation)).magnitude;
		return plane;
	}

	public static Vector3 ClosestPointOnPlaneCustom(this Plane plane, Vector3 point)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		return point - ((Plane)(ref plane)).GetDistanceToPoint(point) * ((Plane)(ref plane)).normal;
	}

	public static bool IsAlmostZero(float f)
	{
		return Mathf.Abs(f) < 0.001f;
	}

	public static bool IsValid(this Plane plane)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Vector3 normal = ((Plane)(ref plane)).normal;
		return ((Vector3)(ref normal)).sqrMagnitude > 0.5f;
	}

	public static void SetKeywordEnabled(this Material mat, string name, bool enabled)
	{
		if (enabled)
		{
			mat.EnableKeyword(name);
		}
		else
		{
			mat.DisableKeyword(name);
		}
	}

	public static void SetShaderKeywordEnabled(string name, bool enabled)
	{
		if (enabled)
		{
			Shader.EnableKeyword(name);
		}
		else
		{
			Shader.DisableKeyword(name);
		}
	}

	public static Matrix4x4 SampleInMatrix(this Gradient self, int floatPackingPrecision)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		Matrix4x4 result = default(Matrix4x4);
		for (int i = 0; i < 16; i++)
		{
			Color color = self.Evaluate(Mathf.Clamp01((float)i / 15f));
			((Matrix4x4)(ref result))[i] = color.PackToFloat(floatPackingPrecision);
		}
		return result;
	}

	public static Color[] SampleInArray(this Gradient self, int samplesCount)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		Color[] array = (Color[])(object)new Color[samplesCount];
		for (int i = 0; i < samplesCount; i++)
		{
			array[i] = self.Evaluate(Mathf.Clamp01((float)i / (float)(samplesCount - 1)));
		}
		return array;
	}

	private static Vector4 Vector4_Floor(Vector4 vec)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		return new Vector4(Mathf.Floor(vec.x), Mathf.Floor(vec.y), Mathf.Floor(vec.z), Mathf.Floor(vec.w));
	}

	public static float PackToFloat(this Color color, int floatPackingPrecision)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		Vector4 val = Vector4_Floor(Color.op_Implicit(color * (float)(floatPackingPrecision - 1)));
		return 0f + val.x * (float)floatPackingPrecision * (float)floatPackingPrecision * (float)floatPackingPrecision + val.y * (float)floatPackingPrecision * (float)floatPackingPrecision + val.z * (float)floatPackingPrecision + val.w;
	}

	public static FloatPackingPrecision GetFloatPackingPrecision()
	{
		if (ms_FloatPackingPrecision == FloatPackingPrecision.Undef)
		{
			ms_FloatPackingPrecision = ((SystemInfo.graphicsShaderLevel >= 35) ? FloatPackingPrecision.High : FloatPackingPrecision.Low);
		}
		return ms_FloatPackingPrecision;
	}

	public static bool HasAtLeastOneFlag(this Enum mask, Enum flags)
	{
		return ((int)(object)mask & (int)(object)flags) != 0;
	}

	public static void MarkCurrentSceneDirty()
	{
	}

	public static void MarkObjectDirty(Object obj)
	{
	}
}
