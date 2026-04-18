using System;
using UnityEngine;

namespace VLB;

public static class MeshGenerator
{
	public enum CapMode
	{
		None,
		OneVertexPerCap_1Cap,
		OneVertexPerCap_2Caps,
		SpecificVerticesPerCap_1Cap,
		SpecificVerticesPerCap_2Caps
	}

	private const float kMinTruncatedRadius = 0.001f;

	private static float GetAngleOffset(int numSides)
	{
		if (numSides != 4)
		{
			return 0f;
		}
		return (float)Math.PI / 4f;
	}

	private static float GetRadiiScale(int numSides)
	{
		if (numSides != 4)
		{
			return 1f;
		}
		return Mathf.Sqrt(2f);
	}

	public static Mesh GenerateConeZ_RadiusAndAngle(float lengthZ, float radiusStart, float coneAngle, int numSides, int numSegments, bool cap, bool doubleSided)
	{
		float radiusEnd = lengthZ * Mathf.Tan(coneAngle * ((float)Math.PI / 180f) * 0.5f);
		return GenerateConeZ_Radii(lengthZ, radiusStart, radiusEnd, numSides, numSegments, cap, doubleSided);
	}

	public static Mesh GenerateConeZ_Angle(float lengthZ, float coneAngle, int numSides, int numSegments, bool cap, bool doubleSided)
	{
		return GenerateConeZ_RadiusAndAngle(lengthZ, 0f, coneAngle, numSides, numSegments, cap, doubleSided);
	}

	public static Mesh GenerateConeZ_Radii(float lengthZ, float radiusStart, float radiusEnd, int numSides, int numSegments, bool cap, bool doubleSided)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fa: Unknown result type (might be due to invalid IL or missing references)
		Mesh val = new Mesh();
		bool flag = cap && radiusStart > 0f;
		radiusStart = Mathf.Max(radiusStart, 0.001f);
		float radiiScale = GetRadiiScale(numSides);
		radiusStart *= radiiScale;
		radiusEnd *= radiiScale;
		int num = numSides * (numSegments + 2);
		int num2 = num;
		if (flag)
		{
			num2 += numSides + 1;
		}
		float angleOffset = GetAngleOffset(numSides);
		Vector3[] array = (Vector3[])(object)new Vector3[num2];
		for (int i = 0; i < numSides; i++)
		{
			float num3 = angleOffset + (float)Math.PI * 2f * (float)i / (float)numSides;
			float num4 = Mathf.Cos(num3);
			float num5 = Mathf.Sin(num3);
			for (int j = 0; j < numSegments + 2; j++)
			{
				float num6 = (float)j / (float)(numSegments + 1);
				float num7 = Mathf.Lerp(radiusStart, radiusEnd, num6);
				array[i + j * numSides] = new Vector3(num7 * num4, num7 * num5, num6 * lengthZ);
			}
		}
		if (flag)
		{
			int num8 = num;
			array[num8] = Vector3.zero;
			num8++;
			for (int k = 0; k < numSides; k++)
			{
				float num9 = angleOffset + (float)Math.PI * 2f * (float)k / (float)numSides;
				float num10 = Mathf.Cos(num9);
				float num11 = Mathf.Sin(num9);
				array[num8] = new Vector3(radiusStart * num10, radiusStart * num11, 0f);
				num8++;
			}
		}
		if (!doubleSided)
		{
			val.vertices = array;
		}
		else
		{
			Vector3[] array2 = (Vector3[])(object)new Vector3[array.Length * 2];
			array.CopyTo(array2, 0);
			array.CopyTo(array2, array.Length);
			val.vertices = array2;
		}
		Vector2[] array3 = (Vector2[])(object)new Vector2[num2];
		int num12 = 0;
		for (int l = 0; l < num; l++)
		{
			array3[num12++] = Vector2.zero;
		}
		if (flag)
		{
			for (int m = 0; m < numSides + 1; m++)
			{
				array3[num12++] = new Vector2(1f, 0f);
			}
		}
		if (!doubleSided)
		{
			val.uv = array3;
		}
		else
		{
			Vector2[] array4 = (Vector2[])(object)new Vector2[array3.Length * 2];
			array3.CopyTo(array4, 0);
			array3.CopyTo(array4, array3.Length);
			for (int n = 0; n < array3.Length; n++)
			{
				Vector2 val2 = array4[n + array3.Length];
				array4[n + array3.Length] = new Vector2(val2.x, 1f);
			}
			val.uv = array4;
		}
		int num13 = numSides * 2 * Mathf.Max(numSegments + 1, 1) * 3;
		if (flag)
		{
			num13 += numSides * 3;
		}
		int[] array5 = new int[num13];
		int num14 = 0;
		for (int num15 = 0; num15 < numSides; num15++)
		{
			int num16 = num15 + 1;
			if (num16 == numSides)
			{
				num16 = 0;
			}
			for (int num17 = 0; num17 < numSegments + 1; num17++)
			{
				int num18 = num17 * numSides;
				array5[num14++] = num18 + num15;
				array5[num14++] = num18 + num16;
				array5[num14++] = num18 + num15 + numSides;
				array5[num14++] = num18 + num16 + numSides;
				array5[num14++] = num18 + num15 + numSides;
				array5[num14++] = num18 + num16;
			}
		}
		if (flag)
		{
			for (int num19 = 0; num19 < numSides - 1; num19++)
			{
				array5[num14++] = num;
				array5[num14++] = num + num19 + 2;
				array5[num14++] = num + num19 + 1;
			}
			array5[num14++] = num;
			array5[num14++] = num + 1;
			array5[num14++] = num + numSides;
		}
		if (!doubleSided)
		{
			val.triangles = array5;
		}
		else
		{
			int[] array6 = new int[array5.Length * 2];
			array5.CopyTo(array6, 0);
			for (int num20 = 0; num20 < array5.Length; num20 += 3)
			{
				array6[array5.Length + num20] = array5[num20] + num2;
				array6[array5.Length + num20 + 1] = array5[num20 + 2] + num2;
				array6[array5.Length + num20 + 2] = array5[num20 + 1] + num2;
			}
			val.triangles = array6;
		}
		val.bounds = ComputeBounds(lengthZ, radiusStart, radiusEnd);
		return val;
	}

	public static Mesh GenerateConeZ_Radii_DoubleCaps(float lengthZ, float radiusStart, float radiusEnd, int numSides, bool inverted)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		Mesh val = new Mesh();
		radiusStart = Mathf.Max(radiusStart, 0.001f);
		int vertCountSides = numSides * 2;
		int num = vertCountSides;
		Func<int, int> vertSidesStartFromSlide = (int slideID) => numSides * slideID;
		Func<int, int> vertCenterFromSlide = (int slideID) => vertCountSides + slideID;
		int num2 = num + 2;
		float angleOffset = GetAngleOffset(numSides);
		Vector3[] array = (Vector3[])(object)new Vector3[num2];
		for (int num3 = 0; num3 < numSides; num3++)
		{
			float num4 = angleOffset + (float)Math.PI * 2f * (float)num3 / (float)numSides;
			float num5 = Mathf.Cos(num4);
			float num6 = Mathf.Sin(num4);
			for (int num7 = 0; num7 < 2; num7++)
			{
				float num8 = num7;
				float num9 = Mathf.Lerp(radiusStart, radiusEnd, num8);
				array[num3 + vertSidesStartFromSlide(num7)] = new Vector3(num9 * num5, num9 * num6, num8 * lengthZ);
			}
		}
		array[vertCenterFromSlide(0)] = Vector3.zero;
		array[vertCenterFromSlide(1)] = new Vector3(0f, 0f, lengthZ);
		val.vertices = array;
		int num10 = numSides * 2 * 3;
		num10 += numSides * 3;
		num10 += numSides * 3;
		int[] indices = new int[num10];
		int ind = 0;
		for (int num11 = 0; num11 < numSides; num11++)
		{
			int num12 = num11 + 1;
			if (num12 == numSides)
			{
				num12 = 0;
			}
			for (int num13 = 0; num13 < 1; num13++)
			{
				int num14 = num13 * numSides;
				indices[ind] = num14 + num11;
				indices[ind + (inverted ? 1 : 2)] = num14 + num12;
				indices[ind + ((!inverted) ? 1 : 2)] = num14 + num11 + numSides;
				indices[ind + 3] = num14 + num12 + numSides;
				indices[ind + (inverted ? 4 : 5)] = num14 + num11 + numSides;
				indices[ind + (inverted ? 5 : 4)] = num14 + num12;
				ind += 6;
			}
		}
		Action<int, bool> action = delegate(int slideID, bool invert)
		{
			int num15 = vertSidesStartFromSlide(slideID);
			for (int i = 0; i < numSides - 1; i++)
			{
				indices[ind] = vertCenterFromSlide(slideID);
				indices[ind + (invert ? 1 : 2)] = num15 + i + 1;
				indices[ind + ((!invert) ? 1 : 2)] = num15 + i;
				ind += 3;
			}
			indices[ind] = vertCenterFromSlide(slideID);
			indices[ind + (invert ? 1 : 2)] = num15;
			indices[ind + ((!invert) ? 1 : 2)] = num15 + numSides - 1;
			ind += 3;
		};
		action(0, inverted);
		action(1, !inverted);
		val.triangles = indices;
		Bounds bounds = default(Bounds);
		((Bounds)(ref bounds))._002Ector(new Vector3(0f, 0f, lengthZ * 0.5f), new Vector3(Mathf.Max(radiusStart, radiusEnd) * 2f, Mathf.Max(radiusStart, radiusEnd) * 2f, lengthZ));
		val.bounds = bounds;
		return val;
	}

	public static Bounds ComputeBounds(float lengthZ, float radiusStart, float radiusEnd)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Max(radiusStart, radiusEnd) * 2f;
		return new Bounds(new Vector3(0f, 0f, lengthZ * 0.5f), new Vector3(num, num, lengthZ));
	}

	private static int GetCapAdditionalVerticesCount(CapMode capMode, int numSides)
	{
		return capMode switch
		{
			CapMode.None => 0, 
			CapMode.OneVertexPerCap_1Cap => 1, 
			CapMode.OneVertexPerCap_2Caps => 2, 
			CapMode.SpecificVerticesPerCap_1Cap => numSides + 1, 
			CapMode.SpecificVerticesPerCap_2Caps => 2 * (numSides + 1), 
			_ => 0, 
		};
	}

	private static int GetCapAdditionalIndicesCount(CapMode capMode, int numSides)
	{
		switch (capMode)
		{
		case CapMode.None:
			return 0;
		case CapMode.OneVertexPerCap_1Cap:
		case CapMode.SpecificVerticesPerCap_1Cap:
			return numSides * 3;
		case CapMode.OneVertexPerCap_2Caps:
		case CapMode.SpecificVerticesPerCap_2Caps:
			return 2 * (numSides * 3);
		default:
			return 0;
		}
	}

	public static int GetVertexCount(int numSides, int numSegments, CapMode capMode, bool doubleSided)
	{
		int num = numSides * (numSegments + 2);
		num += GetCapAdditionalVerticesCount(capMode, numSides);
		if (doubleSided)
		{
			num *= 2;
		}
		return num;
	}

	public static int GetIndicesCount(int numSides, int numSegments, CapMode capMode, bool doubleSided)
	{
		int num = numSides * (numSegments + 1) * 2 * 3;
		num += GetCapAdditionalIndicesCount(capMode, numSides);
		if (doubleSided)
		{
			num *= 2;
		}
		return num;
	}

	public static int GetSharedMeshVertexCount()
	{
		return GetVertexCount(Config.Instance.sharedMeshSides, Config.Instance.sharedMeshSegments, CapMode.SpecificVerticesPerCap_1Cap, Config.Instance.SD_requiresDoubleSidedMesh);
	}

	public static int GetSharedMeshIndicesCount()
	{
		return GetIndicesCount(Config.Instance.sharedMeshSides, Config.Instance.sharedMeshSegments, CapMode.SpecificVerticesPerCap_1Cap, Config.Instance.SD_requiresDoubleSidedMesh);
	}

	public static int GetSharedMeshHDVertexCount()
	{
		return GetVertexCount(Config.Instance.sharedMeshSides, 0, CapMode.OneVertexPerCap_2Caps, doubleSided: false);
	}

	public static int GetSharedMeshHDIndicesCount()
	{
		return GetIndicesCount(Config.Instance.sharedMeshSides, 0, CapMode.OneVertexPerCap_2Caps, doubleSided: false);
	}
}
